using GestureSample.Maui.Handlers;
using GestureSample.Views.Tests;

namespace GestureSample.Maui.Views;

public static class OrAndXorPracticePage
{
    public static Page CreatePractice()
    {
        string key = $"or_and_xor_far_v1_{LanguagePreferences.UserId?.ToString("D") ?? "guest"}";
        return CreateExercise(key, Preferences.Default.Get(key, false));
    }

    private static Page CreateExercise(string key, bool far)
    {
        var page = new SimpleViewCellsPage(new GameConfig
        {
            GameName = far ? "Or-And-Neutralise" : "Or-And-XOR",
            UIQuestionType = UIQuestionType.LogicalKeyboards,
            OperationList = new() { Operation.ExclusiveOr },
            OnlyToTen = true,
            DenyStrangeOrSameGroups = true,
            TwoKeybordsOnOne = !far,
            SecondsTillNextExercise = far ? 2 : 1,
            SecondsTillAllowInput = far ? -1 : 0,
            Plan = new ExercisePlan
            {
                Seed = 123,
                Steps = new()
                {
                    new ExercisePlanStep { Kind = PlanStepKind.NewQuestion, OpMode = PlanOpMode.Fixed, Operation = Operation.Or, UseSecondOperandFromPermutation = true, PermutationPolicy = PermutationPolicy.ConstantForChain },
                    new ExercisePlanStep { Kind = PlanStepKind.RepeatQuestion, OpMode = PlanOpMode.Fixed, Operation = Operation.And, UseSecondOperandFromPermutation = true, PermutationPolicy = PermutationPolicy.ConstantForChain },
                    new ExercisePlanStep { Kind = PlanStepKind.RepeatQuestion, OpMode = PlanOpMode.Fixed, Operation = Operation.ExclusiveOr, UseSecondOperandFromPermutation = true, PermutationPolicy = PermutationPolicy.ConstantForChain }
                },
                Loop = true
            },
            KeyboardConfig = new KeyboardConfig
            {
                SyncType = SyncType.Sync,
                SecondsPressingToAnswer = far ? 2 : 1,
                KeysInRow = 6
            }
        }) { Title = AppLanguage.Text("Or-And-XOR") };
        var settings = new ToolbarItem { Text = AppLanguage.Text("Settings"), AutomationId = "OrAndXorSettings" };
        bool opening = false;
        settings.Clicked += async (_, _) =>
        {
            if (opening) return;
            opening = true;
            try { await page.Navigation.PushAsync(CreateSettings(page, key, far)); }
            finally { opening = false; }
        };
        page.ToolbarItems.Add(settings);
        return page;
    }

    private static Page CreateSettings(Page exercise, string key, bool far)
    {
        var page = new ContentPage { Title = AppLanguage.Text("Or-And-XOR settings") };
        var toggle = new Switch { IsToggled = far, AutomationId = "OrAndXorFar" };
        var row = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        row.Add(new Label { Text = AppLanguage.Text("Far"), FontSize = 18, VerticalOptions = LayoutOptions.Center }, 0);
        row.Add(toggle, 1);
        var content = new VerticalStackLayout { Padding = 20, Spacing = 16 };
        content.Add(row);
        var apply = new Button { Text = AppLanguage.Text("Apply settings"), AutomationId = "OrAndXorApplySettings" };
        bool applying = false;
        apply.Clicked += async (_, _) =>
        {
            if (applying) return;
            applying = true;
            try
            {
                if (toggle.IsToggled != far)
                {
                    var replacement = CreateExercise(key, toggle.IsToggled);
                    page.Navigation.InsertPageBefore(replacement, exercise);
                    page.Navigation.RemovePage(exercise);
                    await page.Navigation.PopAsync(animated: false);
                    Preferences.Default.Set(key, toggle.IsToggled);
                }
                else await page.Navigation.PopAsync(animated: false);
            }
            finally { applying = false; }
        };
        content.Add(apply);
        page.Content = content;
        return page;
    }
}
