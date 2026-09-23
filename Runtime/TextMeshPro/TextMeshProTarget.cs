using TMPro;
using UnityEngine;

namespace JTLStudio.SDK.Localization
{
    public class TextMeshProTarget : ILocalizedTarget
    {
        private readonly TMP_Text _text;
        private readonly float _baseSize;

        public TextMeshProTarget(TMP_Text text)
        {
            _text = text;
            _baseSize = text.fontSize;
        }

        public bool IsAlive => _text != null;

        public void SetText(string text)
        {
            if (_text != null)
            {
                _text.text = text;
            }
        }

        public void SetFont(LanguageFont font)
        {
            if (_text == null || font == null)
            {
                return;
            }

            if (font.FontAsset is TMP_FontAsset asset)
            {
                _text.font = asset;
            }

            _text.fontSize = _baseSize + font.SizeDelta;
        }
    }

    public class TextMeshProTargetFactory : ILocalizedTargetFactory
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            Localization.Register(new TextMeshProTargetFactory());
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void RegisterInEditor()
        {
            Localization.Register(new TextMeshProTargetFactory());
        }
#endif

        public ILocalizedTarget Bind(GameObject host)
        {
            TMP_Text text = host.GetComponent<TMP_Text>();
            return text == null ? null : new TextMeshProTarget(text);
        }
    }
}
