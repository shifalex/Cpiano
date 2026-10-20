namespace GestureSample.Maui.Models;

public sealed class RecordedShiftArrow
{
    public int Delta { get; set; }
    public bool IsVertical { get; set; }
    public bool BaseAtTop { get; set; }
    public bool IsShift { get; set; }
    public bool LeftSide { get; set; }

    // Older recordings have source/target keys but no instruction metadata.
    public static RecordedShiftArrow[] Infer(bool[]? source, bool[]? target, int columns, int rows)
    {
        if (source == null || target == null || source.Length != target.Length || columns < 1)
            return Array.Empty<RecordedShiftArrow>();
        bool vertical = rows > 1 && columns == 2;
        var arrows = new List<RecordedShiftArrow>();
        for (int side = 0; side < 2; side++)
        {
            bool InSide(int index) => vertical ? index % columns == side : (index < source.Length / 2 ? 0 : 1) == side;
            var from = Enumerable.Range(0, source.Length).Where(i => source[i] && InSide(i)).ToArray();
            var to = Enumerable.Range(0, target.Length).Where(i => target[i] && InSide(i)).ToArray();
            if (from.Length == 0 || from.Length != to.Length) continue;
            int lowerDelta = to[0] - from[0], upperDelta = to[^1] - from[^1];
            int delta = lowerDelta != 0 ? lowerDelta : upperDelta;
            if (vertical) delta /= columns;
            if (delta == 0) continue;
            arrows.Add(new RecordedShiftArrow { Delta = delta, IsVertical = vertical,
                LeftSide = side == 0, IsShift = lowerDelta == upperDelta, BaseAtTop = lowerDelta != 0 && upperDelta == 0 });
        }
        return arrows.ToArray();
    }
}
