using Microsoft.Maui.Layouts;

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

        private View BuildChoicesLayout()
        {
            FlexLayout layout = new()
            {
                Direction = FlexDirection.Row,
                Wrap = FlexWrap.Wrap,
                JustifyContent = FlexJustify.Center,
                AlignItems = FlexAlignItems.Center,
                AlignContent = FlexAlignContent.Center
            };

            for (int value = 1; value <= 10; value++)
            {
                Button button = CreateChoiceButton(value);
                layout.Children.Add(button);
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
                MinimumWidthRequest = 52,
                MinimumHeightRequest = 52,
                Margin = new Thickness(3)
            };

            FlexLayout.SetBasis(button, new FlexBasis(18, true));
            FlexLayout.SetGrow(button, 1);
            FlexLayout.SetShrink(button, 1);

            DesignResources.ApplyStyle(button, "NumericKeypadDigitButtonStyle");
            button.Clicked += (_, _) => ChoicePressed?.Invoke(value);
            return button;
        }
    }
}
