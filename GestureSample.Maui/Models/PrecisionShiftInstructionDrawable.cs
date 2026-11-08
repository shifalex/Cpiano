namespace GestureSample.Maui.Models
{
    public sealed class PrecisionShiftInstructionDrawable : IDrawable
    {
        public int Delta { get; set; }
        public bool IsVertical { get; set; } = true;
        public bool BaseAtTop { get; set; }
        public bool IsShift { get; set; }
        public Color StrokeColor { get; set; } = Color.FromArgb("#202733");
        public float TowardArrowTipFromBase { get; set; } = 0.50f;
        public float TowardNumberFromBase { get; set; } = 0.75f;
        public float TowardShaftStopFromBase { get; set; } = 0f;

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (Delta == 0 || dirtyRect.Width <= 0 || dirtyRect.Height <= 0)
                return;

            canvas.SaveState();
            canvas.Antialias = true;
            canvas.StrokeColor = StrokeColor;
            canvas.FillColor = StrokeColor;
            canvas.StrokeSize = 4;
            canvas.StrokeLineCap = LineCap.Round;
            canvas.StrokeLineJoin = LineJoin.Round;

            if (IsVertical)
                DrawVertical(canvas, dirtyRect);
            else
                DrawHorizontal(canvas, dirtyRect);

            canvas.RestoreState();
        }

        private void DrawVertical(ICanvas canvas, RectF bounds)
        {
            float centerX = bounds.Center.X;
            // Use more of the available height so the moving half has breathing
            // room around the unchanged distance badge: --1--> -----|.
            float top = bounds.Top + 10;
            float bottom = bounds.Bottom - 10;
            float baseHalfWidth = MathF.Min(20, bounds.Width * 0.34f);
            bool movesUp = Delta > 0;
            if (IsShift)
            {
                DrawVerticalShift(canvas, bounds, centerX, top, bottom, movesUp);
                return;
            }
            float baseY = BaseAtTop ? top : bottom;
            bool movesAwayFromBase = movesUp != BaseAtTop;
            float oppositeY = BaseAtTop ? bottom : top;
            float fromBaseSign = BaseAtTop ? 1 : -1;
            float span = bottom - top;
            float splitFraction = Math.Clamp(TowardArrowTipFromBase, 0.20f, 0.80f);
            float numberFraction = Math.Clamp(TowardNumberFromBase,
                splitFraction + 0.05f, 0.95f);
            float stopFraction = Math.Clamp(TowardShaftStopFromBase, 0f, 0.25f);
            float splitY = baseY + (fromBaseSign * span * splitFraction);
            // With a 4px round-capped stroke, a 5px geometry gap leaves one
            // visibly empty pixel between the two halves.
            const float halfBreak = 2.5f;
            float fixedHalfStartY = baseY + (fromBaseSign * span * stopFraction);
            float fixedHalfEndY = splitY - (fromBaseSign * halfBreak);
            float movingHalfStartY = splitY + (fromBaseSign * halfBreak);
            float badgeY = baseY + (fromBaseSign * span * numberFraction);

            // The fixed and moving halves meet at a deliberate one-pixel break.
            canvas.DrawLine(centerX - baseHalfWidth, baseY, centerX + baseHalfWidth, baseY);
            canvas.DrawLine(centerX, fixedHalfStartY, centerX, fixedHalfEndY);

            if (movesAwayFromBase)
            {
                float tipY = oppositeY;
                float shaftEndY = movesUp ? tipY + 11 : tipY - 11;
                canvas.DrawLine(centerX, movingHalfStartY, centerX, shaftEndY);
                DrawVerticalArrowHead(canvas, centerX, tipY, movesUp);
            }
            else
            {
                float tipY = movingHalfStartY;
                float shaftEndY = movesUp ? tipY + 11 : tipY - 11;
                canvas.DrawLine(centerX, oppositeY, centerX, shaftEndY);
                DrawVerticalArrowHead(canvas, centerX, tipY, movesUp);
            }

            DrawDistanceBadge(canvas, bounds, centerX, badgeY);
        }

        private void DrawVerticalShift(ICanvas canvas, RectF bounds, float centerX, float top, float bottom, bool movesUp)
        {
            const float extension = 4;
            float extendedTop = Math.Max(bounds.Top + 12, top - extension);
            float extendedBottom = Math.Min(bounds.Bottom - 12, bottom + extension);
            float tipY = movesUp ? extendedTop : extendedBottom;
            float startY = movesUp ? extendedBottom : extendedTop;
            float shaftEndY = movesUp ? tipY + 11 : tipY - 11;
            canvas.StrokeDashPattern = new[] { 2.5f, 2.5f };
            canvas.DrawLine(centerX, startY, centerX, shaftEndY);
            canvas.StrokeDashPattern = null;
            DrawVerticalArrowHead(canvas, centerX, tipY, movesUp);
            DrawDistanceBadge(canvas, bounds, centerX, (extendedTop + extendedBottom) / 2);
        }

        private static void DrawVerticalArrowHead(ICanvas canvas, float centerX, float tipY, bool movesUp)
        {
            const float headWidth = 9;
            const float headHeight = 12;
            PathF arrowHead = new();
            arrowHead.MoveTo(centerX, tipY);
            if (movesUp)
            {
                arrowHead.LineTo(centerX - headWidth, tipY + headHeight);
                arrowHead.LineTo(centerX + headWidth, tipY + headHeight);
            }
            else
            {
                arrowHead.LineTo(centerX - headWidth, tipY - headHeight);
                arrowHead.LineTo(centerX + headWidth, tipY - headHeight);
            }
            arrowHead.Close();
            canvas.FillPath(arrowHead);
        }

        private void DrawHorizontal(ICanvas canvas, RectF bounds)
        {
            float centerY = bounds.Center.Y;
            float left = bounds.Left + 18;
            float right = bounds.Right - 18;
            float baseHalfHeight = MathF.Min(20, bounds.Height * 0.34f);
            bool movesRight = Delta > 0;

            if (IsShift)
            {
                const float extension = 4;
                float extendedLeft = Math.Max(bounds.Left + 12, left - extension);
                float extendedRight = Math.Min(bounds.Right - 12, right + extension);
                float shiftTipX = movesRight ? extendedRight : extendedLeft;
                float startX = movesRight ? extendedLeft : extendedRight;
                float shiftShaftEndX = movesRight ? shiftTipX - 11 : shiftTipX + 11;
                canvas.StrokeDashPattern = new[] { 2.5f, 2.5f };
                canvas.DrawLine(startX, centerY, shiftShaftEndX, centerY);
                canvas.StrokeDashPattern = null;
                DrawHorizontalArrowHead(canvas, shiftTipX, centerY, movesRight);
                DrawDistanceBadge(canvas, bounds, (extendedLeft + extendedRight) / 2, centerY);
                return;
            }

            float baseX = movesRight ? left : right;
            float tipX = movesRight ? right : left;
            float shaftEndX = movesRight ? tipX - 11 : tipX + 11;

            canvas.DrawLine(baseX, centerY - baseHalfHeight, baseX, centerY + baseHalfHeight);
            canvas.DrawLine(baseX, centerY, shaftEndX, centerY);

            const float headWidth = 12;
            const float headHeight = 9;
            PathF arrowHead = new();
            arrowHead.MoveTo(tipX, centerY);
            if (movesRight)
            {
                arrowHead.LineTo(tipX - headWidth, centerY - headHeight);
                arrowHead.LineTo(tipX - headWidth, centerY + headHeight);
            }
            else
            {
                arrowHead.LineTo(tipX + headWidth, centerY - headHeight);
                arrowHead.LineTo(tipX + headWidth, centerY + headHeight);
            }
            arrowHead.Close();
            canvas.FillPath(arrowHead);

            DrawDistanceBadge(canvas, bounds, (left + right) / 2, centerY);
        }

        private static void DrawHorizontalArrowHead(ICanvas canvas, float tipX, float centerY, bool movesRight)
        {
            const float headWidth = 12;
            const float headHeight = 9;
            PathF arrowHead = new();
            arrowHead.MoveTo(tipX, centerY);
            if (movesRight)
            {
                arrowHead.LineTo(tipX - headWidth, centerY - headHeight);
                arrowHead.LineTo(tipX - headWidth, centerY + headHeight);
            }
            else
            {
                arrowHead.LineTo(tipX + headWidth, centerY - headHeight);
                arrowHead.LineTo(tipX + headWidth, centerY + headHeight);
            }
            arrowHead.Close();
            canvas.FillPath(arrowHead);
        }

        private void DrawDistanceBadge(ICanvas canvas, RectF bounds, float centerX, float centerY)
        {
            const float radius = 15;
            canvas.FillColor = Colors.AntiqueWhite;
            canvas.FillCircle(centerX, centerY, radius);
            canvas.StrokeColor = StrokeColor.WithAlpha(0.28f);
            canvas.StrokeSize = 1.5f;
            canvas.DrawCircle(centerX, centerY, radius);

            canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
            canvas.FontSize = 16;
            canvas.FontColor = StrokeColor;
            canvas.DrawString(
                Math.Abs(Delta).ToString(),
                centerX - radius,
                centerY - radius,
                radius * 2,
                radius * 2,
                HorizontalAlignment.Center,
                VerticalAlignment.Center);
        }
    }
}
