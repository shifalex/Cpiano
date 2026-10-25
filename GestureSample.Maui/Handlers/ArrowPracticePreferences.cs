using System.Text.Json;
using Microsoft.Maui.Storage;

namespace GestureSample.Maui.Handlers;

// Store only the options selected in the UI, never a gameplay config or session state.
public sealed record ArrowPracticeSelection
{
    public bool? Sound { get; init; }
    public bool? Reading { get; init; }
    public bool? AccumulatingColor { get; init; }
    public bool? KeyNumbers { get; init; }
    public int? MaxLength { get; init; }
    public int? StartingPosition { get; init; }
    public int? TenBoundary { get; init; }
}

public static class ArrowPracticePreferences
{
    private static string Key(Guid? userId, string stage) =>
        $"arrow_practice_v1_{userId?.ToString("D") ?? "guest"}_{stage}";

    public static ArrowPracticeSelection Load(Guid? userId, string stage)
    {
        string json = Preferences.Default.Get(Key(userId, stage), "");
        if (string.IsNullOrWhiteSpace(json)) return new();
        try
        {
            var saved = JsonSerializer.Deserialize<ArrowPracticeSelection>(json) ?? new();
            return saved with
            {
                MaxLength = saved.MaxLength >= 0 ? saved.MaxLength : null,
                StartingPosition = saved.StartingPosition is >= 0 and <= 3 ? saved.StartingPosition : null,
                TenBoundary = saved.TenBoundary is >= 0 and <= 2 ? saved.TenBoundary : null
            };
        }
        catch (JsonException)
        {
            return new();
        }
    }

    public static void Save(Guid? userId, string stage, ArrowPracticeSelection selection) =>
        Preferences.Default.Set(Key(userId, stage), JsonSerializer.Serialize(selection));

    public static void Reset(Guid? userId, string stage) => Preferences.Default.Remove(Key(userId, stage));

    private static string PathKey(Guid? userId) => $"arrow_label_path_v1_{userId?.ToString("D") ?? "guest"}";

    public static int LoadLabelStep(Guid? userId) =>
        int.TryParse(Preferences.Default.Get(PathKey(userId), ""), out int step) && step is >= 0 and <= 3 ? step : 0;

    public static void SaveLabelStep(Guid? userId, int step)
    {
        if (step is < 0 or > 3) throw new ArgumentOutOfRangeException(nameof(step));
        Preferences.Default.Set(PathKey(userId), step.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }
}
