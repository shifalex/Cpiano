namespace GestureSample.Maui.Models
{
    public sealed class GameCompletionResult
    {
        public Guid GameId { get; init; }
        public bool IsWin { get; init; }
        public TimeSpan Duration { get; init; }
    }

    public sealed class ExerciseCheckResult
    {
        public required bool IsCorrect { get; init; }
        public required string Status { get; init; }
        public bool IsWrongInput { get; init; }
        public GameCompletionResult? Completion { get; init; }
        public bool RefreshCurrentQuestion { get; init; }

        public bool IsGameOver => Completion != null;
        public bool ShouldDelayFeedback => !IsWrongInput;
    }
}
