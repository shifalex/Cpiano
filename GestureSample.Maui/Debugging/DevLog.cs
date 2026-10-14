using System;
using System.Diagnostics;

namespace GestureSample.Debugging
{
    internal static class DevLog
    {
        internal static event Action<string>? Line;

        internal static void Write(string message)
        {
            Debug.WriteLine(message);
            Line?.Invoke(message);
        }
    }
}
