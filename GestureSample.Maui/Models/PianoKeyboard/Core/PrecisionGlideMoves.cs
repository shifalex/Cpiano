namespace GestureSample.Maui.Models;

public static class PrecisionGlideMoves
{
    // Order moves so a finger vacates a shared destination before its partner
    // enters it. Never overwrite occupied keys or move a finger to the other hand.
    public static IReadOnlyList<(int Origin, int From, int To)> Plan(
        IReadOnlyDictionary<int, int> positions, IReadOnlyDictionary<int, int> targets,
        IEnumerable<int> occupiedKeys, Func<int, int, bool> sameHand)
    {
        var current = positions.ToDictionary(pair => pair.Key, pair => pair.Value);
        var occupied = new HashSet<int>(occupiedKeys);
        var moves = new List<(int Origin, int From, int To)>();
        bool changed;
        do
        {
            changed = false;
            foreach (var target in targets)
            {
                if (!current.TryGetValue(target.Key, out int from) || from == target.Value ||
                    target.Value < 0 || occupied.Contains(target.Value) || !sameHand(from, target.Value) ||
                    current.Values.Count(value => value == from) != 1) continue;
                occupied.Remove(from);
                occupied.Add(target.Value);
                current[target.Key] = target.Value;
                moves.Add((target.Key, from, target.Value));
                changed = true;
            }
        } while (changed);
        return moves;
    }
}
