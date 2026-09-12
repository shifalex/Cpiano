namespace GestureSample.Maui.Views
{
    public sealed class ChoiceAnswerKeyboardView : ContentView
    {
        private readonly Border _surface;
        private readonly Dictionary<int, Button> _buttons = new();
        private int _currentColumnCount = -1;
        private int _maxValue = 10;

        public event Action<int>? ChoicePressed;

        public int MaxValue
        {
            get => _maxValue;
            set
            {
                int normalizedValue = Math.Max(1, value);
                if (_maxValue == normalizedValue)
                    return;

                _maxValue = normalizedValue;
                RebuildLayout(force: true);
            }
        }

        public ChoiceAnswerKeyboardView()
        {
            HorizontalOptions = LayoutOptions.Fill;
            VerticalOptions = LayoutOptions.Center;
            Padding = new Thickness(0, 4, 0, 0);

            _surface = new Border();
            DesignResources.ApplyStyle(_surface, "NumericKeypadSurfaceStyle");
            Content = _surface;

            SizeChanged += (_, _) => RebuildLayout();
            RebuildLayout(force: true);
        }

        public void ShowFeedback(int? value, bool isCorrect)
        {
            ResetFeedback();

            if (!value.HasValue || !_buttons.TryGetValue(value.Value, out Button? button))
                return;

            button.BackgroundColor = isCorrect ? Colors.LightGreen : Colors.IndianRed;
            button.TextColor = Colors.Black;
        }

        public void ResetFeedback()
        {
            foreach (Button button in _buttons.Values)
            {
                button.ClearValue(Button.BackgroundColorProperty);
                button.ClearValue(Button.TextColorProperty);
            }
        }

        private void RebuildLayout(bool force = false)
        {
            int targetColumns = _maxValue > 10
                ? (Width >= 720 ? 10 : 5)
                : (Width >= 620 ? 10 : 5);
            if (!force && targetColumns == _currentColumnCount)
                return;

            _currentColumnCount = targetColumns;
            _surface.Content = BuildChoicesLayout(targetColumns);
        }

        private View BuildChoicesLayout(int columns)
        {
            _buttons.Clear();

            Grid layout = new()
            {
                ColumnSpacing = 8,
                RowSpacing = 8,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center,
                Padding = new Thickness(8)
            };

            for (int column = 0; column < columns; column++)
            {
                layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            }

            int rows = (int)Math.Ceiling(_maxValue / (double)columns);
            for (int row = 0; row < rows; row++)
            {
                layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            for (int value = 1; value <= _maxValue; value++)
            {
                Button button = CreateChoiceButton(value);
                _buttons[value] = button;

                int index = value - 1;
                layout.Add(button, index % columns, index / columns);
            }

            return layout;
        }

        private Button CreateChoiceButton(int value)
        {
            Button button = new()
            {
                Text = value.ToString(),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                MinimumWidthRequest = 72,
                MinimumHeightRequest = 64,
                FontSize = 22,
                FontAttributes = FontAttributes.Bold,
                Padding = new Thickness(0)
            };

            DesignResources.ApplyStyle(button, "NumericKeypadDigitButtonStyle");
            button.Clicked += (_, _) => ChoicePressed?.Invoke(value);
            return button;
        }
    }
}
