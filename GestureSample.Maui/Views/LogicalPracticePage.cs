using GestureSample.Maui.Handlers;
using GestureSample.Views.Tests;

namespace GestureSample.Maui.Views;

public static class LogicalPracticePage
{
    private static readonly Operation[] MixedOperations =
        { Operation.Copy, Operation.Not, Operation.Mirror, Operation.MoveBy, Operation.SequenceLTR, Operation.SequenceRTL, Operation.Split };

    public static Page CreateShift() => CreateExercise("Shift by", Key("Shift by"));
    public static Page CreateGroup() => CreateExercise("Group To Side", Key("Group To Side"));
    public static Page CreateMixed() => CreateSettings("Mixed", Key("Mixed"));
    public static Page CreateTwoKeyboardMixed() => CreateSettings("Mixed two keyboards", Key("Mixed two keyboards"));
    private static Operation[] AvailableOperations(string activity) => activity == "Mixed two keyboards"
        ? GameConfig.Operations.BitArray.Concat(GameConfig.Operations.LogicalDual).Distinct().ToArray()
        : MixedOperations;
    private static string Key(string activity) => $"logical_practice_v1_{LanguagePreferences.UserId?.ToString("D") ?? "guest"}_{activity}_";

    private static Page CreateSettings(string activity, string key, Page? previous = null)
    {
        var page = new ContentPage { Title = AppLanguage.Text(activity + " settings"), BackgroundColor = Color.FromArgb("#F7F8FC") };
        var content = new VerticalStackLayout { Padding = 20, Spacing = 14, MaximumWidthRequest = 720 };
        content.Add(new StageNavigationSettingsView(activity));
        void Heading(string text) => content.Add(new Label { Text = AppLanguage.Text(text), FontSize = 17, TextColor = Colors.Black });
        Heading("Number of keys");
        var keys = new Picker { ItemsSource = Enumerable.Range(4, 13).Select(n => n.ToString()).ToArray(), SelectedIndex = Math.Clamp(Preferences.Default.Get(key + "keys", 6), 4, 16) - 4 };
        content.Add(keys);
        Picker? mode = null;
        if (activity == "Shift by")
        {
            Heading("Stage");
            mode = new Picker { ItemsSource = new[] { "One finger", "Two fingers", "Structure" }.Select(text => AppLanguage.Text(text)).ToArray(), SelectedIndex = Math.Clamp(Preferences.Default.Get(key + "step", 0), 0, 2) };
            content.Add(mode);
            Heading("Four consecutive correct answers advance from one finger to two fingers. Structure is optional.");
        }
        Switch Toggle(string label, string suffix)
        {
            var toggle = new Switch { IsToggled = Preferences.Default.Get(key + suffix, false) };
            var row = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
            row.Add(new Label { Text = AppLanguage.Text(label), VerticalOptions = LayoutOptions.Center, TextColor = Colors.Black }, 0);
            row.Add(toggle, 1);
            content.Add(row);
            return toggle;
        }
        var far = Toggle("Far", "far");
        var cyclical = Toggle("Cyclical · wrap around keyboard edges", "cyclical");
        var checks = new Dictionary<Operation, CheckBox>();
        if (activity is "Mixed" or "Mixed two keyboards")
        {
            Heading("Operations");
            foreach (var operation in AvailableOperations(activity))
            {
                var check = new CheckBox { IsChecked = Preferences.Default.Get(key + operation, true) };
                checks.Add(operation, check);
                string label = operation switch { Operation.MoveBy => "Shift by", Operation.SequenceLTR => "Group left to right", Operation.SequenceRTL => "Group right to left", Operation.Split => "Split", _ => operation.ToString() };
                var row = new HorizontalStackLayout { Spacing = 10 };
                row.Add(check);
                row.Add(new Label { Text = AppLanguage.Text(label), VerticalOptions = LayoutOptions.Center, TextColor = Colors.Black });
                content.Add(row);
            }
        }
        var error = new Label { IsVisible = false, Text = "Select at least one operation.", TextColor = Colors.DarkRed };
        content.Add(error);
        var start = new Button { Text = AppLanguage.Text("Start practice") };
        bool opening = false;
        start.Clicked += async (_, _) =>
        {
            if (opening) return;
            if (checks.Count > 0 && checks.Values.All(check => !check.IsChecked))
            {
                error.Text = AppLanguage.Text("Select at least one operation.");
                error.IsVisible = true;
                return;
            }
            if (mode?.SelectedIndex == 2 && !far.IsToggled && keys.SelectedIndex + 4 < 6)
            {
                error.Text = AppLanguage.Text("Structure needs at least six keys when Far is off.");
                error.IsVisible = true;
                return;
            }
            opening = true;
            try
            {
                Preferences.Default.Set(key + "keys", keys.SelectedIndex + 4);
                Preferences.Default.Set(key + "far", far.IsToggled);
                Preferences.Default.Set(key + "cyclical", cyclical.IsToggled);
                if (mode != null) Preferences.Default.Set(key + "step", mode.SelectedIndex);
                foreach (var pair in checks) Preferences.Default.Set(key + pair.Key, pair.Value.IsChecked);
                var exercise = CreateExercise(activity, key);
                if (previous == null)
                {
                    await page.Navigation.PushAsync(exercise, animated: false);
                    page.Navigation.RemovePage(page);
                }
                else
                {
                    page.Navigation.InsertPageBefore(exercise, previous);
                    page.Navigation.RemovePage(previous);
                    await page.Navigation.PopAsync(animated: false);
                }
            }
            finally { opening = false; }
        };
        content.Add(start);
        page.Content = new ScrollView { Content = content };
        return page;
    }

