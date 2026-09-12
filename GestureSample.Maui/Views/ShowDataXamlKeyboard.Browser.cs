using GestureSample.Maui.Data.SQLite;
using GestureSample.Maui.Models;
using GestureSample.Maui;
using GestureSample.Maui.Handlers;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Controls.Shapes;
using System.Text.RegularExpressions;

namespace GestureSample.Views;

public partial class ShowDataXamlKeyboard
{
    private List<MainItem> _browserItems = new();
    private string? _browserCategory;
    private bool _showingQuestion;

    private static Color Palette(string key) => (Color)Application.Current.Resources[key];

    private static string CategoryName(MainItem item)
    {
        var q = item.Question;
        string name = item.IsGripCombinationStage ? "Grip combinations"
            : q.IsSpecialArrowPrompt ? "Complex arrows"
            : q.HasArrowPrompt ? "Arrows"
            : Regex.Replace(q.Op.ToString(), "(?<=[a-z])([A-Z])", " $1");
        return $"{name} · {q.KeyboardRows} × {q.KeyboardKeysInRow}";
    }

    // Only a submission or recorded check event establishes a complete answer time.
    internal static double? AnswerSeconds(MainItem item)
    {
        DateTime? end = item.Question.SubmittedTime ?? item.SubItems?
            .Where(e => e.EventType == 2).OrderBy(e => e.EventTime)
            .Select(e => (DateTime?)e.EventTime).FirstOrDefault();
        if (!end.HasValue || end.Value < item.Question.Time)
            return null;
        return (end.Value - item.Question.Time).TotalSeconds;
    }

    private static string TimeText(MainItem item) => AnswerSeconds(item) is double seconds
        ? $"{seconds:0.0}s" : "— No timing";

    private static Color HeatColor(MainItem item) => AnswerSeconds(item) switch
    {
        null => Palette("Surface"),
        <= 3 => Palette("Secondary"),
        <= 5 => Palette("Blue300Accent"),
        <= 8 => Palette("Primary"),
        _ => Palette("Tertiary")
    };

    private static Color HeatInk(MainItem item) => AnswerSeconds(item) is > 5
        ? Palette("White") : Palette("Ink");

    private static string QuestionDescription(MainItem item)
    {
        var q = item.Question;
        if (q.HasPromptText)
            return q.QuestionPromptText!.Replace("\r", " ").Replace("\n", " ");
        if (q.HasArrowPrompt)
            return $"Start {q.aboveNumber} · {(q.dir == Direction.Right ? "→" : "←")} {q.length}";
        if (q.HasMoveByPrompt)
            return $"Shift {(q.MoveByDirection == Direction.Right ? "→" : "←")} {q.MoveByLength}";
        var keys = q.keyboard1?.Select((on, i) => (on, i)).Where(k => k.on)
            .Select(k => (k.i + 1).ToString()).ToList() ?? new();
        return keys.Count > 0 ? $"Keys {string.Join(", ", keys.Take(8))}{(keys.Count > 8 ? " …" : "")}" : "Keyboard pattern";
    }

    private static Label BrowserLabel(string text, int size = 14) => new()
    {
        Text = text, FontSize = size, TextColor = Palette("Ink"),
        LineBreakMode = LineBreakMode.WordWrap
    };

    private void ShowCategories()
    {
        _browserCategory = null;
        _showingQuestion = false;
        BrowseTitle.Text = "Keyboard categories";
        BrowseBack.IsVisible = false;
        HeatLegend.IsVisible = true;
        Questions.IsVisible = false;
        Questions.ItemsSource = null;
        CategoryBrowser.IsVisible = true;
        BrowserItems.Children.Clear();
        if (_browserItems.Count == 0)
            BrowserItems.Children.Add(BrowserLabel("No keyboard attempts in this session."));

        foreach (var group in _browserItems.GroupBy(CategoryName))
        {
            string category = group.Key;
            var items = group.ToList();
            var content = new VerticalStackLayout { Spacing = 10 };
            content.Children.Add(BrowserLabel(category, 18));
            content.Children.Add(BrowserLabel($"{items.Select(i => i.Question.QuestionNumber).Distinct().Count()} questions · {items.Count} attempts →", 12));
            var heatmap = new FlexLayout { Wrap = FlexWrap.Wrap, Direction = FlexDirection.Row };
            foreach (var item in items.Take(12))
            {
                heatmap.Children.Add(new Border
                {
                    BackgroundColor = HeatColor(item), Stroke = Palette("Outline"),
                    StrokeShape = new RoundRectangle { CornerRadius = 5 },
                    Padding = 6, Margin = new Thickness(0, 0, 4, 4),
                    Content = new Label
                    {
                        Text = AnswerSeconds(item) is double seconds ? $"{seconds:0.0}s" : "—",
                        TextColor = HeatInk(item), FontSize = 12
                    }
                });
            }
            content.Children.Add(heatmap);
            if (items.Count > 12)
                content.Children.Add(BrowserLabel($"+{items.Count - 12} more attempts", 12));
            var card = new Border
            {
                Content = content, Padding = 16, BackgroundColor = Palette("Surface"),
                Stroke = Palette("Outline"), StrokeShape = new RoundRectangle { CornerRadius = 16 }
            };
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => ShowCategory(category);
            card.GestureRecognizers.Add(tap);
            // Native button keeps the category action available to keyboard and screen-reader users.
            var open = new Button { Text = "View questions", HorizontalOptions = LayoutOptions.Start,
                BackgroundColor = Palette("Secondary"), TextColor = Palette("Primary") };
            open.Clicked += (_, _) => ShowCategory(category);
            content.Children.Add(open);
            BrowserItems.Children.Add(card);
        }
    }

