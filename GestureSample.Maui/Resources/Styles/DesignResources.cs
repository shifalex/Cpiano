namespace GestureSample.Maui;

internal static class DesignResources
{
    public static Color GetColor(string key, Color fallback)
    {
        if (Application.Current?.Resources == null)
            return fallback;

        if (!Application.Current.Resources.TryGetValue(key, out object? value) || value == null)
            return fallback;

        return value switch
        {
            Color color => color,
            SolidColorBrush brush => brush.Color,
            _ => fallback
        };
    }

    public static void ApplyStyle(VisualElement element, string styleKey)
    {
        if (Application.Current?.Resources == null)
            return;

        if (Application.Current.Resources.TryGetValue(styleKey, out object? value) && value is Style style)
            element.Style = style;
    }
}
