namespace GestureSample.Maui.Handlers;

public sealed class ArrowLabelProgression
{
    public const int SuccessesToAdvance = 4;
    public int Step { get; private set; }
    public int ConsecutiveSuccesses { get; private set; }

    public ArrowLabelProgression(int step) => MoveTo(step);

    public void MoveTo(int step)
    {
        if (step is < 0 or > 3) throw new ArgumentOutOfRangeException(nameof(step));
        Step = step;
        ConsecutiveSuccesses = 0;
    }

    public bool RecordAttempt(bool correct)
    {
        if (!correct) { ConsecutiveSuccesses = 0; return false; }
        if (Step == 3) return false;
        ConsecutiveSuccesses++;
        if (ConsecutiveSuccesses < SuccessesToAdvance) return false;
        MoveTo(Step + 1);
        return true;
    }
}
