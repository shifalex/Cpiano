using GestureSample.Maui.Handlers;
using Microsoft.Maui.Controls.Shapes;

namespace GestureSample.Maui.Models;

public sealed partial class KeyboardOverlayHost
{
    private Border? _turnHalo;
    private Border? _turnBadge;
    private GraphicsView? _turnIcon;
    private GripTurnDrawable? _turnDrawing;
    private GripInputPhase? _lastTurnPhase;
    private readonly GripPresentationClock _presentationClock = new();
    private bool _temporaryTurnCue;
    private bool _showReadyAfterViolation;
    private CancellationTokenSource? _turnViolationCancellation;
    private readonly Dictionary<string, CancellationTokenSource> _presentationAnimations = new();
    private GraphicsView? _keyboardAmbient;
    private readonly GripAmbientDrawable _ambientDrawing = new();

    public View CreateGripAmbientView(VisualElement keyboardShell)
    {
        _keyboardAmbient = new GraphicsView
        {
            Drawable = _ambientDrawing, InputTransparent = true, ZIndex = -1,
            // The glow must not enlarge the measured cluster or shift header controls.
            Margin = new Thickness(-24),
            HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center
        };
        keyboardShell.SizeChanged += (_, _) =>
        {
            _keyboardAmbient.WidthRequest = Math.Max(0, keyboardShell.Width) + 48;
            _keyboardAmbient.HeightRequest = Math.Max(0, keyboardShell.Height) + 48;
            _keyboardAmbient.Invalidate();
        };
        RefreshGripTurnCue();
        return _keyboardAmbient;
    }

    private sealed class GripAmbientDrawable : IDrawable
    {
        public Color Color = Colors.Transparent;
        public bool Visible;
        public void Draw(ICanvas canvas, RectF bounds)
        {
            if (!Visible) return;
            // Explicit translucent bands render outside the opaque keyboard on iPad;
            // they do not rely on a native shadow escaping a clipped rounded border.
            for (int inset = 0; inset < 24; inset += 2)
            {
                canvas.FillColor = Color.WithAlpha(.012f + inset * .0015f);
                canvas.FillRoundedRectangle(bounds.X + inset, bounds.Y + inset,
                    Math.Max(0, bounds.Width - inset * 2), Math.Max(0, bounds.Height - inset * 2), 28 - inset * .5f);
            }
        }
    }

    private void InitializeGripTurnCue()
    {
        if (Keyboard is not PianoKeyboard piano || piano.GripGate is not { } gate) return;
        _turnHalo = new Border
        {
            InputTransparent = true, ZIndex = -1, Margin = new Thickness(-6),
            StrokeShape = new RoundRectangle { CornerRadius = 24 }, StrokeThickness = 2
        };
        Children.Insert(0, _turnHalo);
        _turnDrawing = new GripTurnDrawable { GetKeyRects = () => _keyRects };
        _turnIcon = new GraphicsView { Drawable = _turnDrawing, InputTransparent = true };
        _turnBadge = new Border
        {
            InputTransparent = true, ZIndex = 100, Padding = 0,
            HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill,
            BackgroundColor = Colors.Transparent, StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            Content = _turnIcon
        };
        Children.Add(_turnBadge);
        gate.Changed += RefreshGripTurnCue;
        gate.PrematurePress += ShowGripViolationCue;
        Loaded += (_, _) => RefreshGripTurnCue();
        Unloaded += (_, _) =>
        {
            _turnViolationCancellation?.Cancel();
            _temporaryTurnCue = false;
            _showReadyAfterViolation = false;
            foreach (var animation in _presentationAnimations.Values.ToArray()) animation.Cancel();
            this.AbortAnimation("GripTurnPulse");
            _lastTurnPhase = null;
        };
        RefreshGripTurnCue();
    }

