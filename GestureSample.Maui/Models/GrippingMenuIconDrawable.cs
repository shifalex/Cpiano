namespace GestureSample.Maui.Models
{
    public enum GrippingMenuIcon
    {
        OneHand,
        TwoHands,
        HandOverKeyboard,
        ChangingHands,
        ShiftDown,
        UpperUpAndDown,
        UpperUpAndShiftUp
    }

    // Draw directly so menu icons also work without packaged image resources.
    public sealed class GrippingMenuIconDrawable(GrippingMenuIcon icon) : IDrawable
    {
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (dirtyRect.Width <= 0 || dirtyRect.Height <= 0)
                return;

            canvas.SaveState();
            float scale = Math.Min(dirtyRect.Width, dirtyRect.Height) / 36f;
            canvas.Translate(dirtyRect.X + (dirtyRect.Width - 36 * scale) / 2,
                dirtyRect.Y + (dirtyRect.Height - 36 * scale) / 2);
            canvas.Scale(scale, scale);
            canvas.Antialias = true;
            canvas.StrokeColor = Colors.White;
            canvas.FillColor = Colors.White;
            canvas.StrokeSize = 2.2f;
            canvas.StrokeLineCap = LineCap.Round;
            canvas.StrokeLineJoin = LineJoin.Round;

            switch (icon)
            {
                case GrippingMenuIcon.OneHand:
                    DrawHand(canvas, 0, 0, 1, 1);
                    break;
                case GrippingMenuIcon.TwoHands:
                    DrawHand(canvas, -3, 5, .7f, .7f);
                    DrawHand(canvas, 39, 5, -.7f, .7f);
                    break;
                case GrippingMenuIcon.HandOverKeyboard:
                    // A vertical column of keys, with the two finger targets colored.
                    for (int key = 0; key < 7; key++)
                    {
                        canvas.FillColor = key is 1 or 4
                            ? Color.FromArgb("#FFE06A") : Colors.White;
                        canvas.FillRoundedRectangle(19, 3 + key * 4.4f, 14, 3.4f, .8f);
                    }
                    canvas.FillColor = Colors.White;
                    DrawHand(canvas, -1, 4, .8f, .8f);
                    break;
                case GrippingMenuIcon.ChangingHands:
                    DrawHand(canvas, -3, 12, .7f, .7f);
                    DrawHand(canvas, 39, -2, -.7f, .7f);
                    canvas.StrokeSize = 1.5f;
                    canvas.DrawLine(9, 3, 9, 11);
                    DrawArrowHead(canvas, 9, 11, false, 2.5f);
                    canvas.DrawLine(27, 33, 27, 25);
                    DrawArrowHead(canvas, 27, 25, true, 2.5f);
                    break;
                case GrippingMenuIcon.ShiftDown:
                    canvas.SaveState();
                    canvas.Translate(0, 36);
                    canvas.Scale(1, -1);
                    DrawShiftUp(canvas, 18);
                    canvas.RestoreState();
                    break;
                case GrippingMenuIcon.UpperUpAndDown:
                    DrawUpperMove(canvas, 10, true);
                    DrawUpperMove(canvas, 26, false);
                    break;
                case GrippingMenuIcon.UpperUpAndShiftUp:
                    DrawUpperMove(canvas, 10, true, upperTipY: 12);
                    DrawShiftUp(canvas, 26, startY: 20);
                    break;
            }
            canvas.RestoreState();
        }

        private static void DrawShiftUp(ICanvas canvas, float x, float startY = 31)
        {
            // A dashed shaft moves the whole grip; it has no fixed base.
            for (float y = startY; y > 11; y -= 6)
                canvas.DrawLine(x, y, x, Math.Max(11, y - 3));
            DrawArrowHead(canvas, x, 5, true);
        }

        private static void DrawUpperMove(ICanvas canvas, float x, bool up, float upperTipY = 5)
        {
            // The lower finger stays on the base; only the upper half moves.
            canvas.DrawLine(x - 5, 31, x + 5, 31);
            canvas.DrawLine(x, 31, x, 23);
            canvas.DrawLine(x, up ? 20 : 5, x, up ? upperTipY + 5 : 13);
            DrawArrowHead(canvas, x, up ? upperTipY : 18, up);
        }

        private static void DrawArrowHead(ICanvas canvas, float x, float y, bool up, float size = 4)
        {
            float back = y + (up ? size * 1.3f : -size * 1.3f);
            var head = new PathF();
            head.MoveTo(x, y);
            head.LineTo(x - size, back);
            head.LineTo(x + size, back);
            head.Close();
            canvas.FillPath(head);
        }

        private static void DrawHand(ICanvas canvas, float x, float y, float sx, float sy)
        {
            canvas.SaveState();
            canvas.Translate(x, y);
            canvas.Scale(sx, sy);
            var hand = new PathF();
            hand.MoveTo(12, 30);
            hand.LineTo(12, 26);
            hand.CurveTo(8, 23, 7, 20, 7, 15);
            hand.LineTo(7, 11);
            hand.CurveTo(7, 7, 10, 5, 14, 5);
            hand.LineTo(23, 5);
            hand.CurveTo(27, 5, 27, 10, 23, 10);
            hand.LineTo(15, 10);
            hand.CurveTo(13, 10, 12, 12, 12, 15);
            hand.LineTo(12, 18);
            hand.LineTo(20, 15);
            hand.CurveTo(24, 14, 26, 18, 22, 20);
            hand.LineTo(17, 23);
            hand.LineTo(17, 30);
            canvas.DrawPath(hand);
            canvas.RestoreState();
        }
    }
}
