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

            public int[] DestIndex { get; set; } = Array.Empty<int>(); // per source index
            public float MapT { get; set; } = 0f;                      // 0..1 animation progress
            public bool UseDistanceBasedSpeed { get; set; } = true;

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

                    RectF r = KeyRects[i];

                    if (DestIndex.Length == KeyRects.Length && MapT > 0f)
                    {
                        int dest = DestIndex[i];
                        if ((uint)dest < (uint)KeyRects.Length && dest != i)
                        {
                            int dist = Math.Abs(dest - i);
                            float tt = UseDistanceBasedSpeed ? EaseByDistance(MapT, dist) : MapT;
                            r = LerpRect(KeyRects[i], KeyRects[dest], tt);
                        }
                    }
                    else if (ShiftKeys != 0f)
                    {
                        r = RectForFractionalIndex(i + ShiftKeys);
                    }
                    canvas.FillRoundedRectangle(r.X, r.Y, r.Width, r.Height, Radius);
                    canvas.DrawRoundedRectangle(r.X, r.Y, r.Width, r.Height, Radius);
                }
            }

            static float EaseByDistance(float t, int dist)
            {
                // dist=0 -> no movement
                if (dist <= 0) return 1f;

                // bigger dist -> smaller exponent -> moves faster early
                // tweak these numbers to taste
                float exponent = 1.8f - MathF.Min(1.2f, 0.15f * dist);  // e.g. dist 1=>1.65, dist 8=>0.6
                exponent = MathF.Max(0.35f, exponent);

                return MathF.Pow(t, exponent);
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

        public async Task Animate(bool[] bits, Operation op, int k,  uint ms = 450, bool commit = false, bool autoDisappear = true)
        {
            SyncOverlay();

            if (op is Operation.Copy) k = 0;
            
            bits ??= Array.Empty<bool>();
            _patternDrawable.Bits = bits;
            _patternDrawable.ShiftKeys = 0f;
            _patternDrawable.CursorIndex = null;

            _patternDrawable.DestIndex = op switch
            {
                Operation.SequenceLTR => BuildDestPackRight(bits),
                Operation.SequenceRTL => BuildDestPackLeft(bits),
                Operation.Split => BuildDestSplitToSidesFromCenter(bits),
                Operation.MoveBy or Operation.Copy => BuildDestShift(bits, k),
                _ => Array.Empty<int>()
            };

            if (_patternDrawable.DestIndex.Length == 0)
                return;

            _patternDrawable.MapT = 0f;
            _patternDrawable.UseDistanceBasedSpeed = true;

            _patternView.IsVisible = true;
            _patternView.Opacity = 1;
            _patternView.Invalidate();

            await RunMapAnimation(ms);

            if (commit)
            {
                bool[] moved = ApplyDest(bits, _patternDrawable.DestIndex);
                SetOverlayState(moved);
            }

            // cleanup mapping state (overlay may remain showing committed bits)
            _patternDrawable.MapT = 0f;
            _patternDrawable.DestIndex = Array.Empty<int>();
            _patternView.Invalidate();

            if (autoDisappear)
            {
                await Task.Delay(1000);
                await FadeOutOverlay();
            }
        }

        private Task RunMapAnimation(uint ms, string name = "MapAnim")
        {
            var tcs = new TaskCompletionSource();
            this.AbortAnimation(name);

            new Animation(v =>
            {
                _patternDrawable.MapT = (float)v;
                _patternView.Invalidate();
            })
            .Commit(this, name, 16, ms, Easing.CubicInOut, (v, c) => tcs.SetResult());

            return tcs.Task;
        }

        static bool[] ApplyDest(bool[] bits, int[] dest)
        {
            int n = bits.Length;
            var outBits = new bool[n];

            for (int i = 0; i < n && i < dest.Length; i++)
            {
                if (!bits[i]) continue;
                int d = dest[i];
                if ((uint)d < (uint)n) outBits[d] = true;
            }
            return outBits;
        }


        private static int[] BuildDestShift(bool[] bits, int k)
        {
            int n = bits.Length;

            int[] dest = new int[n];

            int write = 0;

            for (int i = 0; i < n; i++)
            {
                if (bits[i]) dest[i] = i+k;
                else dest[i] = i;
            }
            return dest;
        }

        static int[] BuildDestPackLeft(bool[] bits)
        {
            int n = bits.Length;
            int[] dest = new int[n];

            int write = 0;
            for (int i = 0; i < n; i++)
            {
                if (bits[i]) dest[i] = write++;
                else dest[i] = i;
            }
            return dest;
        }

        static int[] BuildDestPackRight(bool[] bits)
        {
            int n = bits.Length;
            int[] dest = new int[n];

            int count = bits.Count(b => b);
            int write = n - count;

            for (int i = 0; i < n; i++)
            {
                if (bits[i]) dest[i] = write++;
                else dest[i] = i;
            }
            return dest;
        }

        static int[] BuildDestSplitToSidesFromCenter(bool[] bits)
        {
            int half = bits.Length/2;
            int[] destR = BuildDestPackRight(bits[half..]);
            for(int i = 0; i < destR.Length; i++)
            {
                destR[i] += half;
            }
            return BuildDestPackLeft(bits[0..half]).Concat(destR).ToArray();
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
