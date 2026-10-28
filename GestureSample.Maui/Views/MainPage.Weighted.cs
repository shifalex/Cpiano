namespace GestureSample.Views;

public partial class MainPage
{
    private void ConfigureWeightedMenu(IReadOnlyCollection<PageConfig> items)
    {
        MenuCollectionView.IsVisible = false;
        var left = new VerticalStackLayout { Spacing = 20 };
        var right = new VerticalStackLayout { Spacing = 20 };
        var assigned = new HashSet<PageConfig>();
        void Section(VerticalStackLayout column, string heading, string color,
            params (string Key, string Caption, string Detail)[] stages)
        {
            var matches = stages.Select(stage => (Item: items.FirstOrDefault(item => item.Title == stage.Key), stage.Caption, stage.Detail))
                .Where(stage => stage.Item != null).ToList();
            if (matches.Count == 0) return;
            var accent = Color.FromArgb(color);
            var section = new VerticalStackLayout { Spacing = 10 };
            section.Add(ArrowMenuLabel(heading, 22, accent));
            foreach (var stage in matches)
            {
                var item = stage.Item!;
                assigned.Add(item);
                section.Add(WeightedMenuCard(item, stage.Caption, stage.Detail, accent));
            }
            column.Add(section);
        }

        Section(left, "Number building", "#C65A24",
            ("Weighted one number Abacus", "Abacus", "Build numbers up to 99"),
            ("Weighted Till 150", "Up to 150", "Combine keys with different weights"));
        Section(left, "Binary", "#2D7373",
            ("Weighted one number binary bit array -EASY", "Binary · Easy", "Build numbers up to 35"),
            ("Weighted one number binary bit array", "Binary · Full", "Build numbers up to 1023"));
        Section(right, "Multiplication", "#7C68D9",
            ("Weighted Multiplication -EASY", "Multiplication · Easy", "Products up to 25"),
            ("Weighted Multiplication", "Multiplication · Full", "Products up to 100"));

        // Keep future activities accessible until they are assigned a section.
        var extra = items.Where(item => !assigned.Contains(item)).ToList();
        if (extra.Count > 0)
        {
            var accent = Color.FromArgb("#3167C4");
            right.Add(ArrowMenuLabel("More practice", 22, accent));
            foreach (var item in extra) right.Add(WeightedMenuCard(item, item.Title, "", accent));
        }
        var tracks = new Grid
        {
            ColumnSpacing = 18, RowSpacing = 20,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) }
        };
        tracks.Add(left, 0, 0);
        tracks.Add(right, 1, 0);
        void Arrange(double width)
        {
            bool wide = width >= 720;
            tracks.ColumnDefinitions[1].Width = wide ? GridLength.Star : new GridLength(0);
            Grid.SetColumn(right, wide ? 1 : 0);
            Grid.SetRow(right, wide ? 0 : 1);
        }
        Arrange(DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density);
        tracks.SizeChanged += (_, _) => Arrange(tracks.Width);
        PageLayout.Add(new ScrollView { Content = tracks }, 0, 1);
    }

    private Border WeightedMenuCard(PageConfig item, string caption, string detail, Color accent)
    {
        var text = new VerticalStackLayout { Spacing = 3, VerticalOptions = LayoutOptions.Center };
        text.Add(ArrowMenuLabel(caption, 17, Color.FromArgb("#20232B")));
        if (detail.Length > 0) text.Add(ArrowMenuLabel(detail, 13, Color.FromArgb("#6F7280")));
        var row = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(48)), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }
        };
        row.Add(new Border
        {
            WidthRequest = 48, HeightRequest = 48, StrokeThickness = 0,
            BackgroundColor = accent, VerticalOptions = LayoutOptions.Center,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 13 },
            Content = new GraphicsView { Drawable = new WeightedMenuIconDrawable(item.Title), InputTransparent = true }
        }, 0);
        row.Add(text, 1);
        row.Add(new Label { Text = "›", FontSize = 22, TextColor = accent, VerticalOptions = LayoutOptions.Center }, 2);
        var card = new Border
        {
            Content = row, Padding = 12, MinimumHeightRequest = 78,
            BackgroundColor = Colors.White, Stroke = accent.WithAlpha(0.3f),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 }
        };
        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) => await NavigateToMenuItem(item);
        card.GestureRecognizers.Add(tap);
        return card;
    }

    private sealed class WeightedMenuIconDrawable(string title) : Microsoft.Maui.Graphics.IDrawable
    {
        public void Draw(Microsoft.Maui.Graphics.ICanvas canvas, Microsoft.Maui.Graphics.RectF dirtyRect)
        {
            canvas.SaveState();
            canvas.Translate(dirtyRect.X, dirtyRect.Y);
            canvas.Scale(dirtyRect.Width / 48, dirtyRect.Height / 48);
            canvas.StrokeColor = Colors.White;
            canvas.FillColor = Colors.White;
            canvas.FontColor = Colors.White;
            canvas.StrokeSize = 1.8f;
            void Text(string value, float x, float y, float width, float height, float size)
            {
                canvas.FontSize = size;
                canvas.DrawString(value, x, y, width, height,
                    Microsoft.Maui.Graphics.HorizontalAlignment.Center,
                    Microsoft.Maui.Graphics.VerticalAlignment.Center);
            }
            if (title.Contains("Multiplication", StringComparison.Ordinal))
            {
                Text("×", 8, 3, 32, 25, 27);
                for (int key = 0; key < 3; key++)
                {
                    canvas.DrawRoundedRectangle(7 + key * 12, 30, 10, 12, 2);
                    Text("2", 7 + key * 12, 30, 10, 12, 9);
                }
            }
            else if (title.Contains("binary", StringComparison.Ordinal))
            {
                Text("101", 5, 4, 38, 20, 17);
                for (int key = 0; key < 3; key++)
                {
                    canvas.DrawRoundedRectangle(7 + key * 12, 27, 10, 15, 2);
                    Text((1 << key).ToString(), 7 + key * 12, 27, 10, 15, 9);
                }
            }
            else if (title.Contains("Abacus", StringComparison.Ordinal))
            {
                for (int rail = 0; rail < 3; rail++)
                {
                    float y = 12 + rail * 12;
                    canvas.DrawLine(7, y, 41, y);
                    canvas.FillRoundedRectangle(rail == 1 ? 27 : 12, y - 4, 7, 8, 2);
                }
            }
            else
            {
                Text("150", 5, 3, 38, 22, 17);
                string[] weights = { "1", "5", "50" };
                for (int key = 0; key < 3; key++)
                {
                    canvas.DrawRoundedRectangle(7 + key * 12, 27, 10, 15, 2);
                    Text(weights[key], 7 + key * 12, 27, 10, 15, 8);
                }
            }
            canvas.RestoreState();
        }
    }
}
