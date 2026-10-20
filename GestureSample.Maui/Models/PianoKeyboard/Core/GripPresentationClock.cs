using System.Diagnostics;

namespace GestureSample.Maui.Models;

/// <summary>A monotonic presentation clock that excludes temporary guidance pauses.</summary>
public sealed class GripPresentationClock
{
    private readonly Func<double> _now;
    private double _pausedUntil;
    private double _totalPause;
    public GripPresentationClock(Func<double>? now = null) =>
        _now = now ?? (() => Stopwatch.GetTimestamp() * 1000d / Stopwatch.Frequency);

    public double ElapsedMilliseconds
    {
        get
        {
            double now = _now();
            return now - _totalPause + Math.Max(0, _pausedUntil - now);
        }
    }

    public void Pause(double milliseconds)
    {
        double now = _now();
        double end = now + Math.Max(0, milliseconds);
        _totalPause += Math.Max(0, end - Math.Max(now, _pausedUntil));
        _pausedUntil = Math.Max(_pausedUntil, end);
    }
}
