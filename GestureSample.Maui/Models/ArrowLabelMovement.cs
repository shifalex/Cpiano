namespace GestureSample.Maui;

public static class ArrowLabelMovement
{
    public static SpecificArrowQuestion Next(Random random, int minimum, int maximum, int maxLength,
        bool withinTen, bool throughTen, bool edges, int? previousEnd)
    {
        int limit = maxLength <= 0 ? 10 : Math.Min(10, maxLength);
        var candidates = new List<SpecificArrowQuestion>();
        for (int start = minimum; start <= maximum; start++)
        for (int end = minimum; end <= maximum; end++)
        {
            var question = new SpecificArrowQuestion(start, end);
            if (start == end || question.Distance > limit) continue;
            if (previousEnd.HasValue && start != previousEnd.Value) continue;
            int startKey = SpecificArrowQuestion.KeyNumber(start);
            if (edges && startKey is not (1 or 10)) continue;
            bool crosses = false;
            for (int ten = 10; ten < maximum; ten += 10)
                if (Math.Min(start, end) < ten && Math.Max(start, end) > ten) crosses = true;
            if (withinTen && crosses || throughTen && !crosses) continue;
            candidates.Add(question);
        }
        if (candidates.Count == 0)
            throw new InvalidOperationException("No questions fit this combination of length, starting point and crossing ten.");
        return candidates[random.Next(candidates.Count)];
    }
}
