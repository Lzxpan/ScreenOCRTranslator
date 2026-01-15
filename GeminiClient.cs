using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class GeminiClient
{
    private string _apiKey;
    private string _modelName;

    // ✅ 重用 HttpClient（避免每次 new 造成抖動/連線成本）
    private static readonly HttpClient _http = new HttpClient();

    public GeminiClient(string apiKey, string modelName)
    {
        _apiKey = apiKey;
        _modelName = modelName;
    }

    public void UpdateSettings(string apiKey, string modelName)
    {
        _apiKey = apiKey;
        _modelName = modelName;
    }

    public async Task<string> SendImageForOCRAndTranslate(Bitmap image)
    {
        var r = await SendImageForOCRAndTranslateEx(image);
        return r.Text;
    }

    public async Task<string> TranslateText(string inputText)
    {
        var r = await TranslateTextEx(inputText);
        return r.Text;
    }

    public async Task<GeminiResult> SendImageForOCRAndTranslateEx(Bitmap image)
    {
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/{_modelName}:generateContent?key={_apiKey}";

        string base64Image;
        using (MemoryStream ms = new MemoryStream())
        {
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            base64Image = Convert.ToBase64String(ms.ToArray());
        }

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        // ✅ 縮短 prompt：少 prompt tokens，並要求只輸出譯文避免多餘 output tokens
                        new { text = "擷取圖片中所有可見文字並翻譯成繁體中文，只輸出譯文。" },
                        new
                        {
                            inlineData = new
                            {
                                mimeType = "image/png",
                                data = base64Image
                            }
                        }
                    }
                }
            },

            // ✅ 官方 camelCase（建議）
            generation_config = new
            {
                // ✅ 關閉 thinking（最有效，直接砍 thoughtsTokenCount）
                thinking_config = new { thinking_budget = 0 },

                // ✅ 減少 token/波動的常用參數
                candidate_count = 1,
                temperature = 0.0,
                max_output_tokens = 512,
                response_mime_type = "text/plain",

                // ✅ 你原本就有，保留（省圖像 tokens）
                media_resolution = "MEDIA_RESOLUTION_LOW"
            }
        };

        var jsonContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
        var response = await _http.PostAsync(url, jsonContent);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return BuildErrorResult(response, responseBody);

        return ParseGenerateContentResponse(responseBody);
    }

    public async Task<GeminiResult> TranslateTextEx(string inputText)
    {
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/{_modelName}:generateContent?key={_apiKey}";

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = inputText }
                    }
                }
            },

            generation_config = new
            {
                thinking_config = new { thinking_budget = 0 },
                response_mime_type = "text/plain",
                candidate_count = 1,
                temperature = 0.0,
                max_output_tokens = 256
            }
        };

        var jsonContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
        var response = await _http.PostAsync(url, jsonContent);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return BuildErrorResult(response, responseBody);

        return ParseGenerateContentResponse(responseBody);
    }

    public sealed class GeminiUsage
    {
        public int? PromptTokenCount { get; set; }

        // 有些模型回 candidatesTokenCount，有些回 responseTokenCount：我們在解析時會擇一
        public int? CandidatesTokenCount { get; set; }

        // 這些欄位「有就顯示」，沒有就維持 null
        public int? ThoughtsTokenCount { get; set; }
        public int? ToolUsePromptTokenCount { get; set; }
        public int? CachedContentTokenCount { get; set; }

        public int? TotalTokenCount { get; set; }
    }

    public sealed class GeminiResult
    {
        public string Text { get; set; }
        public GeminiUsage Usage { get; set; }
        public string Error { get; set; }

        // ✅ 讓 UI 可以顯示「多久後再試」
        public int? RetryAfterSeconds { get; set; }

        // ✅ 讓 UI 一眼判斷「是否為日配額打滿」
        public bool IsDailyQuotaExceeded { get; set; }

        public int HttpStatus { get; set; }
    }

    private static GeminiResult BuildErrorResult(HttpResponseMessage response, string responseBody)
    {
        var r = new GeminiResult
        {
            HttpStatus = (int)response.StatusCode,
            Text = $"錯誤：{(int)response.StatusCode} {response.StatusCode}",
            Usage = null,
            Error = $"HTTP {(int)response.StatusCode} {response.StatusCode}"
        };

        try
        {
            // 解析標準 error payload
            var root = JObject.Parse(responseBody);
            var msg = root["error"]?["message"]?.Value<string>();
            if (!string.IsNullOrWhiteSpace(msg))
                r.Error = msg;

            // Retry-After header（若有）
            if (response.Headers.TryGetValues("Retry-After", out var vals))
            {
                foreach (var v in vals)
                {
                    if (int.TryParse(v, out int sec))
                    {
                        r.RetryAfterSeconds = sec;
                        break;
                    }
                }
            }

            // RetryInfo.retryDelay（常見 "37s"）
            var details = root["error"]?["details"] as JArray;
            if (details != null)
            {
                foreach (var d in details)
                {
                    var type = d?["@type"]?.Value<string>() ?? "";
                    if (type.EndsWith("RetryInfo", StringComparison.OrdinalIgnoreCase))
                    {
                        var delay = d["retryDelay"]?.Value<string>(); // e.g. "37s"
                        if (!string.IsNullOrWhiteSpace(delay))
                        {
                            var m = Regex.Match(delay, @"(\d+)");
                            if (m.Success && int.TryParse(m.Groups[1].Value, out int sec))
                                r.RetryAfterSeconds = sec;
                        }
                    }

                    // QuotaFailure.quotaId（判斷是否 PerDay）
                    if (type.EndsWith("QuotaFailure", StringComparison.OrdinalIgnoreCase))
                    {
                        var violations = d["violations"] as JArray;
                        if (violations != null)
                        {
                            foreach (var v in violations)
                            {
                                var quotaId = v["quotaId"]?.Value<string>() ?? "";
                                if (quotaId.IndexOf("PerDay", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    r.IsDailyQuotaExceeded = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            // 兜底：從 message 抓 "Please retry in XXs"
            if (!r.RetryAfterSeconds.HasValue && !string.IsNullOrWhiteSpace(r.Error))
            {
                var m2 = Regex.Match(r.Error, @"Please\s+retry\s+in\s+([\d\.]+)s", RegexOptions.IgnoreCase);
                if (m2.Success && double.TryParse(m2.Groups[1].Value, out double ds))
                    r.RetryAfterSeconds = (int)Math.Ceiling(ds);
            }

            // 若 message 直接包含 PerDay quotaId 字樣（有些回應會放在 message）
            if (!r.IsDailyQuotaExceeded && !string.IsNullOrWhiteSpace(r.Error) &&
                r.Error.IndexOf("GenerateRequestsPerDayPerProjectPerModel-FreeTier", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                r.IsDailyQuotaExceeded = true;
            }
        }
        catch
        {
            // 不可解析就保留最基本錯誤
        }

        // UI 想直接顯示完整 server message：放 Text 也可以（你目前習慣用 Text 顯示）
        if (!string.IsNullOrWhiteSpace(r.Error))
            r.Text = $"錯誤：{r.Error}";

        return r;
    }

    private static GeminiResult ParseGenerateContentResponse(string responseBody)
    {
        try
        {
            var root = JObject.Parse(responseBody);

            string text =
                root["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.Value<string>()
                ?? "無法解析回應";

            var usage = root["usageMetadata"];
            var u = new GeminiUsage
            {
                PromptTokenCount = usage?["promptTokenCount"]?.Value<int?>(),

                // candidatesTokenCount / responseTokenCount 擇一
                CandidatesTokenCount =
                    usage?["candidatesTokenCount"]?.Value<int?>()
                    ?? usage?["responseTokenCount"]?.Value<int?>(),

                // 可能出現的附加 token（有就抓）
                ThoughtsTokenCount = usage?["thoughtsTokenCount"]?.Value<int?>(),
                ToolUsePromptTokenCount = usage?["toolUsePromptTokenCount"]?.Value<int?>(),
                CachedContentTokenCount = usage?["cachedContentTokenCount"]?.Value<int?>(),

                TotalTokenCount = usage?["totalTokenCount"]?.Value<int?>()
            };

            bool hasAny =
                u.PromptTokenCount.HasValue ||
                u.CandidatesTokenCount.HasValue ||
                u.TotalTokenCount.HasValue ||
                u.ThoughtsTokenCount.HasValue ||
                u.ToolUsePromptTokenCount.HasValue ||
                u.CachedContentTokenCount.HasValue;

            return new GeminiResult
            {
                HttpStatus = 200,
                Text = text,
                Usage = hasAny ? u : null
            };
        }
        catch (Exception ex)
        {
            return new GeminiResult
            {
                HttpStatus = 200,
                Text = "無法解析回應",
                Usage = null,
                Error = ex.Message
            };
        }
    }
}
