using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Networking;

namespace JTLStudio.SDK.Localization.Editor
{
    public class TranslateJob
    {
        public const string KeyPreference = "JTLSDK.Localization.TranslateKey";

        private readonly TranslateService _service = new TranslateService();

        public static string ApiKey
        {
            get => EditorPrefs.GetString(KeyPreference, "");
            set => EditorPrefs.SetString(KeyPreference, value ?? "");
        }

        public string Error { get; private set; } = "";

        public int Run(LocalizationTable table, IReadOnlyList<Language> languages, Language source)
        {
            Error = "";

            if (table == null)
            {
                return 0;
            }

            string key = ApiKey;
            bool official = string.IsNullOrWhiteSpace(key) == false;
            int filled = 0;

            try
            {
                foreach (Language language in languages)
                {
                    if (language == source)
                    {
                        continue;
                    }

                    filled += Translate(table, language, source, key, official);

                    if (string.IsNullOrEmpty(Error) == false)
                    {
                        break;
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            table.Invalidate();
            return filled;
        }

        private int Translate(LocalizationTable table, Language language, Language source, string key, bool official)
        {
            List<LocalizationEntry> pending = new List<LocalizationEntry>();
            List<string> texts = new List<string>();

            foreach (LocalizationEntry entry in table.Entries)
            {
                string original = entry.Get(source);

                if (string.IsNullOrEmpty(original) || string.IsNullOrEmpty(entry.Get(language)) == false)
                {
                    continue;
                }

                pending.Add(entry);
                texts.Add(original);
            }

            int filled = 0;

            for (int start = 0; start < pending.Count; start += TranslateService.BatchSize)
            {
                int count = System.Math.Min(TranslateService.BatchSize, pending.Count - start);
                List<string> batch = texts.GetRange(start, count);
                float progress = pending.Count == 0 ? 1f : (start + count) / (float)pending.Count;

                if (EditorUtility.DisplayCancelableProgressBar("JTL SDK", language + ": " + (start + count) + " из " + pending.Count, progress))
                {
                    Error = "Отменено.";
                    return filled;
                }

                List<string> translated = Send(batch, source, language, key, official);

                if (string.IsNullOrEmpty(Error) == false)
                {
                    return filled;
                }

                for (int index = 0; index < translated.Count && index < count; index++)
                {
                    if (string.IsNullOrEmpty(translated[index]) == false)
                    {
                        pending[start + index].Set(language, translated[index]);
                        filled++;
                    }
                }
            }

            return filled;
        }

        private List<string> Send(List<string> texts, Language source, Language language, string key, bool official)
        {
            using (UnityWebRequest request = _service.CreateRequest(texts, source, language, key))
            {
                UnityWebRequestAsyncOperation operation = request.SendWebRequest();

                while (operation.isDone == false)
                {
                    System.Threading.Thread.Sleep(20);
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Error = _service.Error(request);
                    return new List<string>();
                }

                return _service.Read(request.downloadHandler.text, official, texts.Count);
            }
        }
    }
}