    private void RefreshGripTurnCue()
    {
        if (Keyboard is not PianoKeyboard piano || piano.GripGate is not { } gate || _turnHalo == null) return;
        GripInputPhase phase = gate.Phase;
        Color ink = phase == GripInputPhase.Ready ? Color.FromArgb("#168657") : Color.FromArgb("#B75A32");
        _turnHalo.BackgroundColor = ink.WithAlpha(.08f);
        _turnHalo.Stroke = ink.WithAlpha(.42f);
        _turnHalo.Shadow = new Shadow { Brush = new SolidColorBrush(ink), Opacity = .42f, Radius = 24, Offset = new Point(0, 0) };
        bool phaseChanged = _lastTurnPhase != phase;
        if (phaseChanged)
        {
            _turnViolationCancellation?.Cancel();
            _turnViolationCancellation = null;
            _lastTurnPhase = phase;
            _temporaryTurnCue = false;
            if (phase == GripInputPhase.Lift)
            {
                _showReadyAfterViolation = true;
                _temporaryTurnCue = true;
            }
            if (phase == GripInputPhase.Ready && _showReadyAfterViolation)
            {
                _showReadyAfterViolation = false;
                _temporaryTurnCue = true;
            }
            if (_temporaryTurnCue) HideTurnCueAfterDelay();
        }
        bool showCue = _temporaryTurnCue;
        _turnHalo.IsVisible = true;
        _ambientDrawing.Color = ink;
        _ambientDrawing.Visible = true;
        _keyboardAmbient?.Invalidate();
        _turnBadge!.IsVisible = showCue;

        SemanticProperties.SetDescription(_turnBadge, AppLanguage.Text(phase switch
        {
            GripInputPhase.Ready => "You can press now",
            GripInputPhase.Lift => "Lift your fingers",
            _ => "Watch and wait"
        }, true));
        if (!showCue) this.AbortAnimation("GripTurnPulse");
        _turnDrawing!.Phase = phase;
        _turnDrawing.Color = ink;
        _turnIcon!.Invalidate();
        if (phaseChanged) PulseGripTurnCue();
    }

    private void ShowGripViolationCue()
    {
        _turnViolationCancellation?.Cancel();
        _turnViolationCancellation = null;
        _temporaryTurnCue = true;
        _showReadyAfterViolation = true;
        _presentationClock.Pause(1000);
        RefreshGripTurnCue();
        HideTurnCueAfterDelay();
        PulseGripTurnCue();
    }

