namespace Microsoft.Maui.Storage;

// Replace only the device storage boundary; tests use the production preference code.
public sealed class Preferences
{
    public static Preferences Default { get; } = new();
    private readonly Dictionary<string, string> _values = new();
    public string Get(string key, string fallback) => _values.GetValueOrDefault(key, fallback);
    public bool Get(string key, bool fallback) => _values.TryGetValue(key, out string? value) && bool.TryParse(value, out bool parsed) ? parsed : fallback;
    public void Set(string key, string value) => _values[key] = value;
    public void Set(string key, bool value) => _values[key] = value.ToString();
    public void Remove(string key) => _values.Remove(key);
}
