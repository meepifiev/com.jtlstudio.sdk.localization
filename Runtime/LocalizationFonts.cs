using System;
using System.Collections.Generic;
using UnityEngine;

namespace JTLStudio.SDK.Localization
{
    [CreateAssetMenu(fileName = "LocalizationFonts", menuName = "JTL SDK/Localization Fonts")]
    public class LocalizationFonts : ScriptableObject
    {
        [SerializeField] private List<FontSlot> _slots = new List<FontSlot>();

        public IReadOnlyList<FontSlot> Slots => _slots;

        public List<string> SlotNames()
        {
            List<string> names = new List<string>(_slots.Count);

            foreach (FontSlot slot in _slots)
            {
                names.Add(slot.Name);
            }

            return names;
        }

        public FontSlot Find(string name)
        {
            foreach (FontSlot slot in _slots)
            {
                if (slot.Name == name)
                {
                    return slot;
                }
            }

            return _slots.Count > 0 && string.IsNullOrEmpty(name) ? _slots[0] : null;
        }

        public LanguageFont Resolve(string slotName, Language language)
        {
            FontSlot slot = Find(slotName);
            return slot == null ? null : slot.Resolve(language);
        }

        [Serializable]
        public class FontSlot
        {
            [SerializeField] private string _name = "Default";
            [SerializeField] private List<LanguageFont> _fonts = new List<LanguageFont>();

            public string Name
            {
                get => _name;
                set => _name = value;
            }

            public IReadOnlyList<LanguageFont> Fonts => _fonts;

            public LanguageFont Resolve(Language language)
            {
                foreach (LanguageFont font in _fonts)
                {
                    if (font.Language == language)
                    {
                        return font;
                    }
                }

                return null;
            }

            public LanguageFont Add(Language language)
            {
                LanguageFont existing = Resolve(language);

                if (existing != null)
                {
                    return existing;
                }

                LanguageFont font = new LanguageFont { Language = language };
                _fonts.Add(font);
                return font;
            }

            public void Remove(Language language)
            {
                _fonts.RemoveAll(font => font.Language == language);
            }
        }
    }
}
