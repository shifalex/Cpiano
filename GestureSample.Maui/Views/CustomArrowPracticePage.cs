using GestureSample.Maui.Handlers;

namespace GestureSample.Maui.Views;

public sealed class CustomArrowPracticePage : ContentPage
{
    private static readonly string[] Names = { "Keyboard arrow · variable length", "Keyboard arrow · fixed length", "Keyboard arrow · ordinal", "Arrow Puzzles · destination missing", "Arrow Puzzles · distance missing", "Arrow Puzzles · start missing" };
    private static readonly string[] IconKeys = { "First -> variable length", "First ->", "Ordinal With Key Numbers", "Arrow Label", "Arrow Label", "Arrow Label" };

    public CustomArrowPracticePage()
    {
        Guid? userId = LanguagePreferences.UserId;
        var saved = CustomArrowPracticePreferences.Load(userId);
        Title = AppLanguage.Text("Custom arrow practice");
        BackgroundColor = Color.FromArgb("#F7F8FC");
        FlowDirection = LanguagePreferences.Get() == InterfaceLanguage.Hebrew ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        var content = new VerticalStackLayout { Padding = 20, Spacing = 14, MaximumWidthRequest = 720 };
        Label Text(string text, double size = 15) => new() { Text = AppLanguage.Text(text), FontSize = size, TextColor = Color.FromArgb("#20232B") };
        Picker Pick(string[] values, int selected) => new() { ItemsSource = values.Select(value => AppLanguage.Text(value)).ToArray(), SelectedIndex = selected, TextColor = Color.FromArgb("#20232B") };
        content.Add(Text("Custom arrow practice", 26));
        content.Add(Text("Mix the questions you need, or repeat one exact question."));
        var mode = Pick(new[] { "Mix selected question types", "One specific question" }, saved.Specific ? 1 : 0);
        content.Add(mode);
        var checks = new Dictionary<CustomArrowQuestionKind, CheckBox>();
        var mixed = new VerticalStackLayout { Spacing = 6 };
        foreach (var kind in Enum.GetValues<CustomArrowQuestionKind>())
        {
            var check = new CheckBox { IsChecked = saved.Kinds.Contains(kind), Color = Color.FromArgb("#2D7373") };
            checks.Add(kind, check);
            var row = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(new GridLength(44)), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 8 };
            row.Add(check, 0);
            row.Add(new GraphicsView { Drawable = new GestureSample.Views.MainPage.ArrowMenuIconDrawable(IconKeys[(int)kind], Color.FromArgb("#2D7373")), WidthRequest = 44, HeightRequest = 44, InputTransparent = true }, 1);
            row.Add(new Label { Text = AppLanguage.Text(Names[(int)kind]), VerticalOptions = LayoutOptions.Center, TextColor = Color.FromArgb("#20232B"), FontSize = 16 }, 2);
            mixed.Add(new Border { Content = row, Padding = 6, BackgroundColor = Colors.White, Stroke = Color.FromArgb("#E0E8E8"), StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 } });
        }
        mixed.Add(Text("Maximum arrow length"));
        var length = Pick(Enumerable.Range(1, 10).Select(value => value.ToString()).ToArray(), saved.MaxLength - 1);
        mixed.Add(length);
        content.Add(mixed);
        var exact = new VerticalStackLayout { Spacing = 10 };
        exact.Add(Text("Question type"));
        int savedKind = (int)saved.Kinds[0];
        var type = Pick(new[] { "Variable length", "Fixed length", "Ordinal arrow", "Arrow Puzzles" }, Math.Min(savedKind, 3));
        exact.Add(type);
        var missingLabel = Text("What is missing?");
        var missing = Pick(new[] { "Destination", "Distance", "Starting point" }, Math.Max(0, savedKind - 3));
        exact.Add(missingLabel);
        exact.Add(missing);
        void Format() { missing.IsVisible = missingLabel.IsVisible = type.SelectedIndex == 3; }
        type.SelectedIndexChanged += (_, _) => Format();
        Format();
        exact.Add(Text("Starting key"));
        var start = new Entry { Text = saved.Start.ToString(), Keyboard = Keyboard.Numeric, TextColor = Color.FromArgb("#20232B") };
        exact.Add(start);
        exact.Add(Text("Destination key"));
        var end = new Entry { Text = saved.End.ToString(), Keyboard = Keyboard.Numeric, TextColor = Color.FromArgb("#20232B") };
        exact.Add(end);
        exact.Add(Text("Choose two different keys from 1 to 20."));
        exact.Add(Text("The destination may be to the left or right of the start."));
        exact.Add(Text("The keyboard always has ten keys. Numbers wrap around modulo ten."));
        var preview = Text("", 22);
        preview.FlowDirection = FlowDirection.LeftToRight;
        void Preview()
        {
            preview.Text = int.TryParse(start.Text, out int a) && int.TryParse(end.Text, out int b)
                && a is >= 1 and <= 20 && b is >= 1 and <= 20 && b != a
                ? new SpecificArrowQuestion(a, b).Equation : "";
        }
        start.TextChanged += (_, _) => Preview();
        end.TextChanged += (_, _) => Preview();
        Preview();
        exact.Add(preview);
        exact.Add(Text("This practice repeats the exact question you chose."));
        content.Add(exact);
        void Mode() { mixed.IsVisible = mode.SelectedIndex == 0; exact.IsVisible = mode.SelectedIndex == 1; }
        mode.SelectedIndexChanged += (_, _) => Mode();
        Mode();
        var error = new Label { TextColor = Colors.DarkRed, IsVisible = false };
        content.Add(error);
        var launch = new Button { Text = AppLanguage.Text("Start practice"), BackgroundColor = Color.FromArgb("#2D7373"), TextColor = Colors.White, CornerRadius = 14, MinimumHeightRequest = 52 };
        bool opening = false;
        launch.Clicked += async (_, _) =>
        {
            if (opening) return;
            var selection = new CustomArrowPracticeSelection
            {
                Specific = mode.SelectedIndex == 1,
                Kinds = mode.SelectedIndex == 1 ? new[] { (CustomArrowQuestionKind)(type.SelectedIndex == 3 ? 3 + missing.SelectedIndex : type.SelectedIndex) } : checks.Where(pair => pair.Value.IsChecked).Select(pair => pair.Key).ToArray(),
                Start = int.TryParse(start.Text, out int a) ? a : 0, End = int.TryParse(end.Text, out int b) ? b : 0,
                MaxLength = length.SelectedIndex + 1
            };
            string? message = selection.ValidationError();
            error.IsVisible = message != null;
            error.Text = message == null ? "" : AppLanguage.Text(message);
            if (message != null) return;
            opening = true;
            launch.IsEnabled = false;
            try
            {
                CustomArrowPracticePreferences.Save(userId, selection);
                var config = CreateConfig(selection);
                var options = ArrowPracticePreferences.Load(userId, config.GameName);
                ArrowPracticePreferences.Save(userId, config.GameName, options with
                {
                    MaxLength = null,
                    TenBoundary = !selection.Specific && selection.MaxLength == 1 && options.TenBoundary == 2 ? 0 : options.TenBoundary
                });
                var practice = ArrowPracticeOptionsPage.CreatePracticePage(config, page =>
                {
                    var edit = new ToolbarItem { Text = AppLanguage.Text("Edit practice") };
                    edit.Clicked += async (_, _) => await page.Navigation.PopAsync();
                    page.ToolbarItems.Add(edit);
                });
                await Navigation.PushAsync(practice, animated: false);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception);
                error.Text = AppLanguage.Text("Unable to start practice. Please try again.");
                error.IsVisible = true;
            }
            finally { opening = false; launch.IsEnabled = true; }
        };
        content.Add(launch);
        Content = new ScrollView { Content = content };
    }

    internal static GameConfig CreateConfig(CustomArrowPracticeSelection selection)
    {
        if (selection.ValidationError() is string error) throw new ArgumentException(error);
        bool ordinalOnly = selection.Kinds.All(kind => kind == CustomArrowQuestionKind.Ordinal);
        return new GameConfig
        {
            GameName = selection.Specific ? "Custom arrow - specific question" : "Custom arrow - mixed practice",
            UIQuestionType = UIQuestionType.OnlyKeyboard, OperationList = new() { Operation.Copy },
            MinAddend = 1, MaxAddend = 19, MaxAddend2 = 19, MinSum = 1, MaxSum = 20,
            KeyboardConfig = new KeyboardConfig
            {
                SyncType = SyncType.Sync, Rows = 1, KeysInRow = 10, WithoutZero = true,
                IsArrow = true, ArrowType = ordinalOnly ? ArrowType.Rounded : ArrowType.Straight,
                EnableSecondArrowLeftTrace = !ordinalOnly,
                ShowNumbersOnKeys = true, KeyLabelVerticalPosition = KeyLabelVerticalPosition.Top,
                ArrowLabelExerciseMode = ArrowLabelExerciseMode.StartAndLength,
                AllowedArrowPromptKinds = ArrowPromptKindFlags.OnKeyboard | ArrowPromptKindFlags.SpecialPrompt,
                CustomArrowQuestionKinds = selection.Kinds.Distinct().ToArray(),
                MaxArrowLabelDistance = selection.Specific ? Math.Abs(selection.End - selection.Start) : selection.MaxLength,
                SpecificArrowQuestion = selection.Specific ? new(selection.Start, selection.End) : null
            }
        };
    }
}
