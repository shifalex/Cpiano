using System.Text.Json;
using Microsoft.Maui.Storage;

namespace GestureSample.Maui.Handlers;

public static class CustomArrowPracticePreferences
{
    private static string Key(Guid? userId) => $"custom_arrow_builder_v1_{userId?.ToString("D") ?? "guest"}";
    public static CustomArrowPracticeSelection Load(Guid? userId)
    {
        try
        {
            var selection = JsonSerializer.Deserialize<CustomArrowPracticeSelection>(Preferences.Default.Get(Key(userId), "null"));
            return selection?.ValidationError() == null && selection != null ? selection : new();
        }
        catch (JsonException) { return new(); }
    }
    public static void Save(Guid? userId, CustomArrowPracticeSelection selection)
    {
        if (selection.ValidationError() is string error) throw new ArgumentException(error, nameof(selection));
        Preferences.Default.Set(Key(userId), JsonSerializer.Serialize(selection));
    }
}
