using System;
using System.Collections.Generic;
using UnityEngine;

namespace JTLStudio.SDK.Localization
{
    [Serializable]
    public class LocalizationEntry
    {
        [SerializeField] private string _key = "";
        [SerializeField] private List<LocalizationValue> _values = new List<LocalizationValue>();

        public LocalizationEntry(string key)
        {
            _key = key;
        }

        public string Key
        {
            get => _key;
            set => _key = value;
        }

        public IReadOnlyList<LocalizationValue> Values => _values;

        public bool TryGet(Language language, out string text)
        {
            foreach (LocalizationValue value in _values)
            {
                if (value.Language == language && string.IsNullOrEmpty(value.Text) == false)
                {
                    text = value.Text;
                    return true;
                }
            }

            text = "";
            return false;
        }

        public string Get(Language language)
        {
            return TryGet(language, out string text) ? text : "";
        }

        public void Set(Language language, string text)
        {
            for (int index = 0; index < _values.Count; index++)
            {
                if (_values[index].Language == language)
                {
                    if (string.IsNullOrEmpty(text))
                    {
                        _values.RemoveAt(index);
                        return;
                    }

                    _values[index] = new LocalizationValue(language, text);
                    return;
                }
            }

            if (string.IsNullOrEmpty(text) == false)
            {
                _values.Add(new LocalizationValue(language, text));
            }
        }
    }
}
