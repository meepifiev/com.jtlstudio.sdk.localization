using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace JTLStudio.SDK.Localization.Editor
{
    public class LocalizationWindow : EditorWindow
    {
        private const string TableTab = "Table";
        private const string SceneTab = "Scene";
        private const string SettingsTab = "Settings";

        private readonly string[] _tabs = { TableTab, SceneTab, SettingsTab };

        private int _tab;
        private LocalizationTable _table;
        private Vector2 _scroll;
        private Vector2 _sceneScroll;
        private string _search = "";
        private string _newKey = "";
        private string _focusKey = "";
        private bool _scrollToFocus;
        private List<SceneTextScanner.Found> _found;

        [MenuItem("JTL SDK/Localization", false, 10)]
        public static LocalizationWindow Open()
        {
            LocalizationWindow window = GetWindow<LocalizationWindow>();
            window.titleContent = new GUIContent("Localization");
            window.minSize = new Vector2(520f, 320f);
            window.Show();
            return window;
        }

        public static void Open(LocalizationTable table, string key)
        {
            LocalizationWindow window = Open();
            window._tab = 0;

            if (table != null)
            {
                window._table = table;
            }

            window._search = key ?? "";
            window._focusKey = key ?? "";
            window._scrollToFocus = true;
            window.Repaint();
        }

        private void OnGUI()
        {
            _tab = GUILayout.Toolbar(_tab, _tabs);
            EditorGUILayout.Space();

            switch (_tabs[_tab])
            {
                case TableTab:
                    DrawTable();
                    break;

                case SceneTab:
                    DrawScene();
                    break;

                default:
                    DrawSettings();
                    break;
            }
        }

        private void DrawTable()
        {
            DrawTablePicker();

            if (_table == null)
            {
                return;
            }

            EditorGUILayout.BeginHorizontal();
            string search = EditorGUILayout.TextField("Search by key and translation", _search);

            if (search != _search)
            {
                _search = search;
                _focusKey = "";
            }

            if (GUILayout.Button("Export CSV", GUILayout.Width(100f)))
            {
                ExportCsv();
            }

            if (GUILayout.Button("Import CSV", GUILayout.Width(100f)))
            {
                ImportCsv();
            }

            if (GUILayout.Button("Translate empty", GUILayout.Width(130f)))
            {
                TranslateEmpty();
            }

            EditorGUILayout.EndHorizontal();

            List<Language> languages = LocalizationProject.Languages();
            Language fallback = LocalizationProject.DefaultLanguage();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            int shown = 0;

            foreach (LocalizationEntry entry in new List<LocalizationEntry>(_table.Entries))
            {
                if (Matches(entry, _search, languages) == false)
                {
                    continue;
                }

                shown++;
                bool focused = string.IsNullOrEmpty(_focusKey) == false && entry.Key == _focusKey;
                Color background = GUI.backgroundColor;

                if (focused)
                {
                    GUI.backgroundColor = new Color(0.45f, 0.75f, 1f);
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUI.backgroundColor = background;

                if (focused && _scrollToFocus && Event.current.type == EventType.Repaint)
                {
                    _scroll.y = Mathf.Max(0f, GUILayoutUtility.GetLastRect().y - 8f);
                    _scrollToFocus = false;
                }
                EditorGUILayout.BeginHorizontal();
                string renamed = EditorGUILayout.DelayedTextField(entry.Key, EditorStyles.boldLabel);

                if (renamed != entry.Key)
                {
                    Undo.RecordObject(_table, "Rename key");
                    _table.Rename(entry.Key, renamed);
                    EditorUtility.SetDirty(_table);
                }

                if (GUILayout.Button("Delete", GUILayout.Width(80f)))
                {
                    Undo.RecordObject(_table, "Remove key");
                    _table.Remove(entry.Key);
                    EditorUtility.SetDirty(_table);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    continue;
                }

                EditorGUILayout.EndHorizontal();

                foreach (Language language in languages)
                {
                    string current = entry.Get(language);
                    Color color = GUI.color;

                    if (string.IsNullOrEmpty(current))
                    {
                        GUI.color = new Color(1f, 0.8f, 0.4f);
                    }

                    string edited = EditorGUILayout.TextField(language + (language == fallback ? " (default)" : ""), current);
                    GUI.color = color;

                    if (edited != current)
                    {
                        Undo.RecordObject(_table, "Edit translation");
                        entry.Set(language, edited);
                        _table.Invalidate();
                        EditorUtility.SetDirty(_table);
                        Localization.Refresh();
                    }
                }

                EditorGUILayout.EndVertical();
            }

            if (shown == 0)
            {
                EditorGUILayout.HelpBox(string.IsNullOrEmpty(_search)
                    ? "The table has no keys."
                    : "Nothing matches " + _search + " in keys or translations.", MessageType.None);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.BeginHorizontal();
            _newKey = EditorGUILayout.TextField("New key", _newKey);

            if (GUILayout.Button("Add", GUILayout.Width(100f)) && string.IsNullOrWhiteSpace(_newKey) == false)
            {
                Undo.RecordObject(_table, "Add key");
                _table.Add(_newKey);
                EditorUtility.SetDirty(_table);
                _newKey = "";
            }

            EditorGUILayout.EndHorizontal();
        }

        public static bool Matches(LocalizationEntry entry, string search, IReadOnlyList<Language> languages)
        {
            if (string.IsNullOrEmpty(search))
            {
                return true;
            }

            if (entry.Key.IndexOf(search, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            foreach (Language language in languages)
            {
                string value = entry.Get(language);

                if (string.IsNullOrEmpty(value) == false && value.IndexOf(search, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private void DrawTablePicker()
        {
            EditorGUILayout.BeginHorizontal();
            _table = (LocalizationTable)EditorGUILayout.ObjectField("Table", _table, typeof(LocalizationTable), false);

            if (GUILayout.Button("New", GUILayout.Width(80f)))
            {
                _table = LocalizationProject.CreateTable("LocalizationTable");
            }

            EditorGUILayout.EndHorizontal();

            if (_table == null)
            {
                List<LocalizationTable> tables = LocalizationProject.Tables();

                if (tables.Count > 0)
                {
                    _table = tables[0];
                }
                else
                {
                    EditorGUILayout.HelpBox("The project has no tables. Press New.", MessageType.Info);
                    return;
                }
            }

            EditorGUILayout.LabelField(" ", AssetDatabase.GetAssetPath(_table) + "   keys: " + _table.Entries.Count, EditorStyles.miniLabel);
        }

        private void DrawScene()
        {
            DrawTablePicker();
            EditorGUILayout.HelpBox("The pass collects every TextMeshPro of the open scenes and suggests keys. Nothing is written until the button below: it adds the keys to the selected table, puts the current text into the default language and attaches Localized Text.", MessageType.None);

            if (GUILayout.Button("Collect scene texts"))
            {
                _found = SceneTextScanner.Scan();

                foreach (SceneTextScanner.Found item in _found)
                {
                    item.Key = item.Component != null && string.IsNullOrEmpty(item.Component.Key) == false
                        ? item.Component.Key
                        : SceneTextScanner.SuggestKey(item.Host, item.Text);
                }
            }

            if (_found == null)
            {
                return;
            }

            EditorGUILayout.LabelField("Texts found: " + _found.Count);
            _sceneScroll = EditorGUILayout.BeginScrollView(_sceneScroll);

            foreach (SceneTextScanner.Found item in _found)
            {
                EditorGUILayout.BeginHorizontal();
                item.Selected = EditorGUILayout.Toggle(item.Selected, GUILayout.Width(18f));
                EditorGUILayout.ObjectField(item.Host, typeof(GameObject), true, GUILayout.Width(160f));
                item.Key = EditorGUILayout.TextField(item.Key, GUILayout.Width(180f));
                EditorGUILayout.LabelField(item.Text, EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            if (_table == null)
            {
                EditorGUILayout.HelpBox("Pick a table above: the keys go there.", MessageType.Warning);
                return;
            }

            if (GUILayout.Button("Add the keys to " + _table.name + " and attach components"))
            {
                SceneTextScanner.Apply(_found, _table, LocalizationProject.DefaultLanguage());
                Debug.Log("[JTL SDK] Scene keys were written to " + AssetDatabase.GetAssetPath(_table) + ".");
                _found = SceneTextScanner.Scan();
            }
        }

        private void DrawSettings()
        {
            LocalizationSettings settings = LocalizationProject.Settings;

            if (settings == null)
            {
                EditorGUILayout.HelpBox("The settings asset is not created yet.", MessageType.Info);

                if (GUILayout.Button("Create settings"))
                {
                    LocalizationProject.RequireSettings();
                }

                return;
            }

            SerializedObject serialized = new SerializedObject(settings);
            EditorGUILayout.PropertyField(serialized.FindProperty("_tables"), new GUIContent("Tables"), true);
            EditorGUILayout.PropertyField(serialized.FindProperty("_fonts"), new GUIContent("Fonts per language"));
            EditorGUILayout.PropertyField(serialized.FindProperty("_missingTranslation"), new GUIContent("When a translation is missing"));
            serialized.ApplyModifiedProperties();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Auto translate", EditorStyles.boldLabel);
            string key = EditorGUILayout.PasswordField("Google Translate key", TranslateJob.ApiKey);

            if (key != TranslateJob.ApiKey)
            {
                TranslateJob.ApiKey = key;
            }

            EditorGUILayout.HelpBox(string.IsNullOrWhiteSpace(key)
                ? "Without a key the requests go to the free Google endpoint, which guarantees nothing and may refuse. A Cloud Translation API key is safer."
                : "Requests go to Cloud Translation API with this key. The key lives in EditorPrefs of this computer and never reaches the project.", MessageType.None);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Project languages: " + string.Join(", ", LocalizationProject.Languages()));
            EditorGUILayout.LabelField("Default language: " + LocalizationProject.DefaultLanguage());
            EditorGUILayout.HelpBox("Languages come from JTL SDK, the Languages section.", MessageType.None);
        }

        private void TranslateEmpty()
        {
            Language source = LocalizationProject.DefaultLanguage();

            if (EditorUtility.DisplayDialog("JTL SDK", "Translate empty rows from " + source + " with Google Translate? Existing translations stay untouched.", "Translate", "Cancel") == false)
            {
                return;
            }

            Undo.RecordObject(_table, "Translate empty");
            TranslateJob job = new TranslateJob();
            int filled = job.Run(_table, LocalizationProject.Languages(), source);
            EditorUtility.SetDirty(_table);
            AssetDatabase.SaveAssets();
            Localization.Refresh();

            string message = "Translated rows: " + filled + ".";

            if (string.IsNullOrEmpty(job.Error) == false)
            {
                message += "\n\nThe service answered: " + job.Error;
            }

            EditorUtility.DisplayDialog("JTL SDK", message, "Done");
        }

        private void ExportCsv()
        {
            string path = EditorUtility.SaveFilePanel("Export CSV", "", _table.name + ".csv", "csv");

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            File.WriteAllText(path, LocalizationCsv.Export(_table, LocalizationProject.Languages()), new System.Text.UTF8Encoding(true));
            EditorUtility.RevealInFinder(path);
        }

        private void ImportCsv()
        {
            string path = EditorUtility.OpenFilePanel("Import CSV", "", "csv");

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            Undo.RecordObject(_table, "Import CSV");
            int changed = LocalizationCsv.Import(_table, File.ReadAllText(path));
            EditorUtility.SetDirty(_table);
            AssetDatabase.SaveAssets();
            Localization.Refresh();
            EditorUtility.DisplayDialog("JTL SDK", "Translations updated: " + changed + ".", "Done");
        }
    }
}
