using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//TODO: Missing objects task - hide some finger behind a curtain an press only on the missing ones

namespace GestureSample.Maui.Models
{
    public class HandDrawable : IDrawable
    {
        public int[] Bits { get; set; } = new int[5];
        private bool IsLeftHand { get; }

        public HandDrawable(bool isLeftHand)
        {
            IsLeftHand = isLeftHand;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect) 
        {
            canvas.StrokeColor = Colors.Black;
            canvas.StrokeSize = 3;
            canvas.FillColor = Colors.SandyBrown;

            // Calculate scaling factors
            float handWidth = dirtyRect.Width * 0.7f;
            float handHeight = dirtyRect.Height * 0.7f;
            float fingerWidth = handWidth / 5;
            float fingerHeight = handHeight * 0.6f;
            float thumbHeight = handHeight * 0.3f;
            float palmHeight = handHeight * 0.4f;
            float palmY = fingerHeight;

            // Draw palm
            RectF palmRect = new RectF(dirtyRect.X + ((IsLeftHand)?3:(dirtyRect.Width - handWidth-3) ), palmY, handWidth, palmHeight);
            canvas.FillRoundedRectangle(palmRect, 0, 0, 30, 30); // Top corners not rounded, bottom corners rounded
            canvas.DrawRoundedRectangle(palmRect, 0, 0, 30, 30);

            // Coordinates for fingers (relative to the palm)
            var fingerCoordinates = new[]
            {
                    new { Base = new PointF(dirtyRect.Width - handWidth -3+ fingerWidth * 0.1f, palmRect.Top*1.08f), Height = thumbHeight },   // Thumb
                    new { Base = new PointF(dirtyRect.Width - handWidth -3 + fingerWidth * 1.5f, palmRect.Top ), Height = fingerHeight*0.85f },   // Index
                    new { Base = new PointF(dirtyRect.Width - handWidth -3+ fingerWidth * 2.5f, palmRect.Top ), Height = fingerHeight *0.95f },   // Middle
                    new { Base = new PointF(dirtyRect.Width - handWidth -3+ fingerWidth * 3.5f, palmRect.Top ), Height = fingerHeight*0.85f }, // Ring
                    new { Base = new PointF(dirtyRect.Width - handWidth -3+ fingerWidth * 4.5f, palmRect.Top ), Height = fingerHeight*0.65f }  // Pinky
                };

            // Draw fingers based on the bit array
            for (int i = 0; i < Bits.Length; i++)
            {
                var finger = fingerCoordinates[i];
                float baseX = IsLeftHand ? dirtyRect.Width - finger.Base.X : finger.Base.X;
                if (Bits[i] == 1)
                {
                    if (i == 0)
                        DrawThumb(canvas, baseX, finger.Base.Y, fingerWidth, finger.Height, IsLeftHand ? 30 : -30);
                    else
                       DrawFinger(canvas, baseX, finger.Base.Y, fingerWidth, finger.Height);
                }
                else
                {
                    DrawFoldedFinger(canvas, baseX, finger.Base.Y, fingerWidth);
                }
            }
        }

        private void DrawFinger(ICanvas canvas, float baseX, float baseY, float width, float height)
        {
            // Draw half-rounded rectangle for the finger
            RectF rect = new RectF(baseX - width / 2, baseY - height, width, height);
            canvas.FillRoundedRectangle(rect, width / 2, width / 2, 0, 0);
            canvas.DrawRoundedRectangle(rect, width / 2, width / 2, 0, 0);
        }
        private void DrawThumb(ICanvas canvas, float baseX, float baseY, float width, float height, int angel)
        {
            canvas.SaveState();
            canvas.Translate(baseX, baseY);
            canvas.Rotate(angel); // Rotate the thumb to the left
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
            path.LineTo(baseX, baseY + width*2);
            path.Close();
            canvas.FillPath(path);
            canvas.DrawPath(path);
        }
    }
}
