using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace JTLStudio.SDK.Localization.Editor
{
    public static class LocalizationProject
    {
        public const string SettingsAssetPath = "Assets/Resources/JTLSDK/JTLSDKLocalization.asset";
        public const string TablesFolder = "Assets/Settings/JTLSDK/Localization";

        public static LocalizationSettings Settings => AssetDatabase.LoadAssetAtPath<LocalizationSettings>(SettingsAssetPath);

        public static LocalizationSettings RequireSettings()
        {
            LocalizationSettings settings = Settings;

            if (settings != null)
            {
                return settings;
            }

            EnsureFolder(Path.GetDirectoryName(SettingsAssetPath));
            settings = ScriptableObject.CreateInstance<LocalizationSettings>();
            AssetDatabase.CreateAsset(settings, SettingsAssetPath);
            AssetDatabase.SaveAssets();
            Localization.ReloadSettings();
            return settings;
        }

        public static LocalizationTable CreateTable(string name)
        {
            EnsureFolder(TablesFolder);
            LocalizationTable table = ScriptableObject.CreateInstance<LocalizationTable>();
            string path = AssetDatabase.GenerateUniqueAssetPath(TablesFolder + "/" + name + ".asset");
            AssetDatabase.CreateAsset(table, path);
            LocalizationSettings settings = RequireSettings();
            Undo.RecordObject(settings, "Add localization table");
            settings.AddTable(table);
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            return table;
        }

        public static List<LocalizationTable> Tables()
        {
            List<LocalizationTable> tables = new List<LocalizationTable>();
            LocalizationSettings settings = Settings;

            if (settings != null)
            {
                foreach (LocalizationTable table in settings.Tables)
                {
                    if (table != null)
                    {
                        tables.Add(table);
                    }
                }
            }

            foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(LocalizationTable)))
            {
                LocalizationTable table = AssetDatabase.LoadAssetAtPath<LocalizationTable>(AssetDatabase.GUIDToAssetPath(guid));

                if (table != null && tables.Contains(table) == false)
                {
                    tables.Add(table);
                }
            }

            return tables;
        }

        public static List<Language> Languages()
        {
            List<Language> languages = new List<Language>();
            JTLSDKSettings settings = SdkSettings();

            if (settings != null)
            {
                foreach (Language language in settings.SupportedLanguages)
                {
                    if (languages.Contains(language) == false)
                    {
                        languages.Add(language);
                    }
                }

                if (languages.Contains(settings.DefaultLanguage) == false)
                {
                    languages.Insert(0, settings.DefaultLanguage);
                }
            }

            if (languages.Count == 0)
            {
                languages.Add(Language.English);
            }

            return languages;
        }

        public static Language DefaultLanguage()
        {
            JTLSDKSettings settings = SdkSettings();
            return settings == null ? Language.English : settings.DefaultLanguage;
        }

        public static JTLSDKSettings SdkSettings()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(JTLSDKSettings)))
            {
                JTLSDKSettings settings = AssetDatabase.LoadAssetAtPath<JTLSDKSettings>(AssetDatabase.GUIDToAssetPath(guid));

                if (settings != null)
                {
                    return settings;
                }
            }

            return null;
        }

        private static void EnsureFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder) || AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folder);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }
    }
}
