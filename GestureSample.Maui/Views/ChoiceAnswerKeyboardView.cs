namespace GestureSample.Maui.Views
{
    public sealed class ChoiceAnswerKeyboardView : ContentView
    {
        public event Action<int>? ChoicePressed;

        public ChoiceAnswerKeyboardView(int maxChoice = 10, int preferredRowCount = 0)
        {
            HorizontalOptions = LayoutOptions.Center;
            Padding = new Thickness(0, 2, 0, 0);

            maxChoice = Math.Max(1, maxChoice);

            Border surface = new()
            {
                Content = BuildChoicesLayout(maxChoice, preferredRowCount)
            };
            DesignResources.ApplyStyle(surface, "NumericKeypadSurfaceStyle");
            Content = surface;
        }

        private Grid BuildChoicesLayout(int maxChoice, int preferredRowCount)
        {
            int rowCount = preferredRowCount > 0
                ? Math.Min(maxChoice, preferredRowCount)
                : 0;
            int columnCount = rowCount > 0
                ? (int)Math.Ceiling(maxChoice / (double)rowCount)
                : maxChoice <= 10 ? 5 : 10;
            rowCount = rowCount > 0
                ? rowCount
                : (int)Math.Ceiling(maxChoice / (double)columnCount);
            Grid grid = new()
            {
                ColumnSpacing = 6,
                RowSpacing = 6
            };

            for (int column = 0; column < columnCount; column++)
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

            for (int row = 0; row < rowCount; row++)
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            for (int value = 1; value <= maxChoice; value++)
            {
                Button button = CreateChoiceButton(value);
                int index = value - 1;
                grid.Add(button, index % columnCount, index / columnCount);
            }

            return grid;
        }

        private Button CreateChoiceButton(int value)
        {
            Button button = new()
            {
                Text = value.ToString(),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                MinimumHeightRequest = 52
            };

            DesignResources.ApplyStyle(button, "NumericKeypadDigitButtonStyle");
            button.MinimumWidthRequest = 0;
            button.Clicked += (_, _) => ChoicePressed?.Invoke(value);
            return button;
        }
    }
}
