using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static GeminiClient;

namespace ScreenOCRTranslator
{
    public enum LlmProvider
    {
        Gemini,
        MistralPixtral,
        GroqLlama4
    }

    public sealed class LlmCredential
    {
        public LlmProvider Provider { get; set; }
        public string ApiKey { get; set; }
        public string Model { get; set; }
        public string BaseUrl { get; set; }
        public string DisplayName { get; set; }
    }

    public static class LlmErrorPolicy
    {
        public static bool ShouldSwitchProvider(GeminiResult r)
        {
            if (r == null) return false;
            if (r.HttpStatus == 0) return true;
            if (r.HttpStatus == 408 || r.HttpStatus == 429) return true;
            if (r.HttpStatus >= 500 && r.HttpStatus <= 599) return true;
            if (r.HttpStatus == 400 || r.HttpStatus == 401 || r.HttpStatus == 403 || r.HttpStatus == 404) return true;
            if (IsQuotaOrRateLimit(r)) return true;

            string text = (r.Error ?? "") + " " + (r.Text ?? "");
            if (string.IsNullOrWhiteSpace(text)) return false;

            return Regex.IsMatch(text,
                "model.*not.*found|not.*found|unsupported|unauthorized|forbidden|permission|api.?key|invalid.?key|service unavailable|temporarily unavailable|server error",
                RegexOptions.IgnoreCase);
        }

        public static bool IsQuotaOrRateLimit(GeminiResult r)
        {
            if (r == null) return false;
            if (r.HttpStatus == 429 || r.IsDailyQuotaExceeded) return true;

            string text = (r.Error ?? "") + " " + (r.Text ?? "");
            if (string.IsNullOrWhiteSpace(text)) return false;

            return Regex.IsMatch(text,
                "quota|rate.?limit|insufficient_quota|exhausted|too many requests",
                RegexOptions.IgnoreCase);
        }

        public static bool IsRetryableNetworkError(Exception ex)
        {
            if (ex == null) return false;
            return ex is HttpRequestException || ex is TaskCanceledException || ex is IOException;
        }
    }

    public sealed class OpenAiCompatibleClient
    {
        private readonly string _baseUrl;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly string _providerName;
        private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

        public OpenAiCompatibleClient(string baseUrl, string apiKey, string model, string providerName)
        {
            _baseUrl = (baseUrl ?? "").TrimEnd('/');
            _apiKey = apiKey ?? "";
            _model = model ?? "";
            _providerName = providerName ?? "LLM";
        }

        public async Task<GeminiResult> TranslateTextEx(string inputText)
        {
            var payload = new
            {
                model = _model,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = inputText
                    }
                },
                temperature = 0.0,
                max_tokens = 512,
                stream = false
            };

            return await PostChatCompletion(payload);
        }

        public async Task<GeminiResult> SendImageForOCRAndTranslateEx(Bitmap image)
        {
            string base64Image;
            using (var ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                base64Image = Convert.ToBase64String(ms.ToArray());
            }

            string dataUri = "data:image/png;base64," + base64Image;

            var payload = new
            {
                model = _model,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = "擷取圖片中所有可見文字並翻譯成繁體中文，只輸出譯文。" },
                            new { type = "image_url", image_url = new { url = dataUri } }
                        }
                    }
                },
                temperature = 0.0,
                max_tokens = 1024,
                stream = false
            };

            return await PostChatCompletion(payload);
        }

        private async Task<GeminiResult> PostChatCompletion(object payload)
        {
            string url = _baseUrl + "/chat/completions";
            var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            if (!string.IsNullOrWhiteSpace(_apiKey))
                req.Headers.TryAddWithoutValidation("Authorization", "Bearer " + _apiKey);

            HttpResponseMessage resp = null;
            string body = "";
            try
            {
                resp = await _http.SendAsync(req);
                body = await resp.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return new GeminiResult
                {
                    HttpStatus = 0,
                    Error = ex.Message,
                    Text = "錯誤：" + ex.Message
                };
            }

            if (!resp.IsSuccessStatusCode)
                return BuildErrorResult(resp, body);

            return ParseOpenAiChatResult(body);
        }

        private GeminiResult BuildErrorResult(HttpResponseMessage response, string responseBody)
        {
            var r = new GeminiResult
            {
                HttpStatus = (int)response.StatusCode,
                Error = string.IsNullOrWhiteSpace(responseBody) ? response.ReasonPhrase : responseBody,
                Text = string.IsNullOrWhiteSpace(responseBody) ? $"錯誤：HTTP {(int)response.StatusCode}" : $"錯誤：{responseBody}"
            };

            if (response.StatusCode == (HttpStatusCode)429)
            {
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
            }
            return r;
        }

        private GeminiResult ParseOpenAiChatResult(string responseBody)
        {
            try
            {
                var root = JObject.Parse(responseBody);
                var msg = root["choices"]?[0]?["message"]?["content"];

                string text;
                if (msg == null)
                {
                    text = "無法解析回應";
                }
                else if (msg.Type == JTokenType.String)
                {
                    text = msg.Value<string>();
                }
                else
                {
                    text = msg.ToString(Formatting.None);
                }

                var usage = root["usage"];
                GeminiUsage u = null;
                if (usage != null)
                {
                    u = new GeminiUsage
                    {
                        PromptTokenCount = usage["prompt_tokens"]?.Value<int?>(),
                        CandidatesTokenCount = usage["completion_tokens"]?.Value<int?>(),
                        TotalTokenCount = usage["total_tokens"]?.Value<int?>()
                    };
                }

                return new GeminiResult
                {
                    HttpStatus = 200,
                    Text = text,
                    Usage = u
                };
            }
            catch (Exception ex)
            {
                return new GeminiResult
                {
                    HttpStatus = 200,
                    Text = "無法解析回應",
                    Error = ex.Message
                };
            }
        }
    }
}
