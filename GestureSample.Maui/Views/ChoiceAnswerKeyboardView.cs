namespace GestureSample.Maui.Views
{
    public sealed class ChoiceAnswerKeyboardView : ContentView
    {
        public event Action<int>? ChoicePressed;

        public ChoiceAnswerKeyboardView()
        {
            HorizontalOptions = LayoutOptions.Center;
            Padding = new Thickness(0, 2, 0, 0);

            Border surface = new()
            {
                Content = BuildChoicesLayout()
            };
            DesignResources.ApplyStyle(surface, "NumericKeypadSurfaceStyle");
            Content = surface;
        }

        private Grid BuildChoicesLayout()
        {
            Grid grid = new()
            {
                ColumnSpacing = 6,
                RowSpacing = 6,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star }
                },
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                }
            };

            for (int value = 1; value <= 10; value++)
            {
                Button button = CreateChoiceButton(value);
                int index = value - 1;
                grid.Add(button, index % 5, index / 5);
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
