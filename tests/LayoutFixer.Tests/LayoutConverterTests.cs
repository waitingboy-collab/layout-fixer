using LayoutFixer.Core;
using Xunit;

namespace LayoutFixer.Tests;

public class LatinToCyrillicTests
{
    [Theory]
    // Фонетична (нова): v→в, q→ч, w→ш, [→я, `→ю
    [InlineData(BulgarianLayout.Phonetic, "zdravej", "здравей")]
    [InlineData(BulgarianLayout.Phonetic, "Zdravej, kak si?", "Здравей, как си?")]
    [InlineData(BulgarianLayout.Phonetic, "qovek", "човек")]
    [InlineData(BulgarianLayout.Phonetic, "wapka", "шапка")]
    [InlineData(BulgarianLayout.Phonetic, "[bylka", "ябълка")]
    [InlineData(BulgarianLayout.Phonetic, "`nako", "юнако")]
    [InlineData(BulgarianLayout.Phonetic, "xivot", "живот")]
    // Фонетична традиционна: w→в, v→ж, `→ч, [→ш, q→я, \→ю
    [InlineData(BulgarianLayout.PhoneticTraditional, "zdrawej", "здравей")]
    [InlineData(BulgarianLayout.PhoneticTraditional, "`owek", "човек")]
    [InlineData(BulgarianLayout.PhoneticTraditional, "[apka", "шапка")]
    [InlineData(BulgarianLayout.PhoneticTraditional, "qbylka", "ябълка")]
    [InlineData(BulgarianLayout.PhoneticTraditional, "viwot", "живот")]
    [InlineData(BulgarianLayout.PhoneticTraditional, "\\nako", "юнако")]
    // БДС: здравей = p o , d l e x
    [InlineData(BulgarianLayout.Bds, "po,dlex", "здравей")]
    [InlineData(BulgarianLayout.Bds, "Po,dlex", "Здравей")]
    [InlineData(BulgarianLayout.Bds, "'djc,", "чатър")]
    public void Converts_text_typed_with_latin_layout(BulgarianLayout layout, string input, string expected)
    {
        var result = new LayoutConverter(layout).Convert(input);

        Assert.Equal(Direction.LatinToCyrillic, result.Direction);
        Assert.Equal(expected, result.Text);
    }
}

public class CyrillicToLatinTests
{
    [Theory]
    [InlineData(BulgarianLayout.Phonetic, "хелло", "hello")]
    [InlineData(BulgarianLayout.Phonetic, "Хелло wорлд", "Hello world")]
    [InlineData(BulgarianLayout.PhoneticTraditional, "хелло", "hello")]
    [InlineData(BulgarianLayout.PhoneticTraditional, "въиндоъс", "wyindoys")]
    [InlineData(BulgarianLayout.Bds, "геввд", "hello")]
    [InlineData(BulgarianLayout.Bds, "Геввд", "Hello")]
    public void Converts_text_typed_with_cyrillic_layout(BulgarianLayout layout, string input, string expected)
    {
        var result = new LayoutConverter(layout).Convert(input, Direction.CyrillicToLatin);

        Assert.Equal(expected, result.Text);
    }

    [Fact]
    public void Uppercase_letter_without_own_key_is_converted_through_lowercase()
    {
        // В традиционната фонетична „ь“ е на x, а главно „Ь“ няма собствен клавиш.
        var result = new LayoutConverter(BulgarianLayout.PhoneticTraditional)
            .Convert("Ь", Direction.CyrillicToLatin);

        Assert.Equal("X", result.Text);
    }
}

public class DirectionTests
{
    [Theory]
    [InlineData("zdravej", Direction.LatinToCyrillic)]
    [InlineData("здравей", Direction.CyrillicToLatin)]
    [InlineData("hello свят", Direction.LatinToCyrillic)]
    [InlineData("здравей world", Direction.CyrillicToLatin)]
    [InlineData("", Direction.CyrillicToLatin)]
    [InlineData("123 !?", Direction.CyrillicToLatin)]
    public void Detects_direction_by_letter_count(string text, Direction expected)
    {
        Assert.Equal(expected, LayoutConverter.DetectDirection(text));
    }
}

public class TableIntegrityTests
{
    public static IEnumerable<object[]> AllLayouts() =>
        Enum.GetValues<BulgarianLayout>().Select(l => new object[] { l });

    [Theory]
    [MemberData(nameof(AllLayouts))]
    public void Table_builds_without_duplicates(BulgarianLayout layout)
    {
        // Конструкторът хвърля грешка при повтарящ се клавиш или символ.
        var converter = new LayoutConverter(layout);
        Assert.NotEmpty(converter.LatinToCyrillicMap);
    }

    [Theory]
    [MemberData(nameof(AllLayouts))]
    public void Every_mapped_key_survives_a_round_trip(BulgarianLayout layout)
    {
        var converter = new LayoutConverter(layout);

        foreach (var (latin, cyr) in converter.LatinToCyrillicMap)
        {
            var there = converter.Convert(latin.ToString(), Direction.LatinToCyrillic).Text;
            var back = converter.Convert(there, Direction.CyrillicToLatin).Text;

            Assert.Equal(cyr.ToString(), there);
            Assert.Equal(latin.ToString(), back);
        }
    }

    [Theory]
    [MemberData(nameof(AllLayouts))]
    public void All_30_bulgarian_letters_are_reachable(BulgarianLayout layout)
    {
        const string alphabet = "абвгдежзийклмнопрстуфхцчшщъьюя";
        var reachable = new LayoutConverter(layout).LatinToCyrillicMap.Values.ToHashSet();

        foreach (char letter in alphabet)
        {
            Assert.True(reachable.Contains(letter), $"„{letter}“ липсва в {layout}");
            Assert.True(reachable.Contains(char.ToUpperInvariant(letter)) || letter == 'ь',
                $"„{char.ToUpperInvariant(letter)}“ липсва в {layout}");
        }
    }

    [Theory]
    [MemberData(nameof(AllLayouts))]
    public void Digits_and_spaces_are_left_alone(BulgarianLayout layout)
    {
        var converter = new LayoutConverter(layout);

        Assert.Equal("2026 12", converter.Convert("2026 12", Direction.LatinToCyrillic).Text);
        Assert.Equal("2026 12", converter.Convert("2026 12", Direction.CyrillicToLatin).Text);
    }

    [Theory]
    [InlineData("00030402", BulgarianLayout.Bds)]
    [InlineData("00000402", BulgarianLayout.Bds)]
    [InlineData("00020402", BulgarianLayout.Phonetic)]
    [InlineData("00040402", BulgarianLayout.PhoneticTraditional)]
    public void Recognizes_windows_layout_ids(string klid, BulgarianLayout expected)
    {
        Assert.Equal(expected, BulgarianLayoutInfo.FromKlid(klid));
    }

    [Theory]
    [InlineData("00000409")]
    [InlineData("00010402")]
    [InlineData(null)]
    public void Ignores_non_bulgarian_layout_ids(string? klid)
    {
        Assert.Null(BulgarianLayoutInfo.FromKlid(klid));
    }
}
