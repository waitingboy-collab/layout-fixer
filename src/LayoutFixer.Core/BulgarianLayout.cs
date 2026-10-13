namespace LayoutFixer.Core;

/// <summary>Българските клавиатурни подредби в Windows.</summary>
public enum BulgarianLayout
{
    /// <summary>„Bulgarian“ (KBDBULG, KLID 00030402) — БДС 5237:2006.</summary>
    Bds,

    /// <summary>„Bulgarian (Phonetic)“ (KBDBGPH, KLID 00020402) — новата фонетична.</summary>
    Phonetic,

    /// <summary>„Bulgarian (Phonetic Traditional)“ (KBDBGPH1, KLID 00040402) — старата фонетична.</summary>
    PhoneticTraditional,
}

public static class BulgarianLayoutInfo
{
    public static string DisplayName(BulgarianLayout layout) => layout switch
    {
        BulgarianLayout.Bds => "БДС",
        BulgarianLayout.Phonetic => "Фонетична",
        BulgarianLayout.PhoneticTraditional => "Фонетична традиционна",
        _ => layout.ToString(),
    };

    /// <summary>
    /// Разпознава подредбата по KLID от Windows (GetKeyboardLayoutName).
    /// Връща null за небългарски подредби.
    /// </summary>
    public static BulgarianLayout? FromKlid(string? klid) => klid?.ToUpperInvariant() switch
    {
        "00030402" => BulgarianLayout.Bds,
        "00000402" => BulgarianLayout.Bds, // Bulgarian (Typewriter) — старият БДС, почти същият
        "00020402" => BulgarianLayout.Phonetic,
        "00040402" => BulgarianLayout.PhoneticTraditional,
        _ => null,
    };
}
