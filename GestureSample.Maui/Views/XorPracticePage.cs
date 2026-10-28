using GestureSample.Maui.Handlers;
using GestureSample.Views.Tests;

namespace GestureSample.Maui.Views;

public static class XorPracticePage
{
    private static readonly string[] Modes = { "One difference", "Special colors", "Standard colors" };

    public static Page CreatePractice()
    {
        string user = LanguagePreferences.UserId?.ToString("D") ?? "guest";
        string previousKey = $"exclusiveor_progression_v1_{user}";
        string key = $"xor_progression_v2_{user}";
        int saved = Preferences.Default.Get(key, -1);
        if (saved < 0)
        {
            int previous = Preferences.Default.Get(previousKey, -1);
            saved = previous < 0 ? 0 : Math.Clamp(previous, 0, 1) + 1;
        }
        return CreateStage(Math.Clamp(saved, 0, 2), key, previousKey + "_far");
    }

    private static SimpleViewCellsPage CreateStage(int initialStage, string key, string farKey, int initialConsecutive = 0)
    {
        int stage = initialStage;
        int consecutive = initialConsecutive;
        bool far = Preferences.Default.Get(farKey, false);
        var page = new SimpleViewCellsPage(new GameConfig
        {
            GameName = "XOR",
            UIQuestionType = UIQuestionType.LogicalKeyboards,
            OperationList = new() { Operation.ExclusiveOr },
            OnlyToTen = true,
            TwoKeybordsOnOne = !far,
            IsSpecialColor = stage < 2,
            XorSingleDifferenceOnly = stage == 0,
            MinSum = 1, MaxSum = stage == 0 ? 1 : 10,
            KeyboardConfig = new KeyboardConfig { SyncType = SyncType.Sync, IsHelpNeeded = true, KeysInRow = 6 }
        });
        bool moving = false;
        var selector = new ToolbarItem { AutomationId = "XORProgressionSelector" };
        void Refresh() => selector.Text = $"{stage} · {AppLanguage.Text(Modes[stage])}" + (stage < 2 ? $" · {consecutive}/4 ▾" : " ▾");
        async Task<bool> ChangeModeAsync(int next, bool automatic = false)
        {
            if (!await page.SetXorPracticeModeAsync(next == 0, next < 2, automatic)) return false;
            stage = next;
            consecutive = 0;
            Preferences.Default.Set(key, stage);
            Refresh();
            return true;
        }
        selector.Clicked += async (_, _) =>
        {
            if (moving) return;
            moving = true;
            try
            {
                string[] choices = Modes.Select((mode, index) => $"{index} · {AppLanguage.Text(mode)}").ToArray();
                string farChoice = AppLanguage.Text(far ? "Far: On · turn off" : "Far: Off · turn on");
                string selected = await page.DisplayActionSheet(AppLanguage.Text("XOR"), AppLanguage.Text("Cancel"), null,
                    choices.Append(farChoice).ToArray());
                int next = Array.IndexOf(choices, selected);
                if (next >= 0 && next != stage) await ChangeModeAsync(next);
                else if (selected == farChoice)
                {
                    Preferences.Default.Set(farKey, !far);
                    var replacement = CreateStage(stage, key, farKey, consecutive);
                    replacement.PreserveXorQuestionFrom(page);
                    page.Navigation.InsertPageBefore(replacement, page);
                    await page.Navigation.PopAsync(animated: false);
                }
            }
            finally { moving = false; }
        };
        Refresh();
        page.ToolbarItems.Add(selector);
        page.PracticeAttemptCompletedAsync = async correct =>
        {
            if (moving || page.Navigation.NavigationStack.LastOrDefault() != page || stage == 2) return false;
            consecutive = correct ? consecutive + 1 : 0;
            Refresh();
            if (consecutive >= 4) await ChangeModeAsync(stage + 1, automatic: true);
            // A color transition keeps this page; normal next-question flow continues.
            return false;
        };
        return page;
    }
}
