using LayoutFixer.Core;

namespace LayoutFixer;

/// <summary>Копира маркирания текст, превежда го и го поставя обратно.</summary>
internal sealed class Fixer
{
    private readonly Func<Settings> _settings;
    private readonly Func<KeyboardLayouts> _layouts;
    private bool _busy;

    public Fixer(Func<Settings> settings, Func<KeyboardLayouts> layouts)
    {
        _settings = settings;
        _layouts = layouts;
    }

    public BulgarianLayout ActiveLayout => _settings().Layout ?? _layouts().AutoChoice;

    public async Task RunAsync()
    {
        if (_busy) return;
        _busy = true;

        IDataObject? backup = null;
        try
        {
            await Keyboard.WaitForModifiersReleasedAsync();
            backup = ClipboardHelper.Backup();

            var text = await ClipboardHelper.CopySelectionAsync();

            if (text is null && _settings().LastWordFallback)
            {
                Keyboard.SelectPreviousWord();
                await Task.Delay(40);
                text = await ClipboardHelper.CopySelectionAsync();
            }

            if (string.IsNullOrEmpty(text)) return;

            var result = new LayoutConverter(ActiveLayout).Convert(text);
            if (result.ChangedChars == 0) return;

            ClipboardHelper.SetText(result.Text);
            Keyboard.CtrlV();
            await Task.Delay(250); // програмата отпред трябва да прочете клипборда

            if (_settings().SwitchLayoutAfter)
            {
                var layouts = _layouts();
                var target = result.Direction == Direction.LatinToCyrillic
                    ? layouts.Bulgarian.GetValueOrDefault(ActiveLayout)
                    : layouts.Latin;
                KeyboardLayouts.SwitchForegroundTo(target);
            }
        }
        finally
        {
            ClipboardHelper.Restore(backup);
            _busy = false;
        }
    }
}
