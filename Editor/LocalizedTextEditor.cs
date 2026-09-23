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
            EditorGUILayout.PropertyField(_translateText, new GUIContent("Переводить текст"));
            EditorGUILayout.PropertyField(_applyFont, new GUIContent("Менять шрифт"));

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
                    EditorGUILayout.HelpBox("На объекте нет TextMeshPro: подставлять перевод некуда. Компонент работает только с TMP_Text.", MessageType.Warning);
                    return;
                }
            }
        }

        private void DrawKey()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_key, new GUIContent("Ключ"));

            if (GUILayout.Button("…", GUILayout.Width(26f)))
            {
                ShowKeyMenu();
            }

            using (new EditorGUI.DisabledScope(_table.objectReferenceValue == null))
            {
                if (GUILayout.Button(new GUIContent("В таблицу", "Открыть окно локализации на этом ключе"), GUILayout.Width(80f)))
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
                EditorUtility.DisplayDialog("JTL SDK", "Сначала выберите таблицу.", "Понятно");
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
                menu.AddDisabledItem(new GUIContent("В таблице нет ключей"));
            }

            menu.ShowAsContext();
        }

        private void DrawFontSlot()
        {
            LocalizationFonts fonts = LocalizationProject.Settings == null ? null : LocalizationProject.Settings.Fonts;

            if (fonts == null)
            {
                EditorGUILayout.PropertyField(_fontSlot, new GUIContent("Набор шрифтов"));
                EditorGUILayout.HelpBox("Ассет со шрифтами не выбран в окне JTL SDK › Localization.", MessageType.None);
                return;
            }

            List<string> names = fonts.SlotNames();

            if (names.Count == 0)
            {
                EditorGUILayout.HelpBox("В ассете шрифтов нет ни одного набора.", MessageType.None);
                return;
            }

            int index = Mathf.Max(0, names.IndexOf(_fontSlot.stringValue));
            int selected = EditorGUILayout.Popup("Набор шрифтов", index, names.ToArray());
            _fontSlot.stringValue = names[selected];
        }

        private void DrawTranslations()
        {
            LocalizedText component = (LocalizedText)target;
            LocalizationTable table = component.Table;

            if (table == null || string.IsNullOrEmpty(component.Key))
            {
                EditorGUILayout.HelpBox("Выберите таблицу и ключ, тогда переводы можно править прямо здесь.", MessageType.Info);
                DrawCreateKey(table, component);
                return;
            }

            LocalizationEntry entry = table.Find(component.Key);

            if (entry == null)
            {
                EditorGUILayout.HelpBox("Ключа «" + component.Key + "» нет в таблице.", MessageType.Warning);
                DrawCreateKey(table, component);
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Переводы", EditorStyles.boldLabel);
            Language fallback = LocalizationProject.DefaultLanguage();

            foreach (Language language in LocalizationProject.Languages())
            {
                string current = entry.Get(language);
                string label = language + (language == fallback ? " (по умолчанию)" : "");
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

            if (GUILayout.Button("Добавить ключ в таблицу"))
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
