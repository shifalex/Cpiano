namespace GestureSample.Maui.Views
{
    public sealed class NumericKeypadView : ContentView
    {
        public event Action<string>? DigitPressed;
        public event Action? BackspacePressed;
        public event Action? ClearPressed;
        public event Action? SubmitPressed;
        private double _dragStartTranslationX;
        private double _dragStartTranslationY;

        public NumericKeypadView()
        {
            HorizontalOptions = LayoutOptions.Center;
            Padding = new Thickness(0, 2, 0, 0);
            MinimumWidthRequest = 280;

            Border surface = new()
            {
                Content = BuildKeypadLayout()
            };
            DesignResources.ApplyStyle(surface, "NumericKeypadSurfaceStyle");
            PanGestureRecognizer panGesture = new();
            panGesture.PanUpdated += OnPanUpdated;
            surface.GestureRecognizers.Add(panGesture);

            Content = surface;
        }

        private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _dragStartTranslationX = TranslationX;
                    _dragStartTranslationY = TranslationY;
                    break;

                case GestureStatus.Running:
                    TranslationX = _dragStartTranslationX + e.TotalX;
                    TranslationY = _dragStartTranslationY + e.TotalY;
                    break;

                case GestureStatus.Canceled:
                case GestureStatus.Completed:
                    _dragStartTranslationX = TranslationX;
                    _dragStartTranslationY = TranslationY;
                    break;
            }
        }

        private Grid BuildKeypadLayout()
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
                    new ColumnDefinition { Width = new GridLength(1.14, GridUnitType.Star) }
                },
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(54) },
                    new RowDefinition { Height = new GridLength(54) },
                    new RowDefinition { Height = new GridLength(54) },
                    new RowDefinition { Height = new GridLength(54) }
                }
            };

            string[,] digits =
            {
                { "1", "2", "3" },
                { "4", "5", "6" },
                { "7", "8", "9" },
                { "-", "0", "C" }
            };

            for (int row = 0; row < digits.GetLength(0); row++)
            {
                for (int col = 0; col < digits.GetLength(1); col++)
                {
                    string key = digits[row, col];
                    if (string.IsNullOrWhiteSpace(key))
                        continue;

                    grid.Add(CreateGridButton(key), col, row);
                }
            }

            Button backspaceButton = CreateActionButton("<-", () => BackspacePressed?.Invoke());
            grid.Add(backspaceButton, 3, 0);
            Grid.SetRowSpan(backspaceButton, 1);
            backspaceButton.MinimumWidthRequest = 72;

            Button submitButton = CreateActionButton("V", () => SubmitPressed?.Invoke());
            Grid.SetColumn(submitButton, 3);
            Grid.SetRow(submitButton, 1);
            Grid.SetRowSpan(submitButton, 3);
            submitButton.VerticalOptions = LayoutOptions.Fill;
            submitButton.HeightRequest = (54 * 3) + (6 * 2);
            submitButton.MinimumHeightRequest = submitButton.HeightRequest;
            submitButton.MinimumWidthRequest = 72;
            grid.Children.Add(submitButton);

            return grid;
        }

        private Grid BuildDigitGrid()
        {
            Grid grid = new()
            {
                ColumnSpacing = 6,
                RowSpacing = 6,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star }
                },
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                }
            };

            string[,] keys =
            {
                { "1", "2", "3" },
                { "4", "5", "6" },
                { "7", "8", "9" },
                { "C", "0", "<-" }
            };

            for (int row = 0; row < keys.GetLength(0); row++)
            {
                for (int col = 0; col < keys.GetLength(1); col++)
                {
                    string key = keys[row, col];
                    if (string.IsNullOrWhiteSpace(key))
                        continue;

                    grid.Add(CreateGridButton(key), col, row);
                }
            }

            return grid;
        }

        private Button CreateGridButton(string text)
        {
            return text switch
            {
                "C" => CreateActionButton(text, () => ClearPressed?.Invoke()),
                _ => CreateActionButton(text, () => DigitPressed?.Invoke(text))
            };
        }

        private Button CreateActionButton(string text, Action onPressed)
        {
            Button button = new()
            {
                Text = text,
                HorizontalOptions = LayoutOptions.Fill
            };

            string styleKey = text switch
            {
                "V" => "NumericKeypadSubmitButtonStyle",
                "C" => "NumericKeypadActionButtonStyle",
                "-" => "NumericKeypadActionButtonStyle",
                "<-" => "NumericKeypadActionButtonStyle",
                _ => "NumericKeypadDigitButtonStyle"
            };
            DesignResources.ApplyStyle(button, styleKey);

            button.Clicked += (_, _) => onPressed();
            return button;
        }
    }
}
