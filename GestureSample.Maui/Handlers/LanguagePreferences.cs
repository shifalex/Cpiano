using GestureSample.Maui.Data;
using Microsoft.Maui.Storage;

namespace GestureSample.Maui.Handlers;

public enum InterfaceLanguage { English, Hebrew, Russian }

/// <summary>Device-local language preferences, isolated by user and optional sub-app.</summary>
public static class LanguagePreferences
{
    public static Guid? UserId => ServiceHelper.GetService<CurrentUserSession>()?.ActiveUser?.Id;
    private static string Key(Guid? userId, bool narration, bool gripping) =>
        $"user_language_{userId?.ToString("D") ?? "guest"}_{(gripping ? "gripping" : "default")}_{(narration ? "voice" : "text")}";

    public static InterfaceLanguage Get(bool narration = false, bool gripping = false) => Get(UserId, narration, gripping);

    public static InterfaceLanguage Get(Guid? userId, bool narration, bool gripping)
    {
        int fallback = gripping ? (int)Get(userId, narration, false) : (int)InterfaceLanguage.English;
        int value = Preferences.Default.Get(Key(userId, narration, gripping), fallback);
        return Enum.IsDefined(typeof(InterfaceLanguage), value) ? (InterfaceLanguage)value : (InterfaceLanguage)fallback;
    }

    public static bool HasOverride(bool narration) => Preferences.Default.ContainsKey(Key(UserId, narration, true));

    public static void Set(InterfaceLanguage? language, bool narration, bool gripping)
    {
        string key = Key(UserId, narration, gripping);
        if (language.HasValue) Preferences.Default.Set(key, (int)language.Value);
        else Preferences.Default.Remove(key);
    }

    public static string Code(InterfaceLanguage language) => language switch
    {
        InterfaceLanguage.Hebrew => "he",
        InterfaceLanguage.Russian => "ru",
        _ => "en"
    };
}