    private void ShowCategory(string category)
    {
        _browserCategory = category;
        _showingQuestion = false;
        BrowseBack.Text = "← Categories";
        BrowseBack.IsVisible = true;
        BrowseTitle.Text = category;
        HeatLegend.IsVisible = true;
        Questions.IsVisible = false;
        CategoryBrowser.IsVisible = true;
        BrowserItems.Children.Clear();
        var grid = new Grid { ColumnSpacing = 10, RowSpacing = 10,
            ColumnDefinitions = { new ColumnDefinition(), new ColumnDefinition() } };
        int index = 0;
        foreach (var item in _browserItems.Where(i => CategoryName(i) == category))
        {
            if (index % 2 == 0) grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            var tile = new Button
            {
                Text = $"#{item.Question.QuestionNumber} · {item.Question.AttemptText}\n{QuestionDescription(item)}\n{TimeText(item)} · {item.Question.ResultStatusText}",
                BackgroundColor = HeatColor(item), TextColor = HeatInk(item),
                BorderColor = Palette("Outline"), BorderWidth = 1, CornerRadius = 12,
                Padding = 12, MinimumHeightRequest = 112, FontSize = 14,
                LineBreakMode = LineBreakMode.WordWrap
            };
            tile.Clicked += (_, _) => ShowQuestion(item);
            grid.Add(tile, index % 2, index / 2);
            index++;
        }
        BrowserItems.Children.Add(grid);
        _ = CategoryBrowser.ScrollToAsync(0, 0, false);
    }

    private void ShowQuestion(MainItem item)
    {
        _showingQuestion = true;
        BrowseBack.Text = "← Questions";
        BrowseTitle.Text = $"Question {item.Question.QuestionNumber} · {TimeText(item)}";
        HeatLegend.IsVisible = false;
        CategoryBrowser.IsVisible = false;
        Questions.ItemsSource = new[] { item };
        Questions.IsVisible = true;
        Questions.ScrollTo(0, position: ScrollToPosition.Start, animate: false);
    }

    private void OnBrowseBackClicked(object sender, EventArgs e)
    {
        if (_showingQuestion && _browserCategory != null) ShowCategory(_browserCategory);
        else ShowCategories();
    }

    private void OnTimingToggleClicked(object sender, EventArgs e) =>
        TimingRecommendationsContainer.IsVisible = !TimingRecommendationsContainer.IsVisible;

    private void OnSnapshotLayoutSizeChanged(object sender, EventArgs e)
    {
        if (sender is not Grid grid || grid.Width <= 0) return;
        int columns = grid.Width >= 600 ? 2 : 1;
        var visible = grid.Children.OfType<View>().Where(view => view.IsVisible).ToList();
        int rows = (visible.Count + columns - 1) / columns;
        if (grid.ColumnDefinitions.Count != columns)
        {
            grid.ColumnDefinitions.Clear();
            for (int i = 0; i < columns; i++) grid.ColumnDefinitions.Add(new ColumnDefinition());
        }
        if (grid.RowDefinitions.Count != rows)
        {
            grid.RowDefinitions.Clear();
            for (int i = 0; i < rows; i++) grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }
        for (int i = 0; i < visible.Count; i++)
        {
            Grid.SetColumn(visible[i], i % columns);
            Grid.SetRow(visible[i], i / columns);
        }
    }

    private void OnSnapshotPanelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IsVisible) && sender is View { Parent: Grid grid })
            grid.Dispatcher.Dispatch(() => OnSnapshotLayoutSizeChanged(grid, EventArgs.Empty));
    }
}

public partial class MainItem
{
    public List<KeyboardTimingPhase> TimingPhases
    {
        get
        {
            DateTime? end = GetAnswerTime();
            if (!end.HasValue || end <= Question.Time)
                return new();
            var presses = SubItems?.Where(e => e.EventType == 1 && e.EventTime >= Question.Time && e.EventTime <= end.Value)
                .OrderBy(e => e.EventTime).ToList();
            if (presses == null || presses.Count == 0)
                return new();
            double total = (end.Value - Question.Time).TotalSeconds;
            return new()
            {
                new("First key", (presses[0].EventTime - Question.Time).TotalSeconds, total),
                new("Key presses", (presses[^1].EventTime - presses[0].EventTime).TotalSeconds, total),
                new("Before submit", (end.Value - presses[^1].EventTime).TotalSeconds, total)
            };
        }
    }

    public bool HasTimingPhases => TimingPhases.Count > 0;
    public bool MissingTimingPhases => !HasTimingPhases;
    public bool MissingReplay => !HasReplay;
}

public sealed class KeyboardTimingPhase
{
    public KeyboardTimingPhase(string name, double seconds, double total)
    {
        Name = name;
        Duration = $"{seconds:0.0}s";
        Fraction = Math.Clamp(seconds / total, 0, 1);
    }
    public string Name { get; }
    public string Duration { get; }
    public double Fraction { get; }
}
