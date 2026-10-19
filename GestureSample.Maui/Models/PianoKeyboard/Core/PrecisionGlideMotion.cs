namespace GestureSample.Maui.Models;

// Each key owns its finger's motion. Never infer ownership from other screen touches.
public sealed class PrecisionGlideMotion
{
    private readonly Dictionary<int, (double X, double Y)> _positions = new();
    public void Begin(int key, double x, double y)
    {
        if (double.IsFinite(x) && double.IsFinite(y)) _positions[key] = (x, y);
    }
    public void End(int key) => _positions.Remove(key);
    public void Clear() => _positions.Clear();
    public bool Move(int key, double dx, double dy, out (double X, double Y) position)
    {
        position = default;
        if (!_positions.TryGetValue(key, out var previous) || !double.IsFinite(dx) || !double.IsFinite(dy))
            return false;
        position = (previous.X + dx, previous.Y + dy);
        if (!double.IsFinite(position.X) || !double.IsFinite(position.Y)) return false;
        _positions[key] = position;
        return true;
    }
}
