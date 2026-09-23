using UnityEngine;

namespace JTLStudio.SDK.Localization
{
    public interface ILocalizedTarget
    {
        bool IsAlive { get; }

        void SetText(string text);

        void SetFont(LanguageFont font);
    }

    public interface ILocalizedTargetFactory
    {
        ILocalizedTarget Bind(GameObject host);
    }
}
