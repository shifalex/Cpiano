using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureSample.Maui.Models
{
    public sealed class KeyboardOverlayHost : Grid
    {
        private sealed class PatternDrawable : IDrawable
        {
            public RectF[] KeyRects { get; set; } = Array.Empty<RectF>();

            // What’s currently highlighted (static)
            public bool[] Bits { get; set; } = Array.Empty<bool>();

            // Animation shift (in keys)
            public float ShiftKeys { get; set; } = 0f;

            // Optional cursor: draw one rect at a fractional index (e.g., 3.2)
            public float? CursorIndex { get; set; } = null;
            public float CursorAlpha { get; set; } = 0.7f;

            public float FillAlpha { get; set; } = 0.35f;
            public float StrokeAlpha { get; set; } = 0.9f;
            public float Radius { get; set; } = 8f;

            public void Draw(ICanvas canvas, RectF dirtyRect)
            {
                if (KeyRects.Length == 0) return;

                DrawBits(canvas);
                DrawCursor(canvas);
            }

            void DrawBits(ICanvas canvas)
            {
                if (Bits.Length == 0) return;

                canvas.StrokeSize = 2;
                canvas.StrokeColor = Colors.Yellow.WithAlpha(StrokeAlpha);
                canvas.FillColor = Colors.Yellow.WithAlpha(FillAlpha);

                int n = Math.Min(Bits.Length, KeyRects.Length);
                for (int i = 0; i < n; i++)
                {
                    if (!Bits[i]) continue;

                    RectF r = RectForFractionalIndex(i + ShiftKeys);
                    canvas.FillRoundedRectangle(r.X, r.Y, r.Width, r.Height, Radius);
                    canvas.DrawRoundedRectangle(r.X, r.Y, r.Width, r.Height, Radius);
                }
            }

            void DrawCursor(ICanvas canvas)
            {
                if (CursorIndex is null) return;

                RectF r = RectForFractionalIndex(CursorIndex.Value);

                canvas.StrokeSize = 2;
                canvas.StrokeColor = Colors.Yellow.WithAlpha(0.95f);
                canvas.FillColor = Colors.Yellow.WithAlpha(CursorAlpha);

                canvas.FillRoundedRectangle(r.X, r.Y, r.Width, r.Height, Radius);
                canvas.DrawRoundedRectangle(r.X, r.Y, r.Width, r.Height, Radius);
            }

            RectF RectForFractionalIndex(float idx)
            {
                int i0 = (int)MathF.Floor(idx);
                int i1 = i0 + 1;
                if (i0 < 0) i0 = 0;
                if (i0 >= KeyRects.Length) i0 = KeyRects.Length - 1;

                RectF r0 = KeyRects[i0];
                if (i1 < 0 || i1 >= KeyRects.Length) return r0;

                float t = idx - i0;
                RectF r1 = KeyRects[i1];
                return LerpRect(r0, r1, t);
            }

            static RectF LerpRect(RectF a, RectF b, float t)
            {
                float x = a.X + (b.X - a.X) * t;
                float y = a.Y + (b.Y - a.Y) * t;
                float w = a.Width + (b.Width - a.Width) * t;
                float h = a.Height + (b.Height - a.Height) * t;
                return new RectF(x, y, w, h);
            }
        }
        public PianoKeyboardReadOnly Keyboard { get; }
       
       
        private readonly PatternDrawable _patternDrawable = new();
        private readonly GraphicsView _patternView;
        private RectF[] _keyRects = Array.Empty<RectF>();

        public KeyboardOverlayHost(PianoKeyboardReadOnly keyboard)
        {
            Keyboard = keyboard;
            Children.Add(Keyboard);
            Keyboard.SizeChanged += (_, _) => SyncOverlay();

            _patternView = new GraphicsView
            {
                Drawable = _patternDrawable,
                InputTransparent = true,
                ZIndex = 110
            };
            Children.Add(_patternView);
        }

        // Visual sync: paint overlays according to a bool[] state
        public void SetOverlayState(bool[] bits)
        {
            _patternDrawable.Bits = bits ?? Array.Empty<bool>();
            _patternDrawable.ShiftKeys = 0f;
            _patternDrawable.CursorIndex = null;
            _patternView.Invalidate();
        }

        public void SyncOverlay()
        {
            int n = Keyboard.KeyButtons.Count;
            if (_keyRects.Length != n)
                _keyRects = new RectF[n];

            for (int i = 0; i < n; i++)
            {
                var key = Keyboard.KeyButtons[i];
                _keyRects[i] = new RectF((float)key.X, (float)key.Y, (float)key.Width, (float)key.Height);
            }

            // Tell drawable the geometry
            _patternDrawable.KeyRects = _keyRects;
            _patternView.Invalidate();
            Console.WriteLine("Overlay synced.");
        }
        // Animation: move a single “cursor” rect from key A to key B
        public async Task AnimateCursor(int fromIndex, int toIndex, uint ms = 250)
        {
            SyncOverlay();

            _patternDrawable.CursorIndex = fromIndex;
            _patternDrawable.CursorAlpha = 0.7f;
            _patternView.Invalidate();

            var tcs = new TaskCompletionSource();

            new Animation(v =>
            {
                _patternDrawable.CursorIndex = (float)(fromIndex + (toIndex - fromIndex) * v);
                _patternView.Invalidate();
            })
            .Commit(this, "CursorMove", 16, ms, Easing.CubicInOut, (v, c) => tcs.SetResult());

            await tcs.Task;

            _patternDrawable.CursorIndex = null;
            _patternView.Invalidate();
        }


        public async Task AnimateShiftByK(
    bool[] bits,
    int k,
    bool commit,
    bool autoDisappear,
    uint ms = 4000)
        {
            SyncOverlay();
            Console.WriteLine($"AnimateShiftByK");
            _patternDrawable.Bits = bits;
            _patternDrawable.ShiftKeys = 0f;
            Debug.Assert(_patternDrawable.Bits.Length == bits.Length);
            _patternView.Invalidate();

            Console.WriteLine("Starting shift animation...");
            await RunShiftAnimation(k, ms);
            Console.WriteLine("Shift animation completed.");
            if (commit)
            {
                Console.WriteLine("Committing shifted state...");
                var shifted = ShiftBits(bits, k);
                SetOverlayState(shifted);
            }

            if (autoDisappear)
            {
                Console.WriteLine("Fading out overlay...");
                await Task.Delay(1000);
                await FadeOutOverlay();
            }
        }

       
        private static bool[] ShiftBits(bool[] bits, int k)
        {
            int n = bits.Length;
            var outBits = new bool[n];

            for (int i = 0; i < n; i++)
            {
                int j = i + k;
                if ((uint)j < (uint)n) outBits[j] = bits[i];
            }
            return outBits;
        }

        private Task RunShiftAnimation(int k, uint ms, string name = "ShiftBits")
        {
            var tcs = new TaskCompletionSource();

            // (Optional) cancel any previous animation with same name
            //this.AbortAnimation(name);

            new Animation(v =>
            {
                Console.WriteLine($"Animation progress: {v}");
                _patternDrawable.ShiftKeys = (float)(v * k);
                _patternView.Invalidate();
            })
            .Commit(
                owner: this,
                name: name,
                rate: 16,
                length: ms,
                easing: Easing.CubicInOut,
                finished: (v, c) => tcs.SetResult());

            return tcs.Task;
        }

        public async Task FadeInOverlay(uint ms = 150)
        {
            if (_patternView is null)
                return;

            _patternView.IsVisible = true;
            _patternView.Opacity = 0;

            await _patternView.FadeTo(1, ms, Easing.CubicIn);
        }

        public async Task FadeOutOverlay(uint ms = 200)
        {
            if (_patternView is null)
                return;

            if (!_patternView.IsVisible)
                return;

            await _patternView.FadeTo(0, ms, Easing.CubicOut);

            _patternView.IsVisible = false;
            _patternView.Opacity = 1;

            ClearOverlay();
        }

        public void ClearOverlay()
        {
            _patternDrawable.Bits = Array.Empty<bool>();
            _patternDrawable.ShiftKeys = 0f;
            _patternDrawable.CursorIndex = null;
            _patternView.Invalidate();
        }
    }
}
