using UnityEngine;

namespace JTLStudio.SDK.Localization
{
    public class LocalizationRuntime : MonoBehaviour
    {
        private static LocalizationRuntime _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Create()
        {
            if (_instance != null)
            {
                return;
            }

            GameObject host = new GameObject(nameof(LocalizationRuntime));
            host.hideFlags = HideFlags.HideInHierarchy;
            DontDestroyOnLoad(host);
            _instance = host.AddComponent<LocalizationRuntime>();
        }

        private void Update()
        {
            if (JTLSDK.IsCreated == false)
            {
                return;
            }

            Localization.SetLanguage(JTLSDK.Language.Current);
        }
    }
}
