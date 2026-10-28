namespace GestureSample.Maui.Models;

/// <summary>The ordered, zero-based introduction to the movement arrows.</summary>
public static class PrecisionSignLearningIntro
{
    public const int ShiftCount = 5;
    public const int Count = ShiftCount + 4;

    public static (int LowerRow, int UpperRow, int Delta, bool IsShift, bool BaseAtTop) GetStep(int index) => index switch
    {
        0 => (0, 2,  1, true, false),
        1 => (1, 3, -1, true, false),
        2 => (0, 2,  2, true, false),
        3 => (2, 4, -2, true, false),
        4 => (1, 3,  1, true, false),
        // Keep the lower endpoint fixed: expand, then contract upward/downward.
        5 => (0, 2,  2, false, false),
        6 => (0, 4, -2, false, false),
        // Keep the upper endpoint fixed: expand, then contract downward/upward.
        7 => (2, 4, -2, false, true),
        8 => (0, 4,  2, false, true),
        _ => throw new ArgumentOutOfRangeException(nameof(index))
    };
}
