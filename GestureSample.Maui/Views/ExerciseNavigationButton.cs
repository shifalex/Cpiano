using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace GestureSample.Maui.Views;

// Draw the chevrons directly so every platform has an icon without image loading.
internal sealed class ExerciseNavigationButton : Border
{
    private readonly ChevronDrawable _chevron;
    private readonly GraphicsView _icon;
    private bool _hovering;
    public event EventHandler? Clicked;

    public ExerciseNavigationButton(bool forward, bool doubleArrow = false)
    {
        WidthRequest = 48;
        HeightRequest = doubleArrow ? 36 : 88;
        Padding = 0;
        StrokeThickness = 1;
        StrokeShape = new RoundRectangle { CornerRadius = 8 };
        FlowDirection = FlowDirection.LeftToRight;
        _chevron = new ChevronDrawable(forward, doubleArrow);
        _icon = new GraphicsView { Drawable = _chevron, InputTransparent = true };
        Content = _icon;
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { if (IsEnabled) Clicked?.Invoke(this, EventArgs.Empty); };
        GestureRecognizers.Add(tap);
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => { _hovering = true; RefreshAppearance(); };
        pointer.PointerExited += (_, _) => { _hovering = false; RefreshAppearance(); };
        GestureRecognizers.Add(pointer);
        RefreshAppearance();
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName && _icon != null) RefreshAppearance();
    }

    private void RefreshAppearance()
    {
        BackgroundColor = Colors.White.WithAlpha(IsEnabled && _hovering ? 0.55f : 0.12f);
        Stroke = Color.FromArgb("#57726B").WithAlpha(IsEnabled ? 0.2f : 0.09f);
        _chevron.Ink = Color.FromArgb("#29483F").WithAlpha(IsEnabled ? 1 : 0.38f);
        _icon.Invalidate();
    }

    private sealed class ChevronDrawable(bool forward, bool doubleArrow) : IDrawable
    {
        public Color Ink { get; set; } = Color.FromArgb("#29483F");
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.SaveState();
            canvas.StrokeColor = Ink;
            canvas.StrokeSize = 2.8f;
            canvas.StrokeLineCap = LineCap.Round;
            float centerX = dirtyRect.Center.X;
            float centerY = dirtyRect.Center.Y;
            float direction = forward ? 1 : -1;
            float halfHeight = doubleArrow ? 8 : 12;
            foreach (float offset in doubleArrow ? new[] { -5f, 5f } : new[] { 0f })
            {
                float tipX = centerX + offset + direction * 5;
                float tailX = centerX + offset - direction * 5;
                canvas.DrawLine(tailX, centerY - halfHeight, tipX, centerY);
                canvas.DrawLine(tipX, centerY, tailX, centerY + halfHeight);
            }
            canvas.RestoreState();
        }
    }
}
