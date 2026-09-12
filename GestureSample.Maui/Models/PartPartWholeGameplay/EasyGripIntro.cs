namespace GestureSample.Maui.Models;

internal static class EasyGripIntro
{
    // Generation runs before BeginExercise increments the question number.
    public static bool Applies(bool easy, int completedQuestions) => easy && completedQuestions < 4;

    public static (int Initial, int Target) ChooseSizes(Random random, int rows, bool grow, int? previousLeftSize)
    {
        int minimum = grow ? 2 : 3;
        int maximum = grow ? rows - 1 : rows;
        if (previousLeftSize is int previous)
        {
            // A live keyboard-size change can remove the previous rows.
            previous = Math.Clamp(previous, 2, rows);
            minimum = Math.Max(minimum, previous - 1);
            maximum = Math.Min(maximum, previous + 1);
        }
        int initial = random.Next(minimum, maximum + 1);
        return (initial, initial + (grow ? 1 : -1));
    }
}
