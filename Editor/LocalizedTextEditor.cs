using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JTLStudio.SDK.Localization.Editor
{
    [CustomEditor(typeof(LocalizedText))]
    [CanEditMultipleObjects]
    public class LocalizedTextEditor : UnityEditor.Editor
    {
        private SerializedProperty _table;
        private SerializedProperty _key;
        private SerializedProperty _translateText;
        private SerializedProperty _applyFont;
        private SerializedProperty _fontSlot;

        private void OnEnable()
        {
            _table = serializedObject.FindProperty("_table");
            _key = serializedObject.FindProperty("_key");
            _translateText = serializedObject.FindProperty("_translateText");
            _applyFont = serializedObject.FindProperty("_applyFont");
            _fontSlot = serializedObject.FindProperty("_fontSlot");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawTargetWarning();
            EditorGUILayout.PropertyField(_table);
            DrawKey();
            EditorGUILayout.PropertyField(_translateText, new GUIContent("Translate text"));
            EditorGUILayout.PropertyField(_applyFont, new GUIContent("Apply font"));

            if (_applyFont.boolValue)
            {
                DrawFontSlot();
            }

            serializedObject.ApplyModifiedProperties();

            if (_table.objectReferenceValue != null)
            {
                EditorGUILayout.LabelField(" ", AssetDatabase.GetAssetPath(_table.objectReferenceValue), EditorStyles.miniLabel);
            }

            if (targets.Length == 1)
            {
                DrawTranslations();
            }
        }

        private void DrawTargetWarning()
        {
            foreach (Object item in targets)
            {
                if (item is LocalizedText component && Localization.Bind(component.gameObject) == null)
                {
                    EditorGUILayout.HelpBox("No TextMeshPro on this object: nothing to fill. The component works with TMP_Text only.", MessageType.Warning);
                    return;
                }
            }
        }

        private void DrawKey()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_key, new GUIContent("Key"));

            if (GUILayout.Button("…", GUILayout.Width(26f)))
            {
                ShowKeyMenu();
            }

            using (new EditorGUI.DisabledScope(_table.objectReferenceValue == null))
            {
                if (GUILayout.Button(new GUIContent("Open table", "Open the localization window at this key"), GUILayout.Width(80f)))
                {
                    LocalizationWindow.Open(_table.objectReferenceValue as LocalizationTable, _key.stringValue);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void ShowKeyMenu()
        {
            LocalizationTable table = _table.objectReferenceValue as LocalizationTable;

            if (table == null)
            {
                EditorUtility.DisplayDialog("JTL SDK", "Pick a table first.", "Got it");
                return;
            }

            GenericMenu menu = new GenericMenu();

            foreach (string key in table.Keys())
            {
                string captured = key;
                menu.AddItem(new GUIContent(key), key == _key.stringValue, () =>
                {
                    _key.stringValue = captured;
                    serializedObject.ApplyModifiedProperties();
                    ApplyToScene();
                });
            }

            if (table.Keys().Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("The table has no keys"));
            }

            menu.ShowAsContext();
        }

        private void DrawFontSlot()
        {
            LocalizationFonts fonts = LocalizationProject.Settings == null ? null : LocalizationProject.Settings.Fonts;

            if (fonts == null)
            {
                EditorGUILayout.PropertyField(_fontSlot, new GUIContent("Font set"));
                EditorGUILayout.HelpBox("No font asset is selected in JTL SDK, Localization.", MessageType.None);
                return;
            }

            List<string> names = fonts.SlotNames();

            if (names.Count == 0)
            {
                EditorGUILayout.HelpBox("The font asset has no sets.", MessageType.None);
                return;
            }

            int index = Mathf.Max(0, names.IndexOf(_fontSlot.stringValue));
            int selected = EditorGUILayout.Popup("Font set", index, names.ToArray());
            _fontSlot.stringValue = names[selected];
        }

        private void DrawTranslations()
        {
            LocalizedText component = (LocalizedText)target;
            LocalizationTable table = component.Table;

            if (table == null || string.IsNullOrEmpty(component.Key))
            {
                EditorGUILayout.HelpBox("Pick a table and a key to edit translations here.", MessageType.Info);
                DrawCreateKey(table, component);
                return;
            }

            LocalizationEntry entry = table.Find(component.Key);

            if (entry == null)
            {
                EditorGUILayout.HelpBox("The key " + component.Key + " is not in the table.", MessageType.Warning);
                DrawCreateKey(table, component);
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Translations", EditorStyles.boldLabel);
            Language fallback = LocalizationProject.DefaultLanguage();

            foreach (Language language in LocalizationProject.Languages())
            {
                string current = entry.Get(language);
                string label = language + (language == fallback ? " (default)" : "");
                Color color = GUI.color;

                if (string.IsNullOrEmpty(current))
                {
                    GUI.color = new Color(1f, 0.8f, 0.4f);
                }

                string edited = EditorGUILayout.TextField(label, current);
                GUI.color = color;

                if (edited != current)
                {
                    Undo.RecordObject(table, "Edit translation");
                    entry.Set(language, edited);
                    table.Invalidate();
                    EditorUtility.SetDirty(table);
                    ApplyToScene();
                }
            }
        }

        private void DrawCreateKey(LocalizationTable table, LocalizedText component)
        {
            if (table == null || string.IsNullOrWhiteSpace(component.Key))
            {
                return;
            }

            if (GUILayout.Button("Add the key to the table"))
            {
                Undo.RecordObject(table, "Add key");
                table.Add(component.Key);
                EditorUtility.SetDirty(table);
            }
        }

        private void ApplyToScene()
        {
            foreach (Object item in targets)
            {
                if (item is LocalizedText text)
                {
                    text.Apply();
                }
            }

            SceneView.RepaintAll();
        }
    }
}
