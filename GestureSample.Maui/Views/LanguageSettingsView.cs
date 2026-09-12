using GestureSample.Maui.Handlers;

namespace GestureSample.Maui.Views;

public sealed class LanguageSettingsView : VerticalStackLayout
{
    private readonly bool _gripping;
    private bool _loading;
    private readonly Picker _text = new() { AutomationId = "TextLanguagePicker" };
    private readonly Picker _voice = new() { AutomationId = "NarrationLanguagePicker" };
    private readonly Label _heading = new() { FontSize = 20, FontAttributes = FontAttributes.Bold };
    private readonly Label _textLabel = new();
    private readonly Label _voiceLabel = new();
    private readonly Label _voiceStatus = new() { FontSize = 12 };
    private readonly Button _preview = new();
    public event EventHandler? LanguageChanged;

    public LanguageSettingsView(bool gripping = false)
    {
        _gripping = gripping;
        Spacing = 6;
        Children.Add(_heading);
        Children.Add(_textLabel);
        Children.Add(_text);
        Children.Add(_voiceLabel);
        Children.Add(_voice);
        Children.Add(_preview);
        Children.Add(_voiceStatus);
        _text.SelectedIndexChanged += (_, _) => Save(_text, false);
        _voice.SelectedIndexChanged += (_, _) => Save(_voice, true);
        _preview.Clicked += async (_, _) =>
        {
            _preview.IsEnabled = false;
            try
            {
                bool spoken = await AppLanguage.SpeakAsync("Narration preview", _gripping);
                _voiceStatus.Text = spoken ? "" : T("No voice available for this language. Install a voice in your device settings.");
            }
            catch { _voiceStatus.Text = T("Narration is unavailable on this device."); }
            finally { _preview.IsEnabled = true; }
        };
        Reload();
    }

    private string T(string text) => AppLanguage.Text(text, _gripping);

    public void Reload()
    {
        _loading = true;
        FlowDirection = LanguagePreferences.Get(gripping: _gripping) == InterfaceLanguage.Hebrew
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        _heading.Text = T("Language");
        _textLabel.Text = T("Text language");
        _voiceLabel.Text = T("Narration language");
        _preview.Text = T("Preview narration");
        _voiceStatus.Text = "";
        string[] languages = { "English", "עברית", "Русский" };
        foreach (var (picker, narration) in new[] { (_text, false), (_voice, true) })
        {
            picker.ItemsSource = _gripping ? new[] { T("Use user settings") }.Concat(languages).ToArray() : languages;
            picker.SelectedIndex = _gripping && !LanguagePreferences.HasOverride(narration)
                ? 0 : (int)LanguagePreferences.Get(narration, _gripping) + (_gripping ? 1 : 0);
        }
        _loading = false;
    }

    private void Save(Picker picker, bool narration)
    {
        if (_loading || picker.SelectedIndex < 0) return;
        int index = picker.SelectedIndex - (_gripping ? 1 : 0);
        LanguagePreferences.Set(index < 0 ? null : (InterfaceLanguage)index, narration, _gripping);
        Reload();
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }
}