    private async void HideTurnCueAfterDelay()
    {
        _turnViolationCancellation?.Cancel();
        using var cancellation = new CancellationTokenSource();
        _turnViolationCancellation = cancellation;
        try
        {
            // Give the green hand its full display time, even after a long wait
            // or when the last held finger is released after the demonstration.
            await Task.Delay(1000, cancellation.Token);
            if (ReferenceEquals(_turnViolationCancellation, cancellation))
            {
                _temporaryTurnCue = false;
                RefreshGripTurnCue();
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            if (ReferenceEquals(_turnViolationCancellation, cancellation))
                _turnViolationCancellation = null;
        }
    }

    public async Task DelayPresentationAsync(TimeSpan duration)
    {
        if (Keyboard is not PianoKeyboard piano || piano.GripGate == null)
        {
            await Task.Delay(duration);
            return;
        }
        double end = _presentationClock.ElapsedMilliseconds + duration.TotalMilliseconds;
        while (_presentationClock.ElapsedMilliseconds < end)
            await Task.Delay(16);
    }

    private async Task RunPausablePresentationAsync(string name, uint milliseconds, Action<float> setProgress)
    {
        if (_presentationAnimations.TryGetValue(name, out var previous)) previous.Cancel();
        using var cancellation = new CancellationTokenSource();
        _presentationAnimations[name] = cancellation;
        double start = _presentationClock.ElapsedMilliseconds;
        try
        {
            while (true)
            {
                cancellation.Token.ThrowIfCancellationRequested();
                double progress = Math.Clamp((_presentationClock.ElapsedMilliseconds - start) / Math.Max(1u, milliseconds), 0, 1);
                setProgress((float)Easing.CubicInOut.Ease(progress));
                Keyboard.InvalidateOverlay();
                if (progress >= 1) break;
                await Task.Delay(16, cancellation.Token);
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            if (_presentationAnimations.TryGetValue(name, out var current) && ReferenceEquals(current, cancellation))
                _presentationAnimations.Remove(name);
        }
    }

    private void PulseGripTurnCue()
    {
        if (!_temporaryTurnCue || _turnBadge == null || this.AnimationIsRunning("GripTurnPulse")) return;
        new Animation(value =>
        {
            double pulse = Math.Sin(value * Math.PI);
            _turnBadge.Opacity = 1 - .12 * pulse;
            _turnIcon!.TranslationY = _lastTurnPhase == GripInputPhase.Lift ? -5 * pulse : 0;
            _turnHalo!.Opacity = 1 - .25 * pulse;
            if (_keyboardAmbient != null) _keyboardAmbient.Opacity = 1 - .25 * pulse;
        }, 0, 1).Commit(this, "GripTurnPulse", length: 700,
            finished: (_, _) =>
            {
                _turnBadge.Opacity = 1; _turnIcon!.TranslationY = 0; _turnHalo!.Opacity = 1;
                if (_keyboardAmbient != null) _keyboardAmbient.Opacity = 1;
            },
            repeat: () => IsLoaded && _temporaryTurnCue && _lastTurnPhase == GripInputPhase.Lift);
    }

    private sealed class GripTurnDrawable : IDrawable
    {
        public GripInputPhase Phase;
        public Color Color = Colors.DarkOrange;
        public Func<RectF[]> GetKeyRects = () => Array.Empty<RectF>();
        public void Draw(ICanvas canvas, RectF bounds)
        {
            RectF[] keys = GetKeyRects().Where(key => key.Width > 0 && key.Height > 0).ToArray();
            if (keys.Length == 0) return;

            // Size one symbol to the complete key area, excluding the heading.
            float left = keys.Min(key => key.Left);
            float top = keys.Min(key => key.Top);
            float right = keys.Max(key => key.Right);
            float bottom = keys.Max(key => key.Bottom);
            float scale = Math.Min(right - left, bottom - top) * .9f / 28;
            canvas.SaveState();
            canvas.Alpha = .5f;
            canvas.Translate((left + right) / 2 - 14 * scale, (top + bottom) / 2 - 14 * scale);
            canvas.Scale(scale, scale);
            DrawIcon(canvas);
            canvas.RestoreState();
        }

        private void DrawIcon(ICanvas canvas)
        {
            canvas.StrokeColor = Color;
            canvas.FillColor = Color;
            canvas.StrokeSize = 1.8f;
            canvas.StrokeLineCap = LineCap.Round;
            canvas.StrokeLineJoin = LineJoin.Round;
            if (Phase == GripInputPhase.Watching)
            {
                PathF eye = new();
                eye.MoveTo(2, 14); eye.CurveTo(8, 4, 20, 4, 26, 14);
                eye.CurveTo(20, 24, 8, 24, 2, 14);
                canvas.DrawPath(eye); canvas.FillCircle(14, 14, 3);
                return;
            }
            if (Phase == GripInputPhase.Ready)
            {
                // The visible nail identifies the back of an extended finger.
                PathF finger = new();
                finger.MoveTo(10, 27); finger.LineTo(10, 13);
                finger.CurveTo(10, 7, 18, 7, 18, 13);
                finger.LineTo(18, 27);
                canvas.DrawPath(finger);

                canvas.StrokeSize = 1.4f;
                canvas.DrawRoundedRectangle(12, 11, 4, 5, 1.5f);
                PathF waves = new();
                waves.MoveTo(8, 9); waves.CurveTo(6, 11, 6, 15, 8, 17);
                waves.MoveTo(20, 9); waves.CurveTo(22, 11, 22, 15, 20, 17);
                waves.MoveTo(10, 7); waves.CurveTo(12, 5, 16, 5, 18, 7);
                canvas.DrawPath(waves);
                return;
            }
            PathF hand = new();
            hand.MoveTo(9, 25); hand.LineTo(3, 16); hand.CurveTo(2, 13, 5, 12, 8, 16);
            hand.LineTo(8, 7); hand.CurveTo(8, 4, 11, 4, 11, 7);
            hand.LineTo(11, 13); hand.LineTo(11, 4); hand.CurveTo(11, 1, 14, 1, 14, 4);
            hand.LineTo(14, 13); hand.LineTo(14, 6); hand.CurveTo(14, 3, 17, 3, 17, 6);
            hand.LineTo(17, 14); hand.LineTo(17, 9); hand.CurveTo(17, 6, 20, 6, 20, 9);
            hand.LineTo(20, 19); hand.CurveTo(20, 22, 18, 25, 17, 25);
            canvas.DrawPath(hand);
            if (Phase == GripInputPhase.Lift)
            {
                canvas.DrawLine(25, 12, 25, 3);
                canvas.DrawLine(22, 6, 25, 3); canvas.DrawLine(25, 3, 28, 6);
            }
        }
    }
}
