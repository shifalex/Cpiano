using System;
using System.Diagnostics;

namespace GestureSample.Debugging
{
    public static class DevLog
    {
        public static event Action<string>? Line;

        public static void Write(string message)
        {
            Debug.WriteLine(message);
            Line?.Invoke(message);
        }

        public static void Write(string format, params object[] args)
            => Write(string.Format(format, args));
    }
}
