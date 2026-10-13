using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using LayoutFixer.Core;

namespace LayoutFixer;

/// <summary>Иконата до часовника, менюто и връзката между частите.</summary>
internal sealed class TrayApp : ApplicationContext
{
    private const string RepoUrl = "https://github.com/waitingboy-collab/layout-fixer";

    private readonly Settings _settings = Settings.Load();
    private KeyboardLayouts _layouts = KeyboardLayouts.Detect();
    private readonly HotkeyWindow _hotkey = new();
    private readonly Fixer _fixer;
    private readonly NotifyIcon _tray;
    private readonly Icon _icon;

    public TrayApp()
    {
        _fixer = new Fixer(() => _settings, () => _layouts);
        _icon = CreateIcon();

        _tray = new NotifyIcon
        {
            Icon = _icon,
            Text = "Грешна подредба",
            Visible = true,
            ContextMenuStrip = new ContextMenuStrip(),
        };
        _tray.ContextMenuStrip.Opening += (_, e) =>
        {
            BuildMenu();
            e.Cancel = false; // празно меню отменя отварянето по подразбиране
        };
        _tray.DoubleClick += (_, _) => ShowHowTo();

        _hotkey.Pressed += async () =>
        {
            try
            {
                await _fixer.RunAsync();
            }
            catch (Exception ex)
            {
                _tray.ShowBalloonTip(5000, "Неуспешна поправка", ex.Message, ToolTipIcon.Error);
            }
        };
        RegisterHotkey(showErrors: true);

        if (!_settings.WelcomeShown)
        {
            ShowHowTo();
            _settings.WelcomeShown = true;
            _settings.Save();
        }
    }

    private void RegisterHotkey(bool showErrors)
    {
        if (_hotkey.Register(_settings.Hotkey) || !showErrors) return;

        _tray.ShowBalloonTip(6000, "Клавишът е зает",
            $"{Settings.HotkeyText(_settings.Hotkey)} се ползва от друга програма. " +
            "Избери друг от менюто на иконата.", ToolTipIcon.Warning);
    }

    private void ShowHowTo()
    {
        _tray.ShowBalloonTip(8000, "Грешна подредба работи",
            $"Маркирай текста и натисни {Settings.HotkeyText(_settings.Hotkey)}. " +
            $"Без маркиран текст поправя последната дума. Подредба: {BulgarianLayoutInfo.DisplayName(_fixer.ActiveLayout)}.",
            ToolTipIcon.Info);
    }

    private void BuildMenu()
    {
        var menu = _tray.ContextMenuStrip!;
        menu.Items.Clear();

        menu.Items.Add(new ToolStripLabel($"Грешна подредба · {Settings.HotkeyText(_settings.Hotkey)}")
        {
            Font = new Font(menu.Font, FontStyle.Bold),
        });
        menu.Items.Add(new ToolStripSeparator());

        // Подредба
        _layouts = KeyboardLayouts.Detect(); // потребителят може да е добавил подредба
        var layoutMenu = new ToolStripMenuItem("Българска подредба");
        var auto = _layouts.AutoChoice;
        layoutMenu.DropDownItems.Add(Check(
            $"Автоматично ({BulgarianLayoutInfo.DisplayName(auto)})",
            _settings.Layout is null,
            () => _settings.Layout = null));
        layoutMenu.DropDownItems.Add(new ToolStripSeparator());
        foreach (var layout in Enum.GetValues<BulgarianLayout>())
        {
            var installed = _layouts.Bulgarian.ContainsKey(layout) ? "" : " (не е инсталирана)";
            layoutMenu.DropDownItems.Add(Check(
                BulgarianLayoutInfo.DisplayName(layout) + installed,
                _settings.Layout == layout,
                () => _settings.Layout = layout));
        }
        menu.Items.Add(layoutMenu);

        // Клавиш
        var hotkeyMenu = new ToolStripMenuItem("Клавишна комбинация");
        foreach (var choice in Enum.GetValues<HotkeyChoice>())
        {
            hotkeyMenu.DropDownItems.Add(Check(Settings.HotkeyText(choice), _settings.Hotkey == choice, () =>
            {
                _settings.Hotkey = choice;
                RegisterHotkey(showErrors: true);
            }));
        }
        menu.Items.Add(hotkeyMenu);

        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(Check("Без маркиран текст: последната дума", _settings.LastWordFallback,
            () => _settings.LastWordFallback = !_settings.LastWordFallback));
        menu.Items.Add(Check("Смени подредбата след поправка", _settings.SwitchLayoutAfter,
            () => _settings.SwitchLayoutAfter = !_settings.SwitchLayoutAfter));
        menu.Items.Add(Check("Стартирай с Windows", Autostart.IsEnabled,
            () => Autostart.Set(!Autostart.IsEnabled)));

        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Как се ползва", null, (_, _) => ShowHowTo());
        menu.Items.Add("Страница на програмата", null, (_, _) => OpenUrl(RepoUrl));
        menu.Items.Add("Изход", null, (_, _) => ExitThread());
    }

    private ToolStripMenuItem Check(string text, bool isChecked, Action onClick)
    {
        var item = new ToolStripMenuItem(text) { Checked = isChecked };
        item.Click += (_, _) =>
        {
            onClick();
            _settings.Save();
        };
        return item;
    }

    private static void OpenUrl(string url)
    {
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
        catch { /* няма браузър по подразбиране */ }
    }

    /// <summary>Рисува иконата: жълт квадрат с „Б“.</summary>
    private static Icon CreateIcon()
    {
        using var bmp = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.Clear(Color.Transparent);

            using var path = new GraphicsPath();
            const int r = 8;
            path.AddArc(0, 0, r * 2, r * 2, 180, 90);
            path.AddArc(31 - r * 2, 0, r * 2, r * 2, 270, 90);
            path.AddArc(31 - r * 2, 31 - r * 2, r * 2, r * 2, 0, 90);
            path.AddArc(0, 31 - r * 2, r * 2, r * 2, 90, 90);
            path.CloseFigure();
            using var fill = new SolidBrush(Color.FromArgb(0xF2, 0xB3, 0x21));
            g.FillPath(fill, path);

            using var font = new Font("Segoe UI", 17, FontStyle.Bold, GraphicsUnit.Pixel);
            using var ink = new SolidBrush(Color.FromArgb(0x14, 0x1C, 0x28));
            var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("Б", font, ink, new RectangleF(0, 1, 32, 32), format);
        }

        IntPtr handle = bmp.GetHicon();
        var icon = (Icon)Icon.FromHandle(handle).Clone();
        NativeMethods.DestroyIcon(handle);
        return icon;
    }

    protected override void ExitThreadCore()
    {
        _tray.Visible = false;
        _tray.Dispose();
        _hotkey.Dispose();
        _icon.Dispose();
        base.ExitThreadCore();
    }
}
