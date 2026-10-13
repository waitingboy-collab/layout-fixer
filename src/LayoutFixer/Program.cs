namespace LayoutFixer;

internal static class Program
{
    /// <summary>Същото име ползва и инсталаторът (AppMutex), за да затвори програмата при ъпдейт.</summary>
    public const string MutexName = "LayoutFixerSingleInstance";

    [STAThread]
    private static void Main()
    {
        using var mutex = new Mutex(initiallyOwned: true, MutexName, out bool isFirst);
        if (!isFirst) return; // вече работи

        ApplicationConfiguration.Initialize();
        Application.Run(new TrayApp());
    }
}
