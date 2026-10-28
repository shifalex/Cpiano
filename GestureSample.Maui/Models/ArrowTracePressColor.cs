namespace GestureSample.Maui.Models;

public static class ArrowTracePressColor
{
    public static bool UseRedForLabel(bool displayedRtl, int startValue, int endValue, int pressedKey, bool throughTen)
    {
        // A missing-start prompt can point left while its stored equation is addition.
        // The tail of the displayed arrow is the existing quantity, not necessarily
        // the first operand of the equation.
        int tailValue = displayedRtl ? Math.Max(startValue, endValue) : Math.Min(startValue, endValue);
        int tailKey = ((tailValue - 1) % 10 + 10) % 10 + 1;
        return UseRed(displayedRtl, tailKey, Math.Abs(endValue - startValue), pressedKey, throughTen);
    }

    public static bool UseRed(bool isRtl, int startingKey, int distance, int pressedKey, bool throughTen)
    {
        // LTR keeps its existing yellow press color, even when crossing ten.
        if (!isRtl) return false;
        if (throughTen || distance >= startingKey) return true;

        // The starting key belongs to the existing quantity. Keys beyond it do not.
        return pressedKey <= startingKey;
    }
}
