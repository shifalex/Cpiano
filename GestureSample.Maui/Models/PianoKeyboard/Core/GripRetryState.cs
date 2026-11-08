namespace GestureSample.Maui.Models;

public sealed class GripRetryState
{
    private bool[]? _rejected;
    public void Reject(bool[] submitted) => _rejected = (bool[])submitted.Clone();
    public void Reset() => _rejected = null;
    public bool CanTime(bool[] current)
    {
        if (_rejected == null) return true;
        if (_rejected.SequenceEqual(current)) return false;
        _rejected = null;
        return true;
    }
}
