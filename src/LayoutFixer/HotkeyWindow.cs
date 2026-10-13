using static LayoutFixer.NativeMethods;

namespace LayoutFixer;

/// <summary>Невидим прозорец, който получава глобалната клавишна комбинация.</summary>
internal sealed class HotkeyWindow : NativeWindow, IDisposable
{
    private const int HotkeyId = 0x4C46; // "LF"
    private bool _registered;

    public event Action? Pressed;

    public HotkeyWindow() => CreateHandle(new CreateParams());

    /// <summary>Регистрира комбинацията. Връща false, ако друга програма вече я ползва.</summary>
    public bool Register(HotkeyChoice choice)
    {
        Unregister();

        var (mods, vk) = choice switch
        {
            HotkeyChoice.Pause => (0u, (uint)VK_PAUSE),
            HotkeyChoice.CtrlShiftSpace => (MOD_CONTROL | MOD_SHIFT, (uint)VK_SPACE),
            _ => (MOD_CONTROL | MOD_ALT, (uint)VK_SPACE),
        };

        _registered = RegisterHotKey(Handle, HotkeyId, mods | MOD_NOREPEAT, vk);
        return _registered;
    }

    public void Unregister()
    {
        if (_registered) UnregisterHotKey(Handle, HotkeyId);
        _registered = false;
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_HOTKEY && m.WParam == (IntPtr)HotkeyId)
            Pressed?.Invoke();
        base.WndProc(ref m);
    }

    public void Dispose()
    {
        Unregister();
        DestroyHandle();
    }
}
