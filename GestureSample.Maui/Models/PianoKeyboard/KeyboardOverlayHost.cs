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
            public float StaticAlpha { get; set; } = 0.5f;
            public float AnimAlpha { get; set; } = 0.5f;


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

                canvas.FillColor = Colors.DarkOrange.WithAlpha(alpha);
                canvas.StrokeColor = Colors.DarkOrange.WithAlpha(alpha + 0.2f);
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
        public PianoKeyboardReadOnly Keyboard { get; set; }
       
       
        private readonly PatternDrawable _patternDrawable = new();
        private RectF[] _keyRects = Array.Empty<RectF>();

        private readonly BoxView _inputShield;
        public bool IsTutorialMode { get; private set; }

        public KeyboardOverlayHost(PianoKeyboardReadOnly keyboard)
        {
            Keyboard = keyboard;
            Children.Add(Keyboard);

            _inputShield = new BoxView
            {
                BackgroundColor = Color.FromRgba(0, 0, 0, 0),
                IsVisible = false,
                InputTransparent = false, // IMPORTANT: must intercept touches
                ZIndex = 99999
            };
            Children.Add(_inputShield);

            Keyboard.InstallOverlay(_patternDrawable);

            this.Loaded += (_, _) => DelayedSyncOverlay("Host.Loaded");
            Keyboard.KeysRebuilt += (_, _) => DelayedSyncOverlay("Keyboard.KeysRebuilt");
            Keyboard.SizeChanged += (_, _) => DelayedSyncOverlay("Keyboard.SizeChanged");
            Keyboard.LayoutReady += (_, _) => DelayedSyncOverlay("Keyboard.LayoutReady");
            if (Keyboard is VisualElement ve)
            {
                ve.SizeChanged += (_, _) => DelayedSyncOverlay("VE.SizeChanged");
                ve.Loaded += (_, _) => DelayedSyncOverlay("VE.Loaded");
            }
            if (Keyboard.OverlayView is VisualElement ov)
            {
                ov.SizeChanged += (_, _) => DelayedSyncOverlay("Overlay.SizeChanged");
            }

            SyncOverlay();
        }

        public void SetTutorialMode(bool isOn)
        {
            IsTutorialMode = isOn;
            _inputShield.IsVisible = isOn;

            // Keep it above anything the page adds later (buttons, etc.)
            _inputShield.ZIndex = 999999;
        }



        // persistent question / answer state
        public void SetStaticBits(bool[] bits)
        {
            _patternDrawable.StaticBits = bits ?? Array.Empty<bool>();
            // Ensure we have real key geometry. If layout not ready yet,
            // run again next UI tick.
            if (TrySyncOverlay())
            {
                Keyboard.InvalidateOverlay(); 
                return;
            }

            Dispatcher.Dispatch(() =>
            {
                if (TrySyncOverlay())
                    Keyboard.InvalidateOverlay();
                else
                {
                    // layout not ready yet; try once more next frame
                    Dispatcher.Dispatch(() =>
                    {
                        if (TrySyncOverlay())
                            Keyboard.InvalidateOverlay();
                    });
                    return;
                }

                Keyboard.InvalidateOverlay();
            });
        }

        // start an animation
        public void SetAnimBits(bool[] bits)
        {
            _patternDrawable.AnimBits = bits ?? Array.Empty<bool>();
            _patternDrawable.AnimShiftKeys = 0f;
            _patternDrawable.CursorIndex = null;
            Keyboard.InvalidateOverlay();
        }

        // clear only animation layer
        public void ClearAnim()
        {
            _patternDrawable.AnimBits = Array.Empty<bool>();
            _patternDrawable.AnimTargets = Array.Empty<int>();
            _patternDrawable.AnimProgress = 0f;
            _patternDrawable.CursorIndex = null;
            Keyboard.InvalidateOverlay();
        }


        private static Point GetRelativeTo(VisualElement child, VisualElement ancestor)
        {
            double x = 0, y = 0;
            Element? cur = child;

            while (cur is not null && cur != ancestor)
            {
                if (cur is VisualElement ve)
                {
                    x += ve.X + ve.TranslationX;
                    y += ve.Y + ve.TranslationY;
                }
                cur = cur.Parent;
            }

            // if ancestor not found, return NaN so caller can detect
            if (cur != ancestor) return new Point(double.NaN, double.NaN);

            return new Point(x, y);
        }

        public void SyncOverlay()
        {
            Dispatcher.Dispatch(() =>
            {
                if (TrySyncOverlay())
                    Keyboard.InvalidateOverlay();
            });
        }

        private bool _syncing;

