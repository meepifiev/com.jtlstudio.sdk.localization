using UnityEditor;
using UnityEngine;

namespace JTLStudio.SDK.Localization.Editor
{
    public static class LocalizationRuntimeCleanup
    {
        [InitializeOnLoadMethod]
        private static void Watch()
        {
            Remove();
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingPlayMode || change == PlayModeStateChange.EnteredEditMode)
            {
                Remove();
            }
        }

        private static void Remove()
        {
            foreach (LocalizationRuntime runtime in Resources.FindObjectsOfTypeAll<LocalizationRuntime>())
            {
                if (runtime != null && runtime.gameObject != null)
                {
                    Object.DestroyImmediate(runtime.gameObject);
                }
            }
        }
    }
}
