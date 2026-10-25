using GestureSample.Maui.Handlers;
using GestureSample.Views.Tests;

namespace GestureSample.Maui.Views;

public static class ArrowLabelPathPage
{
    private static readonly string[] Captions = { "Start and distance", "Find the distance", "Find the starting point", "Everything mixed" };
    private static readonly string[] Icons = { "↦", "↔?", "?→", "⤨" };

    public static Page CreatePractice() => CreateStage(ArrowPracticePreferences.LoadLabelStep(LanguagePreferences.UserId), LanguagePreferences.UserId);

    private static Page CreateStage(int step, Guid? userId)
    {
        var progression = new ArrowLabelProgression(step);
        return ArrowPracticeOptionsPage.CreatePracticePage(CreateStageConfig(step), page =>
        {
            if (page is not SimpleViewCellsPage exercise || exercise.Content is not Grid grid) return;
            // Match the existing stage-side settings control without changing the keyboard layout.
            var gear = grid.Children.OfType<Button>().FirstOrDefault(button => button.AutomationId == "ArrowPracticeSettingsButton");
            if (gear != null) gear.Margin = new Thickness(0, 12, 12, 0);
            var selector = new Button
            {
                FontSize = 17, WidthRequest = 136, HeightRequest = 44, Padding = new Thickness(8, 0), CornerRadius = 14,
                BackgroundColor = Colors.White, TextColor = Color.FromArgb("#2D7373"),
                BorderColor = Color.FromArgb("#BCD5D5"), BorderWidth = 1,
                HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, 12, 68, 0), ZIndex = 1005, AutomationId = "ArrowLabelProgressionDropdown"
            };
            void RefreshProgress()
            {
                selector.Text = step < 3 ? $"{Icons[step]} {step + 1} · {progression.ConsecutiveSuccesses}/4 ▾" : $"{Icons[step]} ▾";
                SemanticProperties.SetDescription(selector, AppLanguage.Text(Captions[step]) + ". " + AppLanguage.Text("Four consecutive successes advance to the next step."));
            }
            RefreshProgress();
            var shield = new BoxView { Color = Colors.Transparent, IsVisible = false, ZIndex = 1002 };
            var choices = new VerticalStackLayout { Spacing = 4 };
            var dropdown = new Border
            {
                Content = choices, IsVisible = false, WidthRequest = 260, Padding = 8,
                BackgroundColor = Colors.White, Stroke = Color.FromArgb("#BCD5D5"),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 },
                HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, 60, 12, 0), ZIndex = 1004, AutomationId = "ArrowLabelStepChoices"
            };
            void Close() { dropdown.IsVisible = false; shield.IsVisible = false; }
            bool moving = false;
            async Task MoveToAsync(int next)
            {
                if (moving) return;
                moving = true;
                Close();
                try
                {
                    var navigation = page.Navigation;
                    var replacement = CreateStage(next, userId);
                    navigation.InsertPageBefore(replacement, page);
                    await navigation.PopAsync(animated: false);
                    ArrowPracticePreferences.SaveLabelStep(userId, next);
                }
                finally { moving = false; }
            }
            for (int i = 0; i < Captions.Length; i++)
            {
                int target = i;
                var choice = new Button
                {
                    Text = $"{Icons[i]}  {AppLanguage.Text(Captions[i])}", FontSize = 14,
                    FontAttributes = i == step ? FontAttributes.Bold : FontAttributes.None,
                    BackgroundColor = Color.FromArgb(i == step ? "#E8F2F2" : "#FFFFFF"),
                    TextColor = Color.FromArgb("#2D7373"), CornerRadius = 10, MinimumHeightRequest = 44
                };
                choice.Clicked += async (_, _) => { Close(); if (target != step) await MoveToAsync(target); };
                choices.Add(choice);
            }
            if (step < 3)
            {
                var next = new Button { Text = "⏭ " + AppLanguage.Text("Next step"), BackgroundColor = Color.FromArgb("#2D7373"), TextColor = Colors.White, CornerRadius = 10 };
                next.Clicked += async (_, _) => await MoveToAsync(step + 1);
                choices.Add(next);
            }
            choices.Add(new Label { Text = AppLanguage.Text("Four consecutive successes advance to the next step."), FontSize = 12, TextColor = Color.FromArgb("#6F7280"), Margin = 4 });
            selector.Clicked += (_, _) => { dropdown.IsVisible = !dropdown.IsVisible; shield.IsVisible = dropdown.IsVisible; };
            var dismiss = new TapGestureRecognizer();
            dismiss.Tapped += (_, _) => Close();
            shield.GestureRecognizers.Add(dismiss);
            foreach (View overlay in new View[] { shield, dropdown, selector })
            {
                grid.Add(overlay);
                Grid.SetRowSpan(overlay, Math.Max(1, grid.RowDefinitions.Count));
                Grid.SetColumnSpan(overlay, Math.Max(1, grid.ColumnDefinitions.Count));
            }
            exercise.PracticeAttemptCompletedAsync = async correct =>
            {
                if (moving || page.Navigation.NavigationStack.LastOrDefault() != page) return false;
                bool advance = progression.RecordAttempt(correct);
                RefreshProgress();
                if (!advance) return false;
                await MoveToAsync(progression.Step);
                return true;
            };
        });
    }

    // The three existing stages keep their game IDs, defaults, and settings keys.
    internal static GameConfig CreateStageConfig(int step)
    {
        if (step is < 0 or > 3) throw new ArgumentOutOfRangeException(nameof(step));
        var config = new GameConfig
        {
            GameName = step switch
            {
                0 => "Arrow Label - distance",
                1 => "Arrow Label - missing distance",
                2 => "Arrow Label - missing start",
                _ => "Arrow Label - mixed practice"
            },
            UIQuestionType = UIQuestionType.OnlyKeyboard,
            OperationList = new() { Operation.Copy },
            MinSum = 1, MaxSum = 10, NumberOfTasksToWin = -1, NumberOfMistakesToLose = -1,
            KeyboardConfig = new KeyboardConfig
            {
                SyncType = SyncType.Sync, KeysInRow = 10, WithoutZero = true, ShowNumbersOnKeys = true,
                SecondsPressingToAnswer = 2, MaxArrowLabelDistance = 4,
                EnableSecondArrowLeftTrace = true,
                KeyLabelVerticalPosition = KeyLabelVerticalPosition.Top,
                ArrowLabelExerciseMode = step switch
                {
                    1 => ArrowLabelExerciseMode.StartAndEndWithMissingLength,
                    2 => ArrowLabelExerciseMode.EndAndLengthWithMissingStart,
                    _ => ArrowLabelExerciseMode.StartAndLength
                }
            }
        };
        if (step is 1 or 3)
        {
            config.KeyboardConfig.AllowedArrowPromptKinds = ArrowPromptKindFlags.SpecialPrompt;
            config.KeyboardConfig.AllowedArrowRouteKinds = ArrowRouteKindFlags.Cardinal;
            config.KeyboardConfig.SpecialArrowMissingTargets = step == 1 ? MissingValueTargetFlags.Addend2
                : MissingValueTargetFlags.Sum | MissingValueTargetFlags.Addend2 | MissingValueTargetFlags.Addend1;
        }
        return config;
    }
}
