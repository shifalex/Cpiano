namespace GestureSample.Views;

public partial class MainPage
{
    private void ConfigureLogicalMenu(IReadOnlyCollection<PageConfig> items)
    {
        MenuCollectionView.IsVisible = false;
        var left = new VerticalStackLayout { Spacing = 14 };
        var right = new VerticalStackLayout { Spacing = 14 };
        var assigned = new HashSet<PageConfig>();
        void Section(VerticalStackLayout column, string heading, IEnumerable<PageConfig> contents)
        {
            var list = contents.ToList();
            if (list.Count == 0) return;
            Color accent = Color.FromArgb(heading switch
            {
                "One operation" => "#C65A24",
                "Two-keyboard operations" => "#2D7373",
                "Special sequences" => "#3167C4",
                "Experimental" => "#7C68D9",
                _ => "#2D7373"
            });
            column.Add(ArrowMenuLabel(heading, 22, accent));
            foreach (var item in list)
            {
                assigned.Add(item);
                string caption = item.Title.StartsWith("Group By Color", StringComparison.Ordinal)
                    ? item.Title + " (Experimental)" : item.Title == "Mixed two keyboards" ? "Mixed" : item.Title;
                var row = new Grid { ColumnSpacing = 12, ColumnDefinitions = { new ColumnDefinition(new GridLength(40)), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
                row.Add(new Border
                {
                    WidthRequest = 40, HeightRequest = 40, StrokeThickness = 0,
                    BackgroundColor = accent,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 11 },
                    Content = new GraphicsView { Drawable = new LogicalMenuIconDrawable(item.Title), InputTransparent = true }
                }, 0);
                row.Add(ArrowMenuLabel(caption, 16, Color.FromArgb("#20232B")), 1);
                row.Add(new Label { Text = "›", FontSize = 22, TextColor = accent, VerticalOptions = LayoutOptions.Center }, 2);
                var card = new Border
                {
                    Content = row, Padding = 14, MinimumHeightRequest = 54,
                    BackgroundColor = Colors.White, Stroke = accent.WithAlpha(0.3f),
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 }
                };
                var tap = new TapGestureRecognizer();
                tap.Tapped += async (_, _) => await NavigateToMenuItem(item);
                card.GestureRecognizers.Add(tap);
                column.Add(card);
            }
        }
        string[] singles = { "Copy", "Not", "Mirror", "Shift by", "Group To Side", "Mixed" };
        Section(left, "One operation", singles.Select(name => items.FirstOrDefault(item => item.Title == name)).OfType<PageConfig>());
        string[] sequences = { "Copy-Not", "Not-Not", "Mirror-Mirror", "Dyslexia - Mirror or Shift by 4", "Or-And-XOR" };
        Section(right, "Two-keyboard operations", items.Where(item => !assigned.Contains(item)
            && !sequences.Contains(item.Title)
            && !item.Title.StartsWith("Group By Color", StringComparison.Ordinal)
            && !item.Title.Contains("Experimental", StringComparison.Ordinal)));
        Section(right, "Special sequences", items.Where(item => sequences.Contains(item.Title)));
        Section(left, "Experimental", items.Where(item => !assigned.Contains(item)));
        var tracks = new Grid
        {
            ColumnSpacing = 18, RowSpacing = 18,
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

    private sealed class LogicalMenuIconDrawable(string title) : Microsoft.Maui.Graphics.IDrawable
    {
        public void Draw(Microsoft.Maui.Graphics.ICanvas canvas, Microsoft.Maui.Graphics.RectF dirtyRect)
        {
            canvas.SaveState();
            canvas.Translate(dirtyRect.X, dirtyRect.Y);
            canvas.Scale(dirtyRect.Width / 40, dirtyRect.Height / 40);
            canvas.StrokeColor = Colors.White;
            canvas.FillColor = Colors.White;
            canvas.FontColor = Colors.White;
            canvas.StrokeSize = 1.8f;
            void Symbol(string text, float size = 24)
            {
                canvas.FontSize = size;
                canvas.DrawString(text, 2, 2, 36, 36,
                    Microsoft.Maui.Graphics.HorizontalAlignment.Center,
                    Microsoft.Maui.Graphics.VerticalAlignment.Center);
            }
            if (title == "Copy")
            {
                canvas.DrawRoundedRectangle(10, 9, 16, 19, 2);
                canvas.FillRoundedRectangle(15, 14, 16, 19, 2);
            }
            else if (title == "Quantity")
            {
                for (int i = 0; i < 3; i++) canvas.FillCircle(11 + i * 9, 11, 2.4f);
                canvas.FontSize = 17;
                canvas.DrawString("3", 4, 16, 32, 20,
                    Microsoft.Maui.Graphics.HorizontalAlignment.Center,
                    Microsoft.Maui.Graphics.VerticalAlignment.Center);
            }
            else if (title == "Mirror")
            {
                canvas.DrawLine(20, 7, 20, 33);
                var left = new Microsoft.Maui.Graphics.PathF();
                left.MoveTo(8, 13); left.LineTo(16, 20); left.LineTo(8, 27); left.Close();
                canvas.DrawPath(left);
                var right = new Microsoft.Maui.Graphics.PathF();
                right.MoveTo(32, 13); right.LineTo(24, 20); right.LineTo(32, 27); right.Close();
                canvas.DrawPath(right);
            }
            else if (title == "Shift by")
            {
                // The same spaced pattern moves right without changing its shape.
                canvas.DrawRoundedRectangle(7, 9, 6, 6, 1);
                canvas.DrawRoundedRectangle(17, 9, 6, 6, 1);
                canvas.DrawLine(11, 21, 30, 21);
                canvas.DrawLine(26, 17, 30, 21);
                canvas.DrawLine(26, 25, 30, 21);
                canvas.FillRoundedRectangle(17, 28, 6, 6, 1);
                canvas.FillRoundedRectangle(27, 28, 6, 6, 1);
            }
            else if (title == "Group To Side")
            {
                // Scattered keys become a compact group against the left edge.
                canvas.DrawRoundedRectangle(8, 8, 5, 5, 1);
                canvas.DrawRoundedRectangle(19, 8, 5, 5, 1);
                canvas.DrawRoundedRectangle(30, 8, 5, 5, 1);
                canvas.DrawLine(28, 21, 12, 21);
                canvas.DrawLine(16, 17, 12, 21);
                canvas.DrawLine(16, 25, 12, 21);
                canvas.DrawLine(6, 26, 6, 35);
                for (int key = 0; key < 3; key++)
                    canvas.FillRoundedRectangle(9 + key * 7, 28, 6, 6, 1);
            }
            else if (title.StartsWith("Group By Color", StringComparison.Ordinal))
            {
                Color[] colors = { Color.FromArgb("#FDBA74"), Color.FromArgb("#93C5FD"), Color.FromArgb("#C4B5FD") };
                for (int column = 0; column < 3; column++)
                {
                    canvas.FillColor = colors[column];
                    for (int row = 0; row < 2; row++) canvas.FillCircle(10 + column * 10, 14 + row * 12, 3.5f);
                }
            }
            else if (title == "Copy-Not")
            {
                canvas.DrawRoundedRectangle(6, 6, 11, 13, 1.5f);
                canvas.DrawRoundedRectangle(10, 10, 11, 13, 1.5f);
                canvas.DrawLine(14, 29, 23, 29);
                canvas.DrawLine(20, 26, 23, 29);
                canvas.DrawLine(20, 32, 23, 29);
                canvas.DrawLine(26, 26, 34, 26);
                canvas.DrawLine(34, 26, 34, 33);
            }
            else if (title == "Not-Not")
            {
                // A pattern, its inverse, then the original pattern again.
                for (int row = 0; row < 3; row++)
                    for (int key = 0; key < 3; key++)
                    {
                        float x = 7 + key * 10;
                        float y = 7 + row * 10;
                        if ((key == 1) == (row == 1)) canvas.FillRoundedRectangle(x, y, 6, 6, 1);
                        else canvas.DrawRoundedRectangle(x, y, 6, 6, 1);
                    }
            }
            else if (title == "Mirror-Mirror")
            {
                // Opposing triangles and a return path show repeated reflection.
                canvas.DrawLine(20, 6, 20, 28);
                var left = new Microsoft.Maui.Graphics.PathF();
                left.MoveTo(7, 9); left.LineTo(16, 17); left.LineTo(7, 25); left.Close();
                canvas.FillPath(left);
                var right = new Microsoft.Maui.Graphics.PathF();
                right.MoveTo(33, 9); right.LineTo(24, 17); right.LineTo(33, 25); right.Close();
                canvas.DrawPath(right);
                canvas.DrawLine(30, 33, 10, 33);
                canvas.DrawLine(14, 29, 10, 33);
                canvas.DrawLine(14, 37, 10, 33);
            }
            else if (title == "Dyslexia - Mirror or Shift by 4")
            {
                canvas.DrawLine(16, 6, 16, 22);
                canvas.DrawLine(7, 8, 12, 14);
                canvas.DrawLine(12, 14, 7, 20);
                canvas.DrawLine(25, 8, 20, 14);
                canvas.DrawLine(20, 14, 25, 20);
                canvas.DrawLine(8, 30, 32, 30);
                canvas.DrawLine(28, 26, 32, 30);
                canvas.DrawLine(28, 34, 32, 30);
                canvas.FontSize = 12;
                canvas.DrawString("4", 28, 5, 10, 16,
                    Microsoft.Maui.Graphics.HorizontalAlignment.Center,
                    Microsoft.Maui.Graphics.VerticalAlignment.Center);
            }
            else if (title == "Or-And-XOR")
            {
                // Three connected steps carry the union, intersection and XOR signs.
                string[] operations = { "∪", "∩", "⊕" };
                for (int step = 0; step < 3; step++)
                {
                    float x = 10 + step * 10;
                    float y = 10 + step * 10;
                    if (step < 2) canvas.DrawLine(x + 4, y + 4, x + 7, y + 7);
                    canvas.DrawCircle(x, y, 6);
                    canvas.FontSize = 10;
                    canvas.DrawString(operations[step], x - 6, y - 6, 12, 12,
                        Microsoft.Maui.Graphics.HorizontalAlignment.Center,
                        Microsoft.Maui.Graphics.VerticalAlignment.Center);
                }
            }
            else Symbol(title switch
            {
                "Not" => "¬",
                "Mixed" or "Mixed two keyboards" => "⤨",
                "And" => "∩",
                "Or" => "∪",
                "Sum" => "+",
                _ when title.StartsWith("XOR", StringComparison.Ordinal) => "⊕",
                _ when title.Contains("Hand", StringComparison.Ordinal) => "☞",
                _ => "▤"
            });
            canvas.RestoreState();
        }
    }
}
