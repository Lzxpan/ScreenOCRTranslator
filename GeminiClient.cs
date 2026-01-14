using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class GeminiClient
{
    private string _apiKey;
    private string _modelName;

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
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/{_modelName}:generateContent?key={_apiKey}";

        // 將圖片轉為 base64
        string base64Image;
        using (MemoryStream ms = new MemoryStream())
        {
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            base64Image = Convert.ToBase64String(ms.ToArray());
        }

        // 建立請求內容
        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new { text = "請從圖片中辨識出所有可見文字，並直接翻譯成中文：（只輸出翻譯結果）" },
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
            }
        };

        using (var client = new HttpClient())
        {
            var jsonContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, jsonContent);

            if (!response.IsSuccessStatusCode)
                return $"錯誤：{response.StatusCode}";

            var responseBody = await response.Content.ReadAsStringAsync();

            // 嘗試解析回應
            dynamic json = JsonConvert.DeserializeObject(responseBody);
            return json?.candidates?[0]?.content?.parts?[0]?.text ?? "無法解析回應";
        }
    }

    public async Task<string> TranslateText(string inputText)
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
        }
        };

        using (var client = new HttpClient())
        {
            var jsonContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, jsonContent);

            if (!response.IsSuccessStatusCode)
                return $"錯誤：{response.StatusCode}";

            var responseBody = await response.Content.ReadAsStringAsync();
            dynamic json = JsonConvert.DeserializeObject(responseBody);
            return json?.candidates?[0]?.content?.parts?[0]?.text ?? "無法解析回應";
        }
    }
}
