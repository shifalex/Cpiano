using GestureSample.Maui.Handlers;
using GestureSample.Views.Tests;

namespace GestureSample.Maui.Views;

public static class CopyPracticePage
{
    public static Page CreatePractice() => CreatePractice(Operation.Copy);

    public static Page CreateMirrorPractice() => CreatePractice(Operation.Mirror);
    public static Page CreateXorPractice() => XorPracticePage.CreatePractice();

    private static Page CreatePractice(Operation operation)
    {
        Guid? userId = LanguagePreferences.UserId;
        string key = $"{operation.ToString().ToLowerInvariant()}_progression_v1_{userId?.ToString("D") ?? "guest"}";
        return CreateStage(Math.Clamp(Preferences.Default.Get(key, 0), 0, 1), key, operation);
    }

    private static Page CreateStage(int step, string preferenceKey, Operation operation)
    {
        bool xor = operation == Operation.ExclusiveOr;
        bool far = xor && Preferences.Default.Get(preferenceKey + "_far", false);
        string activity = xor ? "XOR" : operation == Operation.Mirror ? "Mirror" : "Copy";
        string firstStage = xor ? "Special colors" : "One hand";
        var page = new SimpleViewCellsPage(new GameConfig
        {
            // Preserve the existing game identities and each stage's behavior.
            GameName = xor ? "XOR" : step == 0 ? operation == Operation.Mirror ? "Mirror - one hand" : "Copy -one hand" : activity,
            UIQuestionType = UIQuestionType.LogicalKeyboards,
            OperationList = new() { operation },
            OnlyToTen = xor || operation == Operation.Mirror || step == 0,
            IsOnlyOneHand = !xor && step == 0,
            isOnlyKeyboard = !xor,
            TwoKeybordsOnOne = xor && !far,
            IsSpecialColor = xor && step == 0,
            KeyboardConfig = new KeyboardConfig
            {
                SyncType = SyncType.Sync,
                IsHelpNeeded = xor || operation == Operation.Mirror || step == 1,
                KeysInRow = 6
            }
        });
        page.Title = AppLanguage.Text(activity);
        int consecutive = 0;
        bool moving = false;
        var selector = new ToolbarItem { AutomationId = activity + "ProgressionSelector" };
        void Refresh() => selector.Text = step == 0
            ? $"{AppLanguage.Text(firstStage)} · {consecutive}/4 ▾"
            : $"{AppLanguage.Text(activity)} · 2/2 ▾";
        async Task MoveToAsync(int next, bool? nextFar = null)
        {
            if (moving || page.Navigation.NavigationStack.LastOrDefault() != page) return;
            moving = true;
            try
            {
                if (nextFar.HasValue) Preferences.Default.Set(preferenceKey + "_far", nextFar.Value);
                var replacement = CreateStage(next, preferenceKey, operation);
                page.Navigation.InsertPageBefore(replacement, page);
                await page.Navigation.PopAsync(animated: false);
                Preferences.Default.Set(preferenceKey, next);
            }
            finally { moving = false; }
        }
        selector.Clicked += async (_, _) =>
        {
            string first = $"1 · {AppLanguage.Text(firstStage)}";
            string second = $"2 · {AppLanguage.Text(activity)}";
            string farChoice = AppLanguage.Text(far ? "Far: On · turn off" : "Far: Off · turn on");
            string selected = await page.DisplayActionSheet(AppLanguage.Text(activity), AppLanguage.Text("Cancel"), null,
                xor ? new[] { first, second, farChoice } : new[] { first, second });
            if (selected == first && step != 0) await MoveToAsync(0);
            else if (selected == second && step != 1) await MoveToAsync(1);
            else if (xor && selected == farChoice) await MoveToAsync(step, !far);
        };
        Refresh();
        page.ToolbarItems.Add(selector);
        page.PracticeAttemptCompletedAsync = async correct =>
        {
            if (moving || page.Navigation.NavigationStack.LastOrDefault() != page || step == 1) return false;
            consecutive = correct ? consecutive + 1 : 0;
            Refresh();
            if (consecutive < 4) return false;
            await MoveToAsync(1);
            return true;
        };
        return page;
    }
}
