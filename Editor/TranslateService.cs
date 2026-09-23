using System;
using System.Collections.Generic;
using System.Text;
using JTLStudio.SDK.Services.Json;
using UnityEngine.Networking;

namespace JTLStudio.SDK.Localization.Editor
{
    public class TranslateService
    {
        public const string OfficialEndpoint = "https://translation.googleapis.com/language/translate/v2";
        public const string FreeEndpoint = "https://translate.googleapis.com/translate_a/t";
        public const int BatchSize = 40;

        private readonly JsonParser _parser = new JsonParser();
        private readonly LanguageCodes _codes = new LanguageCodes();

        public string Code(Language language)
        {
            return _codes.ToCode(language);
        }

        public UnityWebRequest CreateRequest(IReadOnlyList<string> texts, Language from, Language to, string key)
        {
            StringBuilder query = new StringBuilder();
            bool official = string.IsNullOrWhiteSpace(key) == false;
            query.Append(official ? OfficialEndpoint : FreeEndpoint);
            query.Append("?sl=").Append(Code(from));
            query.Append("&tl=").Append(Code(to));

            if (official)
            {
                query.Clear();
                query.Append(OfficialEndpoint);
                query.Append("?key=").Append(UnityWebRequest.EscapeURL(key.Trim()));
                query.Append("&source=").Append(Code(from));
                query.Append("&target=").Append(Code(to));
                query.Append("&format=text");
            }
            else
            {
                query.Append("&client=gtx&dt=t&ie=UTF-8&oe=UTF-8");
            }

            foreach (string text in texts)
            {
                query.Append("&q=").Append(UnityWebRequest.EscapeURL(text));
            }

            UnityWebRequest request = UnityWebRequest.Get(query.ToString());
            request.SetRequestHeader("Accept", "application/json");
            return request;
        }

        public List<string> Read(string response, bool official, int expected)
        {
            List<string> translations = new List<string>();

            if (string.IsNullOrWhiteSpace(response))
            {
                return translations;
            }

            object parsed = _parser.Parse(response);

            if (official)
            {
                ReadOfficial(parsed, translations);
            }
            else
            {
                ReadFree(parsed, translations);
            }

            while (translations.Count > expected)
            {
                translations.RemoveAt(translations.Count - 1);
            }

            return translations;
        }

        private void ReadOfficial(object parsed, List<string> translations)
        {
            if (parsed is Dictionary<string, object> root == false
                || root.TryGetValue("data", out object data) == false
                || data is Dictionary<string, object> values == false
                || values.TryGetValue("translations", out object list) == false
                || list is List<object> items == false)
            {
                return;
            }

            foreach (object item in items)
            {
                if (item is Dictionary<string, object> translation && translation.TryGetValue("translatedText", out object text) && text is string value)
                {
                    translations.Add(value);
                }
            }
        }

        private void ReadFree(object parsed, List<string> translations)
        {
            if (parsed is List<object> items == false)
            {
                return;
            }

            foreach (object item in items)
            {
                if (item is string text)
                {
                    translations.Add(text);
                    continue;
                }

                if (item is List<object> nested && nested.Count > 0 && nested[0] is string first)
                {
                    translations.Add(first);
                }
            }
        }

        public string Error(UnityWebRequest request)
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                return "";
            }

            string body = request.downloadHandler == null ? "" : request.downloadHandler.text;

            try
            {
                if (_parser.Parse(body) is Dictionary<string, object> root
                    && root.TryGetValue("error", out object error)
                    && error is Dictionary<string, object> values
                    && values.TryGetValue("message", out object message)
                    && message is string text)
                {
                    return text;
                }
            }
            catch (FormatException)
            {
                return request.error;
            }

            return string.IsNullOrEmpty(request.error) ? "Сервис не ответил." : request.error;
        }
    }
}
