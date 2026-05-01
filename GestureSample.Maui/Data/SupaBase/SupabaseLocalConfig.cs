using System.Text;
using System.Text.Json;
using System.Reflection;

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

            Console.WriteLine($"[SupabaseConfig] Env URL present: {!string.IsNullOrWhiteSpace(envUrl)}");
            Console.WriteLine($"[SupabaseConfig] Env URL value: {SafeValue(envUrl)}");
            Console.WriteLine($"[SupabaseConfig] Env anon key present: {!string.IsNullOrWhiteSpace(envAnonKey)}");
            Console.WriteLine($"[SupabaseConfig] Env anon key value: {MaskSecret(envAnonKey)}");

            if (!string.IsNullOrWhiteSpace(envUrl) && !string.IsNullOrWhiteSpace(envAnonKey))
            {
                SupabaseLocalConfig config = new SupabaseLocalConfig
                {
                    Url = envUrl.Trim(),
                    AnonKey = envAnonKey.Trim()
                };

                ValidateClientKey(config.AnonKey);
                Console.WriteLine("[SupabaseConfig] Loaded config from environment variables.");
                return config;
            }

            SupabaseLocalConfig? packagedConfig = TryLoadFromAppPackage();
            if (packagedConfig != null)
            {
                ValidateClientKey(packagedConfig.AnonKey);
                Console.WriteLine("[SupabaseConfig] Loaded config from app package asset: supabase.local.json");
                return packagedConfig;
            }

            SupabaseLocalConfig? embeddedConfig = TryLoadFromEmbeddedResource();
            if (embeddedConfig != null)
            {
                ValidateClientKey(embeddedConfig.AnonKey);
                Console.WriteLine("[SupabaseConfig] Loaded config from embedded resource: supabase.local.json");
                return embeddedConfig;
            }

            foreach (string candidatePath in EnumerateCandidatePaths())
            {
                Console.WriteLine($"[SupabaseConfig] Checking file: {candidatePath}");
                if (!File.Exists(candidatePath))
                    continue;

                Console.WriteLine($"[SupabaseConfig] Found config file: {candidatePath}");
                string json = File.ReadAllText(candidatePath);
                SupabaseLocalConfig? config = JsonSerializer.Deserialize<SupabaseLocalConfig>(json);
                if (config == null)
                {
                    Console.WriteLine($"[SupabaseConfig] Failed to deserialize config file: {candidatePath}");
                    continue;
                }

                Console.WriteLine($"[SupabaseConfig] File URL value: {SafeValue(config.Url)}");
                Console.WriteLine($"[SupabaseConfig] File anon key value: {MaskSecret(config.AnonKey)}");

                if (!string.IsNullOrWhiteSpace(config.Url) && !string.IsNullOrWhiteSpace(config.AnonKey))
                {
                    ValidateClientKey(config.AnonKey);
                    Console.WriteLine($"[SupabaseConfig] Loaded config from file: {candidatePath}");
                    return config;
                }
            }

            Console.WriteLine($"[SupabaseConfig] AppContext.BaseDirectory: {SafeValue(AppContext.BaseDirectory)}");
            Console.WriteLine($"[SupabaseConfig] Directory.GetCurrentDirectory(): {SafeValue(Directory.GetCurrentDirectory())}");
            Console.WriteLine($"[SupabaseConfig] LocalApplicationData: {SafeValue(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))}");
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

        private static void ValidateClientKey(string key)
        {
            if (LooksLikeServiceRoleKey(key))
            {
                throw new InvalidOperationException(
                    "The configured Supabase key looks like a service_role key. Do not put service_role secrets in the MAUI client app. Use the project anon key here.");
            }
        }

        private static bool LooksLikeServiceRoleKey(string key)
        {
            string[] segments = key.Split('.');
            if (segments.Length < 2)
                return false;

            try
            {
                string payload = segments[1]
                    .Replace('-', '+')
                    .Replace('_', '/');

                int padding = 4 - (payload.Length % 4);
                if (padding is > 0 and < 4)
                    payload = payload.PadRight(payload.Length + padding, '=');

                string json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
                using JsonDocument document = JsonDocument.Parse(json);
                if (document.RootElement.TryGetProperty("role", out JsonElement roleElement))
                {
                    string? role = roleElement.GetString();
                    return string.Equals(role, "service_role", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private static SupabaseLocalConfig? TryLoadFromAppPackage()
        {
            try
            {
                Console.WriteLine("[SupabaseConfig] Checking app package asset: supabase.local.json");
                using Stream stream = FileSystem.Current.OpenAppPackageFileAsync("supabase.local.json").GetAwaiter().GetResult();
                using StreamReader reader = new(stream);
                string json = reader.ReadToEnd();
                SupabaseLocalConfig? config = JsonSerializer.Deserialize<SupabaseLocalConfig>(json);
                if (config == null)
                {
                    Console.WriteLine("[SupabaseConfig] App package asset exists but failed to deserialize.");
                    return null;
                }

                Console.WriteLine($"[SupabaseConfig] App package URL value: {SafeValue(config.Url)}");
                Console.WriteLine($"[SupabaseConfig] App package anon key value: {MaskSecret(config.AnonKey)}");
                if (string.IsNullOrWhiteSpace(config.Url) || string.IsNullOrWhiteSpace(config.AnonKey))
                    return null;

                return config;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupabaseConfig] App package asset not available: {ex.Message}");
                return null;
            }
        }

        private static SupabaseLocalConfig? TryLoadFromEmbeddedResource()
        {
            try
            {
                Console.WriteLine("[SupabaseConfig] Checking embedded resource: supabase.local.json");
                Assembly assembly = typeof(SupabaseLocalConfig).Assembly;
                string? resourceName = assembly
                    .GetManifestResourceNames()
                    .FirstOrDefault(name => string.Equals(name, "supabase.local.json", StringComparison.OrdinalIgnoreCase)
                        || name.EndsWith(".supabase.local.json", StringComparison.OrdinalIgnoreCase));

                if (resourceName == null)
                {
                    Console.WriteLine("[SupabaseConfig] Embedded resource not found.");
                    return null;
                }

                using Stream? stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    Console.WriteLine($"[SupabaseConfig] Embedded resource stream missing for: {resourceName}");
                    return null;
                }

                using StreamReader reader = new(stream);
                string json = reader.ReadToEnd();
                SupabaseLocalConfig? config = JsonSerializer.Deserialize<SupabaseLocalConfig>(json);
                if (config == null)
                {
                    Console.WriteLine($"[SupabaseConfig] Embedded resource exists but failed to deserialize: {resourceName}");
                    return null;
                }

                Console.WriteLine($"[SupabaseConfig] Embedded resource name: {resourceName}");
                Console.WriteLine($"[SupabaseConfig] Embedded URL value: {SafeValue(config.Url)}");
                Console.WriteLine($"[SupabaseConfig] Embedded anon key value: {MaskSecret(config.AnonKey)}");
                if (string.IsNullOrWhiteSpace(config.Url) || string.IsNullOrWhiteSpace(config.AnonKey))
                    return null;

                return config;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupabaseConfig] Embedded resource not available: {ex.Message}");
                return null;
            }
        }

        private static string SafeValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "<empty>" : value;
        }

        private static string MaskSecret(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "<empty>";

            if (value.Length <= 8)
                return new string('*', value.Length);

            return $"{value[..4]}...{value[^4..]}";
        }
    }
}
