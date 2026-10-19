namespace GestureSample.Maui.Models;

internal static class GripAnswerTiming
{
    // The explicit child option takes precedence over the stage's legacy shortcut.
    public static bool AllowsImmediate(bool childStage, bool requireHold, bool stageImmediate) =>
        childStage ? !requireHold : stageImmediate;

    public static bool HasHeldLongEnough(double? elapsedSeconds, int seconds) =>
        elapsedSeconds.HasValue && elapsedSeconds.Value >= Math.Max(1, seconds);
}
