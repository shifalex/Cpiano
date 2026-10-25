using GestureSample.Maui.Handlers;

namespace GestureSample.Views;

public partial class MainPage
{
    private readonly List<(Label Label, string Key)> _arrowLabels = new();

    private void ConfigureArrowMenu(IReadOnlyCollection<PageConfig> items)
    {
        MenuCollectionView.IsVisible = false;
        MenuCollectionView.RemoveBinding(ItemsView.ItemsSourceProperty);
        MenuCollectionView.ItemsSource = null;

        Color practiceAccent = Color.FromArgb("#C65A24");
        Color labelAccent = Color.FromArgb("#2D7373");
        var practice = new VerticalStackLayout { Spacing = 14 };
        var assigned = new HashSet<PageConfig>();

        void AddStageGroup(VerticalStackLayout target, string heading, Color accent,
            params (string Key, string Caption)[] stages)
        {
            var matches = stages.Select(stage => (Item: items.FirstOrDefault(item => !item.IsTest && item.Title == stage.Key), stage.Caption))
                .Where(stage => stage.Item != null).ToList();
            if (matches.Count == 0) return;
            var section = new VerticalStackLayout { Spacing = 10 };
            section.Add(ArrowMenuLabel(heading, 22, accent));
            foreach (var stage in matches)
            {
                assigned.Add(stage.Item!);
                section.Add(ArrowMenuCard(stage.Item!, stage.Caption, accent));
            }
            target.Add(section);
        }

        AddStageGroup(practice, "1 · Variable length", practiceAccent,
            ("First -> variable length", "Variable length"));
        AddStageGroup(practice, "2 · Fixed length", practiceAccent,
            ("First ->", "Fixed length"));
        AddStageGroup(practice, "3 · Ordinal", practiceAccent,
            ("Ordinal With Key Numbers", "Ordinal with key numbers"));

        var labels = new VerticalStackLayout { Spacing = 14 };
        AddStageGroup(labels, "Arrow Puzzles", labelAccent,
            ("Arrow Label", "Arrow Puzzles · progression or mixed practice"));
        AddStageGroup(labels, "Custom arrow practice", labelAccent,
            ("Custom arrow practice", "Choose a mix or one exact question"));

        var tracks = new Grid
        {
            ColumnSpacing = 14, RowSpacing = 14,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) }
        };
        tracks.Add(practice, 0, 0);
        tracks.Add(labels, 1, 0);

        // Keep test flags authoritative, and preserve access if another stage is added later.
        var extras = items.Where(item => !assigned.Contains(item)).ToList();
        if (extras.Count > 0)
        {
            var extraSections = new VerticalStackLayout { Spacing = 10 };
            foreach (var group in extras.GroupBy(item => item.IsTest))
            {
                Color accent = Color.FromArgb("#7C68D9");
                extraSections.Add(ArrowMenuLabel(group.Key ? "Test activities" : "More practice", 22, accent));
                foreach (var item in group)
                    extraSections.Add(ArrowMenuCard(item, item.Title, accent));
            }
            tracks.Add(extraSections, 0, 2);
            Grid.SetColumnSpan(extraSections, 2);
        }

        bool? previousWide = null;
        void Arrange(double width)
        {
            if (width <= 0) return;
            bool wide = width >= 720;
            if (previousWide == wide) return;
            previousWide = wide;
            tracks.ColumnDefinitions[0].Width = wide ? new GridLength(1.45, GridUnitType.Star) : GridLength.Star;
            tracks.ColumnDefinitions[1].Width = wide ? GridLength.Star : new GridLength(0);
            Grid.SetColumn(labels, wide ? 1 : 0);
            Grid.SetRow(labels, wide ? 0 : 1);
        }
        Arrange(DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density);
        tracks.SizeChanged += (_, _) => Arrange(tracks.Width);
        PageLayout.Add(new ScrollView { Content = tracks, VerticalScrollBarVisibility = ScrollBarVisibility.Default }, 0, 1);
    }

    private Label ArrowMenuLabel(string key, double size, Color color)
    {
        var label = new Label
        {
            Text = AppLanguage.Text(key), FontSize = size, FontFamily = "OpenSansSemibold",
            TextColor = color, VerticalTextAlignment = TextAlignment.Center
        };
        _arrowLabels.Add((label, key));
        return label;
    }

    private Border ArrowMenuCard(PageConfig item, string caption, Color accent)
    {
        var icon = new Border
        {
            WidthRequest = 44, HeightRequest = 44, StrokeThickness = 0, BackgroundColor = accent,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 13 },
            Content = new GraphicsView { Drawable = new ArrowMenuIconDrawable(item.Title),
                InputTransparent = true }
        };
        var content = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(44)), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }
        };
        content.Add(icon, 0);
        content.Add(ArrowMenuLabel(caption, 17, Color.FromArgb("#20232B")), 1);
        content.Add(new Label { Text = "›", FontSize = 22, TextColor = accent.WithAlpha(0.72f), VerticalTextAlignment = TextAlignment.Center }, 2);
        var card = new Border
        {
            BindingContext = item, Content = content, BackgroundColor = Colors.White,
            Stroke = accent.WithAlpha(0.30f), StrokeThickness = 1.2,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
            Padding = new Thickness(10, 9), MinimumHeightRequest = 64
        };
        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) => await NavigateToMenuItem(item);
        card.GestureRecognizers.Add(tap);
        return card;
    }

    internal sealed class ArrowMenuIconDrawable(string title, Color? ink = null) : Microsoft.Maui.Graphics.IDrawable
    {
        public void Draw(Microsoft.Maui.Graphics.ICanvas canvas, Microsoft.Maui.Graphics.RectF dirtyRect)
        {
            canvas.SaveState();
            canvas.Translate(dirtyRect.X, dirtyRect.Y);
            canvas.Scale(dirtyRect.Width / 44f, dirtyRect.Height / 44f);
            canvas.StrokeColor = ink ?? Colors.White;
            canvas.FillColor = ink ?? Colors.White;
            canvas.StrokeSize = 1.7f;

            // A shared keyboard anchors each symbol to the activity's input surface.
            for (int key = 0; key < 4; key++)
                canvas.DrawRoundedRectangle(7 + key * 7.5f, 33, 7.5f, 8, 1.2f);

            void Arrow(float start, float end, float y)
            {
                canvas.DrawLine(start, 32, start, y);
                canvas.DrawLine(start, y, end, y);
                canvas.DrawLine(end - 4, y - 4, end, y);
                canvas.DrawLine(end - 4, y + 4, end, y);
            }

            if (title == "First -> variable length")
            {
                Arrow(8, 23, 11);
                Arrow(8, 35, 21);
            }
            else if (title == "First ->")
            {
                Arrow(9, 35, 16);
                canvas.FillCircle(9, 32, 2);
                canvas.FillCircle(35, 32, 2);
            }
            else if (title == "Ordinal With Key Numbers" || title.Contains("ordinal", StringComparison.OrdinalIgnoreCase))
            {
                canvas.FontColor = ink ?? Colors.White;
                canvas.FontSize = 7;
                for (int key = 0; key < 4; key++)
                    canvas.DrawString((key + 1).ToString(), 7 + key * 7.5f, 33, 7.5f, 8,
                        Microsoft.Maui.Graphics.HorizontalAlignment.Center,
                        Microsoft.Maui.Graphics.VerticalAlignment.Center);

                var roundedArrow = new Microsoft.Maui.Graphics.PathF();
                roundedArrow.MoveTo(10, 31);
                roundedArrow.CurveTo(10, 17, 19, 13, 27, 17);
                roundedArrow.LineTo(35, 21);
                canvas.DrawPath(roundedArrow);
                canvas.DrawLine(32, 15, 35, 21);
                canvas.DrawLine(28, 23, 35, 21);
            }
            else if (title == "Custom arrow practice")
            {
                // Three adjustable controls represent choosing question settings.
                for (int row = 0; row < 3; row++)
                {
                    float y = 9 + row * 6;
                    canvas.DrawLine(9, y, 35, y);
                    canvas.FillCircle(row == 1 ? 28 : 16, y, 2.5f);
                }
            }
            else
            {
                canvas.DrawLine(8, 23, 8, 17);
                canvas.DrawLine(8, 17, 35, 17);
                canvas.DrawLine(31, 13, 35, 17);
                canvas.DrawLine(31, 21, 35, 17);
                canvas.FontColor = ink ?? Colors.White;
                canvas.FontSize = 11;
                canvas.DrawString("?", 15, 1, 14, 14,
                    Microsoft.Maui.Graphics.HorizontalAlignment.Center,
                    Microsoft.Maui.Graphics.VerticalAlignment.Center);
            }

            canvas.RestoreState();
        }
    }
}
