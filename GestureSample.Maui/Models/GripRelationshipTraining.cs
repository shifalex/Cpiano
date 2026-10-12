namespace GestureSample.Maui.Models;

public readonly record struct TrainingGrip(int Bottom, int Length)
{
    public int Top => Bottom + Length;
}

public sealed record GripRelationshipTrial(TrainingGrip Left, TrainingGrip Right)
{
    // Compare lengths, independently of either grip's vertical position.
    public int LengthDifference => Math.Abs(Left.Length - Right.Length);
    // Difference 3 is deliberately not classified: this tutorial uses 0–2 and 4 only.
    public bool? LengthsAreClose => LengthDifference <= 2 ? true : LengthDifference == 4 ? false : null;
    public string CompareLength(bool left)
    {
        int difference = left ? Left.Length - Right.Length : Right.Length - Left.Length;
        return difference > 0 ? "ארוכה יותר" : difference < 0 ? "קצרה יותר" : "שוות באורך";
    }
    public bool HasSmaller => Left.Length != Right.Length;
    public TrainingGrip Smaller => Left.Length < Right.Length ? Left : Right;
    public TrainingGrip Larger => Left.Length > Right.Length ? Left : Right;
    public string Alignment => Smaller.Top == Larger.Top ? "למעלה"
        : Smaller.Bottom == Larger.Bottom ? "למטה" : "ללא הצמדה";

    public static IReadOnlyList<GripRelationshipTrial> Examples { get; } = new[]
    {
        new GripRelationshipTrial(new(1, 4), new(1, 2)),
        new GripRelationshipTrial(new(1, 4), new(3, 2)),
        new GripRelationshipTrial(new(2, 2), new(2, 4)),
        new GripRelationshipTrial(new(1, 4), new(2, 2)),
        new GripRelationshipTrial(new(1, 6), new(1, 7)),
        new GripRelationshipTrial(new(1, 6), new(1, 8)),
        new GripRelationshipTrial(new(1, 4), new(1, 8)),
        new GripRelationshipTrial(new(0, 6), new(5, 7)),
        new GripRelationshipTrial(new(0, 6), new(5, 6))
    };
}
