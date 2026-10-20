namespace GestureSample.Maui.Models;

public static class KeyboardReplayTransitions
{
    // Keys are one-based in persisted events. An accepted glide transfers the
    // existing state; it must not cycle colors or count as a new press.
    public static bool ApplyGlide<T>(T[] state, int? fromKey, int toKey, T free)
    {
        if (fromKey is not int from || from < 1 || from > state.Length ||
            toKey < 1 || toKey > state.Length || from == toKey)
            return false;
        state[toKey - 1] = state[from - 1];
        state[from - 1] = free;
        return true;
    }
}
