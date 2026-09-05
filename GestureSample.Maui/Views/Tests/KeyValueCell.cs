namespace GestureSample.Views.Tests
{
    public class KeyValueCell : ViewCell
    {
        public KeyValueCell(string key, string value)
        {
            Grid grid = new()
            {
                Padding = new Thickness(15, 10),
                VerticalOptions = LayoutOptions.Center,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            grid.Add(new Label
            {
                Text = key,
                TextColor = Colors.Purple,
                HorizontalOptions = LayoutOptions.Start
            }, 0, 0);

            grid.Add(new Label
            {
                Text = value,
                TextColor = Colors.Gray,
                HorizontalOptions = LayoutOptions.End
            }, 1, 0);

            View = grid;
        }
    }
}