public async Task EnsureOverlaySyncedAsync(int maxTries = 20)
{
    if (_syncing) return;
    _syncing = true;

    try
    {
        for (int i = 0; i < maxTries; i++)
        {
            // Wait a UI tick (Android often needs at least 1-2)
            await Task.Yield();

            // Re-span overlay if the grid was rebuilt
            Keyboard.FixOverlaySpan(); // see small method below (or call an internal hook)

            if (TrySyncOverlay())
            {
                Keyboard.InvalidateOverlay();
                return;
            }

            await Task.Delay(16); // ~1 frame
        }
    }
    finally
    {
        _syncing = false;
    }
}

        int _syncSeq = 0;
        bool _syncPending;

        void LogSync(string reason)
        {
            var keys = Keyboard.KeyButtons;
            var k0 = (keys != null && keys.Count > 0) ? keys[0] : null;

            System.Diagnostics.Debug.WriteLine(
                $"[KOH] #{_syncSeq} {reason} " +
                $"Keyboard(W,H)=({Keyboard.Width:F1},{Keyboard.Height:F1}) " +
                $"Keys={keys?.Count ?? 0} " +
                $"k0(X,Y,W,H)=({k0?.X:F1},{k0?.Y:F1},{k0?.Width:F1},{k0?.Height:F1})"
            );
        }

        async void DelayedSyncOverlay(string reason)
        {
            if (_syncPending) return;
            _syncPending = true;

            _syncSeq++;
            LogSync(reason + " (before)");

            // Android often needs a frame or two
            await Task.Yield();
            await Task.Delay(16);   // 1 frame
            await Task.Delay(16);   // 2nd frame (often the one that fixes X/Y)

            bool ok = TrySyncOverlay();
            LogSync(reason + $" (after) ok={ok}");

            if (ok)
                Keyboard.InvalidateOverlay();

            _syncPending = false;
        }

        public bool TrySyncOverlay()
        {
            var keys = Keyboard.KeyButtons;
            if (keys == null || keys.Count == 0) return false;

            if (keys[0].Width <= 0 || keys[0].Height <= 0) return false;

            GraphicsView? overlay = Keyboard.OverlayView;
            if (overlay == null || overlay.Width <= 0 || overlay.Height <= 0) return false;

            int n = keys.Count;
            if (_keyRects.Length != n)
                _keyRects = new RectF[n];

            // positions relative to the Keyboard root
            Point ovInKb = GetRelativeTo(overlay, Keyboard);
            if (double.IsNaN(ovInKb.X)) return false;

            for (int i = 0; i < n; i++)
            {
                var key = keys[i];

                Point keyInKb = GetRelativeTo(key, Keyboard);
                if (double.IsNaN(keyInKb.X)) return false;

                double rx = keyInKb.X - ovInKb.X;
                double ry = keyInKb.Y - ovInKb.Y;

                _keyRects[i] = new RectF(
                    (float)rx,
                    (float)ry,
                    (float)key.Width,
                    (float)key.Height
                );
            }

            _patternDrawable.KeyRects = _keyRects;
            overlay.Invalidate();
            return true;
        }
        // Animation: move a single “cursor” rect from key A to key B
        public async Task AnimateCursor(int fromIndex, int toIndex, uint ms = 250)
        {
            TrySyncOverlay();

            _patternDrawable.CursorIndex = fromIndex;
            _patternDrawable.CursorAlpha = 0.7f;
            Keyboard.InvalidateOverlay();

            var tcs = new TaskCompletionSource();

            new Animation(v =>
            {
                _patternDrawable.CursorIndex = (int?)(fromIndex + (toIndex - fromIndex) * v);
                Keyboard.InvalidateOverlay();
            })
            .Commit(this, "CursorMove", 16, ms, Easing.CubicInOut, (v, c) => tcs.SetResult());

            await tcs.Task;

            _patternDrawable.CursorIndex = null;
            Keyboard.InvalidateOverlay();
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

            //_patternView.IsVisible = true;
            //_patternView.OpacOpacity = 1;
            Keyboard.InvalidateOverlay();

            // nothing to animate
            if (_patternDrawable.AnimTargets.Length == 0)
                return;

            if (op == Operation.MoveBy)
            {

                for (int i = 0; i < Math.Abs(shiftByK); i++)
                {
                    // Each "Next" press:
                    await AnimateMoveByStepAsync(bits, shiftBy1: Math.Abs(shiftByK) / shiftByK, ms: 2200);
                    bits = ShiftOnce(bits, Math.Abs(shiftByK) / shiftByK);
                }
            }
            else
            {
                // --- run distance-based animation ---
                await RunProgressAnimation(
                    $"Anim_{op}",
                    ms,
                    t => _patternDrawable.AnimProgress = t
                );
            }
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
                Keyboard.InvalidateOverlay();
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

        

        public Task AnimateMoveByStepAsync(bool[] bits, int shiftBy1, uint ms = 220, string animName = "TutShiftStep")
        {
            TrySyncOverlay();

            bits ??= Array.Empty<bool>();
            if (bits.Length == 0)
                return Task.CompletedTask;

            // prepare animation layer
            _patternDrawable.AnimBits = bits;
            _patternDrawable.AnimTargets = BuildShiftTargets(bits, shiftBy1);
            _patternDrawable.AnimProgress = 0f;
            _patternDrawable.CursorIndex = null;

            Keyboard.InvalidateOverlay();

            // nothing to animate
            if (_patternDrawable.AnimTargets.Length == 0)
                return Task.CompletedTask;

            // run animation
            return RunProgressAnimation(animName, ms, t => _patternDrawable.AnimProgress = t);
        }

        public void StopTutorialOverlayNow(string animName = "TutShiftStep")
        {
            this.AbortAnimation(animName);
            ClearAnim();
        }

        public static bool[] ShiftOnce(bool[] bits, int shiftBy1)
        {
            int n = bits.Length;
            var next = new bool[n];

            for (int i = 0; i < n; i++)
            {
                if (!bits[i]) continue;

                int j = i + shiftBy1;
                if ((uint)j < (uint)n)
                    next[j] = true;
            }

            return next;
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
