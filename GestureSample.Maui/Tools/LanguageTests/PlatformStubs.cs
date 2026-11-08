// Platform boundaries only. Tests execute the production preference and translation code.
global using Microsoft.Maui.Controls;
global using Microsoft.Maui.Media;

namespace Microsoft.Maui.Storage
{
    public class Preferences
    {
        public static Preferences Default { get; } = new();
        private readonly Dictionary<string, int> values = new();
        public int Get(string key, int fallback) => values.GetValueOrDefault(key, fallback);
        public void Set(string key, int value) => values[key] = value;
        public bool ContainsKey(string key) => values.ContainsKey(key);
        public void Remove(string key) => values.Remove(key);
    }
}
namespace GestureSample.Maui.Handlers
{
    public sealed class CurrentUserSession { public TestUser? ActiveUser { get; set; } }
    public sealed class TestUser { public Guid Id { get; set; } }
}
namespace GestureSample.Maui.Data
{
    public static class ServiceHelper
    {
        public static Handlers.CurrentUserSession Session { get; } = new();
        public static T GetService<T>() => (T)(object)Session;
    }
}
namespace Microsoft.Maui
{
    public interface IVisualTreeElement { IReadOnlyList<IVisualTreeElement> GetVisualChildren(); }
}
namespace Microsoft.Maui.Controls
{
    public class BindableProperty { }
    public class BindableObject
    {
        private readonly Dictionary<BindableProperty, object> values = new();
        public object? GetValue(BindableProperty property) => values.GetValueOrDefault(property);
        public void SetValue(BindableProperty property, object value) => values[property] = value;
    }
    public class Element : BindableObject { }
    public class Label : Element { public static BindableProperty TextProperty = new(); }
    public class Button : Element { public static BindableProperty TextProperty = new(); }
    public class Page : Element { public static BindableProperty TitleProperty = new(); }
    public class ContentPage : Page { }
}
namespace GestureSample.Maui.Views { public class LanguageSettingsView : Element { } }
namespace Microsoft.Maui.Media
{
    public class Locale { public string Language { get; set; } = ""; }
    public class SpeechOptions { public Locale? Locale { get; set; } }
    public class TextToSpeech
    {
        public static TextToSpeech Default { get; } = new();
        public List<Locale> Locales { get; } = new();
        public string? Spoken { get; private set; }
        public string? Language { get; private set; }
        public Task<IEnumerable<Locale>> GetLocalesAsync() => Task.FromResult<IEnumerable<Locale>>(Locales);
        public Task SpeakAsync(string text, SpeechOptions options, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Spoken = text; Language = options.Locale?.Language; return Task.CompletedTask;
        }
    }
}
