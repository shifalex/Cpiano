using Microsoft.Maui.Controls.Shapes;

namespace GestureSample.Maui.Models
{
    internal static class KeyboardArrowVisualFactory
    {
        public static Grid CreateArrowVisual(
            string pathData,
            int numberAbove,
            int strokeThickness,
            string? labelTextOverride = null,
            double? labelCenterX = null)
        {
            Grid arrowVisual = new()
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = 30 }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star }
                },
                FlowDirection = FlowDirection.LeftToRight
            };

            bool showLabel = labelTextOverride != string.Empty && (labelTextOverride != null || numberAbove > -1);
            Label numberLabel = new()
            {
                Text = labelTextOverride ?? numberAbove.ToString(),
                TextColor = Color.FromArgb("#111827"),
                FontSize = 20,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                FontAttributes = Microsoft.Maui.Controls.FontAttributes.Bold,
                FontFamily = DeviceInfo.Platform == DevicePlatform.iOS ? "HelveticaNeue-Bold" : null,
                FlowDirection = FlowDirection.LeftToRight
            };

            Border labelChip = new()
            {
                Content = numberLabel,
                BackgroundColor = Color.FromArgb("#F8FAFC"),
                Stroke = Color.FromArgb("#CBD5E1"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                HeightRequest = 28,
                WidthRequest = 42,
                Padding = new Thickness(0),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                IsVisible = showLabel
            };
            if (labelCenterX != null)
            {
                const double labelWidth = 42;
                labelChip.HorizontalOptions = LayoutOptions.Start;
                labelChip.TranslationX = Math.Max(0, labelCenterX.Value - (labelWidth / 2));
            }

            Microsoft.Maui.Controls.Shapes.Path arrow = new()
            {
                Data = (Geometry)new PathGeometryConverter().ConvertFromInvariantString(pathData),
                Fill = Colors.Transparent,
                Stroke = Color.FromArgb("#F8FAFC"),
                StrokeThickness = strokeThickness,
                StrokeLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center
            };

            arrowVisual.Add(labelChip, 0, 0);
            arrowVisual.Add(arrow, 0, 1);
            return arrowVisual;
        }
    }
}
