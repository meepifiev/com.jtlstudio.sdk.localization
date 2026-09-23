using System;
using UnityEngine;

namespace JTLStudio.SDK.Localization
{
    [Serializable]
    public struct LocalizationValue
    {
        [SerializeField] private Language _language;
        [SerializeField] private string _text;

        public LocalizationValue(Language language, string text)
        {
            _language = language;
            _text = text;
        }

        public Language Language => _language;
        public string Text => _text;
    }
}
