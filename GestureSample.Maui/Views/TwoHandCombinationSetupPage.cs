using GestureSample.Maui;
using GestureSample.Views.Tests;
using Microsoft.Maui.Controls.Shapes;

namespace GestureSample.Views;

public sealed class TwoHandCombinationSetupPage : ContentPage
{
    private static readonly Color Accent = Color.FromArgb("#D46A24");
    private static readonly Color Soft = Color.FromArgb("#FFF2E8");
    private static readonly Color Ink = Color.FromArgb("#2D2530");
    private readonly Dictionary<TwoHandCombinationOptions, CheckBox> _choices = new();
    private readonly CheckBox _animate = new();
    private readonly CheckBox _vary = new();
    private readonly CheckBox _readAloud = new();
    private readonly CheckBox _askOnlyTarget = new();
    private readonly Picker _magnitudeVocabulary = new()
    {
        Title = "Magnitude wording",
        ItemsSource = new[] { "Intuitive", "By a little / by a lot", "By exact number" }
    };
    private readonly Slider _rows = new() { Minimum = 7, Maximum = 12 };
    private readonly Slider _seconds = new() { Minimum = 1, Maximum = 5 };
    private readonly Label _summary = new();
    private readonly Page? _exercisePage;

    public TwoHandCombinationSetupPage(KeyboardConfig? current = null, Page? exercisePage = null)
    {
        _exercisePage = exercisePage;
        current ??= new KeyboardConfig { TwoHandCombinationOptions = TwoHandCombinationOptions.Default,
            AnimateTwoHandCombinations = true, RandomizeTwoHandCombinationSizes = true,
            Rows = 8, PrecisionPinchMemorizeDelaySeconds = 2 };
        Title = "Stage 5.1 settings";
        BackgroundColor = Color.FromArgb("#FFF9F4");
        _animate.IsChecked = current.AnimateTwoHandCombinations;
        _vary.IsChecked = current.RandomizeTwoHandCombinationSizes;
        _readAloud.IsChecked = current.ReadTwoHandCombinationInstructionAloud;
        _askOnlyTarget.IsChecked = current.AskOnlyTwoHandCombinationTarget;
        _magnitudeVocabulary.SelectedIndex = (int)current.TwoHandMagnitudeVocabularyMode;
        _rows.Value = Math.Clamp(current.Rows, 7, 12);
        _seconds.Value = Math.Clamp(current.PrecisionPinchMemorizeDelaySeconds, 1, 5);

        VerticalStackLayout body = new() { Padding = new Thickness(18, 18, 18, 32), Spacing = 13 };
        body.Add(Hero());
        body.Add(Toolbar());
        AddGroup(body, "Transformations", current,
            (TwoHandCombinationOptions.Commutativity, "⇄", "Commutativity", "Synchronous exchange"),
            (TwoHandCombinationOptions.Associativity, "⌁", "Move shared boundary up/down", "Keep the whole; shift where the parts meet"),
            (TwoHandCombinationOptions.ResizeUpper, "↥", "Resize upper", "Change the upper hand"),
            (TwoHandCombinationOptions.ResizeLowerAttached, "↕", "Resize attached", "Keep the other hand connected"),
            (TwoHandCombinationOptions.IncreaseLowerByOne | TwoHandCombinationOptions.DecreaseLowerByOne,
                "±1↓", "Change lower by one", "5+2 ↔ 6+2"),
            (TwoHandCombinationOptions.IncreaseUpperByOne | TwoHandCombinationOptions.DecreaseUpperByOne,
                "±1↑", "Change upper by one", "5+2 ↔ 5+3"),
            (TwoHandCombinationOptions.FlipAdditionSubtraction, "±", "Large ± small", "Mirror across the boundary"),
            (TwoHandCombinationOptions.SubtrahendOneStepBigger | TwoHandCombinationOptions.SubtrahendOneStepSmaller,
                "−±1", "Change subtraction by one", "5−2 ↔ 5−3; 8−6 ↔ 8−7"),
            (TwoHandCombinationOptions.Difference, "−", "Attach small part to other edge", "Move the same part across the whole"));
        AddGroup(body, "Parts and halves", current,
            (TwoHandCombinationOptions.Split, "◐", "Complementary parts", "Keep the whole; change the part"),
            (TwoHandCombinationOptions.SplitJump, "⌇", "Split a jump", "One jump becomes two"),
            (TwoHandCombinationOptions.Half, "½", "One half, other half", "Full + one half → full + the other half"),
            (TwoHandCombinationOptions.MoreThanHalf | TwoHandCombinationOptions.LessThanHalf,
                "≈", "Around half", "Whole + half → one above or below half"),
            (TwoHandCombinationOptions.HalfOfHalf, "¼", "Half of half", "Continue from half to quarter"));
        AddGroup(body, "Relative size", current,
            (TwoHandCombinationOptions.LittleSmaller, "↘", "A little smaller", "One row"),
            (TwoHandCombinationOptions.MuchSmaller, "⇘", "Much smaller", "Several rows"),
            (TwoHandCombinationOptions.LittleBigger, "↗", "A little bigger", "One row"),
            (TwoHandCombinationOptions.MuchBigger, "⇗", "Much bigger", "Several rows"));
        body.Add(PracticeCard());

        Label warning = new() { Text = "Choose at least one exercise.", TextColor = Colors.Firebrick,
            FontAttributes = FontAttributes.Bold, IsVisible = false, HorizontalTextAlignment = TextAlignment.Center };
        Button save = new() { Text = exercisePage == null ? "Start Stage 5.1" : "Apply and restart Stage 5.1",
            HeightRequest = 56, CornerRadius = 18, BackgroundColor = Accent, TextColor = Colors.White,
            FontSize = 17, FontAttributes = FontAttributes.Bold };
        save.Clicked += async (_, _) => await SaveAsync(warning);
        body.Add(warning); body.Add(save);
        Content = new ScrollView { Content = body };
        UpdateSummary();
    }

