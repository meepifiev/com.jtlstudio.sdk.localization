using System;
using System.Collections.Generic;
using UnityEngine;

namespace JTLStudio.SDK.Localization
{
    public static class Localization
    {
        private const string Marker = "#";

        private static readonly List<ILocalizedTargetFactory> Factories = new List<ILocalizedTargetFactory>();
        private static readonly List<LocalizationTable> RuntimeTables = new List<LocalizationTable>();
        private static LocalizationSettings _settings;
        private static bool _settingsLoaded;
        private static Language _language = SDK.Language.English;
        private static bool _languageKnown;
        private static Language _fallback = SDK.Language.English;
        private static bool _fallbackLoaded;

        public static event Action Changed;

        public static Language Language => _languageKnown ? _language : Fallback;

        public static LocalizationSettings Settings
        {
            get
            {
                if (_settingsLoaded == false)
                {
                    _settings = Resources.Load<LocalizationSettings>(LocalizationSettings.ResourcePath);
                    _settingsLoaded = true;
                }

                return _settings;
            }
        }

        public static LocalizationFonts Fonts => Settings == null ? null : Settings.Fonts;

        public static Language Fallback
        {
            get
            {
                if (_fallbackLoaded == false)
                {
                    JTLSDKSettings settings = Resources.Load<JTLSDKSettings>(JTLSDKSettings.ResourcePath);
                    _fallback = settings == null ? SDK.Language.English : settings.DefaultLanguage;
                    _fallbackLoaded = true;
                }

                return _fallback;
            }
        }

        public static string Translate(string key)
        {
            return Translate(key, Language);
        }

        public static string Translate(string key, Language language)
        {
            return Translate(null, key, language);
        }

        public static string Translate(LocalizationTable preferred, string key, Language language)
        {
            if (TryTranslate(preferred, key, language, out string text))
            {
                return text;
            }

            MissingTranslation missing = Settings == null ? MissingTranslation.ShowKey : Settings.MissingTranslation;

            switch (missing)
            {
                case MissingTranslation.ShowEmpty:
                    return "";

                case MissingTranslation.ShowMarker:
                    return Marker + key + Marker;

                default:
                    return key;
            }
        }

        public static bool TryTranslate(string key, out string text)
        {
            return TryTranslate(key, Language, out text);
        }

        public static bool TryTranslate(string key, Language language, out string text)
        {
            return TryTranslate(null, key, language, out text);
        }

        public static bool TryTranslate(LocalizationTable preferred, string key, Language language, out string text)
        {
            text = "";

            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            if (Search(preferred, key, language, out text))
            {
                return true;
            }

            Language fallback = Fallback;
            return fallback != language && Search(preferred, key, fallback, out text);
        }

        private static bool Search(LocalizationTable preferred, string key, Language language, out string text)
        {
            if (preferred != null && preferred.TryGet(key, language, out text))
            {
                return true;
            }

            foreach (LocalizationTable table in Tables())
            {
                if (table != null && table != preferred && table.TryGet(key, language, out text))
                {
                    return true;
                }
            }

            text = "";
            return false;
        }

        public static IEnumerable<LocalizationTable> Tables()
        {
            if (Settings != null)
            {
                foreach (LocalizationTable table in Settings.Tables)
                {
                    yield return table;
                }
            }

            foreach (LocalizationTable table in RuntimeTables)
            {
                yield return table;
            }
        }

        public static void AddTable(LocalizationTable table)
        {
            if (table != null && RuntimeTables.Contains(table) == false)
            {
                RuntimeTables.Add(table);
                Refresh();
            }
        }

        public static void RemoveTable(LocalizationTable table)
        {
            if (RuntimeTables.Remove(table))
            {
                Refresh();
            }
        }

        public static void Register(ILocalizedTargetFactory factory)
        {
            if (factory == null)
            {
                return;
            }

            foreach (ILocalizedTargetFactory registered in Factories)
            {
                if (registered.GetType() == factory.GetType())
                {
                    return;
                }
            }

            Factories.Add(factory);
        }

        public static ILocalizedTarget Bind(GameObject host)
        {
            if (host == null)
            {
                return null;
            }

            foreach (ILocalizedTargetFactory factory in Factories)
            {
                ILocalizedTarget target = factory.Bind(host);

                if (target != null)
                {
                    return target;
                }
            }

            return null;
        }

        public static void Refresh()
        {
            Changed?.Invoke();
        }

        internal static void SetLanguage(Language language)
        {
            if (_languageKnown && _language == language)
            {
                return;
            }

            _language = language;
            _languageKnown = true;
            Refresh();
        }

        public static void ReloadSettings()
        {
            _settingsLoaded = false;
            _settings = null;
            _fallbackLoaded = false;
        }
    }
}
