using System.Text;
using LayoutFixer.Core;
using static LayoutFixer.NativeMethods;

namespace LayoutFixer;

/// <summary>Кои подредби са инсталирани в Windows и как да превключим към тях.</summary>
internal sealed class KeyboardLayouts
{
    private const int LangBulgarian = 0x0402;

    /// <summary>Инсталираните български подредби и техните HKL.</summary>
    public IReadOnlyDictionary<BulgarianLayout, IntPtr> Bulgarian { get; }

    /// <summary>Първата небългарска подредба (за предпочитане английска).</summary>
    public IntPtr Latin { get; }

    private KeyboardLayouts(Dictionary<BulgarianLayout, IntPtr> bulgarian, IntPtr latin)
    {
        Bulgarian = bulgarian;
        Latin = latin;
    }

    public static KeyboardLayouts Detect()
    {
        var bulgarian = new Dictionary<BulgarianLayout, IntPtr>();
        IntPtr latin = IntPtr.Zero;

        int count = GetKeyboardLayoutList(0, null);
        var list = new IntPtr[Math.Max(count, 0)];
        if (count > 0) GetKeyboardLayoutList(count, list);

        IntPtr original = GetKeyboardLayout(0);
        try
        {
            foreach (var hkl in list)
            {
                int lang = (int)((long)hkl & 0xFFFF);
                if (lang == LangBulgarian)
                {
                    // KLID се чете само от активната подредба на нишката, затова
                    // за момент я активираме в нашата програма (не в чужди прозорци).
                    ActivateKeyboardLayout(hkl, 0);
                    var name = new StringBuilder(KL_NAMELENGTH);
                    if (GetKeyboardLayoutName(name) && BulgarianLayoutInfo.FromKlid(name.ToString()) is { } layout)
                        bulgarian.TryAdd(layout, hkl);
                }
                else if (latin == IntPtr.Zero || lang == 0x0409)
                {
                    latin = hkl;
                }
            }
        }
        finally
        {
            ActivateKeyboardLayout(original, 0);
        }

        return new KeyboardLayouts(bulgarian, latin);
    }

    /// <summary>Подредбата, която да ползваме, ако потребителят е избрал „Автоматично“.</summary>
    public BulgarianLayout AutoChoice =>
        Bulgarian.Keys.OrderBy(PreferenceOrder).Cast<BulgarianLayout?>().FirstOrDefault()
        ?? BulgarianLayout.Phonetic;

    private static int PreferenceOrder(BulgarianLayout l) => l switch
    {
        BulgarianLayout.Phonetic => 0,
        BulgarianLayout.Bds => 1,
        _ => 2,
    };

    /// <summary>Иска от прозореца отпред да смени подредбата си.</summary>
    public static void SwitchForegroundTo(IntPtr hkl)
    {
        if (hkl == IntPtr.Zero) return;
        var window = GetForegroundWindow();
        if (window != IntPtr.Zero)
            PostMessage(window, WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, hkl);
    }
}
