using GestureSample.Maui.Models;

int checks = 0;
void Check(bool condition, string description)
{
    checks++;
    if (!condition) throw new Exception(description);
}
var cases = GripRelationshipTrial.Examples;
Check(cases.Count == 9, "Nine progressive practice pairs");
Check(cases[0].Alignment == "למטה", "Small grip aligned at bottom");
Check(cases[1].Alignment == "למעלה", "Small grip aligned at top");
Check(cases[2].Alignment == "למטה" && cases[2].Left.Length < cases[2].Right.Length,
    "Swapping hands does not change the alignment criterion");
Check(cases[3].Alignment == "ללא הצמדה", "Interior small grip has no aligned endpoint");
Check(cases[4].LengthDifference == 1 && cases[4].LengthsAreClose == true, "6 and 7 are long and close");
Check(cases[5].LengthDifference == 2 && cases[5].LengthsAreClose == null, "6 and 8 are not a close/far question");
Check(cases[6].LengthDifference == 5 && cases[6].LengthsAreClose == false, "2 and 7 are far in length despite aligned bottoms");
Check(cases[7].LengthDifference == cases[4].LengthDifference && cases[7].LengthsAreClose == true,
    "Different starting heights do not change closeness of 6 and 7");
Check(!cases[8].HasSmaller && cases[8].LengthsAreClose == null, "Equal lengths skip close/far and alignment questions");
Check(cases[4].CompareLength(true) == "קצרה יותר" && cases[4].CompareLength(false) == "ארוכה יותר",
    "Even two long grips have a relatively shorter and longer member");
Check(cases[8].CompareLength(true) == "שוות באורך", "Equal length is a separate answer");
Check(new GripRelationshipTrial(new(0, 2), new(0, 5)).LengthsAreClose == false,
    "Difference 3 and ratio greater than two are far");
Check(new GripRelationshipTrial(new(0, 4), new(0, 5)).LengthsAreClose == null,
    "Both close lengths must exceed four");
Check(new GripRelationshipTrial(new(0, 5), new(0, 6)).LengthsAreClose == true,
    "Difference one with both lengths above four is close");
Check(new GripRelationshipTrial(new(0, 3), new(0, 6)).LengthsAreClose == null,
    "Exactly double is not far");
Check(new GripRelationshipTrial(new(0, 1), new(0, 3)).LengthsAreClose == null,
    "A large ratio with difference below three is not far");
foreach (var example in cases)
{
    foreach (var grip in new[] { example.Left, example.Right })
        Check(grip.Bottom >= 0 && grip.Top <= 12 && grip.Length > 0, "Visible grip");
    Check(example.LengthDifference == new GripRelationshipTrial(example.Right, example.Left).LengthDifference,
        "Length difference is symmetric");
    Check(new GripRelationshipTrial(example.Left with { Bottom = 10 }, example.Right with { Bottom = -5 }).LengthsAreClose
        == example.LengthsAreClose, "Closeness is independent of placement");
}
Console.WriteLine($"Passed {checks} grip relationship checks.");
