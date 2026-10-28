using GestureSample.Maui.Handlers;
using GestureSample.Views.Tests;

namespace GestureSample.Maui.Views;

// Presentation only: stage definitions and gameplay continue to use the existing config.
public sealed class ArrowPracticeOptionsPage : ContentPage
{
    private bool _starting;
    private Button? _applyButton;

    private async Task ShowAppliedAsync()
    {
        if (_applyButton == null) return;
        _applyButton.Text = "✓";
        _applyButton.BackgroundColor = Color.FromArgb("#27834A");
        await Task.Delay(350);
    }

    public static Page CreatePracticePage(GameConfig stage, Action<Page>? configurePage = null) =>
        CreatePracticePage(stage, LanguagePreferences.UserId, configurePage);

    private static Page CreatePracticePage(GameConfig stage, Guid? userId, Action<Page>? configurePage)
    {
        string defaults = GameConfigJson.Serialize(stage);
        var config = ApplySavedOptions(GameConfigJson.Deserialize(defaults), ArrowPracticePreferences.Load(userId, stage.GameName));
        var page = new SimpleViewCellsPage(config);
        var gear = new Button
        {
            Text = "⚙\uFE0E", FontSize = 22, CharacterSpacing = 0, WidthRequest = 44, HeightRequest = 44,
            Padding = 0, CornerRadius = 22, BackgroundColor = Colors.White,
            TextColor = Color.FromArgb("#2D2530"), BorderColor = Color.FromArgb("#E9E1DC"), BorderWidth = 1,
            HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(0, 12, 12, 0), ZIndex = 1001, AutomationId = "ArrowPracticeSettingsButton"
        };
        SemanticProperties.SetDescription(gear, AppLanguage.Text("Arrow practice options"));
        gear.Clicked += async (_, _) =>
        {
            if (!gear.IsEnabled) return;
            gear.IsEnabled = false;
            try
            {
                ArrowPracticeOptionsPage settings = null!;
                settings = new ArrowPracticeOptionsPage(GameConfigJson.Deserialize(defaults), userId, async () =>
                {
                    var replacement = CreatePracticePage(GameConfigJson.Deserialize(defaults), userId, configurePage);
                    // Use the still-attached settings page's navigation after removing the old exercise.
                    var navigation = settings.Navigation;
                    navigation.InsertPageBefore(replacement, page);
                    navigation.RemovePage(page);
                    await settings.ShowAppliedAsync();
                    await navigation.PopAsync(animated: false);
                });
                await page.Navigation.PushAsync(settings);
            }
            finally { gear.IsEnabled = true; }
        };
        if (page.Content is Grid grid)
        {
            grid.Add(gear);
            Grid.SetRowSpan(gear, Math.Max(1, grid.RowDefinitions.Count));
            Grid.SetColumnSpan(gear, Math.Max(1, grid.ColumnDefinitions.Count));
        }
        else
        {
            View original = page.Content;
            page.Content = null;
            var overlay = new Grid();
            overlay.Add(original);
            overlay.Add(gear);
            page.Content = overlay;
        }
        configurePage?.Invoke(page);
        return page;
    }

