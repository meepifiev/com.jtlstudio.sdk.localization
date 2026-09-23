using System.Collections.Generic;
using UnityEngine;

namespace JTLStudio.SDK.Localization
{
    public class LocalizationSettings : ScriptableObject
    {
        public const string ResourcePath = "JTLSDK/JTLSDKLocalization";

        [SerializeField] private List<LocalizationTable> _tables = new List<LocalizationTable>();
        [SerializeField] private LocalizationFonts _fonts;
        [SerializeField] private MissingTranslation _missingTranslation = MissingTranslation.ShowKey;

        public IReadOnlyList<LocalizationTable> Tables => _tables;

        public LocalizationFonts Fonts
        {
            get => _fonts;
            set => _fonts = value;
        }

        public MissingTranslation MissingTranslation
        {
            get => _missingTranslation;
            set => _missingTranslation = value;
        }

        public void AddTable(LocalizationTable table)
        {
            if (table != null && _tables.Contains(table) == false)
            {
                _tables.Add(table);
            }
        }

        public void RemoveTable(LocalizationTable table)
        {
            _tables.Remove(table);
        }
    }
}
