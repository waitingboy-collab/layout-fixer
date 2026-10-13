using System.Globalization;

namespace LayoutFixer.Core;

public enum Direction
{
    /// <summary>Написано на латиница, а трябваше да е на кирилица.</summary>
    LatinToCyrillic,

    /// <summary>Написано на кирилица, а трябваше да е на латиница.</summary>
    CyrillicToLatin,
}

public sealed record ConversionResult(string Text, Direction Direction, int ChangedChars);

/// <summary>
/// Преобразува текст, написан при грешно включена подредба.
/// Приема, че целият текст е въведен с една и съща (грешна) подредба.
/// </summary>
public sealed class LayoutConverter
{
    private readonly Dictionary<char, char> _toCyrillic = new();
    private readonly Dictionary<char, char> _toLatin = new();

    public BulgarianLayout Layout { get; }

    public LayoutConverter(BulgarianLayout layout)
    {
        Layout = layout;
        var (lower, upper) = LayoutTables.For(layout);
        Add(LayoutTables.UsLower, lower);
        Add(LayoutTables.UsUpper, upper);
    }

    private void Add(string us, string bg)
    {
        if (us.Length != bg.Length)
            throw new InvalidOperationException($"Таблицата за {Layout} е с грешна дължина: {us.Length} срещу {bg.Length}.");

        for (int i = 0; i < us.Length; i++)
        {
            char latin = us[i], cyr = bg[i];
            if (latin == cyr) continue; // същият символ и в двете подредби

            if (!_toCyrillic.TryAdd(latin, cyr))
                throw new InvalidOperationException($"Повтарящ се клавиш '{latin}' в {Layout}.");
            if (!_toLatin.TryAdd(cyr, latin))
                throw new InvalidOperationException($"Повтарящ се символ '{cyr}' в {Layout}.");
        }
    }

    /// <summary>Познава посоката: ако латинските букви са повече, превежда към кирилица.</summary>
    public static Direction DetectDirection(string text)
    {
        int latin = 0, cyrillic = 0;
        foreach (char c in text)
        {
            if (c is >= 'a' and <= 'z' or >= 'A' and <= 'Z') latin++;
            else if (c is >= 'Ѐ' and <= 'ӿ') cyrillic++;
        }
        return latin >= cyrillic && latin > 0 ? Direction.LatinToCyrillic : Direction.CyrillicToLatin;
    }

    public ConversionResult Convert(string text) => Convert(text, DetectDirection(text));

    public ConversionResult Convert(string text, Direction direction)
    {
        var map = direction == Direction.LatinToCyrillic ? _toCyrillic : _toLatin;
        var chars = text.ToCharArray();
        int changed = 0;

        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            if (map.TryGetValue(c, out char mapped))
            {
                chars[i] = mapped;
                changed++;
            }
            else if (direction == Direction.CyrillicToLatin && char.IsUpper(c))
            {
                // Главни букви, които нямат собствен клавиш (напр. Ь, Ѝ),
                // се превеждат чрез малката буква.
                char lower = char.ToLower(c, CultureInfo.InvariantCulture);
                if (map.TryGetValue(lower, out char viaLower))
                {
                    chars[i] = char.ToUpper(viaLower, CultureInfo.InvariantCulture);
                    changed++;
                }
            }
        }

        return new ConversionResult(new string(chars), direction, changed);
    }

    /// <summary>Всички двойки латиница → кирилица (за тестове и за таблицата в „За програмата“).</summary>
    public IReadOnlyDictionary<char, char> LatinToCyrillicMap => _toCyrillic;
}