    private static View Hero() => new Border
    {
        Padding = 20, BackgroundColor = Color.FromArgb("#3C2C3E"), StrokeThickness = 0,
        StrokeShape = new RoundRectangle { CornerRadius = 24 },
        Content = new VerticalStackLayout { Spacing = 5, Children =
        {
            new Label { Text = "⚙  CUSTOM PRACTICE", TextColor = Color.FromArgb("#FFC998"), FontSize = 12, FontAttributes = FontAttributes.Bold },
            new Label { Text = "What do you want to work on?", TextColor = Colors.White, FontSize = 25, FontAttributes = FontAttributes.Bold },
            new Label { Text = "Mix any exercises. Reopen this from the smiley row.", TextColor = Colors.White.WithAlpha(.78f) }
        }}
    };

    private View Toolbar()
    {
        _summary.TextColor = Ink.WithAlpha(.7f);
        Button all = MiniButton("Select all"), clear = MiniButton("Clear");
        all.Clicked += (_, _) => SetAll(true); clear.Clicked += (_, _) => SetAll(false);
        Grid g = new() { ColumnDefinitions = { new(GridLength.Star), new(GridLength.Auto), new(GridLength.Auto) }, ColumnSpacing = 7 };
        g.Add(_summary, 0, 0); g.Add(all, 1, 0); g.Add(clear, 2, 0); return g;
    }

    private void AddGroup(VerticalStackLayout body, string title, KeyboardConfig current,
        params (TwoHandCombinationOptions Option, string Icon, string Title, string Detail)[] items)
    {
        body.Add(new Label { Text = title, FontSize = 19, FontAttributes = FontAttributes.Bold,
            TextColor = Ink, Margin = new Thickness(2, 9, 0, 1) });
        foreach (var item in items) body.Add(ChoiceCard(item, current.TwoHandCombinationOptions.HasFlag(item.Option)));
    }

