using Microsoft.Maui.Controls.Shapes;

namespace GestureSample.Maui.Models
{
    internal static class KeyboardArrowVisualFactory
    {
        public static Grid CreateArrowVisual(string pathData, int numberAbove, int strokeThickness, string? labelTextOverride = null)
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

            Label numberLabel = new()
            {
                Text = labelTextOverride ?? numberAbove.ToString(),
                TextColor = Colors.White,
                FontSize = 25,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                FontAttributes = Microsoft.Maui.Controls.FontAttributes.Bold,
                FontFamily = DeviceInfo.Platform == DevicePlatform.iOS ? "HelveticaNeue-Bold" : null,
                FlowDirection = FlowDirection.LeftToRight,
                IsVisible = labelTextOverride != string.Empty && (labelTextOverride != null || numberAbove > -1)
            };

            Microsoft.Maui.Controls.Shapes.Path arrow = new()
            {
                Data = (Geometry)new PathGeometryConverter().ConvertFromInvariantString(pathData),
                Fill = Colors.Transparent,
                Stroke = Colors.White,
                StrokeThickness = strokeThickness,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center
            };

            arrowVisual.Add(numberLabel, 0, 0);
            arrowVisual.Add(arrow, 0, 1);
            return arrowVisual;
        }
    }
}
