using System.Runtime.InteropServices;
using static LayoutFixer.NativeMethods;

namespace LayoutFixer;

/// <summary>
/// Работа с клипборда с повторни опити — друга програма може да го държи
/// заключен за няколко милисекунди.
/// </summary>
internal static class ClipboardHelper
{
    /// <summary>Копие на всичко в клипборда, за да го върнем след превода.</summary>
    public static IDataObject? Backup()
    {
        return Retry<IDataObject?>(() =>
        {
            var source = Clipboard.GetDataObject();
            if (source is null) return null;

            var copy = new DataObject();
            foreach (var format in source.GetFormats(false))
            {
                try
                {
                    var data = source.GetData(format, false);
                    if (data is not null) copy.SetData(format, false, data);
                }
                catch
                {
                    // Някои формати не могат да се копират — пропускаме ги.
                }
            }
            return copy;
        }, fallback: null);
    }

    public static void Restore(IDataObject? backup)
    {
        Retry(() =>
        {
            if (backup is null || backup.GetFormats(false).Length == 0) Clipboard.Clear();
            else Clipboard.SetDataObject(backup, copy: true);
            return true;
        }, fallback: false);
    }

    public static void Clear() => Retry(() => { Clipboard.Clear(); return true; }, fallback: false);

    public static void SetText(string text) =>
        Retry(() => { Clipboard.SetText(text, TextDataFormat.UnicodeText); return true; }, fallback: false);

    /// <summary>Натиска Ctrl+C и чака клипбордът да се промени. Връща null, ако нищо не е копирано.</summary>
    public static async Task<string?> CopySelectionAsync(int timeoutMs = 450)
    {
        Clear();
        uint before = GetClipboardSequenceNumber();
        Keyboard.CtrlC();

        var until = Environment.TickCount64 + timeoutMs;
        while (Environment.TickCount64 < until)
        {
            await Task.Delay(20);
            if (GetClipboardSequenceNumber() == before) continue;

            var text = Retry<string?>(() => Clipboard.ContainsText() ? Clipboard.GetText(TextDataFormat.UnicodeText) : null, fallback: null);
            if (!string.IsNullOrEmpty(text)) return text;
        }
        return null;
    }

    private static T Retry<T>(Func<T> action, T fallback, int attempts = 8)
    {
        for (int i = 0; i < attempts; i++)
        {
            try { return action(); }
            catch (ExternalException) { Thread.Sleep(25); }
        }
        return fallback;
    }
}
