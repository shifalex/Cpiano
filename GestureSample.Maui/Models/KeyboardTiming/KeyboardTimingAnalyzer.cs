using GestureSample.Maui.Data.SQLite;

namespace GestureSample.Maui.Models
{
    public enum KeyboardPressPatternKind
    {
        Unknown = 0,
        SingleKey = 1,
        GroupPress = 2,
        PartialGroup = 3,
        SequencePress = 4
    }

    internal sealed class KeyboardAttemptTimingMetrics
    {
        public static KeyboardAttemptTimingMetrics Empty { get; } = new();

        public int KeyDownCount { get; init; }
        public int DistinctKeyCount { get; init; }
        public int PressClusterCount { get; init; }
        public int LargestPressClusterSize { get; init; }
        public int MaxInterKeyGapMs { get; init; }
        public int AverageInterKeyGapMs { get; init; }
        public int FirstKeyToSubmitMs { get; init; }
        public int LastKeyToSubmitMs { get; init; }
        public KeyboardPressPatternKind PressPatternKind { get; init; }
    }

    internal sealed class KeyboardTimingRecommendation
    {
        public int AttemptCount { get; init; }
        public KeyboardPressPatternKind DominantPattern { get; init; }
        public int RecommendedAfterLastKeySeconds { get; init; }
        public int RecommendedWholeAnswerSeconds { get; init; }
        public int P80MaxGapMs { get; init; }
        public int P80FirstKeyToSubmitMs { get; init; }
    }

    internal static class KeyboardTimingAnalyzer
    {
        private const int ClusterGapThresholdMs = 180;
        private const int MinimumRecommendationAttempts = 4;
        private const int MinimumTimerSeconds = 1;
        private const int MaximumTimerSeconds = 5;

        public static KeyboardAttemptTimingMetrics AnalyzeAttempt(IEnumerable<KeyEvent>? attemptEvents, DateTime submittedTime)
        {
            List<KeyEvent> keyDownEvents = attemptEvents?
                .Where(item => item.EventType == 1)
                .OrderBy(item => item.EventTime)
                .ThenBy(item => item.id)
                .ToList() ?? new List<KeyEvent>();

            if (keyDownEvents.Count == 0)
                return KeyboardAttemptTimingMetrics.Empty;

            List<int> gaps = new();
            List<int> clusterSizes = new();
            int currentClusterSize = 1;

            for (int i = 1; i < keyDownEvents.Count; i++)
            {
                int gapMs = ToMilliseconds(keyDownEvents[i].EventTime - keyDownEvents[i - 1].EventTime);
                gaps.Add(gapMs);

                if (gapMs <= ClusterGapThresholdMs)
                {
                    currentClusterSize++;
                }
                else
                {
                    clusterSizes.Add(currentClusterSize);
                    currentClusterSize = 1;
                }
            }

            clusterSizes.Add(currentClusterSize);

            DateTime firstKeyTime = keyDownEvents[0].EventTime;
            DateTime lastKeyTime = keyDownEvents[^1].EventTime;
            int firstKeyToSubmitMs = ToMilliseconds(submittedTime - firstKeyTime);
            int lastKeyToSubmitMs = ToMilliseconds(submittedTime - lastKeyTime);

            return new KeyboardAttemptTimingMetrics
            {
                KeyDownCount = keyDownEvents.Count,
                DistinctKeyCount = keyDownEvents.Select(item => item.KeyNumber).Distinct().Count(),
                PressClusterCount = clusterSizes.Count,
                LargestPressClusterSize = clusterSizes.Max(),
                MaxInterKeyGapMs = gaps.Count == 0 ? 0 : gaps.Max(),
                AverageInterKeyGapMs = gaps.Count == 0 ? 0 : (int)Math.Round(gaps.Average()),
                FirstKeyToSubmitMs = firstKeyToSubmitMs,
                LastKeyToSubmitMs = lastKeyToSubmitMs,
                PressPatternKind = ClassifyPattern(keyDownEvents.Count, clusterSizes)
            };
        }

