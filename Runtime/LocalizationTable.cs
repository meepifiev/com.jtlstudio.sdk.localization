using System;
using System.Collections.Generic;
using UnityEngine;

namespace JTLStudio.SDK.Localization
{
    [CreateAssetMenu(fileName = "LocalizationTable", menuName = "JTL SDK/Localization Table")]
    public class LocalizationTable : ScriptableObject
    {
        [SerializeField] private List<LocalizationEntry> _entries = new List<LocalizationEntry>();

        private readonly Dictionary<string, LocalizationEntry> _index = new Dictionary<string, LocalizationEntry>();
        private bool _indexed;

        public IReadOnlyList<LocalizationEntry> Entries => _entries;

        public bool TryGet(string key, Language language, out string text)
        {
            text = "";

            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            BuildIndex();

            if (_index.TryGetValue(key, out LocalizationEntry entry) == false)
            {
                return false;
            }

            return entry.TryGet(language, out text);
        }

        public bool Contains(string key)
        {
            BuildIndex();
            return string.IsNullOrEmpty(key) == false && _index.ContainsKey(key);
        }

        public LocalizationEntry Find(string key)
        {
            BuildIndex();
            return string.IsNullOrEmpty(key) == false && _index.TryGetValue(key, out LocalizationEntry entry) ? entry : null;
        }

        public List<string> Keys()
        {
            List<string> keys = new List<string>(_entries.Count);

            foreach (LocalizationEntry entry in _entries)
            {
                keys.Add(entry.Key);
            }

            return keys;
        }

        public LocalizationEntry Add(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(nameof(key));
            }

            LocalizationEntry existing = Find(key);

            if (existing != null)
            {
                return existing;
            }

            LocalizationEntry entry = new LocalizationEntry(key.Trim());
            _entries.Add(entry);
            _indexed = false;
            return entry;
        }

        public bool Remove(string key)
        {
            LocalizationEntry entry = Find(key);

            if (entry == null)
            {
                return false;
            }

            _entries.Remove(entry);
            _indexed = false;
            return true;
        }

        public void Rename(string key, string renamed)
        {
            LocalizationEntry entry = Find(key);

            if (entry == null || string.IsNullOrWhiteSpace(renamed) || Contains(renamed.Trim()))
            {
                return;
            }

            entry.Key = renamed.Trim();
            _indexed = false;
        }

        public void Invalidate()
        {
            _indexed = false;
        }

        private void BuildIndex()
        {
            if (_indexed)
            {
                return;
            }

            _index.Clear();

            foreach (LocalizationEntry entry in _entries)
            {
                if (string.IsNullOrEmpty(entry.Key) == false)
                {
                    _index[entry.Key] = entry;
                }
            }

            _indexed = true;
        }

        private void OnValidate()
        {
            _indexed = false;
        }
    }
}