    private View ChoiceCard((TwoHandCombinationOptions Option, string Icon, string Title, string Detail) item, bool selected)
    {
        CheckBox check = new() { IsChecked = selected, Color = Accent, VerticalOptions = LayoutOptions.Center };
        _choices[item.Option] = check;
        Border card = new() { Padding = new Thickness(14, 10), BackgroundColor = selected ? Soft : Colors.White,
            Stroke = selected ? Accent.WithAlpha(.45f) : Color.FromArgb("#E9E1DC"),
            StrokeShape = new RoundRectangle { CornerRadius = 17 } };
        Grid row = new() { ColumnDefinitions = { new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto) }, ColumnSpacing = 10 };
        row.Add(new Label { Text = item.Icon, FontSize = 25, TextColor = Accent, WidthRequest = 36,
            VerticalTextAlignment = TextAlignment.Center }, 0, 0);
        row.Add(new VerticalStackLayout { Spacing = 1, Children =
        {
            new Label { Text = item.Title, FontAttributes = FontAttributes.Bold, TextColor = Ink },
            new Label { Text = item.Detail, FontSize = 12, TextColor = Ink.WithAlpha(.62f) }
        }}, 1, 0);
        row.Add(check, 2, 0); card.Content = row;
        check.CheckedChanged += (_, e) => { card.BackgroundColor = e.Value ? Soft : Colors.White;
            card.Stroke = e.Value ? Accent.WithAlpha(.45f) : Color.FromArgb("#E9E1DC"); UpdateSummary(); };
        TapGestureRecognizer tap = new(); tap.Tapped += (_, _) => check.IsChecked = !check.IsChecked;
        card.GestureRecognizers.Add(tap); return card;
    }

    private View PracticeCard()
    {
        VerticalStackLayout v = new() { Spacing = 12 };
        v.Add(new Label { Text = "Practice feel", FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Ink });
        v.Add(ToggleRow("Quick movement animations", "Normal question color, no tutorial pause", _animate));
        v.Add(ToggleRow("Vary interval sizes", "Keep the bottom anchor while changing proportions", _vary));
        v.Add(ToggleRow("Read instruction aloud", "Speak the transformation shown above the keyboard", _readAloud));
        v.Add(ToggleRow("Ask only for the target", "Keep the initial state visible while answering", _askOnlyTarget));
        v.Add(new VerticalStackLayout { Spacing = 3, Children =
        {
            new Label { Text = "Magnitude vocabulary", FontAttributes = FontAttributes.Bold, TextColor = Ink },
            _magnitudeVocabulary
        }});
        v.Add(SliderRow("Number-line height", _rows, x => $"{x:0} rows"));
        v.Add(SliderRow("Memorize each position", _seconds, x => $"{x:0} sec"));
        return new Border { Margin = new Thickness(0, 9, 0, 0), Padding = 17, BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#E9E1DC"), StrokeShape = new RoundRectangle { CornerRadius = 20 }, Content = v };
    }

    private async Task SaveAsync(Label warning)
    {
        TwoHandCombinationOptions selected = _choices.Where(x => x.Value.IsChecked)
            .Aggregate(TwoHandCombinationOptions.None, (value, x) => value | x.Key);
        if (selected == TwoHandCombinationOptions.None) { warning.IsVisible = true; return; }
        GameConfig config = MainPage.CreateTwoHandCombinationMemorizeConfig(selected, _animate.IsChecked,
            _vary.IsChecked, (int)Math.Round(_rows.Value), (int)Math.Round(_seconds.Value),
            _readAloud.IsChecked, _askOnlyTarget.IsChecked,
            (TwoHandMagnitudeVocabularyMode)Math.Max(0, _magnitudeVocabulary.SelectedIndex));
        SimpleViewCellsPage replacement = new(config);
        if (_exercisePage != null && Navigation.NavigationStack.Contains(_exercisePage))
        {
            Navigation.InsertPageBefore(replacement, this); Navigation.RemovePage(_exercisePage); await Navigation.PopAsync();
        }
        else await Navigation.PushAsync(replacement);
    }

    private void SetAll(bool value) { foreach (CheckBox c in _choices.Values) c.IsChecked = value; UpdateSummary(); }
    private void UpdateSummary() { int n = _choices.Values.Count(x => x.IsChecked);
        _summary.Text = n == 0 ? "No exercises selected" : $"{n} exercise{(n == 1 ? "" : "s")} selected"; }
    private static Button MiniButton(string text) => new() { Text = text, FontSize = 12,
        Padding = new Thickness(11, 5), CornerRadius = 13, BackgroundColor = Soft, TextColor = Accent };

    private static View ToggleRow(string title, string detail, CheckBox check)
    {
        Grid g = new() { ColumnDefinitions = { new(GridLength.Star), new(GridLength.Auto) } };
        g.Add(new VerticalStackLayout { Spacing = 1, Children = { new Label { Text = title, FontAttributes = FontAttributes.Bold, TextColor = Ink },
            new Label { Text = detail, FontSize = 11, TextColor = Ink.WithAlpha(.6f) } } }, 0, 0); g.Add(check, 1, 0); return g;
    }

    private static View SliderRow(string title, Slider slider, Func<double, string> format)
    {
        slider.MinimumTrackColor = Accent; slider.ThumbColor = Accent;
        Label value = new() { Text = format(slider.Value), TextColor = Accent, FontAttributes = FontAttributes.Bold };
        slider.ValueChanged += (_, e) => value.Text = format(Math.Round(e.NewValue));
        Grid h = new() { ColumnDefinitions = { new(GridLength.Star), new(GridLength.Auto) } };
        h.Add(new Label { Text = title, FontAttributes = FontAttributes.Bold, TextColor = Ink }, 0, 0); h.Add(value, 1, 0);
        return new VerticalStackLayout { Spacing = 1, Children = { h, slider } };
    }
}