        public static KeyboardTimingRecommendation? BuildRecommendation(IEnumerable<KeyboardQuestion>? attempts)
        {
            List<KeyboardQuestion> usableAttempts = attempts?
                .Where(item => item != null)
                .Where(item => item.ResultStatus == 1)
                .Where(item => item.KeyDownCount > 0)
                .Where(item => item.FirstKeyToSubmitMs > 0)
                .ToList() ?? new List<KeyboardQuestion>();

            if (usableAttempts.Count < MinimumRecommendationAttempts)
                return null;

            List<int> maxGapMs = usableAttempts
                .Select(item => item.MaxInterKeyGapMs)
                .Where(value => value > 0)
                .OrderBy(value => value)
                .ToList();

            List<int> firstKeyToSubmitMs = usableAttempts
                .Select(item => item.FirstKeyToSubmitMs)
                .Where(value => value > 0)
                .OrderBy(value => value)
                .ToList();

            if (maxGapMs.Count == 0 || firstKeyToSubmitMs.Count == 0)
                return null;

            int p80GapMs = Percentile(maxGapMs, 0.80);
            int p80WholeAnswerMs = Percentile(firstKeyToSubmitMs, 0.80);
            KeyboardPressPatternKind dominantPattern = usableAttempts
                .GroupBy(item => (KeyboardPressPatternKind)item.PressPatternKind)
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key == KeyboardPressPatternKind.Unknown ? int.MaxValue : (int)group.Key)
                .Select(group => group.Key)
                .FirstOrDefault();

            int afterLastKeyMs = p80GapMs + Math.Max(220, (int)Math.Round(p80GapMs * 0.25));
            int wholeAnswerMs = p80WholeAnswerMs + Math.Max(280, (int)Math.Round(p80WholeAnswerMs * 0.12));

            return new KeyboardTimingRecommendation
            {
                AttemptCount = usableAttempts.Count,
                DominantPattern = dominantPattern,
                RecommendedAfterLastKeySeconds = ClampSeconds(afterLastKeyMs),
                RecommendedWholeAnswerSeconds = ClampSeconds(wholeAnswerMs),
                P80MaxGapMs = p80GapMs,
                P80FirstKeyToSubmitMs = p80WholeAnswerMs
            };
        }

        public static string ToDisplayText(KeyboardPressPatternKind kind)
        {
            return kind switch
            {
                KeyboardPressPatternKind.SingleKey => "Single key",
                KeyboardPressPatternKind.GroupPress => "Group press",
                KeyboardPressPatternKind.PartialGroup => "Partial group",
                KeyboardPressPatternKind.SequencePress => "Sequence press",
                _ => "Unknown"
            };
        }

        private static KeyboardPressPatternKind ClassifyPattern(int keyDownCount, IReadOnlyList<int> clusterSizes)
        {
            if (keyDownCount <= 0)
                return KeyboardPressPatternKind.Unknown;

            if (keyDownCount == 1)
                return KeyboardPressPatternKind.SingleKey;

            if (clusterSizes.Count <= 1)
                return KeyboardPressPatternKind.GroupPress;

            if (clusterSizes.All(size => size == 1))
                return KeyboardPressPatternKind.SequencePress;

            return KeyboardPressPatternKind.PartialGroup;
        }

        private static int Percentile(IReadOnlyList<int> sortedValues, double percentile)
        {
            if (sortedValues.Count == 0)
                return 0;

            int index = (int)Math.Ceiling(sortedValues.Count * percentile) - 1;
            index = Math.Clamp(index, 0, sortedValues.Count - 1);
            return sortedValues[index];
        }

        private static int ClampSeconds(int durationMs)
        {
            int seconds = (int)Math.Ceiling(durationMs / 1000d);
            return Math.Clamp(seconds, MinimumTimerSeconds, MaximumTimerSeconds);
        }

        private static int ToMilliseconds(TimeSpan duration)
        {
            if (duration < TimeSpan.Zero)
                duration = TimeSpan.Zero;

            return (int)Math.Round(duration.TotalMilliseconds);
        }
    }
}
