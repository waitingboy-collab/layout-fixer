using System.Runtime.InteropServices;
using static LayoutFixer.NativeMethods;

namespace LayoutFixer;

/// <summary>Симулира натискания на клавиши в прозореца отпред.</summary>
internal static class Keyboard
{
    private static readonly int InputSize = Marshal.SizeOf<INPUT>();

    public static void CtrlC() => Chord(VK_C, extended: false, VK_CONTROL);
    public static void CtrlV() => Chord(VK_V, extended: false, VK_CONTROL);

    /// <summary>Ctrl + Shift + ← — маркира думата преди курсора.</summary>
    public static void SelectPreviousWord() => Chord(VK_LEFT, extended: true, VK_CONTROL, VK_SHIFT);

    /// <summary>Чака потребителят да пусне Ctrl, Alt, Shift и Win, иначе те ще се смесят с нашите клавиши.</summary>
    public static async Task WaitForModifiersReleasedAsync(int timeoutMs = 1500)
    {
        var until = Environment.TickCount64 + timeoutMs;
        while (Environment.TickCount64 < until)
        {
            if (!IsDown(VK_CONTROL) && !IsDown(VK_MENU) && !IsDown(VK_SHIFT) && !IsDown(VK_LWIN) && !IsDown(VK_RWIN))
                return;
            await Task.Delay(15);
        }
    }

    private static bool IsDown(int vk) => (GetAsyncKeyState(vk) & 0x8000) != 0;

    private static void Chord(ushort key, bool extended, params ushort[] modifiers)
    {
        var inputs = new List<INPUT>();
        foreach (var m in modifiers) inputs.Add(Key(m, up: false, extended: false));
        inputs.Add(Key(key, up: false, extended));
        inputs.Add(Key(key, up: true, extended));
        foreach (var m in Enumerable.Reverse(modifiers)) inputs.Add(Key(m, up: true, extended: false));

        SendInput((uint)inputs.Count, inputs.ToArray(), InputSize);
    }

    private static INPUT Key(ushort vk, bool up, bool extended) => new()
    {
        type = INPUT_KEYBOARD,
        U = new InputUnion
        {
            ki = new KEYBDINPUT
            {
                wVk = vk,
                dwFlags = (up ? KEYEVENTF_KEYUP : 0) | (extended ? KEYEVENTF_EXTENDEDKEY : 0),
            },
        },
    };
}
