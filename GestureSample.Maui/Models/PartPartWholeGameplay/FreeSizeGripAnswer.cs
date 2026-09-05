namespace GestureSample.Maui.Models;

internal enum FreeSizeGripRule
{
    Boundary, BoundaryLittle, BoundaryMuch, SplitJump,
    InsidePart, OutsidePart, ResizeUpper, ResizeLowerAttached,
    MuchSmaller, MuchBigger
}

internal static class FreeSizeGripAnswer
{
    internal readonly record struct Grip(int Lower, int Upper)
    {
        public int Size => Upper - Lower + 1;
    }

    internal static Grip[]? Read(bool[] bits)
    {
        if (bits.Length % 2 != 0) return null;
        var grips = new Grip[2];
        for (int column = 0; column < 2; column++)
        {
            int[] rows = Enumerable.Range(0, bits.Length / 2)
                .Where(row => bits[row * 2 + column]).ToArray();
            if (rows.Length != 2) return null;
            grips[column] = new Grip(rows[0], rows[1]);
        }
        return grips;
    }

    internal static bool Accepts(bool[] firstBits, bool[] exampleBits, bool[] answerBits,
        FreeSizeGripRule rule, bool boundaryMovesUp)
    {
        if (firstBits.Length != answerBits.Length || exampleBits.Length != answerBits.Length)
            return false;
        var first = Read(firstBits);
        var example = Read(exampleBits);
        var answer = Read(answerBits);
        if (first == null || example == null || answer == null) return false;

        if (rule is FreeSizeGripRule.Boundary or FreeSizeGripRule.BoundaryLittle or
            FreeSizeGripRule.BoundaryMuch or FreeSizeGripRule.SplitJump)
        {
            int lower = example[0].Lower < example[1].Lower ? 0 : 1;
            int upper = 1 - lower;
            if (answer[lower].Lower != first[lower].Lower ||
                answer[upper].Upper != first[upper].Upper ||
                answer[lower].Upper + 1 != answer[upper].Lower)
                return false;
            if (rule == FreeSizeGripRule.SplitJump) return true;
            int movement = answer[upper].Lower - first[upper].Lower;
            int directedMovement = boundaryMovesUp ? movement : -movement;
            return rule switch
            {
                FreeSizeGripRule.BoundaryLittle => directedMovement == 1,
                FreeSizeGripRule.BoundaryMuch => directedMovement >= 2,
                _ => directedMovement > 0
            };
        }

        if (rule == FreeSizeGripRule.ResizeLowerAttached)
        {
            int lower = first[0].Lower < first[1].Lower ? 0 : 1;
            int upper = 1 - lower;
            return answer[lower].Lower == first[lower].Lower &&
                answer[lower].Size != first[lower].Size &&
                answer[upper].Size == first[upper].Size &&
                answer[lower].Upper + 1 == answer[upper].Lower;
        }

        int fixedColumn = first[0] == example[0] ? 0 : 1;
        int changedColumn = 1 - fixedColumn;
        if (answer[fixedColumn] != first[fixedColumn]) return false;
        Grip whole = answer[fixedColumn];
        Grip part = answer[changedColumn];
        if (rule == FreeSizeGripRule.InsidePart)
            return part.Upper == whole.Upper && part.Lower >= whole.Lower + 2;
        if (rule == FreeSizeGripRule.OutsidePart)
            return part.Lower == whole.Upper + 1 && whole.Size - part.Size >= 2;

        if (part.Lower != first[changedColumn].Lower) return false;
        int difference = part.Size - first[changedColumn].Size;
        return rule switch
        {
            FreeSizeGripRule.ResizeUpper => difference != 0,
            FreeSizeGripRule.MuchSmaller => difference <= -2,
            FreeSizeGripRule.MuchBigger => difference >= 2,
            _ => false
        };
    }

    // Follow-up geometry comes from the accepted grip, never the example answer.
    internal static bool[]? BuildPartsFollowUp(bool[] accepted, bool moveOutside)
    {
        var grips = Read(accepted);
        if (grips == null) return null;
        int wholeColumn = grips[0].Size > grips[1].Size ? 0 : 1;
        int partColumn = 1 - wholeColumn;
        Grip whole = grips[wholeColumn];
        Grip part = grips[partColumn];
        if (part.Upper != whole.Upper || part.Lower < whole.Lower + 2)
            return null;
        Grip next = moveOutside
            ? new Grip(whole.Upper + 1, whole.Upper + part.Size)
            : new Grip(whole.Lower, part.Lower - 1);
        if (next.Upper >= accepted.Length / 2) return null;
        bool[] result = accepted.ToArray();
        for (int row = 0; row < result.Length / 2; row++)
            result[row * 2 + partColumn] = row == next.Lower || row == next.Upper;
        return result;
    }
}
