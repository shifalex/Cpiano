using GestureSample.Maui.Handlers;

namespace GestureSample.Maui.Views;

public sealed class StageNavigationSettingsView : ContentView
{
    public StageNavigationSettingsView(string stage)
    {
        var toggle = new Switch { IsToggled = StageNavigationPreferences.IsManual(stage), AutomationId = "ManualExerciseAdvance" };
        toggle.Toggled += (_, args) => StageNavigationPreferences.SetManual(stage, args.Value);
        var row = new Grid { ColumnSpacing = 12, ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        row.Add(new Label { Text = AppLanguage.Text("Advance only with the next button"), FontSize = 16, TextColor = Color.FromArgb("#20232B"), VerticalOptions = LayoutOptions.Center }, 0);
        row.Add(toggle, 1);
        Content = new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                new Label { Text = AppLanguage.Text("Exercise navigation"), FontSize = 20, TextColor = Color.FromArgb("#2D7373") },
                row,
                new Label { Text = AppLanguage.Text("Wait after solving. Previous exercises are read-only; the double arrow returns to the current exercise."), FontSize = 13, TextColor = Color.FromArgb("#6F7280") }
            }
        };
    }
}
