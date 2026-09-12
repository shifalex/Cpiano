using Microsoft.Maui.Storage;

namespace GestureSample.Maui.Handlers;

/// <summary>One device-wide choice shared by every stage.</summary>
public static class GripCuePreferences
{
    // The old show/hide preference also controlled the halo. Start the separate
    // always-visible icon preference in reactive mode, including existing installs.
    private const string ShowCueKey = "always_show_grip_eye_hand_v2";
    public static event Action? Changed;

    public static bool ShowEyeHand
    {
        // Reactive by default; an explicit always-visible choice still persists.
        get => Preferences.Default.Get(ShowCueKey, false);
        set
        {
            if (ShowEyeHand == value) return;
            Preferences.Default.Set(ShowCueKey, value);
            Changed?.Invoke();
        }
    }
}
