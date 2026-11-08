using Microsoft.Maui.Graphics;

namespace GestureSample.Maui.Models;

/// <summary>A side-on pinch. The two contact pads are exactly half a key high.</summary>
public sealed class SideGripDrawable
{
    public void Draw(ICanvas canvas, float tipX, float upperCenter, float lowerCenter,
        float keyHeight, float width, float opacity = 1, float upperFingerExtension = 0)
    {
        canvas.SaveState();
        canvas.Alpha = Math.Clamp(opacity, 0, 1);
        float half = keyHeight / 2;
        float back = tipX + width;
        // The long index finger reaches the upper key; the shorter thumb reaches
        // the lower key. This shared profile is also used by the tutorials.
        float upperTip = tipX - upperFingerExtension;
        float lowerTip = tipX;
        var hand = new PathF();
        hand.MoveTo(upperTip + half / 2, upperCenter - half);
        // Keep the rounded wrist spanning both fingers, as in the reference.
        hand.CurveTo(back - width * .2f, upperCenter - half,
            back + half, upperCenter - half, back + half, upperCenter + keyHeight);
        hand.LineTo(back + half, lowerCenter + half);
        hand.CurveTo(back + half, lowerCenter + keyHeight,
            back - half, lowerCenter + keyHeight, back - width * .4f, lowerCenter + half);
        hand.LineTo(lowerTip + half / 2, lowerCenter + half);
        hand.CurveTo(lowerTip - half / 2, lowerCenter + half,
            lowerTip - half / 2, lowerCenter, lowerTip + half / 2, lowerCenter);
        hand.LineTo(back - width * .48f, lowerCenter);
        hand.CurveTo(back - width * .14f, lowerCenter - half,
            back - width * .14f, upperCenter, back - width * .22f, upperCenter);
        hand.LineTo(upperTip + half / 2, upperCenter);
        hand.CurveTo(upperTip - half / 2, upperCenter,
            upperTip - half / 2, upperCenter - half, upperTip + half / 2, upperCenter - half);
        hand.Close();
        canvas.FillColor = Color.FromArgb("#E8BA95");
        canvas.StrokeColor = Color.FromArgb("#795741");
        canvas.StrokeSize = 1.5f;
        canvas.FillPath(hand);
        canvas.DrawPath(hand);
        // Only the thin outer nail edges are visible from the side.
        float nailThickness = half * .14f;
        canvas.FillColor = Color.FromArgb("#FFF0E3");
        canvas.FillRoundedRectangle(upperTip + half / 2, upperCenter - half + 1,
            width * .2f, nailThickness, nailThickness / 2);
        canvas.FillRoundedRectangle(lowerTip + half / 2, lowerCenter + half - nailThickness - 1,
            width * .24f, nailThickness, nailThickness / 2);
        canvas.StrokeColor = Color.FromArgb("#B18060");
        canvas.DrawLine(back - width * .48f, upperCenter - half + 4, back - width * .46f, upperCenter - 4);
        canvas.DrawLine(back - width * .48f, lowerCenter + 4, back - width * .46f, lowerCenter + half - 4);
        canvas.RestoreState();
    }
}
