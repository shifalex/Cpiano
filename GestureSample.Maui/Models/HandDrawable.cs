using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.ApplicationModel;

namespace GestureSample.Maui.Models
{
    public class HandDrawable : IDrawable
    {
        public int[] Bits { get; set; } = new int[5];

        // Position is the top-left offset applied to the whole hand drawing.
        public PointF Position { get; set; } = new PointF(0, 0);

        // Opacity from 0 (invisible) to 1 (opaque).
        public float Opacity { get; set; } = 1f;

        private bool IsLeftHand { get; }

        public HandDrawable(bool isLeftHand)
        {
            IsLeftHand = isLeftHand;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (Opacity <= 0.001f)
                return;

            // apply translation and draw with alpha
            canvas.SaveState();
            canvas.Translate(Position.X, Position.Y);

            var fillColor = Colors.SandyBrown.WithAlpha(Opacity);
            var strokeColor = Colors.Black.WithAlpha(Opacity);

            canvas.StrokeColor = strokeColor;
            canvas.StrokeSize = 3;
            canvas.FillColor = fillColor;

            // Calculate scaling factors relative to the available dirty rect
            float handWidth = dirtyRect.Width * 0.7f;
            float handHeight = dirtyRect.Height * 0.7f;
            float fingerWidth = handWidth / 5;
            float fingerHeight = handHeight * 0.6f;
            float thumbHeight = handHeight * 0.3f;
            float palmHeight = handHeight * 0.4f;
            float palmY = fingerHeight;

            // Left/right placement within the dirty rect
            float baseX = dirtyRect.X + (IsLeftHand ? 3f : (dirtyRect.Width - handWidth - 3f));
            RectF palmRect = new RectF(baseX, palmY, handWidth, palmHeight);
            canvas.FillRoundedRectangle(palmRect, 0, 0, 30, 30);
            canvas.DrawRoundedRectangle(palmRect, 0, 0, 30, 30);

            // Coordinates for fingers (relative to baseX)
            var fingerCoordinates = new[]
            {
                new { X = baseX + fingerWidth * 0.1f, Y = palmRect.Top * 1.08f, Height = thumbHeight },   // Thumb
                new { X = baseX + fingerWidth * 1.5f, Y = palmRect.Top, Height = fingerHeight * 0.85f },   // Index
                new { X = baseX + fingerWidth * 2.5f, Y = palmRect.Top, Height = fingerHeight * 0.95f },   // Middle
                new { X = baseX + fingerWidth * 3.5f, Y = palmRect.Top, Height = fingerHeight * 0.85f },   // Ring
                new { X = baseX + fingerWidth * 4.5f, Y = palmRect.Top, Height = fingerHeight * 0.65f }    // Pinky
            };

            for (int i = 0; i < Bits.Length && i < fingerCoordinates.Length; i++)
            {
                var finger = fingerCoordinates[i];
                float fingerBaseX = IsLeftHand ? (dirtyRect.Width - finger.X) : finger.X;

                if (Bits[i] == 1)
                {
                    if (i == 0)
                        DrawThumb(canvas, fingerBaseX, finger.Y, fingerWidth, finger.Height, IsLeftHand ? 30 : -30);
                    else
                        DrawFinger(canvas, fingerBaseX, finger.Y, fingerWidth, finger.Height);
                }
                else
                {
                    DrawFoldedFinger(canvas, fingerBaseX, finger.Y, fingerWidth);
                }
            }

            canvas.RestoreState();
        }

        private void DrawFinger(ICanvas canvas, float baseX, float baseY, float width, float height)
        {
            RectF rect = new RectF(baseX - width / 2, baseY - height, width, height);
            canvas.FillRoundedRectangle(rect, width / 2, width / 2, 0, 0);
            canvas.DrawRoundedRectangle(rect, width / 2, width / 2, 0, 0);
        }

        private void DrawThumb(ICanvas canvas, float baseX, float baseY, float width, float height, int angle)
        {
            canvas.SaveState();
            canvas.Translate(baseX, baseY);
            canvas.Rotate(angle);
            RectF rect = new RectF(-width / 2, -height, width, height);
            canvas.FillRoundedRectangle(rect, width / 2, width / 2, 0, 0);
            canvas.DrawRoundedRectangle(rect, width / 2, width / 2, 0, 0);
            canvas.RestoreState();
        }

        private void DrawFoldedFinger(ICanvas canvas, float baseX, float baseY, float width)
        {
            PathF path = new();
            path.MoveTo(baseX - width / 2, baseY);
            path.LineTo(baseX + width / 2, baseY);
            path.LineTo(baseX, baseY + width * 2);
            path.Close();
            canvas.FillPath(path);
            canvas.DrawPath(path);
        }

        // ---- Animation helpers ----
        private static PointF Lerp(PointF a, PointF b, float t)
            => new PointF(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);

        private void InvalidateGraphicsView(GraphicsView? view)
        {
            if (view == null) return;
            MainThread.BeginInvokeOnMainThread(() => view.Invalidate());
        }

        // Move the hand smoothly to `to` in the given duration. `steps` controls smoothness.
        public async Task AnimateMoveAsync(GraphicsView host, PointF to, TimeSpan duration, int steps = 30)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            var from = Position;
            if (duration <= TimeSpan.Zero || steps <= 0)
            {
                Position = to;
                InvalidateGraphicsView(host);
                return;
            }

            for (int i = 1; i <= steps; i++)
            {
                float t = i / (float)steps;
                Position = Lerp(from, to, t);
                InvalidateGraphicsView(host);
                await Task.Delay(duration / steps).ConfigureAwait(false);
            }
            Position = to;
            InvalidateGraphicsView(host);
        }

        // Fade in to opacity 1
        public async Task ShowAsync(GraphicsView host, TimeSpan duration, int steps = 15)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            float from = Opacity;
            for (int i = 1; i <= steps; i++)
            {
                float t = i / (float)steps;
                Opacity = from + (1f - from) * t;
                InvalidateGraphicsView(host);
                await Task.Delay(duration / steps).ConfigureAwait(false);
            }
            Opacity = 1f;
            InvalidateGraphicsView(host);
        }

        // Fade out to opacity 0
        public async Task HideAsync(GraphicsView host, TimeSpan duration, int steps = 15)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            float from = Opacity;
            for (int i = 1; i <= steps; i++)
            {
                float t = i / (float)steps;
                Opacity = from + (0f - from) * t;
                InvalidateGraphicsView(host);
                await Task.Delay(duration / steps).ConfigureAwait(false);
            }
            Opacity = 0f;
            InvalidateGraphicsView(host);
        }

        // Convenience: show, move, then optionally hide
        public async Task ShowMoveHideAsync(GraphicsView host, PointF target, TimeSpan moveDuration, TimeSpan fadeDuration, bool hideAfter = true)
        {
            // start invisible, show quickly, move, then hide
            Opacity = 0f;
            InvalidateGraphicsView(host);
            await ShowAsync(host, TimeSpan.FromMilliseconds(Math.Min(300, fadeDuration.TotalMilliseconds)));
            await AnimateMoveAsync(host, target, moveDuration);
            if (hideAfter)
                await HideAsync(host, fadeDuration);
        }
    }
}
