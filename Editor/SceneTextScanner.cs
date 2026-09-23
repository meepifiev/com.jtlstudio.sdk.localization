using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JTLStudio.SDK.Localization.Editor
{
    public static class SceneTextScanner
    {
        public class Found
        {
            public Found(GameObject host, string text, LocalizedText component)
            {
                Host = host;
                Text = text;
                Component = component;
            }

            public GameObject Host { get; }
            public string Text { get; }
            public LocalizedText Component { get; }
            public bool Selected { get; set; } = true;
            public string Key { get; set; } = "";
        }

        public static List<Found> Scan()
        {
            List<Found> found = new List<Found>();

            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);

                if (scene.isLoaded == false)
                {
                    continue;
                }

                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    Collect(root, found);
                }
            }

            return found;
        }

        public static void Apply(IReadOnlyList<Found> items, LocalizationTable table, Language language)
        {
            if (table == null)
            {
                return;
            }

            Undo.RecordObject(table, "Localize scene texts");

            foreach (Found item in items)
            {
                if (item.Selected == false || string.IsNullOrWhiteSpace(item.Key))
                {
                    continue;
                }

                LocalizationEntry entry = table.Add(item.Key);

                if (string.IsNullOrEmpty(entry.Get(language)))
                {
                    entry.Set(language, item.Text);
                }

                LocalizedText component = item.Component;

                if (component == null)
                {
                    component = Undo.AddComponent<LocalizedText>(item.Host);
                }

                SerializedObject serialized = new SerializedObject(component);
                serialized.FindProperty("_table").objectReferenceValue = table;
                serialized.FindProperty("_key").stringValue = item.Key;
                serialized.ApplyModifiedProperties();
                component.Apply();
                EditorUtility.SetDirty(component);
            }

            table.Invalidate();
            EditorUtility.SetDirty(table);
            EditorSceneManager.MarkAllScenesDirty();
        }

        public static string SuggestKey(GameObject host, string text)
        {
            string source = string.IsNullOrWhiteSpace(text) ? host.name : text;
            StringBuilder builder = new StringBuilder();
            bool separator = false;

            foreach (char symbol in source.Trim().ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(symbol) && symbol < 128)
                {
                    builder.Append(symbol);
                    separator = false;
                    continue;
                }

                if (separator == false && builder.Length > 0)
                {
                    builder.Append('.');
                    separator = true;
                }
            }

            string key = builder.ToString().Trim('.');

            if (key.Length > 40)
            {
                key = key.Substring(0, 40).Trim('.');
            }

            return string.IsNullOrEmpty(key) ? Path(host) : key;
        }

        private static void Collect(GameObject host, List<Found> found)
        {
            LocalizedText existing = host.GetComponent<LocalizedText>();

            if (Localization.Bind(host) != null)
            {
                found.Add(new Found(host, TextOf(host), existing));
            }

            foreach (Transform child in host.transform)
            {
                Collect(child.gameObject, found);
            }
        }

        private static string TextOf(GameObject host)
        {
            foreach (Component component in host.GetComponents<Component>())
            {
                if (component == null)
                {
                    continue;
                }

                SerializedObject serialized = new SerializedObject(component);
                SerializedProperty property = serialized.FindProperty("m_text");

                if (property != null && property.propertyType == SerializedPropertyType.String)
                {
                    return property.stringValue;
                }
            }

            return "";
        }

        private static string Path(GameObject host)
        {
            string path = host.name;
            Transform parent = host.transform.parent;

            while (parent != null)
            {
                path = parent.name + "." + path;
                parent = parent.parent;
            }

            return path.ToLowerInvariant().Replace(' ', '.');
        }
    }
}
