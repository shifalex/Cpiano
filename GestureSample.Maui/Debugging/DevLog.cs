using System.Diagnostics;

namespace GestureSample.Debugging;

public static class DevLog
{
    public static event Action<string> Line;

    public static void Write(string message)
    {
        string line = message ?? string.Empty;
        Debug.WriteLine(line);
        Line?.Invoke(line);
    }
}
