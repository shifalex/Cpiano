using Microsoft.Maui.Storage;

namespace GestureSample.Maui.Handlers;

public static class StageNavigationPreferences
{
    private static string StageKey(string stage) => stage switch
    {
        "Copy -one hand" => "Copy",
        "Mirror - one hand" => "Mirror",
        "Or-And-Neutralise" => "Or-And-XOR",
        "Group To Side" => "Group to side",
        "Mixed" => "Mixed one operation",
        _ when stage.StartsWith("Shift by", StringComparison.Ordinal) => "Shift by",
        _ when stage.StartsWith("Arrow Label", StringComparison.Ordinal) => "Arrow Label",
        _ => stage
    };
    private static string Key(string stage) => $"stage_manual_advance_v1_{LanguagePreferences.UserId?.ToString("D") ?? "guest"}_{StageKey(stage)}";
    public static bool IsManual(string stage) => Preferences.Default.Get(Key(stage), false);
    public static void SetManual(string stage, bool manual) => Preferences.Default.Set(Key(stage), manual);
}
