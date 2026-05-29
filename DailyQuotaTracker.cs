using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static GeminiClient;

namespace ScreenOCRTranslator
{
    public sealed class DailyQuotaEntry
    {
        public string Provider { get; set; }
        public string Model { get; set; }
        public int UsedSuccessRequests { get; set; }
        public int UsedFailedRequests { get; set; }
        public int UsedPromptTokens { get; set; }
        public int UsedOutputTokens { get; set; }
        public int UsedTotalTokens { get; set; }
        public int DailyLimit { get; set; }
        public int? RpmLimit { get; set; }
        public string LastError { get; set; }
    }

    internal sealed class DailyQuotaStore
    {
        public string Date { get; set; }
        public List<DailyQuotaEntry> Entries { get; set; } = new List<DailyQuotaEntry>();
    }

    public sealed class DailyQuotaTracker
    {
        private readonly string _filePath;
        private readonly object _lock = new object();
        private DailyQuotaStore _store;

        private static readonly Dictionary<string, (int daily, int? rpm)> _defaults =
            new Dictionary<string, (int daily, int? rpm)>(StringComparer.OrdinalIgnoreCase)
            {
                ["gemini|gemini-2.5-flash"] = (20, 5),
                ["gemini|gemini-2.5-flash-lite"] = (20, 10),
                ["gemini|gemini-3-flash-preview"] = (20, 5),
                ["gemini|gemini-3.1-flash-lite"] = (500, 15),

                ["groq|meta-llama/llama-4-scout-17b-16e-instruct"] = (1000, 30),

                ["mistral|pixtral-12b-2409"] = (500, 30),
                ["mistral|mistralai/pixtral-12b-2409"] = (500, 30),
                ["mistral|mistral-large-2512"] = (200, 10)
            };

        private DailyQuotaTracker(string filePath, DailyQuotaStore store)
        {
            _filePath = filePath;
            _store = store;
        }

        public static DailyQuotaTracker LoadOrCreate(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    var s = JsonConvert.DeserializeObject<DailyQuotaStore>(json) ?? new DailyQuotaStore();
                    return new DailyQuotaTracker(filePath, s);
                }
            }
            catch
            {
            }

            return new DailyQuotaTracker(filePath, new DailyQuotaStore());
        }

        public void EnsureToday()
        {
            lock (_lock)
            {
                string today = DateTime.Now.ToString("yyyy-MM-dd");
                if (!string.Equals(_store.Date, today, StringComparison.Ordinal))
                {
                    _store = new DailyQuotaStore { Date = today };
                }
            }
        }

        private static string Key(string provider, string model)
        {
            return (provider ?? "").Trim().ToLowerInvariant() + "|" + (model ?? "").Trim().ToLowerInvariant();
        }

        public void EnsureEntry(string provider, string model)
        {
            lock (_lock)
            {
                EnsureToday();
                string key = Key(provider, model);
                if (_store.Entries.Any(e => Key(e.Provider, e.Model) == key))
                    return;

                var d = GetDefault(provider, model);
                _store.Entries.Add(new DailyQuotaEntry
                {
                    Provider = provider,
                    Model = model,
                    DailyLimit = d.daily,
                    RpmLimit = d.rpm,
                    LastError = ""
                });
            }
        }

        public void RecordResult(string provider, string model, GeminiUsage usage, bool success, string error)
        {
            lock (_lock)
            {
                EnsureToday();
                EnsureEntry(provider, model);
                var e = _store.Entries.First(x => Key(x.Provider, x.Model) == Key(provider, model));

                if (success)
                {
                    e.UsedSuccessRequests += 1;
                    e.UsedPromptTokens += usage?.PromptTokenCount ?? 0;
                    e.UsedOutputTokens += usage?.CandidatesTokenCount ?? 0;
                    e.UsedTotalTokens += usage?.TotalTokenCount ?? 0;
                    e.LastError = "";
                }
                else
                {
                    e.UsedFailedRequests += 1;
                    e.LastError = string.IsNullOrWhiteSpace(error) ? "失敗" : error;
                }
            }
        }

        public List<DailyQuotaEntry> GetSnapshot(IEnumerable<LlmCredential> configuredCredentials)
        {
            lock (_lock)
            {
                EnsureToday();
                // 補齊預設列（至少把目前設定模型顯示出來）
                if (configuredCredentials != null)
                {
                    foreach (var c in configuredCredentials)
                    {
                        string p = MapProvider(c.Provider, c.Model);
                        EnsureEntry(p, c.Model);
                    }
                }

                // 補齊圖片中基準列（此專案重點模型）
                EnsureEntry("gemini", "gemini-2.5-flash");
                EnsureEntry("gemini", "gemini-2.5-flash-lite");
                EnsureEntry("gemini", "gemini-3-flash-preview");
                EnsureEntry("gemini", "gemini-3.1-flash-lite");
                EnsureEntry("groq", "meta-llama/llama-4-scout-17b-16e-instruct");
                EnsureEntry("mistral", "pixtral-12b-2409");
                EnsureEntry("mistral", "mistral-large-2512");

                return _store.Entries
                    .OrderBy(x => x.Provider)
                    .ThenBy(x => x.Model)
                    .Select(x => new DailyQuotaEntry
                    {
                        Provider = x.Provider,
                        Model = x.Model,
                        UsedSuccessRequests = x.UsedSuccessRequests,
                        UsedFailedRequests = x.UsedFailedRequests,
                        UsedPromptTokens = x.UsedPromptTokens,
                        UsedOutputTokens = x.UsedOutputTokens,
                        UsedTotalTokens = x.UsedTotalTokens,
                        DailyLimit = x.DailyLimit,
                        RpmLimit = x.RpmLimit,
                        LastError = x.LastError
                    }).ToList();
            }
        }

        public void Save()
        {
            lock (_lock)
            {
                try
                {
                    var json = JsonConvert.SerializeObject(_store, Formatting.Indented);
                    File.WriteAllText(_filePath, json);
                }
                catch
                {
                }
            }
        }

        private static (int daily, int? rpm) GetDefault(string provider, string model)
        {
            if (_defaults.TryGetValue(Key(provider, model), out var v))
                return v;

            return (500, null);
        }

        public static string MapProvider(LlmProvider provider, string model)
        {
            switch (provider)
            {
                case LlmProvider.Gemini:
                    return "gemini";
                case LlmProvider.GroqLlama4:
                    return "groq";
                case LlmProvider.MistralPixtral:
                    return "mistral";
                default:
                    return "other";
            }
        }
    }
}
