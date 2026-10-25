namespace GestureSample.Maui;

public enum CustomArrowQuestionKind { VariableLength, FixedLength, Ordinal, Destination, Distance, Start }

public sealed record SpecificArrowQuestion(int Start, int End)
{
    public int Distance => Math.Abs(End - Start);
    public bool MovesLeft => End < Start;
    public string Equation => $"{Start}{(MovesLeft ? "−" : "+")}{Distance}={End}";
    public static int KeyNumber(int value, int keys = 10) => ((value - 1) % keys + keys) % keys + 1;
    public int KeyboardFromIndex(int keys) => MovesLeft ? KeyNumber(End, keys) % keys : KeyNumber(Start, keys) - 1;
    public int[] GetLabelAnswerKeys() => Enumerable.Range(0, Distance)
        .Select(index => MovesLeft ? Start - index : Start + index + 1).ToArray();
}

public sealed record CustomArrowPracticeSelection
{
    public CustomArrowQuestionKind[] Kinds { get; init; } = Enum.GetValues<CustomArrowQuestionKind>();
    public bool Specific { get; init; }
    public int Start { get; init; } = 3;
    public int End { get; init; } = 7;
    public int MaxLength { get; init; } = 4;

    public string? ValidationError()
    {
        if (Kinds == null || Kinds.Length == 0) return "Choose at least one question type.";
        if (Kinds.Any(kind => !Enum.IsDefined(kind))) return "Choose a valid question type.";
        if (MaxLength is < 1 or > 10) return "Choose a maximum length between 1 and 10.";
        if (!Specific) return null;
        if (Kinds.Distinct().Count() != 1) return "Choose one question type for a specific question.";
        if (Start is < 1 or > 20 || End is < 1 or > 20 || End == Start)
            return "Choose two different keys from 1 to 20.";
        if (Math.Abs(End - Start) > 10) return "For a ten-key keyboard, choose a distance from 1 to 10.";
        return null;
    }
}