    private static GameConfig ApplySavedOptions(GameConfig config, ArrowPracticeSelection saved)
    {
        KeyboardConfig keyboard = config.KeyboardConfig;
        if (saved.Sound.HasValue && saved.Sound.Value != (keyboard.IsVoice || keyboard.IsVoices))
        {
            keyboard.IsVoice = saved.Sound.Value;
            keyboard.IsVoices = false;
        }
        if (saved.Reading.HasValue) keyboard.IsNumberVoice = saved.Reading.Value;
        if (saved.KeyNumbers.HasValue) keyboard.ShowNumbersOnKeys = saved.KeyNumbers.Value;
        if (saved.MaxLength.HasValue && keyboard.SpecificArrowQuestion == null)
            keyboard.MaxArrowLabelDistance = Math.Clamp(saved.MaxLength.Value, 0,
                Math.Max(Math.Max(2, keyboard.Rows * keyboard.KeysInRow) - 1, keyboard.MaxArrowLabelDistance));
        keyboard.EnableSecondArrowLeftTrace = SupportsAccumulatingColor(keyboard)
            && (saved.AccumulatingColor ?? keyboard.EnableSecondArrowLeftTrace);
        if (keyboard.SpecificArrowQuestion != null) return config;
        int positionIndex = keyboard.ImposeEdges ? 2 : config.QuestionOrder switch
        {
            QuestionOrder.CyclicalLeft or QuestionOrder.CyclicalRight or QuestionOrder.CyclicalMixed => 1,
            QuestionOrder.Random => 3,
            _ => 0
        };
        if (saved.StartingPosition.HasValue && saved.StartingPosition != positionIndex && saved.StartingPosition != 0)
        {
            keyboard.ImposeEdges = saved.StartingPosition == 2;
            config.QuestionOrder = saved.StartingPosition == 1 ? QuestionOrder.CyclicalMixed : QuestionOrder.Random;
        }
        int boundaryIndex = config.TenBoundaryModes.HasFlag(TenBoundaryMode.ThroughTen) ? 2
            : config.TenBoundaryModes.HasFlag(TenBoundaryMode.ToTen) ? 1 : 0;
        if (saved.TenBoundary.HasValue && saved.TenBoundary != boundaryIndex)
        {
            config.TenBoundaryModes = (TenBoundaryMode)saved.TenBoundary.Value;
            if (config.TenBoundaryModes == TenBoundaryMode.ThroughTen)
            {
                config.MaxSum = Math.Max(20, config.MaxSum);
                config.MaxAddend = Math.Max(9, config.MaxAddend);
            }
        }
        return config;
    }

    private static bool SupportsAccumulatingColor(KeyboardConfig keyboard) =>
        (keyboard.CustomArrowQuestionKinds is { Length: > 0 } kinds
            ? kinds.Any(kind => kind != CustomArrowQuestionKind.Ordinal)
            : keyboard.ArrowType != ArrowType.Rounded && keyboard.ArrowLabelExerciseMode != ArrowLabelExerciseMode.OrdinalStartAndLength);

    private static bool SupportsKeyboardArrows(KeyboardConfig keyboard) =>
        keyboard.CustomArrowQuestionKinds is { Length: > 0 } kinds
            ? kinds.Any(kind => kind is CustomArrowQuestionKind.VariableLength or CustomArrowQuestionKind.FixedLength or CustomArrowQuestionKind.Ordinal)
            : keyboard.IsArrow;

