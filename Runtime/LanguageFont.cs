using System;
using UnityEngine;

namespace JTLStudio.SDK.Localization
{
    [Serializable]
    public class LanguageFont
    {
        [SerializeField] private Language _language;
        [SerializeField] private Font _font;
        [SerializeField] private UnityEngine.Object _fontAsset;
        [SerializeField] private int _sizeDelta;

        public Language Language
        {
            get => _language;
            set => _language = value;
        }

        public Font Font
        {
            get => _font;
            set => _font = value;
        }

        public UnityEngine.Object FontAsset
        {
            get => _fontAsset;
            set => _fontAsset = value;
        }

        public int SizeDelta
        {
            get => _sizeDelta;
            set => _sizeDelta = value;
        }
    }
}
