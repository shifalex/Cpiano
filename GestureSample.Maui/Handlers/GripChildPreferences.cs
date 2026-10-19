using Microsoft.Maui.Storage;
namespace GestureSample.Maui.Handlers;
/// <summary>Optional assistance for the first grip activities.</summary>
public static class GripChildPreferences
{
    public static bool RequireHold { get => Preferences.Default.Get("grip_child_require_hold", true); set => Preferences.Default.Set("grip_child_require_hold", value); }
    public static bool GripOnly { get => Preferences.Default.Get("grip_child_grip_only", false); set => Preferences.Default.Set("grip_child_grip_only", value); }
    public static bool HandTutorial { get => Preferences.Default.Get("grip_child_hand_tutorial", false); set => Preferences.Default.Set("grip_child_hand_tutorial", value); }
    public static bool StartAtBottom { get => Preferences.Default.Get("grip_child_bottom", false); set => Preferences.Default.Set("grip_child_bottom", value); }
    public static bool ColorFeedback { get => Preferences.Default.Get("grip_child_colors", false); set => Preferences.Default.Set("grip_child_colors", value); }
    public static bool StartImmediately { get => Preferences.Default.Get("grip_child_immediate", false); set => Preferences.Default.Set("grip_child_immediate", value); }
}
