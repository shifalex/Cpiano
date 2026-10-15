using GestureSample.Maui.Models;

int checks = 0;
void Check(bool condition, string message)
{
    checks++;
    if (!condition) throw new Exception(message);
}
bool[] Bits(int rows, int l0, int l1, int r0, int r1, bool swap = false)
{
    var bits = new bool[rows * 2];
    int left = swap ? 1 : 0;
    bits[l0 * 2 + left] = bits[l1 * 2 + left] = true;
    bits[r0 * 2 + 1 - left] = bits[r1 * 2 + 1 - left] = true;
    return bits;
}

foreach (bool swap in new[] { false, true })
for (int rows = 7; rows <= 12; rows++)
{
    int initialBoundary = rows / 2;
    var initial = Bits(rows, 0, initialBoundary - 1, initialBoundary, rows - 1, swap);
    var example = Bits(rows, 0, initialBoundary, initialBoundary + 1, rows - 1, swap);
    for (int boundary = 1; boundary < rows; boundary++)
    {
        var answer = Bits(rows, 0, boundary - 1, boundary, rows - 1, swap);
        foreach (bool up in new[] { false, true })
        {
            int movement = up ? boundary - initialBoundary : initialBoundary - boundary;
            bool validParts = boundary >= 2 && rows - boundary >= 2;
            foreach (var rule in new[] { FreeSizeGripRule.Boundary, FreeSizeGripRule.BoundaryLittle, FreeSizeGripRule.BoundaryMuch })
            {
                bool expected = validParts && (rule == FreeSizeGripRule.Boundary ? movement > 0 :
                    rule == FreeSizeGripRule.BoundaryLittle ? movement == 1 : movement >= 2);
                Check(FreeSizeGripAnswer.Accepts(initial, example, answer, rule, up) == expected,
                    $"Boundary {rule}, rows={rows}, boundary={boundary}, up={up}, swap={swap}");
            }
        }
    }

    // Decreasing through ten: accept a different inside part, and build the
    // complementary-parts follow-up from that actual answer, in either hand.
    var outside = Bits(rows, 0, 4, 5, 6, swap);
    var insideExample = Bits(rows, 0, 4, 3, 4, swap);
    for (int lower = 0; lower <= 4; lower++)
    {
        var answer = Bits(rows, 0, 4, lower, 4, swap);
        bool accepted = FreeSizeGripAnswer.Accepts(outside, insideExample, answer,
            FreeSizeGripRule.InsidePart, false);
        Check(accepted == (lower is 2 or 3), "Inside part must leave at least two rows");
        if (accepted)
        {
            var next = FreeSizeGripAnswer.BuildPartsFollowUp(answer, false);
            Check(next != null && next.SequenceEqual(Bits(rows, 0, 4, 0, lower - 1, swap)),
                "Complement must use accepted boundary");
            Check(answer.SequenceEqual(Bits(rows, 0, 4, lower, 4, swap)), "Do not mutate submitted answer");
        }
    }
    var malformed = (bool[])insideExample.Clone();
    malformed[2 * 2 + (swap ? 0 : 1)] = true;
    Check(!FreeSizeGripAnswer.Accepts(outside, insideExample, malformed, FreeSizeGripRule.InsidePart, false),
        "Reject extra pressed keys");
    Check(!FreeSizeGripAnswer.Accepts(outside, insideExample, Bits(rows, 0, 3, 2, 4, swap),
        FreeSizeGripRule.InsidePart, false), "Whole must stay fixed");
}

var splitFirst = Bits(8, 0, 7, 0, 7);
var splitExample = Bits(8, 0, 2, 3, 7);
Check(FreeSizeGripAnswer.Accepts(splitFirst, splitExample, Bits(8, 0, 4, 5, 7), FreeSizeGripRule.SplitJump, false), "Alternate split");
Check(!FreeSizeGripAnswer.Accepts(splitFirst, splitExample, Bits(8, 0, 4, 4, 7), FreeSizeGripRule.SplitJump, false), "No overlapping split");
Check(!FreeSizeGripAnswer.Accepts(splitFirst, splitExample, Bits(8, 0, 2, 4, 7), FreeSizeGripRule.SplitJump, false), "No gap in split");
var equal = Bits(10, 0, 3, 0, 3);
var bigger = Bits(10, 0, 3, 0, 5);
Check(FreeSizeGripAnswer.Accepts(equal, bigger, Bits(10, 0, 3, 0, 7), FreeSizeGripRule.MuchBigger, false), "Much bigger allows another magnitude");
Check(!FreeSizeGripAnswer.Accepts(equal, bigger, Bits(10, 0, 3, 0, 4), FreeSizeGripRule.MuchBigger, false), "One more is not much bigger");
Check(FreeSizeGripAnswer.BuildPartsFollowUp(Bits(8, 0, 4, 2, 4), true)!.SequenceEqual(Bits(8, 0, 4, 5, 7)), "Outside follow-up preserves actual part size");
Check(FreeSizeGripAnswer.BuildPartsFollowUp(Bits(7, 0, 4, 2, 4), true) == null, "No out-of-bounds follow-up");
var upperFirst = Bits(9, 0, 1, 2, 4);
var upperExample = Bits(9, 0, 1, 2, 5);
Check(FreeSizeGripAnswer.Accepts(upperFirst, upperExample, Bits(9, 0, 1, 2, 7), FreeSizeGripRule.ResizeUpper, false), "Free upper resize");
Check(!FreeSizeGripAnswer.Accepts(upperFirst, upperExample, upperFirst, FreeSizeGripRule.ResizeUpper, false), "Resize must change size");
var lowerFirst = Bits(9, 0, 2, 3, 4);
var lowerExample = Bits(9, 0, 3, 4, 5);
Check(FreeSizeGripAnswer.Accepts(lowerFirst, lowerExample, Bits(9, 0, 4, 5, 6), FreeSizeGripRule.ResizeLowerAttached, false), "Free lower resize with attached upper");
Check(!FreeSizeGripAnswer.Accepts(lowerFirst, lowerExample, Bits(9, 0, 4, 5, 7), FreeSizeGripRule.ResizeLowerAttached, false), "Preserve upper size");
Check(FreeSizeGripAnswer.Accepts(Bits(9, 0, 7, 0, 7), Bits(9, 0, 7, 0, 5), Bits(9, 0, 7, 0, 3), FreeSizeGripRule.MuchSmaller, false), "Alternate much smaller size");
Check(FreeSizeGripAnswer.Accepts(Bits(9, 0, 4, 3, 4), Bits(9, 0, 4, 5, 6), Bits(9, 0, 4, 5, 7), FreeSizeGripRule.OutsidePart, false), "Free outside part");
Check(!FreeSizeGripAnswer.Accepts(Bits(9, 0, 4, 3, 4), Bits(9, 0, 4, 5, 6), Bits(9, 0, 4, 5, 8), FreeSizeGripRule.OutsidePart, false), "Outside part must leave a two-row complement");
Console.WriteLine($"Passed {checks} free-size grip checks.");
