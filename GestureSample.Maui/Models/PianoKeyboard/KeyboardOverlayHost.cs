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
            public int[]? AnimTargets;   // same length as AnimBits
            public float AnimProgress;   // 0..1

            // persistent / question state
            public bool[] StaticBits { get; set; } = Array.Empty<bool>();

            // animation / tutorial layer
            public bool[] AnimBits { get; set; } = Array.Empty<bool>();

            public float AnimShiftKeys { get; set; } = 0f;
            public int? CursorIndex { get; set; } = null;

            // tuning
            public float StaticAlpha { get; set; } = 0.7f;
            public float AnimAlpha { get; set; } = 0.3f;


            public RectF[] KeyRects { get; set; } = Array.Empty<RectF>();

           
            // Optional cursor: draw one rect at a fractional index (e.g., 3.2)
             public float CursorAlpha { get; set; } = 0.7f;

            public float Radius { get; set; } = 8f;

            public void Draw(ICanvas canvas, RectF dirtyRect)
            {
                if (KeyRects == null || KeyRects.Length == 0)
                    return;

                // 1️⃣ static layer (calm, no shift)
                DrawStaticBits(
                    canvas,
                    StaticBits,
                    shiftKeys: 0f,
                    alpha: StaticAlpha
                );

                // 2️⃣ animation layer (shifted / cursor)
                DrawAnimBits(
                    canvas
                );

                DrawCursor(canvas); // uses CursorIndex (anim-only)
            }

            void DrawStaticBits(ICanvas canvas, bool[] bits, float shiftKeys, float alpha)
            {
                if (bits == null || bits.Length == 0) return;

                canvas.FillColor = Colors.Yellow.WithAlpha(alpha);
                canvas.StrokeColor = Colors.Yellow.WithAlpha(alpha + 0.2f);
                canvas.StrokeSize = 2;

                int n = Math.Min(bits.Length, KeyRects.Length);

                for (int i = 0; i < n; i++)
                {
                    if (!bits[i]) continue;

                    RectF r = shiftKeys == 0f
                        ? KeyRects[i]
                        : RectForFractionalIndex(i + shiftKeys);

                    canvas.FillRoundedRectangle(r, 6);
                    canvas.DrawRoundedRectangle(r, 6);
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


            void DrawAnimBits(ICanvas canvas)
            {
                if (AnimBits == null || AnimTargets == null)
                    return;

                canvas.FillColor = Colors.Yellow.WithAlpha(AnimAlpha);
                canvas.StrokeColor = Colors.Yellow.WithAlpha(AnimAlpha + 0.2f);
                canvas.StrokeSize = 2;

                int n = Math.Min(AnimBits.Length, KeyRects.Length);

                for (int i = 0; i < n; i++)
                {
                    if (!AnimBits[i]) continue;

                    int target = AnimTargets[i];
                    float current = i + (target - i) * AnimProgress;

                    RectF r = RectForFractionalIndex(current);
                    canvas.FillRoundedRectangle(r, 6);
                    canvas.DrawRoundedRectangle(r, 6);
                }
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
            Keyboard.SizeChanged += (_, _) => TrySyncOverlay();

            _patternView = new GraphicsView
            {
                Drawable = _patternDrawable,
                InputTransparent = true,
                ZIndex = 110,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };
            Children.Add(_patternView);
        }

        // persistent question / answer state
        public void SetStaticBits(bool[] bits)
        {
            _patternDrawable.StaticBits = bits ?? Array.Empty<bool>();
            // Ensure we have real key geometry. If layout not ready yet,
            // run again next UI tick.
            if (TrySyncOverlay())
            {
                _patternView.Invalidate();
                return;
            }

            Dispatcher.Dispatch(() =>
            {
                if (TrySyncOverlay())
                    _patternView.Invalidate();
                else
                {
                    // layout not ready yet; try once more next frame
                    Dispatcher.Dispatch(() =>
                    {
                        if (TrySyncOverlay())
                            _patternView.Invalidate();
                    });
                    return;
                }

                _patternView.Invalidate();
            });
        }

        // start an animation
        public void SetAnimBits(bool[] bits)
        {
            _patternDrawable.AnimBits = bits ?? Array.Empty<bool>();
            _patternDrawable.AnimShiftKeys = 0f;
            _patternDrawable.CursorIndex = null;
            _patternView.Invalidate();
        }

        // clear only animation layer
        public void ClearAnim()
        {
            _patternDrawable.AnimBits = Array.Empty<bool>();
            _patternDrawable.AnimTargets = Array.Empty<int>();
            _patternDrawable.AnimProgress = 0f;
            _patternDrawable.CursorIndex = null;
            _patternView.Invalidate();
        }

        public bool TrySyncOverlay()
        {
            var keys = Keyboard.KeyButtons;
            if (keys == null || keys.Count == 0) return false;

            // Layout not ready yet (common when called immediately after creating UI)
            if (keys[0].Width <= 0 || keys[0].Height <= 0) return false;

            int n = keys.Count;
            if (_keyRects.Length != n)
                _keyRects = new RectF[n];

            for (int i = 0; i < n; i++)
            {
                var key = keys[i];
                _keyRects[i] = new RectF((float)key.X, (float)key.Y, (float)key.Width, (float)key.Height);
            }

            _patternDrawable.KeyRects = _keyRects;
            return true;
        }
        // Animation: move a single “cursor” rect from key A to key B
        public async Task AnimateCursor(int fromIndex, int toIndex, uint ms = 250)
        {
            TrySyncOverlay();

            _patternDrawable.CursorIndex = fromIndex;
            _patternDrawable.CursorAlpha = 0.7f;
            _patternView.Invalidate();

            var tcs = new TaskCompletionSource();

            new Animation(v =>
            {
                _patternDrawable.CursorIndex = (int?)(fromIndex + (toIndex - fromIndex) * v);
                _patternView.Invalidate();
            })
            .Commit(this, "CursorMove", 16, ms, Easing.CubicInOut, (v, c) => tcs.SetResult());

            await tcs.Task;

            _patternDrawable.CursorIndex = null;
            _patternView.Invalidate();
        }

        public async Task Animate(
     bool[] bits,
     Operation op,
     int shiftByK = 0,
     uint ms = 450)
        {
            TrySyncOverlay();

            bits ??= Array.Empty<bool>();
            if (bits.Length == 0)
                return;

            // --- prepare animation layer ---
            _patternDrawable.AnimBits = bits;
            _patternDrawable.AnimTargets = BuildTargets(bits, op, shiftByK);
            _patternDrawable.AnimProgress = 0f;
            _patternDrawable.CursorIndex = null;

            _patternView.IsVisible = true;
            _patternView.Opacity = 1;
            _patternView.Invalidate();

            // nothing to animate
            if (_patternDrawable.AnimTargets.Length == 0)
                return;

            // --- run distance-based animation ---
            await RunProgressAnimation(
                $"Anim_{op}",
                ms,
                t => _patternDrawable.AnimProgress = t
            );
            await Task.Delay(2000);
            // --- clear animation layer ---
            ClearAnim();

        }

        private Task RunProgressAnimation(
    string name,
    uint ms,
    Action<float> setProgress)
        {
            var tcs = new TaskCompletionSource();
            this.AbortAnimation(name);

            new Animation(v =>
            {
                setProgress((float)v);
                _patternView.Invalidate();
            })
            .Commit(this, name, 16, ms, Easing.CubicInOut,
                (_, _) => tcs.SetResult());

            return tcs.Task;
        }

        private int[] BuildTargets(bool[] bits, Operation op, int shiftByK = 0)
        {
            return op switch
            {
                Operation.SequenceRTL
                    => BuildPackLeft(bits),

                Operation.SequenceLTR
                    => BuildPackRight(bits),

                Operation.Split
                    => BuildSplitFromCenter(bits),

                Operation.MoveBy
                    => BuildShiftTargets(bits, shiftByK),

                _ => BuildShiftTargets(bits, 0)
            };
        }

        private static int[] BuildShiftTargets(bool[] bits, int shiftByK)
        {
            int n = bits.Length;
            var dest = new int[n];
            for (int i = 0; i < n; i++)
                dest[i] = bits[i] ? i + shiftByK : i;
            return dest;
        }

        private static int[] BuildPackLeft(bool[] bits)
        {
            int n = bits.Length;
            var dest = new int[n];

            int write = 0;
            for (int i = 0; i < n; i++)
                dest[i] = bits[i] ? write++ : i;

            return dest;
        }

        private static int[] BuildPackRight(bool[] bits)
        {
            int n = bits.Length;
            var dest = new int[n];

            int count = bits.Count(b => b);
            int write = n - count;

            for (int i = 0; i < n; i++)
                dest[i] = bits[i] ? write++ : i;

            return dest;
        }

        private static int[] BuildSplitFromCenter(bool[] bits)
        {
            int n = bits.Length;
            var dest = new int[n];

            int active = bits.Count(b => b);
            if (active == 0)
                return Enumerable.Range(0, n).ToArray();

            int leftCount = active / 2;
            int rightCount = active - leftCount;

            int leftWrite = 0;
            int rightWrite = n - rightCount;

            int seen = 0;
            for (int i = 0; i < n; i++)
            {
                if (!bits[i])
                {
                    dest[i] = i;
                    continue;
                }

                dest[i] = (seen < leftCount) ? leftWrite++ : rightWrite++;
                seen++;
            }

            return dest;
        }

    }
}
