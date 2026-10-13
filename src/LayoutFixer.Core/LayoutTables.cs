namespace LayoutFixer.Core;

/// <summary>
/// Таблици клавиш → символ. Всички низове са в един и същ ред на клавишите:
///   `  q w e r t y u i o p [ ] \  a s d f g h j k l ; '  z x c v b n m , . /  - =
/// Данните са взети от официалните KLC файлове на Windows (kbdlayout.info),
/// по scancode. Цифровият ред и ISO клавишът до левия Shift не се преобразуват.
/// </summary>
internal static class LayoutTables
{
    public const string UsLower = "`qwertyuiop[]\\asdfghjkl;'zxcvbnm,./-=";
    public const string UsUpper = "~QWERTYUIOP{}|ASDFGHJKL:\"ZXCVBNM<>?_+";

    public static (string Lower, string Upper) For(BulgarianLayout layout) => layout switch
    {
        // KBDBULG — „Bulgarian“, БДС 5237:2006
        BulgarianLayout.Bds => (
            "(,уеишщксдзц;„ьяаожгтнвмчюйъэфхпрлб-.",
            ")ыУЕИШЩКСДЗЦ§“ѝЯАОЖГТНВМЧЮЙЪЭФХПРЛБ$€"),

        // KBDBGPH — „Bulgarian (Phonetic)“, новата фонетична
        BulgarianLayout.Phonetic => (
            "ючшертъуиопящьасдфгхйкл;'зжцвбнм,./-=",
            "ЮЧШЕРТЪУИОПЯЩѝАСДФГХЙКЛ:\"ЗЖЦВБНМ„“?–+"),

        // KBDBGPH1 — „Bulgarian (Phonetic Traditional)“, старата фонетична
        BulgarianLayout.PhoneticTraditional => (
            "чявертъуиопшщюасдфгхйкл;'зьцжбнм,./-=",
            "ЧЯВЕРТЪУИОПШЩЮАСДФГХЙКЛ:\"ЗѝЦЖБНМ<>?_+"),

        _ => throw new ArgumentOutOfRangeException(nameof(layout)),
    };
}
