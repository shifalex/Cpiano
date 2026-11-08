namespace GestureSample.Maui.Models;

public sealed record XorQuestionPair(bool[] First, bool[] Second);

public static class XorSingleDifferenceSequence
{
    public static XorQuestionPair Next(Random random, int keyCount, bool[]? previousFirst = null, bool[]? previousSecond = null)
    {
        if (keyCount < 2) throw new ArgumentOutOfRangeException(nameof(keyCount));
        bool hasPrevious = previousFirst?.Length == keyCount && previousSecond?.Length == keyCount;
        bool keepFirst = random.Next(2) == 0;
        bool[] retained = hasPrevious
            ? (keepFirst ? previousFirst! : previousSecond!).ToArray()
            : Enumerable.Range(0, keyCount).Select(_ => random.Next(2) == 1).ToArray();
        int[] oldDifferences = hasPrevious
            ? Enumerable.Range(0, keyCount).Where(i => previousFirst![i] != previousSecond![i]).ToArray()
            : Array.Empty<int>();
        // Move the unique difference so the next question actually changes.
        int[] candidates = Enumerable.Range(0, keyCount)
            .Where(i => oldDifferences.Length != 1 || i != oldDifferences[0]).ToArray();
        int difference = candidates[random.Next(candidates.Length)];
        bool[] changed = retained.ToArray();
        changed[difference] = !changed[difference];
        return keepFirst ? new(retained, changed) : new(changed, retained);
    }
}
