using UnityEngine;

namespace JTLStudio.SDK.Localization
{
    [AddComponentMenu("JTL SDK/Localized Text")]
    [DisallowMultipleComponent]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private LocalizationTable _table;
        [SerializeField] private string _key = "";
        [SerializeField] private bool _translateText = true;
        [SerializeField] private bool _applyFont = true;
        [SerializeField] private string _fontSlot = "";

        private ILocalizedTarget _target;
        private object[] _arguments;

        public LocalizationTable Table
        {
            get => _table;
            set
            {
                _table = value;
                Apply();
            }
        }

        public string Key
        {
            get => _key;
            set
            {
                _key = value;
                Apply();
            }
        }

        public string FontSlot
        {
            get => _fontSlot;
            set
            {
                _fontSlot = value;
                Apply();
            }
        }

        public void SetArguments(params object[] arguments)
        {
            _arguments = arguments;
            Apply();
        }

        public void Apply()
        {
            ILocalizedTarget target = Target();

            if (target == null)
            {
                return;
            }

            Language language = Localization.Language;

            if (_translateText)
            {
                target.SetText(Text(language));
            }

            if (_applyFont)
            {
                LocalizationFonts fonts = Localization.Fonts;
                LanguageFont font = fonts == null ? null : fonts.Resolve(_fontSlot, language);

                if (font != null)
                {
                    target.SetFont(font);
                }
            }
        }

        public string Text(Language language)
        {
            string text = Localization.Translate(_table, _key, language);
            return _arguments == null || _arguments.Length == 0 ? text : string.Format(text, _arguments);
        }

        private void OnEnable()
        {
            Localization.Changed += Apply;
            Apply();
        }

        private void OnDisable()
        {
            Localization.Changed -= Apply;
        }

        private ILocalizedTarget Target()
        {
            if (_target != null && _target.IsAlive)
            {
                return _target;
            }

            _target = Localization.Bind(gameObject);
            return _target;
        }
    }
}
