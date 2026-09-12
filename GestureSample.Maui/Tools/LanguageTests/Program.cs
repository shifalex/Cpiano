using GestureSample.Maui.Data;
using GestureSample.Maui.Handlers;
using System.Text.RegularExpressions;

int checks = 0;
void Check(bool condition, string message)
{
    checks++;
    if (!condition) throw new Exception(message);
}
var firstUser = new TestUser { Id = Guid.NewGuid() };
ServiceHelper.Session.ActiveUser = firstUser;
Check(LanguagePreferences.Get() == InterfaceLanguage.English, "Existing users default to English");
LanguagePreferences.Set(InterfaceLanguage.Hebrew, false, false);
LanguagePreferences.Set(InterfaceLanguage.Russian, true, false);
Check(LanguagePreferences.Get(gripping: true) == InterfaceLanguage.Hebrew, "Gripping inherits text");
Check(LanguagePreferences.Get(true, true) == InterfaceLanguage.Russian, "Narration is independent");
LanguagePreferences.Set(InterfaceLanguage.English, false, true);
Check(LanguagePreferences.Get(gripping: true) == InterfaceLanguage.English, "Explicit English override");
Check(LanguagePreferences.Get() == InterfaceLanguage.Hebrew, "Override leaves global preference intact");
ServiceHelper.Session.ActiveUser = new TestUser { Id = Guid.NewGuid() };
Check(LanguagePreferences.Get() == InterfaceLanguage.English && !LanguagePreferences.HasOverride(false), "Users are isolated");
ServiceHelper.Session.ActiveUser = firstUser;
Check(LanguagePreferences.Get(gripping: true) == InterfaceLanguage.English, "Override persists across user switch");
LanguagePreferences.Set(null, false, true);
Check(LanguagePreferences.Get(gripping: true) == InterfaceLanguage.Hebrew, "Removing override restores inheritance");

string sourceRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));
string gameplay = File.ReadAllText(Path.Combine(sourceRoot, "Models/PartPartWholeGameplay/BitArrayGamePlay.cs"));
string instructions = gameplay.Split("public string GetTwoHandCombinationActionText()")[1].Split("private string BuildSharedBoundaryActionText()")[0];
foreach (Match match in Regex.Matches(instructions, "\"([^\"]+)\""))
    foreach (var language in new[] { InterfaceLanguage.Hebrew, InterfaceLanguage.Russian })
        Check(AppLanguage.Translate(match.Groups[1].Value, language) != match.Groups[1].Value, "Missing instruction: " + match.Value);
foreach (string direction in new[] { "UP", "DOWN" })
    foreach (string amount in new[] { "", " BY A LITTLE", " BY A LOT", " BY 3" })
        foreach (var language in new[] { InterfaceLanguage.Hebrew, InterfaceLanguage.Russian })
            Check(!Regex.IsMatch(AppLanguage.Translate($"MOVE SHARED BOUNDARY {direction}{amount}", language), "[A-Z]"), "Dynamic instruction translation");
Check(AppLanguage.Translate("USER SETTINGS (Alex)", InterfaceLanguage.Russian).EndsWith(" (Alex)"), "User name preserved");
Check(AppLanguage.Translate("5+2 ↔ 6+2", InterfaceLanguage.Hebrew) == "5+2 ↔ 6+2", "Mathematical notation preserved");
var label = new Label();
label.SetValue(Label.TextProperty, "Gripping");
AppLanguage.LocalizeSettings(label, false);
Check((string?)label.GetValue(Label.TextProperty) == "אחיזות", "Settings Hebrew refresh");
LanguagePreferences.Set(InterfaceLanguage.Russian, false, false);
AppLanguage.LocalizeSettings(label, false);
Check((string?)label.GetValue(Label.TextProperty) == "Хваты", "Settings retain source across language changes");
LanguagePreferences.Set(InterfaceLanguage.English, false, false);
AppLanguage.LocalizeSettings(label, false);
Check((string?)label.GetValue(Label.TextProperty) == "Gripping", "Settings can switch back to English");

TextToSpeech.Default.Locales.AddRange(new[] { new Locale { Language = "en-US" }, new Locale { Language = "ru-RU" }, new Locale { Language = "iw-IL" } });
Check(await AppLanguage.SpeakAsync("COMMUTATIVITY"), "Russian voice available");
Check(TextToSpeech.Default.Language == "ru-RU" && TextToSpeech.Default.Spoken == "Переместительное свойство", "Narration language overrides text");
LanguagePreferences.Set(InterfaceLanguage.Hebrew, true, true);
Check(await AppLanguage.SpeakAsync("COMMUTATIVITY") && TextToSpeech.Default.Language == "iw-IL", "Legacy Hebrew locale supported");
TextToSpeech.Default.Locales.Clear();
Check(AppLanguage.GetNarrationParts(" first , second, third ", InterfaceLanguage.English)
    .SequenceEqual(new[] { "first", "second", "third" }), "Generic comma splitting supports more than two parts");
Check(AppLanguage.GetNarrationParts("first,,third", InterfaceLanguage.English)
    .SequenceEqual(new[] { "first", "", "third" }), "Empty clauses preserve presentation positions");
Check(AppLanguage.GetNarrationParts("single instruction", InterfaceLanguage.English).Length == 1,
    "Unsplit instructions remain one part");
TextToSpeech.Default.Locales.Add(new Locale { Language = "iw-IL" });
Check(await AppLanguage.SpeakAsync("Large-small, to Large+small", partIndex: 0) &&
    TextToSpeech.Default.Spoken == "גדול פחות קטן", "Hard-stage first part uses narration language");
Check(await AppLanguage.SpeakAsync("Large-small, to Large+small", partIndex: 1) &&
    TextToSpeech.Default.Spoken == "לגדול ועוד קטן", "Hard-stage second part is spoken separately");
Check(!await AppLanguage.SpeakAsync("first,,third", partIndex: 1), "Empty part is silent");
Check(!await AppLanguage.SpeakAsync("first,second", partIndex: 2), "Missing part is silent");
TextToSpeech.Default.Locales.Clear();
foreach (var (language, first, second) in new[]
{
    (InterfaceLanguage.English, "Remember the first", "then the second"),
    (InterfaceLanguage.Hebrew, "זוכרים את הראשון", "ואז את השני"),
    (InterfaceLanguage.Russian, "Запоминаем первый", "затем второй")
})
{
    Check(AppLanguage.Translate("Remember the first, then the second", language) == first + ", " + second,
        $"Full recall instruction preserves the comma in {language}");
    Check(AppLanguage.Translate("Remember the first", language) == first &&
        AppLanguage.Translate("Then the second", language).Equals(second, StringComparison.OrdinalIgnoreCase),
        $"Spoken clauses match their respective previews in {language}");
}
Check(!await AppLanguage.SpeakAsync("COMMUTATIVITY"), "Missing voice does not use the wrong language");
Console.WriteLine($"Passed {checks} language checks.");
