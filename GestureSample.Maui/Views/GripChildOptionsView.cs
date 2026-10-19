using GestureSample.Maui.Handlers;
namespace GestureSample.Maui.Views;
public sealed class GripChildOptionsView : VerticalStackLayout
{
    private static readonly Color Accent = Color.FromArgb("#D46A24");
    private static readonly Color Soft = Color.FromArgb("#FFF2E8");
    private static readonly Color Ink = Color.FromArgb("#2D2530");
    public GripChildOptionsView() => Reload();
    public void Reload()
    {
        Children.Clear();
        Spacing = 13;
        Children.Add(new Border
        {
            Padding = 20, BackgroundColor = Color.FromArgb("#3C2C3E"), StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 24 },
            Content = new VerticalStackLayout { Spacing = 8, Children =
            {
                new Label { Text = T("Options for young children"), FontSize = 25,
                    FontAttributes = FontAttributes.Bold, TextColor = Colors.White },
                new Label { Text = T("For Copy a grip, Remember a grip and Remember two grips. Changes apply to the next exercise."),
                    FontSize = 13, TextColor = Colors.White.WithAlpha(.78f) }
            }}
        });
        Add("Show our grip hand when tapping ?", "GripChildHandTutorial", GripChildPreferences.HandTutorial, v => GripChildPreferences.HandTutorial = v);
        Add("Always start at the bottom", "GripChildBottom", GripChildPreferences.StartAtBottom, v => GripChildPreferences.StartAtBottom = v);
        Add("Color pressed keys: green for correct, red for retry", "GripChildColors", GripChildPreferences.ColorFeedback, v => GripChildPreferences.ColorFeedback = v);
        Add("Start answering immediately with a faint grip guide", "GripChildImmediate", GripChildPreferences.StartImmediately, v => GripChildPreferences.StartImmediately = v);
        Add("Grip only: skip watch-then-click", "GripChildGripOnly", GripChildPreferences.GripOnly, v => GripChildPreferences.GripOnly = v);
        Add("Hold the correct grip to answer", "GripChildRequireHold", GripChildPreferences.RequireHold, v => GripChildPreferences.RequireHold = v);
    }
    private static string T(string text) => AppLanguage.Text(text, true);
    private void Add(string text, string id, bool value, Action<bool> save)
    {
        var toggle = new Switch { IsToggled = value, AutomationId = id,
            OnColor = Accent, VerticalOptions = LayoutOptions.Center };
        var card = new Border
        {
            Padding = new Thickness(16, 12), BackgroundColor = value ? Soft : Colors.White,
            Stroke = value ? Accent.WithAlpha(.45f) : Color.FromArgb("#E9E1DC"),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 17 }
        };
        toggle.Toggled += (_, e) =>
        {
            save(e.Value);
            card.BackgroundColor = e.Value ? Soft : Colors.White;
            card.Stroke = e.Value ? Accent.WithAlpha(.45f) : Color.FromArgb("#E9E1DC");
        };
        var row = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 12 };
        var label = new Label { Text = T(text), TextColor = Ink, FontSize = 16,
            FontAttributes = FontAttributes.Bold, VerticalOptions = LayoutOptions.Center };
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => toggle.IsToggled = !toggle.IsToggled;
        label.GestureRecognizers.Add(tap);
        row.Add(label);
        row.Add(toggle, 1);
        card.Content = row;
        Children.Add(card);
    }
}
