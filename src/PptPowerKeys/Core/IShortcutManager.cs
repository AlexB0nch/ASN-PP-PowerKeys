namespace PptPowerKeys.Core
{
    public interface IShortcutManager : System.IDisposable
    {
        void Initialize();

        void ReloadBindings();
    }
}