    private ArrowPracticeOptionsPage(GameConfig stage, Guid? userId, Func<Task> apply)
    {
        Title = "Arrow practice options";
        BackgroundColor = Color.FromArgb("#F7F8FC");
        var keyboard = stage.KeyboardConfig;
        var saved = ArrowPracticePreferences.Load(userId, stage.GameName);
        bool onKeyboard = SupportsKeyboardArrows(keyboard);
        bool accumulatingAvailable = SupportsAccumulatingColor(keyboard);
        bool exactQuestion = keyboard.SpecificArrowQuestion != null;
        FlowDirection = LanguagePreferences.Get() == InterfaceLanguage.Hebrew ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        var content = new VerticalStackLayout { Spacing = 14, Padding = 20, MaximumWidthRequest = 720 };
        content.Add(new StageNavigationSettingsView(stage.GameName));
        string stageCaption = keyboard.CustomArrowQuestionKinds?.Length > 0 ? "Custom arrow practice"
            : !onKeyboard ? "Arrow Label" : keyboard.ArrowType == ArrowType.Rounded ? "Ordinal with key numbers"
            : keyboard.IsArrowLengthDynamic == true ? "Variable length" : "Fixed length";
        content.Add(new Label { Text = stageCaption, FontSize = 26, FontFamily = "OpenSansSemibold", TextColor = Color.FromArgb("#20232B") });
        content.Add(new Label { Text = "Your options are remembered for each stage on this device.", TextColor = Color.FromArgb("#6F7280") });

        Picker Choice(string title, string[] values, int selected)
        {
            content.Add(new Label { Text = title, FontFamily = "OpenSansSemibold", TextColor = Color.FromArgb("#20232B") });
            var picker = new Picker { Title = AppLanguage.Text(title), ItemsSource = values.Select(value => AppLanguage.Text(value)).ToArray(), SelectedIndex = selected, TextColor = Color.FromArgb("#20232B") };
            content.Add(new Border { Content = picker, Padding = new Thickness(12, 2), BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#DCE7E7"), StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 } });
            return picker;
        }
        Switch Toggle(string title, bool value, bool visible = true)
        {
            var control = new Switch { IsToggled = value, OnColor = Color.FromArgb("#2D7373") };
            var row = new Grid { ColumnSpacing = 12, ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
            row.Add(new Label { Text = title, VerticalOptions = LayoutOptions.Center, TextColor = Color.FromArgb("#20232B") }, 0);
            row.Add(control, 1);
            content.Add(new Border { Content = row, IsVisible = visible, Padding = new Thickness(12, 6), BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#DCE7E7"), StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 } });
            return control;
        }

        void Section(string icon, string heading)
        {
            var row = new HorizontalStackLayout { Spacing = 10, Margin = new Thickness(0, 8, 0, 0) };
            row.Add(new Border { WidthRequest = 36, HeightRequest = 36, StrokeThickness = 0, BackgroundColor = Color.FromArgb("#E8F2F2"),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                Content = new Label { Text = icon, FontSize = 20, TextColor = Color.FromArgb("#2D7373"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center } });
            row.Add(new Label { Text = heading, FontSize = 20, FontFamily = "OpenSansSemibold", TextColor = Color.FromArgb("#2D7373"), VerticalOptions = LayoutOptions.Center });
            content.Add(row);
        }

        Section("♪", "Listening");
        int defaultSound = (keyboard.IsVoice || keyboard.IsVoices ? 1 : 0) + (keyboard.IsNumberVoice ? 2 : 0);
        int savedSound = ((saved.Sound ?? (keyboard.IsVoice || keyboard.IsVoices)) ? 1 : 0)
            + ((saved.Reading ?? keyboard.IsNumberVoice) ? 2 : 0);
        var sound = Choice("What would you like to hear?", new[] { "Quiet", "Key sounds", "Spoken numbers", "Sounds and numbers" }, savedSound);
        Section("▤", "Keyboard helpers");
        var accumulating = Toggle("Color trail", saved.AccumulatingColor ?? keyboard.EnableSecondArrowLeftTrace, accumulatingAvailable);
        var numbers = Toggle("Show key numbers", saved.KeyNumbers ?? keyboard.ShowNumbersOnKeys);
        if (!accumulatingAvailable)
            content.Add(new Label { Text = "Color trail is turned off for ordinal arrows.", FontSize = 12, TextColor = Color.FromArgb("#6F7280") });
        Section("↔", "Arrow movement");
        int keyCount = Math.Max(2, keyboard.Rows * keyboard.KeysInRow);
        int maximum = Math.Max(keyCount - 1, keyboard.MaxArrowLabelDistance);
        var length = Choice("How far can the arrow move?", new[] { "Full keyboard" }.Concat(Enumerable.Range(1, maximum).Select(value => value.ToString())).ToArray(),
            Math.Clamp(exactQuestion ? keyboard.MaxArrowLabelDistance : saved.MaxLength ?? keyboard.MaxArrowLabelDistance, 0, maximum));
        length.IsEnabled = !exactQuestion;
        int positionIndex = keyboard.ImposeEdges ? 2 : stage.QuestionOrder switch
        {
            QuestionOrder.CyclicalLeft or QuestionOrder.CyclicalRight or QuestionOrder.CyclicalMixed => 1,
            QuestionOrder.Random => 3,
            _ => 0
        };
        var position = Choice("Where should the next arrow start?", new[] { "Stage default", "Continue from previous answer", "From an edge", "Random start" }, saved.StartingPosition ?? positionIndex);
        position.IsEnabled = !exactQuestion;
        int boundaryIndex = stage.TenBoundaryModes.HasFlag(TenBoundaryMode.ThroughTen) ? 2
            : stage.TenBoundaryModes.HasFlag(TenBoundaryMode.ToTen) ? 1 : 0;
        var boundary = Choice("Crossing ten", new[] { "Any movement", "Stay within ten", "Cross ten" }, saved.TenBoundary ?? boundaryIndex);
        boundary.IsEnabled = !exactQuestion;
        if (exactQuestion)
            content.Add(new Label { Text = "This practice repeats the exact question you chose.", FontSize = 12, TextColor = Color.FromArgb("#6F7280") });

        bool resetting = false;
        void SaveSelection()
        {
            if (resetting) return;
            ArrowPracticePreferences.Save(userId, stage.GameName, new ArrowPracticeSelection
            {
            Sound = sound.SelectedIndex is 1 or 3,
            Reading = sound.SelectedIndex is 2 or 3,
            AccumulatingColor = accumulatingAvailable ? accumulating.IsToggled : null,
            KeyNumbers = numbers.IsToggled,
            MaxLength = length.SelectedIndex,
            StartingPosition = position.SelectedIndex,
            TenBoundary = boundary.SelectedIndex
            });
        }
        // Subscribe after initialization so merely opening a stage does not overwrite preferences.
        foreach (var toggle in new[] { accumulating, numbers })
            toggle.Toggled += (_, _) => SaveSelection();
        foreach (var picker in new[] { sound, length, position, boundary })
            picker.SelectedIndexChanged += (_, _) => SaveSelection();

        var reset = new Button { Text = "Reset to defaults", BackgroundColor = Color.FromArgb("#E8F2F2"), TextColor = Color.FromArgb("#2D7373"), CornerRadius = 14 };
        reset.Clicked += (_, _) =>
        {
            resetting = true;
            try
            {
                ArrowPracticePreferences.Reset(userId, stage.GameName);
                sound.SelectedIndex = defaultSound;
                accumulating.IsToggled = keyboard.EnableSecondArrowLeftTrace;
                numbers.IsToggled = keyboard.ShowNumbersOnKeys;
                length.SelectedIndex = Math.Clamp(keyboard.MaxArrowLabelDistance, 0, maximum);
                position.SelectedIndex = positionIndex;
                boundary.SelectedIndex = boundaryIndex;
            }
            finally { resetting = false; }
        };
        content.Add(reset);
        content.Add(new Label { Text = "Apply settings to restart the current practice.", FontSize = 12, TextColor = Color.FromArgb("#6F7280") });

        var error = new Label { IsVisible = false, TextColor = Colors.DarkRed };
        content.Add(error);
        var start = new Button { Text = "Apply and restart practice", BackgroundColor = Color.FromArgb("#2D7373"), TextColor = Colors.White, CornerRadius = 14, HeightRequest = 54 };
        _applyButton = start;
        start.Clicked += async (_, _) =>
        {
            if (_starting) return;
            _starting = true;
            start.IsEnabled = false;
            error.IsVisible = false;
            try
            {
                if (!exactQuestion && boundary.SelectedIndex == 2 && length.SelectedIndex == 1)
                {
                    error.Text = AppLanguage.Text("Crossing ten needs a maximum length of at least two.");
                    error.IsVisible = true;
                    return;
                }
                await apply();
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception);
                start.Text = AppLanguage.Text("Apply and restart practice");
                start.BackgroundColor = Color.FromArgb("#2D7373");
                error.Text = AppLanguage.Text("Unable to start practice. Please try again.");
                error.IsVisible = true;
            }
            finally
            {
                _starting = false;
                start.IsEnabled = true;
            }
        };
        content.Add(start);
        Content = new ScrollView { Content = content };
        AppLanguage.LocalizeSettings(this, gripping: false);
    }
}
