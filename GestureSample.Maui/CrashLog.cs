using System;
using System.IO;

namespace GestureSample.Maui;

internal static class CrashLog
{
    private static readonly object _sync = new();

    private static string LogPath
    {
        get
        {
            try
            {
                var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(folder, "startup.log");
            }
            catch
            {
                return "startup.log";
            }
        }
    }

    public static void Write(string message)
    {
        var line = $"[{DateTime.UtcNow:O}] {message}";

        try
        {
            Console.WriteLine(line);
        }
        catch
        {
            // ignored
        }

        try
        {
            lock (_sync)
            {
                File.AppendAllText(LogPath, line + Environment.NewLine);
            }
        }
        catch
        {
            // ignored
        }
    }

    public static void WriteException(string prefix, Exception ex)
    {
        Write($"{prefix}: {ex}");
    }

    public static string GetPath() => LogPath;
}
