using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureSample.Maui.Models
{
    internal class BitArrayHelper
    {
        // Added helper methods to determine whether a candidate bit array represents a sequential
        // run of set bits. Supports directional checks and optional circular (wrap-around) sequences.
        public static bool IsSequential(bool[] candidate, Direction? dir = null, bool circular = true)
        {
            if (candidate == null || candidate.Length == 0)
                return false;

            int len = candidate.Length;

            // count set bits
            int countOnes = 0;
            for (int i = 0; i < len; i++)
                if (candidate[i]) countOnes++;
            if (countOnes == 0)
                return false;

            // local attempt for specific direction
            bool TryDirection(Direction d)
            {
                for (int start = 0; start < len; start++)
                {
                    if (!candidate[start])
                        continue;

                    // for non-circular, make sure the run fits without wrapping
                    if (!circular)
                    {
                        if (d == Direction.Right)
                        {
                            if (start + countOnes - 1 >= len)
                                continue;
                        }
                        else // Left
                        {
                            if (start - (countOnes - 1) < 0)
                                continue;
                        }
                    }

                    // verify contiguous run of countOnes in direction d starting at `start`
                    bool ok = true;
                    for (int k = 0; k < countOnes; k++)
                    {
                        int idx = d == Direction.Right
                            ? (start + k)
                            : (start - k);
                        if (circular)
                        {
                            idx = ((idx % len) + len) % len;
                        }
                        // if non-circular and idx out of range, break
                        if (idx < 0 || idx >= len)
                        {
                            ok = false;
                            break;
                        }
                        if (!candidate[idx])
                        {
                            ok = false;
                            break;
                        }
                    }
                    if (!ok) continue;

                    // ensure there are no extra set bits outside the contiguous run
                    // (we already counted ones and verified exactly countOnes positions in the run are true)
                    // so reaching here means the run consumes all true bits
                    return true;
                }
                return false;
            }

            if (dir.HasValue)
                return TryDirection(dir.Value);

            // check both directions if none specified
            return TryDirection(Direction.Right) || TryDirection(Direction.Left);
        }

        // Convenience overload to accept PianoKeyboard directly
        public static bool IsSequential(PianoKeyboard keyboard, Direction? dir = null, bool circular = true)
        {
            if (keyboard == null) return false;
            return IsSequential(keyboard.ToBitArray(), dir, circular);
        }

        public static int CountSetBits(bool[] candidate)
        {
            if (candidate == null || candidate.Length == 0)
                return 0;
            int count = 0;
            for (int i = 0; i < candidate.Length; i++)
                if (candidate[i]) count++;
            return count;
        }

        public enum Direction
        {
            Left,
            Right
        }
    }
}