    private static Page CreateExercise(string activity, string key)
    {
        int step = Math.Clamp(Preferences.Default.Get(key + "step", 0), 0, 2);
        bool shift = activity == "Shift by";
        bool twoKeyboards = activity == "Mixed two keyboards";
        bool far = Preferences.Default.Get(key + "far", false);
        bool cyclical = Preferences.Default.Get(key + "cyclical", false);
        int keys = Math.Clamp(Preferences.Default.Get(key + "keys", shift && step == 2 ? 8 : 6), shift && step == 2 && !far ? 6 : 4, 16);
        var operations = shift ? new List<Operation> { Operation.MoveBy }
            : activity is "Mixed" or "Mixed two keyboards" ? AvailableOperations(activity).Where(operation => Preferences.Default.Get(key + operation, true)).ToList()
            : new List<Operation> { Operation.SequenceLTR, Operation.SequenceRTL, Operation.Split };
        if (operations.Count == 0) operations.Add(Operation.Copy);
        var config = new GameConfig
        {
            GameName = activity == "Shift by" ? step switch { 0 => "Shift by -  - One Finger", 1 => "Shift by - Two Fingers", _ => "Shift by - Structure" } : twoKeyboards ? "Mixed two keyboards" : activity == "Mixed" ? "Mixed one operation" : "Group to side",
            UIQuestionType = UIQuestionType.LogicalKeyboards,
            OperationList = operations,
            OnlyToTen = !cyclical,
            // OnlyToTen controls legacy shift wraparound; question order stays independent.
            QuestionOrder = QuestionOrder.Random,
            IsOnlyOneHand = shift && step == 0,
            isOnlyKeyboard = !twoKeyboards && !far,
            TwoKeybordsOnOne = twoKeyboards && !far,
            isOnlySequence = false,
            MinSum = shift ? step == 0 ? 1 : step == 1 ? 2 : 3 : twoKeyboards ? 1 : 2,
            MaxSum = shift && step < 2 ? step + 1 : Math.Min(10, keys),
            KeyboardConfig = new KeyboardConfig { SyncType = SyncType.Sync, IsHelpNeeded = true, KeysInRow = keys }
        };
        var page = new SimpleViewCellsPage(config) { Title = AppLanguage.Text(activity) };
        var settings = new ToolbarItem { Text = AppLanguage.Text("Settings"), AutomationId = "LogicalPracticeSettings" };
        bool moving = false;
        settings.Clicked += async (_, _) =>
        {
            if (moving) return;
            moving = true;
            try { await page.Navigation.PushAsync(CreateSettings(activity, key, page)); }
            finally { moving = false; }
        };
        page.ToolbarItems.Add(settings);
        if (shift)
        {
            int correctCount = 0;
            var progress = new ToolbarItem();
            void Refresh() => progress.Text = step == 0 ? $"{AppLanguage.Text("One finger")} · {correctCount}/4" : AppLanguage.Text(step == 1 ? "Two fingers" : "Structure");
            Refresh();
            page.ToolbarItems.Add(progress);
            page.PracticeAttemptCompletedAsync = async correct =>
            {
                if (step != 0 || moving || page.Navigation.NavigationStack.LastOrDefault() != page) return false;
                correctCount = correct ? correctCount + 1 : 0;
                Refresh();
                if (correctCount < 4) return false;
                moving = true;
                try
                {
                    Preferences.Default.Set(key + "step", 1);
                    page.Navigation.InsertPageBefore(CreateExercise(activity, key), page);
                    await page.Navigation.PopAsync(animated: false);
                    return true;
                }
                finally { moving = false; }
            };
        }
        return page;
    }
}
