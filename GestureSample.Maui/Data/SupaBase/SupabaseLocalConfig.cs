using System.Text.Json;

namespace GestureSample.Maui.Data.SupaBase
{
    internal sealed class SupabaseLocalConfig
    {
        public string Url { get; set; } = string.Empty;
        public string AnonKey { get; set; } = string.Empty;

        public static SupabaseLocalConfig LoadOrThrow()
        {
            string? envUrl = Environment.GetEnvironmentVariable("GESTURE_SAMPLE_SUPABASE_URL");
            string? envAnonKey = Environment.GetEnvironmentVariable("GESTURE_SAMPLE_SUPABASE_ANON_KEY");

            if (!string.IsNullOrWhiteSpace(envUrl) && !string.IsNullOrWhiteSpace(envAnonKey))
            {
                return new SupabaseLocalConfig
                {
                    Url = envUrl.Trim(),
                    AnonKey = envAnonKey.Trim()
                };
            }

            foreach (string candidatePath in EnumerateCandidatePaths())
            {
                if (!File.Exists(candidatePath))
                    continue;

                string json = File.ReadAllText(candidatePath);
                SupabaseLocalConfig? config = JsonSerializer.Deserialize<SupabaseLocalConfig>(json);
                if (config == null)
                    continue;

                if (!string.IsNullOrWhiteSpace(config.Url) && !string.IsNullOrWhiteSpace(config.AnonKey))
                    return config;
            }

            throw new InvalidOperationException(
                "Supabase config was not found. Add GESTURE_SAMPLE_SUPABASE_URL and GESTURE_SAMPLE_SUPABASE_ANON_KEY environment variables, or create a local supabase.local.json file.");
        }

        private static IEnumerable<string> EnumerateCandidatePaths()
        {
            yield return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "supabase.local.json");

            string? appBase = AppContext.BaseDirectory;
            if (!string.IsNullOrWhiteSpace(appBase))
            {
                foreach (string path in EnumeratePathAndParents(appBase, "supabase.local.json", 8))
                    yield return path;
            }

            string currentDirectory = Directory.GetCurrentDirectory();
            foreach (string path in EnumeratePathAndParents(currentDirectory, "supabase.local.json", 6))
                yield return path;
        }

        private static IEnumerable<string> EnumeratePathAndParents(string startDirectory, string fileName, int maxDepth)
        {
            DirectoryInfo? directory = new DirectoryInfo(startDirectory);
            int depth = 0;

            while (directory != null && depth <= maxDepth)
            {
                yield return Path.Combine(directory.FullName, fileName);
                directory = directory.Parent;
                depth++;
            }
        }
    }
}
