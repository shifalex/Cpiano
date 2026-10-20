namespace GestureSample.Maui.Models;

public sealed partial class KeyboardOverlayHost
{
    private readonly SideGripDrawable _tutorialGripHand = new();
    private bool[] _tutorialGripBits = Array.Empty<bool>();
    private float _tutorialGripOpacity;

    private void DrawGripTutorialHands(ICanvas canvas)
    {
        if (_tutorialGripOpacity <= 0) return;
        int columns = Math.Max(1, Keyboard.Config.KeysInRow);
        for (int column = 0; column < columns; column++)
        {
            var keys = Enumerable.Range(0, Math.Min(_tutorialGripBits.Length, _patternDrawable.KeyRects.Length))
                .Where(i => i % columns == column && _tutorialGripBits[i])
                .Select(i => _patternDrawable.KeyRects[i]).OrderBy(r => r.Center.Y).ToArray();
            if (keys.Length != 2) continue;
            RectF upper = keys[0], lower = keys[1];
            float tip = upper.Center.X;
            float width = upper.Width * .36f;
            canvas.SaveState();
            // Mirror the left hand, keeping both fingertips centered on their keys.
            if (column == 0)
            {
                canvas.Translate(tip * 2, 0);
                canvas.Scale(-1, 1);
            }
            canvas.Translate((1 - _tutorialGripOpacity) * width, 0);
            _tutorialGripHand.Draw(canvas, tip, upper.Center.Y, lower.Center.Y,
                Math.Min(upper.Height, lower.Height), width, _tutorialGripOpacity,
                upperFingerExtension: upper.Width / 2 - Math.Min(upper.Height, lower.Height) / 8);
            canvas.RestoreState();
        }
    }

    public async Task ShowGripHandTutorialAsync(bool[] bits)
    {
        await EnsureOverlaySyncedAsync();
        _tutorialGripBits = (bool[])bits.Clone();
        try
        {
            await RunProgressAnimation("GripHandArrive", 350, t => _tutorialGripOpacity = t);
            await DelayPresentationAsync(TimeSpan.FromSeconds(2));
            await RunProgressAnimation("GripHandLeave", 250, t => _tutorialGripOpacity = 1 - t);
        }
        finally
        {
            _tutorialGripBits = Array.Empty<bool>();
            _tutorialGripOpacity = 0;
            Keyboard.InvalidateOverlay();
        }
    }
}
