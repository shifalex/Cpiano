namespace Microsoft.Maui.Storage;

// Replace only the device storage boundary; tests use the production preference code.
public sealed class Preferences
{
    public static Preferences Default { get; } = new();
    private readonly Dictionary<string, string> _values = new();
    public string Get(string key, string fallback) => _values.GetValueOrDefault(key, fallback);
    public void Set(string key, string value) => _values[key] = value;
    public void Remove(string key) => _values.Remove(key);
}
