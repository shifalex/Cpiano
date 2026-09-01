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
        public const float DefaultStaticOverlayAlpha = 0.5f;

        public enum HighlightBand
        {
            Full,
            UpperRowBottomThird,
            LowerRowTopThird,
            TowardMiddleBetweenRows
        }

        private sealed class PatternDrawable : IDrawable
        {
            public sealed class MultiAnimGroup
            {
                public bool[] Bits { get; set; } = Array.Empty<bool>();
                public int[] Targets { get; set; } = Array.Empty<int>();
                public Color Color { get; set; } = Colors.Yellow;
            }

            public sealed class MultiSpawnGroup
            {
                public bool[] Bits { get; set; } = Array.Empty<bool>();
                public Color Color { get; set; } = Colors.Yellow;
            }

            public int[]? AnimTargets;   // same length as AnimBits
            public float AnimProgress;   // 0..1

            // persistent / question state
            public bool[] StaticBits { get; set; } = Array.Empty<bool>();

            // animation / tutorial layer
            public bool[] AnimBits { get; set; } = Array.Empty<bool>();
            public bool[] SpawnBits { get; set; } = Array.Empty<bool>();

            public float AnimShiftKeys { get; set; } = 0f;
            public int? CursorIndex { get; set; } = null;

            // tuning
            public float StaticAlpha { get; set; } = DefaultStaticOverlayAlpha;
            public float AnimAlpha { get; set; } = 0.5f;
            public float SpawnAlpha { get; set; } = 0.5f;
            public Color AnimColor { get; set; } = Colors.Yellow;
            public Color SpawnColor { get; set; } = Colors.Yellow;
            public HighlightBand AnimHighlightBand { get; set; } = HighlightBand.Full;
            public List<MultiAnimGroup> MultiAnimGroups { get; set; } = new();
            public List<MultiSpawnGroup> MultiSpawnGroups { get; set; } = new();
            public int[] TutorialArcIndices { get; set; } = Array.Empty<int>();
            public float TutorialArcAlpha { get; set; } = 0.92f;
            public Color TutorialArcColor { get; set; } = Colors.White;
            public int TutorialArcCompletedSegments { get; set; } = int.MaxValue;
            public float TutorialArcCurrentSegmentProgress { get; set; } = 1f;
            public bool UseFlipInterpolation { get; set; }
            public bool ShowFlipAxis { get; set; }
            public bool ShowPrecisionPinchGuide { get; set; }
            public bool ShowPrecisionLearningSign { get; set; }
            public int[] PrecisionLearningDeltas { get; set; } = new int[2];
            public bool[] PrecisionLearningIsShift { get; set; } = new bool[2];
            public bool[] PrecisionLearningBaseAtTop { get; set; } = new bool[2];
            public int[] PrecisionLearningTrailOrigins { get; set; } = new[] { -1, -1 };
            public float PrecisionLearningSignAlpha { get; set; } = 1f;
            public float PrecisionLearningArrowAlpha { get; set; }
            public float FlipAxisY { get; set; }
            public float FlipAxisLeft { get; set; }
            public float FlipAxisRight { get; set; }


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

                DrawFlipAxis(canvas);

                DrawSpawnBits(canvas);

                DrawTutorialArcs(canvas);

                DrawPrecisionPinchGuide(canvas);

                DrawPrecisionLearningSign(canvas);

                DrawCursor(canvas); // uses CursorIndex (anim-only)
            }

            void DrawPrecisionLearningSign(ICanvas canvas)
            {
                if (!ShowPrecisionLearningSign || AnimBits == null || AnimTargets == null)
                    return;

                int columns = Math.Max(1, KeyboardColumns);
                for (int column = 0; column < Math.Min(2, columns); column++)
                {
                    List<int> active = new();
                    for (int i = column; i < Math.Min(AnimBits.Length, KeyRects.Length); i += columns)
                        if (AnimBits[i]) active.Add(i);
                    if (active.Count < 2 || PrecisionLearningDeltas[column] == 0)
                        continue;

                    int lower = active.Min();
                    int upper = active.Max();
                    bool isShift = PrecisionLearningIsShift[column];
                    bool baseAtTop = PrecisionLearningBaseAtTop[column];
                    RectF lowerRect = KeyRects[lower];
                    RectF upperRect = KeyRects[upper];
                    PointF lowerCenter = lowerRect.Center;
                    PointF upperCenter = upperRect.Center;

                    int trailOriginIndex = column < PrecisionLearningTrailOrigins.Length
                        ? PrecisionLearningTrailOrigins[column]
                        : -1;

                    // For a resize, the black |-- is the original sign. Do not let
                    // its moving endpoint advance on continuation steps; otherwise a
                    // black tip briefly appears ahead of and alters the blue tail.
                    if (!isShift && trailOriginIndex >= 0 && trailOriginIndex < KeyRects.Length)
                    {
                        if (baseAtTop)
                            lowerCenter = KeyRects[trailOriginIndex].Center;
                        else
                            upperCenter = KeyRects[trailOriginIndex].Center;
                    }

                    // During a whole-pinch shift, the dashed interval is part of the
                    // moving grip, so keep it attached to the animated yellow keys.
                    if (isShift)
                    {
                        int lowerTarget = Math.Clamp(AnimTargets[lower], 0, KeyRects.Length - 1);
                        int upperTarget = Math.Clamp(AnimTargets[upper], 0, KeyRects.Length - 1);
                        lowerCenter = LerpPoint(lowerCenter, KeyRects[lowerTarget].Center, AnimProgress);
                        upperCenter = LerpPoint(upperCenter, KeyRects[upperTarget].Center, AnimProgress);
                    }

                    float signAlpha = Math.Clamp(PrecisionLearningSignAlpha, 0f, 1f);
                    canvas.StrokeColor = Color.FromArgb("#243447").WithAlpha(0.95f * signAlpha);
                    canvas.FillColor = Color.FromArgb("#243447").WithAlpha(0.95f * signAlpha);
                    canvas.StrokeSize = 4;
                    canvas.StrokeLineCap = LineCap.Round;
                    canvas.StrokeDashPattern = isShift ? new[] { 7f, 6f } : null;
                    canvas.DrawLine(lowerCenter.X, lowerCenter.Y, upperCenter.X, upperCenter.Y);
                    canvas.StrokeDashPattern = null;

                    if (!isShift)
                    {
                        PointF pinned = baseAtTop ? upperCenter : lowerCenter;
                        float halfPin = Math.Max(9, lowerRect.Width * 0.22f);
                        canvas.DrawLine(pinned.X - halfPin, pinned.Y, pinned.X + halfPin, pinned.Y);
                    }

                    // A whole-pinch shift is already identified by the dotted sign and
                    // the persistent direction arrow beside the keyboard. Drawing the
                    // blue movement arrow here duplicates that instruction.
                    if (isShift || PrecisionLearningArrowAlpha <= 0)
                        continue;

                    int movingIndex = isShift ? lower : (baseAtTop ? lower : upper);
                    int movingTarget = Math.Clamp(AnimTargets[movingIndex], 0, KeyRects.Length - 1);
                    PointF source = KeyRects[movingIndex].Center;
                    PointF destination = isShift
                        ? Midpoint(KeyRects[Math.Clamp(AnimTargets[lower], 0, KeyRects.Length - 1)].Center,
                                   KeyRects[Math.Clamp(AnimTargets[upper], 0, KeyRects.Length - 1)].Center)
                        : KeyRects[movingTarget].Center;
                    PointF current = LerpPoint(source, destination, AnimProgress);

                    PointF trailOrigin = trailOriginIndex >= 0 && trailOriginIndex < KeyRects.Length
                        ? KeyRects[trailOriginIndex].Center
                        : source;

                    // Leave one tiny visual break where the arrow first leaves the
                    // original |-- sign. Later steps reuse this origin, preserving a
                    // continuous blue trace rather than introducing new gaps.
                    // The black and blue strokes have rounded caps (4 px and 5 px
                    // wide), so their centers need about 7 px separation to leave
                    // an actual ~2 px visible gap between their painted edges.
                    const float continuationCenterGap = 7f;
                    PointF trailStart = MovePointToward(trailOrigin, destination, continuationCenterGap);
                    if (Distance(trailOrigin, current) <= continuationCenterGap)
                        continue;

                    // Use one solid opacity across the complete accumulated trail.
                    // During a squeeze the trail keeps growing; no completed part is
                    // shortened or deleted.
                    canvas.StrokeColor = Colors.DodgerBlue.WithAlpha(PrecisionLearningArrowAlpha);
                    canvas.StrokeSize = 5;
                    canvas.DrawLine(trailStart.X, trailStart.Y, current.X, current.Y);
                    DrawLearningArrowHead(canvas, trailStart, current, destination, PrecisionLearningArrowAlpha);
                }
            }

            static PointF Midpoint(PointF a, PointF b) =>
                new((a.X + b.X) / 2f, (a.Y + b.Y) / 2f);

            static PointF LerpPoint(PointF a, PointF b, float t) =>
                new(a.X + ((b.X - a.X) * t), a.Y + ((b.Y - a.Y) * t));

            static float Distance(PointF a, PointF b)
            {
                float dx = b.X - a.X;
                float dy = b.Y - a.Y;
                return MathF.Sqrt((dx * dx) + (dy * dy));
            }

            static PointF MovePointToward(PointF source, PointF destination, float distance)
            {
                float length = Distance(source, destination);
                if (length <= 0.001f)
                    return source;

                float amount = Math.Min(distance, length) / length;
                return LerpPoint(source, destination, amount);
            }

            static void DrawLearningArrowHead(ICanvas canvas, PointF source, PointF tip, PointF destination, float alpha)
            {
                float dx = destination.X - tip.X;
                float dy = destination.Y - tip.Y;
                if (Math.Abs(dx) + Math.Abs(dy) < 0.5f)
                {
                    dx = destination.X - source.X;
                    dy = destination.Y - source.Y;
                }
                float length = MathF.Sqrt((dx * dx) + (dy * dy));
                dx /= length;
                dy /= length;
                float px = -dy;
                float py = dx;
                const float headLength = 13;
                const float headWidth = 8;
                PathF head = new();
                head.MoveTo(tip.X, tip.Y);
                head.LineTo(tip.X - (dx * headLength) + (px * headWidth), tip.Y - (dy * headLength) + (py * headWidth));
                head.LineTo(tip.X - (dx * headLength) - (px * headWidth), tip.Y - (dy * headLength) - (py * headWidth));
                head.Close();
                canvas.FillColor = Colors.DodgerBlue.WithAlpha(alpha);
                canvas.FillPath(head);
            }

            void DrawPrecisionPinchGuide(ICanvas canvas)
            {
                if (!ShowPrecisionPinchGuide)
                    return;

                bool useAnimation = AnimBits?.Any(bit => bit) == true &&
                                    AnimTargets != null && AnimTargets.Length > 0;
                bool[] bits = useAnimation ? AnimBits : StaticBits;
                if (bits == null || bits.Length == 0)
                    return;

                int columns = Math.Max(1, KeyboardColumns);
                canvas.StrokeColor = Colors.Red;
                canvas.StrokeSize = 5;
                canvas.StrokeLineCap = LineCap.Round;

                for (int column = 0; column < columns; column++)
                {
                    List<int> selected = new();
                    for (int index = column; index < Math.Min(bits.Length, KeyRects.Length); index += columns)
                    {
                        if (bits[index])
                            selected.Add(index);
                    }

                    if (selected.Count < 2)
                        continue;

                    RectF first = GuideRect(selected.First(), useAnimation);
                    RectF last = GuideRect(selected.Last(), useAnimation);
                    canvas.DrawLine(
                        first.X + first.Width / 2f, first.Y + first.Height / 2f,
                        last.X + last.Width / 2f, last.Y + last.Height / 2f);
                }
            }

            RectF GuideRect(int index, bool animated)
            {
                if (!animated || AnimTargets == null || index >= AnimTargets.Length)
                    return KeyRects[index];

                int target = Math.Clamp(AnimTargets[index], 0, KeyRects.Length - 1);
                return UseFlipInterpolation
                    ? FlipRect(KeyRects[index], KeyRects[target], AnimProgress)
                    : LerpRect(KeyRects[index], KeyRects[target], AnimProgress);
            }

            public int KeyboardColumns { get; set; } = 1;

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
                if (MultiAnimGroups.Count > 0)
                {
                    foreach (MultiAnimGroup group in MultiAnimGroups)
                        DrawSingleAnimGroup(canvas, group.Bits, group.Targets, group.Color);

                    return;
                }

                if (AnimBits == null || AnimTargets == null)
                    return;

                DrawSingleAnimGroup(canvas, AnimBits, AnimTargets, AnimColor);
            }

            void DrawSingleAnimGroup(ICanvas canvas, bool[] bits, int[]? targets, Color color)
            {
                if (bits == null || targets == null)
                    return;

                canvas.FillColor = color.WithAlpha(AnimAlpha);
                canvas.StrokeColor = color.WithAlpha(Math.Min(1f, AnimAlpha + 0.2f));
                canvas.StrokeSize = 2;

                int n = Math.Min(bits.Length, KeyRects.Length);

                for (int i = 0; i < n; i++)
                {
                    if (!bits[i]) continue;

                    int target = Math.Clamp(targets[i], 0, KeyRects.Length - 1);
                    // Interpolate the actual key rectangles. Numeric interpolation is
                    // wrong for multi-column keyboards: moving 7 -> 5 passes through key
                    // 6 in the other column and makes a vertical shift look diagonal.
                    RectF movingRect = UseFlipInterpolation
                        ? FlipRect(KeyRects[i], KeyRects[target], AnimProgress)
                        : LerpRect(KeyRects[i], KeyRects[target], AnimProgress);
                    RectF r = ApplyHighlightBand(movingRect, i);
                    canvas.FillRoundedRectangle(r, 6);
                    canvas.DrawRoundedRectangle(r, 6);
                }
            }

            RectF FlipRect(RectF source, RectF target, float progress)
            {
                progress = Math.Clamp(progress, 0f, 1f);
                RectF collapsedSource = new(source.X, FlipAxisY - 0.5f, source.Width, 1);
                RectF collapsedTarget = new(target.X, FlipAxisY - 0.5f, target.Width, 1);
                return progress <= 0.5f
                    ? LerpRect(source, collapsedSource, progress * 2f)
                    : LerpRect(collapsedTarget, target, (progress - 0.5f) * 2f);
            }

            void DrawFlipAxis(ICanvas canvas)
            {
                if (!ShowFlipAxis)
                    return;

                canvas.StrokeColor = Colors.Red;
                canvas.StrokeSize = 3;
                canvas.StrokeLineCap = LineCap.Round;
                canvas.DrawLine(FlipAxisLeft, FlipAxisY, FlipAxisRight, FlipAxisY);
            }

            RectF ApplyHighlightBand(RectF rect, int index)
            {
                const float third = 1f / 3f;

                return AnimHighlightBand switch
                {
                    HighlightBand.UpperRowBottomThird => new RectF(
                        rect.X,
                        rect.Y + (rect.Height * 2f * third),
                        rect.Width,
                        rect.Height * third),

                    HighlightBand.LowerRowTopThird => new RectF(
                        rect.X,
                        rect.Y,
                        rect.Width,
                        rect.Height * third),

                    HighlightBand.TowardMiddleBetweenRows => IsUpperVisualRow(index)
                        ? new RectF(
                            rect.X,
                            rect.Y + (rect.Height * 2f * third),
                            rect.Width,
                            rect.Height * third)
                        : new RectF(
                            rect.X,
                            rect.Y,
                            rect.Width,
                            rect.Height * third),

                    _ => rect
                };
            }

            bool IsUpperVisualRow(int index)
            {
                if (KeyRects == null || KeyRects.Length < 2 || index < 0 || index >= KeyRects.Length)
                    return false;

                float minCenterY = KeyRects.Min(rect => rect.Y + (rect.Height / 2f));
                float maxCenterY = KeyRects.Max(rect => rect.Y + (rect.Height / 2f));
                if (Math.Abs(maxCenterY - minCenterY) < 1f)
                    return false;

                float middleY = (minCenterY + maxCenterY) / 2f;
                float centerY = KeyRects[index].Y + (KeyRects[index].Height / 2f);
                return centerY < middleY;
            }

            void DrawSpawnBits(ICanvas canvas)
            {
                if (MultiSpawnGroups.Count > 0)
                {
                    foreach (MultiSpawnGroup group in MultiSpawnGroups)
                        DrawSingleSpawnGroup(canvas, group.Bits, group.Color);

                    return;
                }

                if (SpawnBits == null || SpawnBits.Length == 0)
                    return;

                DrawSingleSpawnGroup(canvas, SpawnBits, SpawnColor);
            }

            void DrawSingleSpawnGroup(ICanvas canvas, bool[] bits, Color color)
            {
                if (bits == null || bits.Length == 0)
                    return;

                canvas.FillColor = color.WithAlpha(SpawnAlpha);
                canvas.StrokeColor = color.WithAlpha(Math.Min(1f, SpawnAlpha + 0.2f));
                canvas.StrokeSize = 2;

                int n = Math.Min(bits.Length, KeyRects.Length);

                for (int i = 0; i < n; i++)
                {
                    if (!bits[i]) continue;

                    RectF r = KeyRects[i];
                    canvas.FillRoundedRectangle(r, 6);
                    canvas.DrawRoundedRectangle(r, 6);
                }
            }

            void DrawTutorialArcs(ICanvas canvas)
            {
                if (TutorialArcIndices == null || TutorialArcIndices.Length < 2)
                    return;

                canvas.StrokeColor = TutorialArcColor.WithAlpha(TutorialArcAlpha);
                canvas.StrokeSize = 4;
                canvas.StrokeLineCap = LineCap.Round;
                canvas.StrokeLineJoin = LineJoin.Round;

                for (int i = 1; i < TutorialArcIndices.Length; i++)
                {
                    int segmentIndex = i - 1;
                    float progress;
                    if (segmentIndex < TutorialArcCompletedSegments)
                        progress = 1f;
                    else if (segmentIndex == TutorialArcCompletedSegments)
                        progress = TutorialArcCurrentSegmentProgress;
                    else
                        continue;

                    int from = TutorialArcIndices[i - 1];
                    int to = TutorialArcIndices[i];
                    if (from < 0 || to < 0 || from >= KeyRects.Length || to >= KeyRects.Length || from == to)
                        continue;

                    DrawArcSegment(canvas, KeyRects[from], KeyRects[to], i == TutorialArcIndices.Length - 1, progress);
                }
            }

            private static void DrawArcSegment(ICanvas canvas, RectF fromRect, RectF toRect, bool drawArrowHead, float progress)
            {
                progress = Math.Clamp(progress, 0f, 1f);
                if (progress <= 0f)
                    return;

                float startX = fromRect.X + (fromRect.Width / 2f);
                float endX = toRect.X + (toRect.Width / 2f);
                float baselineY = MathF.Min(fromRect.Top, toRect.Top) - 10;
                float arcHeight = MathF.Min(54, MathF.Max(24, MathF.Abs(endX - startX) * 0.22f));
                float controlX = (startX + endX) / 2f;
                float controlY = MathF.Max(6, baselineY - arcHeight);

                const int steps = 18;
                PointF previous = QuadraticPoint(startX, baselineY, controlX, controlY, endX, baselineY, 0);
                int visibleSteps = Math.Max(1, (int)MathF.Ceiling(steps * progress));
                for (int step = 1; step <= visibleSteps; step++)
                {
                    float t = MathF.Min(progress, step / (float)steps);
                    PointF current = QuadraticPoint(startX, baselineY, controlX, controlY, endX, baselineY, t);
                    canvas.DrawLine(previous, current);
                    previous = current;
                }

                if (!drawArrowHead || progress < 0.98f)
                    return;

                PointF tip = QuadraticPoint(startX, baselineY, controlX, controlY, endX, baselineY, 1f);
                PointF beforeTip = QuadraticPoint(startX, baselineY, controlX, controlY, endX, baselineY, 0.9f);
                float angle = MathF.Atan2(tip.Y - beforeTip.Y, tip.X - beforeTip.X);
                const float headLength = 13;
                const float headAngle = 0.72f;
                canvas.DrawLine(tip, new PointF(
                    tip.X - (headLength * MathF.Cos(angle - headAngle)),
                    tip.Y - (headLength * MathF.Sin(angle - headAngle))));
                canvas.DrawLine(tip, new PointF(
                    tip.X - (headLength * MathF.Cos(angle + headAngle)),
                    tip.Y - (headLength * MathF.Sin(angle + headAngle))));
            }

            private static PointF QuadraticPoint(float startX, float startY, float controlX, float controlY, float endX, float endY, float t)
            {
                float oneMinusT = 1f - t;
                float x = (oneMinusT * oneMinusT * startX) + (2 * oneMinusT * t * controlX) + (t * t * endX);
                float y = (oneMinusT * oneMinusT * startY) + (2 * oneMinusT * t * controlY) + (t * t * endY);
                return new PointF(x, y);
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
            _patternDrawable.KeyboardColumns = Math.Max(1, keyboard.Config?.KeysInRow ?? 1);
            Children.Add(Keyboard);

            _inputShield = new BoxView
            {
                // A zero-alpha black surface can be composited as opaque black by
                // some Android GPU/Material combinations. A nearly transparent white
                // surface remains hit-testable without visually darkening the keys.
                BackgroundColor = Colors.White.WithAlpha(0.001f),
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

        public void SetPrecisionPinchGuideVisible(bool visible)
        {
            _patternDrawable.ShowPrecisionPinchGuide = visible;
            Keyboard.InvalidateOverlay();
        }

        public void SetStaticOverlayAlpha(float alpha)
        {
            _patternDrawable.StaticAlpha = Math.Clamp(alpha, 0f, 1f);
            Keyboard.InvalidateOverlay();
        }

        public Task FadeStaticOverlayAlphaAsync(float targetAlpha, uint ms, string animName = "StaticOverlayAlpha")
        {
            float startAlpha = _patternDrawable.StaticAlpha;
            float endAlpha = Math.Clamp(targetAlpha, 0f, 1f);

            return RunProgressAnimation(animName, ms, t =>
            {
                _patternDrawable.StaticAlpha = startAlpha + ((endAlpha - startAlpha) * t);
            });
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
            _patternDrawable.SpawnBits = Array.Empty<bool>();
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
            _patternDrawable.AnimAlpha = 0.5f;
            _patternDrawable.AnimColor = Colors.Yellow;
            _patternDrawable.AnimHighlightBand = HighlightBand.Full;
            _patternDrawable.SpawnBits = Array.Empty<bool>();
            _patternDrawable.SpawnAlpha = 0.5f;
            _patternDrawable.SpawnColor = Colors.Yellow;
            _patternDrawable.MultiAnimGroups.Clear();
            _patternDrawable.MultiSpawnGroups.Clear();
            _patternDrawable.CursorIndex = null;
            _patternDrawable.TutorialArcIndices = Array.Empty<int>();
            _patternDrawable.UseFlipInterpolation = false;
            _patternDrawable.ShowFlipAxis = false;
            _patternDrawable.ShowPrecisionLearningSign = false;
            _patternDrawable.PrecisionLearningTrailOrigins = new[] { -1, -1 };
            _patternDrawable.PrecisionLearningSignAlpha = 1f;
            _patternDrawable.PrecisionLearningArrowAlpha = 0;
            Keyboard.InvalidateOverlay();
        }

        public void ShowTutorialArcs(IReadOnlyList<int> indices, Color? color = null, float alpha = 0.92f)
        {
            TrySyncOverlay();

            _patternDrawable.TutorialArcIndices = indices?
                .Where(index => index >= 0)
                .ToArray() ?? Array.Empty<int>();
            _patternDrawable.TutorialArcColor = color ?? Colors.White;
            _patternDrawable.TutorialArcAlpha = Math.Clamp(alpha, 0f, 1f);
            _patternDrawable.TutorialArcCompletedSegments = int.MaxValue;
            _patternDrawable.TutorialArcCurrentSegmentProgress = 1f;
            Keyboard.InvalidateOverlay();
        }

        public void ClearTutorialArcs()
        {
            _patternDrawable.TutorialArcIndices = Array.Empty<int>();
            _patternDrawable.TutorialArcCompletedSegments = int.MaxValue;
            _patternDrawable.TutorialArcCurrentSegmentProgress = 1f;
            Keyboard.InvalidateOverlay();
        }

        public Task AnimateTutorialArcPrefixAsync(
            IReadOnlyList<int> indices,
            int visiblePointCount,
            uint ms = 420,
            Color? color = null,
            float alpha = 0.92f,
            string animName = "TutArcPrefix")
        {
            if (indices == null || visiblePointCount < 2)
                return Task.CompletedTask;

            TrySyncOverlay();
            int pointCount = Math.Min(visiblePointCount, indices.Count);
            _patternDrawable.TutorialArcIndices = indices.Take(pointCount).ToArray();
            _patternDrawable.TutorialArcColor = color ?? Colors.White;
            _patternDrawable.TutorialArcAlpha = Math.Clamp(alpha, 0f, 1f);
            _patternDrawable.TutorialArcCompletedSegments = Math.Max(0, pointCount - 2);
            _patternDrawable.TutorialArcCurrentSegmentProgress = 0f;
            Keyboard.InvalidateOverlay();

            return RunProgressAnimation(animName, ms, t =>
            {
                _patternDrawable.TutorialArcCurrentSegmentProgress = t;
                if (t >= 1f)
                {
                    _patternDrawable.TutorialArcCompletedSegments = int.MaxValue;
                    _patternDrawable.TutorialArcCurrentSegmentProgress = 1f;
                }
            });
        }

        public async Task AnimateTutorialArcsOneByOneAsync(
            IReadOnlyList<int> indices,
            uint stepMs = 150,
            uint holdMs = 160,
            Color? color = null,
            float alpha = 0.92f)
        {
            if (indices == null || indices.Count < 2)
                return;

            TrySyncOverlay();
            List<int> visibleIndices = new() { indices[0] };
            _patternDrawable.TutorialArcColor = color ?? Colors.White;
            _patternDrawable.TutorialArcAlpha = Math.Clamp(alpha, 0f, 1f);

            for (int i = 1; i < indices.Count; i++)
            {
                visibleIndices.Add(indices[i]);
                _patternDrawable.TutorialArcIndices = visibleIndices.ToArray();
                Keyboard.InvalidateOverlay();
                await Task.Delay((int)stepMs);
            }

            await Task.Delay((int)holdMs);
        }

        public void ShowHighlightedBits(
            bool[] bits,
            Color? color = null,
            float alpha = 0.55f,
            HighlightBand highlightBand = HighlightBand.Full)
        {
            TrySyncOverlay();

            bits ??= Array.Empty<bool>();
            _patternDrawable.AnimBits = bits;
            _patternDrawable.AnimTargets = BuildShiftTargets(bits, 0);
            _patternDrawable.AnimProgress = 1f;
            _patternDrawable.AnimAlpha = Math.Clamp(alpha, 0f, 1f);
            _patternDrawable.AnimColor = color ?? Colors.Yellow;
            _patternDrawable.AnimHighlightBand = highlightBand;
            _patternDrawable.SpawnBits = Array.Empty<bool>();
            _patternDrawable.SpawnAlpha = 0f;
            _patternDrawable.MultiAnimGroups.Clear();
            _patternDrawable.MultiSpawnGroups.Clear();
            _patternDrawable.CursorIndex = null;
            Keyboard.InvalidateOverlay();
        }

        public async Task FadeInHighlightedBitsThenClearAsync(
            bool[] bits,
            Color? color = null,
            float targetAlpha = 0.58f,
            uint fadeInMs = 2000,
            HighlightBand highlightBand = HighlightBand.Full,
            string animName = "TutBitsFadeIn")
        {
            ShowHighlightedBits(bits, color, 0f, highlightBand);
            targetAlpha = Math.Clamp(targetAlpha, 0f, 1f);

            await RunProgressAnimation(animName, fadeInMs, t =>
            {
                _patternDrawable.AnimAlpha = targetAlpha * t;
            });

            ClearAnim();
        }

        public Task FadeOutHighlightedBitsAsync(uint ms = 180, string animName = "TutBitsFade")
        {
            if ((_patternDrawable.AnimBits?.Length ?? 0) == 0)
                return Task.CompletedTask;

            float startAlpha = _patternDrawable.AnimAlpha;
            return RunProgressAnimation(animName, ms, t =>
            {
                _patternDrawable.AnimAlpha = startAlpha * (1f - t);
                if (t >= 1f)
                    ClearAnim();
            });
        }

        public async Task AnimatePackedGroupsAsync(
            IReadOnlyList<(bool[] Bits, bool[] TargetBits, Color Color)> groups,
            uint moveMs = 900,
            uint holdMs = 480,
            uint fadeOutMs = 180,
            string animName = "TutMultiGroup")
        {
            TrySyncOverlay();
            if (groups == null || groups.Count == 0)
                return;

            List<(bool[] Bits, bool[] TargetBits, Color Color)> normalizedGroups = groups
                .Where(group => group.Bits != null && group.Bits.Length > 0 && group.TargetBits != null)
                .ToList();

            _patternDrawable.MultiAnimGroups = new();
            foreach ((bool[] Bits, bool[] TargetBits, Color Color) group in normalizedGroups)
            {
                int activeCount = group.Bits.Count(bit => bit);
                if (activeCount == 0)
                    continue;

                int[] targets = BuildExplicitTargets(group.Bits, group.TargetBits);

                _patternDrawable.MultiAnimGroups.Add(new PatternDrawable.MultiAnimGroup
                {
                    Bits = group.Bits,
                    Targets = targets,
                    Color = group.Color
                });
            }

            if (_patternDrawable.MultiAnimGroups.Count == 0)
                return;

            _patternDrawable.MultiSpawnGroups.Clear();
            _patternDrawable.AnimProgress = 0f;
            _patternDrawable.AnimAlpha = 0.55f;
            _patternDrawable.SpawnAlpha = 0f;
            _patternDrawable.CursorIndex = null;
            Keyboard.InvalidateOverlay();

            await RunProgressAnimation(animName + "_move", moveMs, t =>
            {
                _patternDrawable.AnimProgress = t;
                _patternDrawable.AnimAlpha = 0.55f;
            });

            _patternDrawable.MultiSpawnGroups = new();
            foreach ((bool[] Bits, bool[] TargetBits, Color Color) group in normalizedGroups)
            {
                if (group.Bits.Count(bit => bit) == 0)
                    continue;

                _patternDrawable.MultiSpawnGroups.Add(new PatternDrawable.MultiSpawnGroup
                {
                    Bits = group.TargetBits.ToArray(),
                    Color = group.Color
                });
            }

            _patternDrawable.AnimProgress = 1f;
            _patternDrawable.AnimAlpha = 0.55f;
            _patternDrawable.SpawnAlpha = 0.55f;
            Keyboard.InvalidateOverlay();

            await Task.Delay((int)holdMs);

            await RunProgressAnimation(animName + "_out", fadeOutMs, t =>
            {
                _patternDrawable.AnimAlpha = 0.55f * (1f - t);
                _patternDrawable.SpawnAlpha = 0.55f * (1f - t);
            });

            ClearAnim();
        }

        private static int[] BuildExplicitTargets(bool[] bits, bool[] targetBits)
        {
            int n = bits.Length;
            int[] dest = Enumerable.Range(0, n).ToArray();

            List<int> sourceIndices = new();
            List<int> targetIndices = new();

            for (int i = 0; i < n; i++)
            {
                if (bits[i])
                    sourceIndices.Add(i);

                if (i < targetBits.Length && targetBits[i])
                    targetIndices.Add(i);
            }

            int pairCount = Math.Min(sourceIndices.Count, targetIndices.Count);
            for (int i = 0; i < pairCount; i++)
                dest[sourceIndices[i]] = targetIndices[i];

            return dest;
        }

        private static bool[] BuildPackedLeftBits(bool[] bits, int startOffset = 0)
        {
            bool[] packed = new bool[bits.Length];
            int count = bits.Count(bit => bit);

            for (int i = 0; i < count && startOffset + i < packed.Length; i++)
                packed[startOffset + i] = true;

            return packed;
        }

        private static bool[] BuildPackedRightBits(bool[] bits, int rightOffset = 0)
        {
            bool[] packed = new bool[bits.Length];
            int count = bits.Count(bit => bit);

            for (int i = 0; i < count && i < packed.Length; i++)
            {
                int targetIndex = packed.Length - 1 - rightOffset - i;
                if (targetIndex < 0)
                    break;
                packed[targetIndex] = true;
            }

            return packed;
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

        public async Task PulseBitsAsync(
            bool[] bits,
            uint fadeInMs = 300,
            uint holdMs = 1800,
            uint fadeOutMs = 300,
            string animName = "TutPulse")
        {
            await PulseBitsAsync(bits, Colors.Yellow, fadeInMs, holdMs, fadeOutMs, animName);
        }

        public async Task PulseBitsAsync(
            bool[] bits,
            Color color,
            uint fadeInMs = 300,
            uint holdMs = 1800,
            uint fadeOutMs = 300,
            string animName = "TutPulse")
        {
            TrySyncOverlay();

            bits ??= Array.Empty<bool>();
            if (bits.Length == 0)
                return;

            _patternDrawable.AnimBits = bits;
            _patternDrawable.AnimTargets = BuildShiftTargets(bits, 0);
            _patternDrawable.AnimProgress = 1f;
            _patternDrawable.AnimAlpha = 0f;
            _patternDrawable.AnimColor = color;
            _patternDrawable.CursorIndex = null;
            Keyboard.InvalidateOverlay();

            await RunProgressAnimation(animName + "_in", fadeInMs, t =>
            {
                _patternDrawable.AnimAlpha = 0.55f * t;
            });

            await Task.Delay((int)holdMs);

            await RunProgressAnimation(animName + "_out", fadeOutMs, t =>
            {
                _patternDrawable.AnimAlpha = 0.55f * (1f - t);
            });

            ClearAnim();
        }

        public async Task AnimateCursorSequenceAsync(
            IReadOnlyList<int> indices,
            int rounds = 2,
            uint stepMs = 260,
            uint holdMs = 140)
        {
            TrySyncOverlay();

            if (indices == null || indices.Count == 0)
                return;

            _patternDrawable.AnimBits = Array.Empty<bool>();
            _patternDrawable.AnimTargets = Array.Empty<int>();
            _patternDrawable.AnimProgress = 0f;
            _patternDrawable.CursorAlpha = 0.7f;

            for (int round = 0; round < rounds; round++)
            {
                _patternDrawable.CursorIndex = indices[0];
                Keyboard.InvalidateOverlay();
                await Task.Delay((int)holdMs);

                for (int i = 1; i < indices.Count; i++)
                {
                    await AnimateCursor(indices[i - 1], indices[i], stepMs);
                    _patternDrawable.CursorIndex = indices[i];
                    Keyboard.InvalidateOverlay();
                    await Task.Delay((int)holdMs);
                }

                _patternDrawable.CursorIndex = null;
                Keyboard.InvalidateOverlay();
                await Task.Delay(120);
            }
        }

        public async Task AnimateCyclicalStatesAsync(
            bool[] bits,
            int shiftBy1,
            int rounds = 2,
            int stepsPerRound = -1,
            uint fadeMs = 110,
            uint holdMs = 140)
        {
            TrySyncOverlay();

            bits ??= Array.Empty<bool>();
            if (bits.Length == 0)
                return;

            if (stepsPerRound <= 0)
                stepsPerRound = bits.Length;

            bool[] current = bits.ToArray();

            for (int round = 0; round < rounds; round++)
            {
                for (int step = 0; step < stepsPerRound; step++)
                {
                    _patternDrawable.AnimBits = BuildCyclicalMovingBits(current, shiftBy1);
                    _patternDrawable.AnimTargets = BuildCyclicalStepTargets(current, shiftBy1);
                    _patternDrawable.SpawnBits = BuildCyclicalSpawnBits(current, shiftBy1);
                    _patternDrawable.AnimProgress = 0f;
                    _patternDrawable.CursorIndex = null;
                    _patternDrawable.AnimAlpha = 0.55f;
                    _patternDrawable.SpawnAlpha = 0f;
                    Keyboard.InvalidateOverlay();

                    await RunProgressAnimation($"Cyclical_{round}_{step}_move", fadeMs, t =>
                    {
                        _patternDrawable.AnimAlpha = 0.55f;
                        _patternDrawable.AnimProgress = t;
                        _patternDrawable.SpawnAlpha = 0.55f * t;
                    });

                    _patternDrawable.AnimProgress = 1f;
                    _patternDrawable.AnimAlpha = 0.55f;
                    _patternDrawable.SpawnAlpha = 0.55f;
                    Keyboard.InvalidateOverlay();
                    await Task.Delay((int)holdMs);

                    current = ShiftOnceCyclical(current, shiftBy1);
                }
            }

            await RunProgressAnimation("Cyclical_end", Math.Max(220u, fadeMs / 5), t =>
            {
                _patternDrawable.AnimAlpha = 0.55f * (1f - t);
                _patternDrawable.SpawnAlpha = 0.55f * (1f - t);
            });

            ClearAnim();
        }

        public async Task Animate(
     bool[] bits,
     Operation op,
     int shiftByK = 0,
     uint ms = 450)
        {
            await Animate(bits, op, Colors.Yellow, shiftByK, ms);
        }

        public async Task Animate(
     bool[] bits,
     Operation op,
     Color color,
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
            _patternDrawable.AnimColor = color;

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

        public async Task AnimateToTargetsAsync(
            bool[] bits,
            int[] targets,
            uint ms = 2200,
            Color? color = null,
            uint settleMs = 900)
        {
            TrySyncOverlay();
            bits ??= Array.Empty<bool>();
            targets ??= Array.Empty<int>();
            if (bits.Length == 0 || targets.Length == 0)
                return;

            _patternDrawable.AnimBits = bits;
            _patternDrawable.AnimTargets = targets;
            _patternDrawable.AnimProgress = 0f;
            _patternDrawable.CursorIndex = null;
            _patternDrawable.AnimColor = color ?? Colors.Yellow;
            Keyboard.InvalidateOverlay();

            await RunProgressAnimation("PrecisionShiftTutorial", ms,
                progress => _patternDrawable.AnimProgress = progress);
            if (settleMs > 0)
                await Task.Delay((int)settleMs);
            ClearAnim();
        }

        public async Task AnimatePrecisionSignLearningAsync(
            bool[] bits,
            int[] finalTargets,
            int leftDelta,
            bool leftIsShift,
            bool leftBaseAtTop,
            int rightDelta,
            bool rightIsShift,
            bool rightBaseAtTop,
            uint stepMs = 900)
        {
            TrySyncOverlay();
            bits ??= Array.Empty<bool>();
            finalTargets ??= Array.Empty<int>();
            if (bits.Length == 0 || finalTargets.Length == 0)
                return;

            int columns = Math.Max(1, Keyboard.Config?.KeysInRow ?? 1);
            List<(int Current, int Target)> movers = bits
                .Select((selected, index) => (selected, index))
                .Where(item => item.selected && item.index < finalTargets.Length)
                .Select(item => (item.index, Math.Clamp(finalTargets[item.index], 0, bits.Length - 1)))
                .ToList();

            _patternDrawable.ShowPrecisionLearningSign = true;
            _patternDrawable.PrecisionLearningDeltas = new[] { leftDelta, rightDelta };
            _patternDrawable.PrecisionLearningIsShift = new[] { leftIsShift, rightIsShift };
            _patternDrawable.PrecisionLearningBaseAtTop = new[] { leftBaseAtTop, rightBaseAtTop };
            _patternDrawable.PrecisionLearningTrailOrigins = Enumerable.Range(0, Math.Min(2, columns))
                .Select(column =>
                {
                    List<int> active = new();
                    for (int index = column; index < bits.Length; index += columns)
                        if (bits[index]) active.Add(index);
                    if (active.Count < 2)
                        return -1;
                    return (column == 0 ? leftBaseAtTop : rightBaseAtTop)
                        ? active.Min()
                        : active.Max();
                })
                .Concat(Enumerable.Repeat(-1, Math.Max(0, 2 - Math.Min(2, columns))))
                .ToArray();
            _patternDrawable.AnimAlpha = 0.20f;
            _patternDrawable.AnimColor = Colors.Yellow;
            _patternDrawable.PrecisionLearningSignAlpha = 1f;
            _patternDrawable.PrecisionLearningArrowAlpha = 0;

            int stepNumber = 0;
            while (movers.Any(mover => mover.Current != mover.Target))
            {
                bool[] currentBits = new bool[bits.Length];
                int[] stepTargets = Enumerable.Range(0, bits.Length).ToArray();
                for (int i = 0; i < movers.Count; i++)
                {
                    (int current, int target) = movers[i];
                    currentBits[current] = true;
                    if (current == target)
                        continue;

                    int stride = current % columns == target % columns ? columns : 1;
                    int next = current + (Math.Sign(target - current) * stride);
                    if ((target - current) * (target - next) < 0)
                        next = target;
                    stepTargets[current] = Math.Clamp(next, 0, bits.Length - 1);
                }

                _patternDrawable.AnimBits = currentBits;
                _patternDrawable.AnimTargets = stepTargets;
                _patternDrawable.AnimProgress = 0;
                Keyboard.InvalidateOverlay();

                await Task.Delay(stepNumber == 0 ? 650 : 280);
                if (stepNumber == 0)
                {
                    await RunProgressAnimation("PrecisionSignArrowIn", 300,
                        progress => _patternDrawable.PrecisionLearningArrowAlpha = progress);
                }
                await RunProgressAnimation($"PrecisionSignStep_{stepNumber}", stepMs,
                    progress => _patternDrawable.AnimProgress = progress);

                for (int i = 0; i < movers.Count; i++)
                {
                    (int current, int target) = movers[i];
                    movers[i] = (stepTargets[current], target);
                }
                stepNumber++;
            }

            await Task.Delay(500);
            await RunProgressAnimation("PrecisionSignCompleteFade", 180,
                progress =>
                {
                    float alpha = 1f - progress;
                    _patternDrawable.PrecisionLearningArrowAlpha = alpha;
                    _patternDrawable.PrecisionLearningSignAlpha = alpha;
                });
            ClearAnim();
        }

        public async Task AnimateFlipAcrossAxisAsync(
            bool[] bits,
            int[] targets,
            int sourceIndexAdjacentToAxis,
            uint ms = 2200,
            bool axisAboveSource = false,
            Color? color = null,
            uint settleMs = 900,
            bool showLeadIn = true)
        {
            TrySyncOverlay();
            bits ??= Array.Empty<bool>();
            targets ??= Array.Empty<int>();
            if (bits.Length == 0 || targets.Length == 0 ||
                sourceIndexAdjacentToAxis < 0 || sourceIndexAdjacentToAxis >= _keyRects.Length)
                return;

            int columns = Math.Max(1, Keyboard.Config?.KeysInRow ?? 1);
            int neighborIndex = sourceIndexAdjacentToAxis + (axisAboveSource ? columns : -columns);
            if (neighborIndex < 0 || neighborIndex >= _keyRects.Length)
                return;

            RectF sourceRect = _keyRects[sourceIndexAdjacentToAxis];
            RectF neighborRect = _keyRects[neighborIndex];
            _patternDrawable.FlipAxisY = axisAboveSource
                ? (sourceRect.Top + neighborRect.Bottom) / 2f
                : (sourceRect.Bottom + neighborRect.Top) / 2f;
            _patternDrawable.FlipAxisLeft = sourceRect.Left - 4;
            _patternDrawable.FlipAxisRight = sourceRect.Right + 4;
            _patternDrawable.ShowFlipAxis = true;
            _patternDrawable.UseFlipInterpolation = true;
            _patternDrawable.AnimBits = bits;
            _patternDrawable.AnimTargets = targets;
            _patternDrawable.AnimProgress = 0f;
            _patternDrawable.CursorIndex = null;
            _patternDrawable.AnimColor = color ?? Colors.Yellow;
            Keyboard.InvalidateOverlay();

            if (showLeadIn)
                await Task.Delay(ScaleDuration(ms, 0.18));
            await RunProgressAnimation("PrecisionShiftFlipTutorial", ms,
                progress => _patternDrawable.AnimProgress = progress);
            if (settleMs > 0)
                await Task.Delay((int)settleMs);
            ClearAnim();
        }

        private static int ScaleDuration(uint duration, double factor) =>
            Math.Max(1, (int)Math.Round(duration * factor));

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

        public static bool[] ShiftOnceCyclical(bool[] bits, int shiftBy1)
        {
            int n = bits.Length;
            var next = new bool[n];
            if (n == 0)
                return next;

            for (int i = 0; i < n; i++)
            {
                if (!bits[i]) continue;
                int j = (i + shiftBy1 + n) % n;
                next[j] = true;
            }

            return next;
        }

        private static int[] BuildCyclicalStepTargets(bool[] bits, int shiftBy1)
        {
            int n = bits.Length;
            var dest = Enumerable.Range(0, n).ToArray();

            for (int i = 0; i < n; i++)
            {
                if (!bits[i])
                    continue;

                int j = i + shiftBy1;
                if ((uint)j < (uint)n)
                    dest[i] = j;
            }

            return dest;
        }

        private static bool[] BuildCyclicalMovingBits(bool[] bits, int shiftBy1)
        {
            int n = bits.Length;
            var moving = new bool[n];

            for (int i = 0; i < n; i++)
            {
                if (!bits[i])
                    continue;

                int j = i + shiftBy1;
                if ((uint)j < (uint)n)
                    moving[i] = true;
            }

            return moving;
        }

        private static bool[] BuildCyclicalSpawnBits(bool[] bits, int shiftBy1)
        {
            int n = bits.Length;
            var spawn = new bool[n];

            if (n == 0)
                return spawn;

            for (int i = 0; i < n; i++)
            {
                if (!bits[i])
                    continue;

                int j = i + shiftBy1;
                if ((uint)j >= (uint)n)
                {
                    int wrapped = (j + n) % n;
                    spawn[wrapped] = true;
                }
            }

            return spawn;
        }

        private static int[] BuildShiftTargets(bool[] bits, int shiftByK)
        {
            int n = bits.Length;
            var dest = new int[n];
            for (int i = 0; i < n; i++)
                dest[i] = bits[i] ? i + shiftByK : i;
            return dest;
        }

        private static int[] BuildPackLeft(bool[] bits, int startOffset = 0)
        {
            int n = bits.Length;
            var dest = new int[n];

            int write = startOffset;
            for (int i = 0; i < n; i++)
                dest[i] = bits[i] ? write++ : i;

            return dest;
        }

        private static int[] BuildPackRight(bool[] bits, int rightOffset = 0)
        {
            int n = bits.Length;
            var dest = new int[n];

            int count = bits.Count(b => b);
            int write = n - rightOffset - count;

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
