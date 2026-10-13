using System.Text.Json;
using System.Text.Json.Serialization;

namespace LayoutFixer;

public enum HotkeyChoice
{
    CtrlAltSpace,
    Pause,
    CtrlShiftSpace,
}

/// <summary>Настройки в %AppData%\LayoutFixer\settings.json.</summary>
public sealed class Settings
{
    /// <summary>null = автоматично според инсталираната в Windows подредба.</summary>
    public Core.BulgarianLayout? Layout { get; set; }

    public HotkeyChoice Hotkey { get; set; } = HotkeyChoice.CtrlAltSpace;

    /// <summary>Ако няма маркиран текст, взима последната дума преди курсора.</summary>
    public bool LastWordFallback { get; set; } = true;

    /// <summary>След превод превключва подредбата на правилната.</summary>
    public bool SwitchLayoutAfter { get; set; } = true;

    public bool WelcomeShown { get; set; }

    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "LayoutFixer", "settings.json");

    public static Settings Load()
    {
        try
        {
            if (File.Exists(FilePath))
                return JsonSerializer.Deserialize<Settings>(File.ReadAllText(FilePath), Json) ?? new Settings();
        }
        catch
        {
            // Повреден файл — започваме с настройките по подразбиране.
        }
        return new Settings();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this, Json));
        }
        catch
        {
            // Настройките не са критични — програмата продължава да работи.
        }
    }

    public static string HotkeyText(HotkeyChoice choice) => choice switch
    {
        HotkeyChoice.CtrlAltSpace => "Ctrl + Alt + Space",
        HotkeyChoice.Pause => "Pause",
        HotkeyChoice.CtrlShiftSpace => "Ctrl + Shift + Space",
        _ => choice.ToString(),
    };
}
