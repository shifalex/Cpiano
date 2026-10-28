namespace GestureSample.Maui.Models;

// The live exercise is never replaced by a historical exercise.
public sealed class ExerciseReviewHistory<T>
{
    private readonly List<T> _completed = new();
    private int? _reviewIndex;
    public int Count => _completed.Count;
    public bool IsReviewing => _reviewIndex.HasValue;
    public int? ReviewIndex => _reviewIndex;
    public T Current => _reviewIndex.HasValue ? _completed[_reviewIndex.Value] : throw new InvalidOperationException("The live exercise is selected.");

    public void AddCompleted(T snapshot)
    {
        _completed.Add(snapshot);
        ReturnToLive();
    }

    private int LivePosition(bool currentIsCompleted) => Count - (currentIsCompleted ? 1 : 0);
    public int DistanceFromLive(bool currentIsCompleted) => _reviewIndex.HasValue
        ? LivePosition(currentIsCompleted) - _reviewIndex.Value : 0;
    public bool CanMoveBack(bool currentIsCompleted) => (_reviewIndex ?? LivePosition(currentIsCompleted)) > 0;

    public bool MoveBack(bool currentIsCompleted)
    {
        if (!CanMoveBack(currentIsCompleted)) return false;
        _reviewIndex = (_reviewIndex ?? LivePosition(currentIsCompleted)) - 1;
        return true;
    }

    public void MoveForward(bool currentIsCompleted)
    {
        if (!_reviewIndex.HasValue) return;
        int next = _reviewIndex.Value + 1;
        _reviewIndex = next >= LivePosition(currentIsCompleted) ? null : next;
    }

    public void ReturnToLive() => _reviewIndex = null;
}
