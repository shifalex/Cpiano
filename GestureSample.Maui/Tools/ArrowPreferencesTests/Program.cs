using GestureSample.Maui.Handlers;
using GestureSample.Maui;
using Microsoft.Maui.Storage;

int checks = 0;
void Check(bool condition, string message)
{
    checks++;
    if (!condition) throw new Exception(message);
}
var user = Guid.NewGuid();
var otherUser = Guid.NewGuid();
const string stage = "First ->";
var defaults = new ArrowPracticeSelection();
Check(ArrowPracticePreferences.Load(user, stage) == defaults, "First visit should leave stage defaults available");
var selected = new ArrowPracticeSelection
{
    Sound = false, Reading = true, AccumulatingColor = true, KeyNumbers = false,
    MaxLength = 0, StartingPosition = 1, TenBoundary = 2
};
ArrowPracticePreferences.Save(user, stage, selected);
Check(ArrowPracticePreferences.Load(user, stage) == selected, "All options, including false and zero, should survive reopening");
Check(ArrowPracticePreferences.Load(user, "First -> variable length") == defaults, "Another stage should keep its defaults");
Check(ArrowPracticePreferences.Load(otherUser, stage) == defaults, "Another user's choices should remain separate");
Check(ArrowPracticePreferences.Load(null, stage) == defaults, "Guest preferences should remain separate");
ArrowPracticePreferences.Save(null, stage, selected with { MaxLength = 4 });
Check(ArrowPracticePreferences.Load(null, stage).MaxLength == 4, "Guest choices should persist");
Check(ArrowPracticePreferences.Load(user, stage).MaxLength == 0, "Guest changes should not affect the user");
ArrowPracticePreferences.Save(user, stage, selected with { TenBoundary = 0, StartingPosition = 0 });
Check(ArrowPracticePreferences.Load(user, stage).TenBoundary == 0, "None should persist");
Check(ArrowPracticePreferences.Load(user, stage).StartingPosition == 0, "Stage default should persist");
ArrowPracticePreferences.Save(user, "First -> variable length", selected);
ArrowPracticePreferences.Save(otherUser, stage, selected);
ArrowPracticePreferences.Reset(user, stage);
Check(ArrowPracticePreferences.Load(user, stage) == defaults, "Reset should restore the original stage defaults");
Check(ArrowPracticePreferences.Load(user, "First -> variable length") == selected, "Reset must not clear another stage");
Check(ArrowPracticePreferences.Load(otherUser, stage) == selected, "Reset must not clear another user");
Check(ArrowPracticePreferences.Load(null, stage).MaxLength == 4, "Reset must not clear guest choices");
ArrowPracticePreferences.Reset(user, stage);
Check(ArrowPracticePreferences.Load(user, stage) == defaults, "Repeated reset should be safe");
string key = $"arrow_practice_v1_{user:D}_{stage}";
Preferences.Default.Set(key, "{invalid json");
Check(ArrowPracticePreferences.Load(user, stage) == defaults, "Damaged storage should fall back to stage defaults");
Preferences.Default.Set(key, "null");
Check(ArrowPracticePreferences.Load(user, stage) == defaults, "Null storage should fall back to defaults");
Preferences.Default.Set(key, "{\"Reading\":true,\"MaxLength\":-5,\"StartingPosition\":99,\"TenBoundary\":99}");
var recovered = ArrowPracticePreferences.Load(user, stage);
Check(recovered.Reading == true && recovered.MaxLength == null && recovered.StartingPosition == null && recovered.TenBoundary == null,
    "Invalid picker values should fall back without discarding other options");
Check(ArrowPracticePreferences.LoadLabelStep(user) == 0, "A new progression should begin at step zero");
ArrowPracticePreferences.SaveLabelStep(user, 2);
Check(ArrowPracticePreferences.LoadLabelStep(user) == 2, "Progression position should survive reopening");
Check(ArrowPracticePreferences.LoadLabelStep(otherUser) == 0, "Progression positions must be user-specific");
Check(ArrowPracticePreferences.LoadLabelStep(null) == 0, "Guest progression should be separate");
ArrowPracticePreferences.SaveLabelStep(user, 3);
Check(ArrowPracticePreferences.LoadLabelStep(user) == 3, "Progression may reach mixed practice");
ArrowPracticePreferences.Reset(user, stage);
Check(ArrowPracticePreferences.LoadLabelStep(user) == 3, "Resetting stage options must not reset progression");
ArrowPracticePreferences.SaveLabelStep(user, 0);
Check(ArrowPracticePreferences.LoadLabelStep(user) == 0, "Restarting progression should return to the beginning");
Preferences.Default.Set($"arrow_label_path_v1_{user:D}", "invalid");
Check(ArrowPracticePreferences.LoadLabelStep(user) == 0, "Corrupt progression should fall back safely");
Preferences.Default.Set($"arrow_label_path_v1_{user:D}", "99");
Check(ArrowPracticePreferences.LoadLabelStep(user) == 0, "Unknown progression should fall back safely");
bool rejected = false;
try { ArrowPracticePreferences.SaveLabelStep(user, 4); }
catch (ArgumentOutOfRangeException) { rejected = true; }
Check(rejected, "Invalid progression must not be saved");
var progression = new ArrowLabelProgression(0);
for (int i = 0; i < 3; i++) Check(!progression.RecordAttempt(true), "Three successes must not advance");
Check(progression.ConsecutiveSuccesses == 3 && progression.Step == 0, "Streak should show three of four");
Check(!progression.RecordAttempt(false) && progression.ConsecutiveSuccesses == 0, "An incorrect response must reset the streak");
for (int expected = 1; expected <= 3; expected++)
{
    for (int i = 0; i < 3; i++) Check(!progression.RecordAttempt(true), "Advancement must require four consecutive successes");
    Check(progression.RecordAttempt(true) && progression.Step == expected && progression.ConsecutiveSuccesses == 0,
        "Fourth success must advance exactly one step and clear the streak");
}
for (int i = 0; i < 8; i++) Check(!progression.RecordAttempt(true) && progression.Step == 3, "Mixed practice must remain mixed");
progression.MoveTo(1);
progression.RecordAttempt(true);
progression.MoveTo(2);
Check(progression.Step == 2 && progression.ConsecutiveSuccesses == 0, "Manual movement must reset the streak");
var custom = new CustomArrowPracticeSelection();
Check(custom.ValidationError() == null && custom.Kinds.Length == 6, "Default custom mix should contain all six types");
Check((custom with { Kinds = Array.Empty<CustomArrowQuestionKind>() }).ValidationError() != null, "Empty mixes must be rejected");
Check((custom with { Kinds = new[] { (CustomArrowQuestionKind)99 } }).ValidationError() != null, "Unknown types must be rejected");
Check((custom with { MaxLength = 0 }).ValidationError() != null, "Zero-length movement must be rejected");
Check((custom with { MaxLength = 20 }).ValidationError() != null, "Length beyond the keyboard must be rejected");
Check((custom with { Specific = true }).ValidationError() != null, "Specific practice requires one type");
foreach (var kind in Enum.GetValues<CustomArrowQuestionKind>())
{
    var exact = custom with { Specific = true, Kinds = new[] { kind }, Start = 3, End = 7 };
    Check(exact.ValidationError() == null, "Each type must support a specific forward question");
    Check((exact with { Start = 7, End = 7 }).ValidationError() != null, "Zero distance must be rejected");
    Check((exact with { Start = 0 }).ValidationError() != null, "Key zero must be rejected");
    Check((exact with { End = 21 }).ValidationError() != null, "Out-of-range destination must be rejected");
    CustomArrowPracticePreferences.Save(user, exact);
    var restored = CustomArrowPracticePreferences.Load(user);
    Check(restored.Specific && restored.Start == 3 && restored.End == 7 && restored.Kinds.SequenceEqual(new[] { kind }), "Exact specification must survive reopening");
}
Check(new SpecificArrowQuestion(3, 7).Distance == 4, "Specific question must preserve the requested distance");
var exactRoundTrip = System.Text.Json.JsonSerializer.Deserialize<SpecificArrowQuestion>(
    System.Text.Json.JsonSerializer.Serialize(new SpecificArrowQuestion(8, 14)));
Check(exactRoundTrip is { Start: 8, End: 14, Distance: 6 }, "A fixed question must survive config serialization without losing its values");
var subtraction = new SpecificArrowQuestion(12, 7);
Check(subtraction.MovesLeft && subtraction.Distance == 5 && subtraction.Equation == "12−5=7", "12 to 7 must be represented as subtraction by five");
Check(subtraction.GetLabelAnswerKeys().SequenceEqual(new[] { 12, 11, 10, 9, 8 }), "Leftward label help must follow the subtraction route");
Check(subtraction.KeyboardFromIndex(20) == 7, "Leftward keyboard response must begin at key eight");
Check(SpecificArrowQuestion.KeyNumber(12) == 2 && SpecificArrowQuestion.KeyNumber(20) == 10,
    "Higher numbers must map to the ten-key modulo keyboard");
var subtractionRestored = System.Text.Json.JsonSerializer.Deserialize<SpecificArrowQuestion>(System.Text.Json.JsonSerializer.Serialize(subtraction));
Check(subtractionRestored == subtraction && subtractionRestored.MovesLeft, "Subtraction must survive config serialization");
foreach (var kind in Enum.GetValues<CustomArrowQuestionKind>())
{
    var selection = custom with { Specific = true, Kinds = new[] { kind }, Start = 12, End = 7 };
    Check(selection.ValidationError() == null, "All six custom types must permit subtraction");
    CustomArrowPracticePreferences.Save(user, selection);
    Check(CustomArrowPracticePreferences.Load(user).End == 7, "Leftward setup must survive reopening");
}
for (int startKey = 1; startKey <= 20; startKey++)
for (int endKey = 1; endKey <= 20; endKey++)
{
    if (startKey == endKey) continue;
    var question = new SpecificArrowQuestion(startKey, endKey);
    if (question.Distance > 10)
    {
        Check((custom with { Specific = true, Kinds = new[] { CustomArrowQuestionKind.Ordinal }, Start = startKey, End = endKey }).ValidationError() != null,
            "Specific questions longer than one full keyboard must be rejected");
        continue;
    }
    Check(question.Distance == Math.Abs(startKey - endKey), "Both directions must use a positive distance");
    var route = question.GetLabelAnswerKeys();
    Check(route.Length == question.Distance && route.Distinct().Count() == route.Length && route.All(key => key is >= 1 and <= 20), "Every label response must stay within the keyboard");
    int from = question.KeyboardFromIndex(10);
    var physicalKeys = Enumerable.Range(0, question.Distance).Select(offset => (from + offset) % 10).ToArray();
    Check(from is >= 0 and < 10 && physicalKeys.All(key => key is >= 0 and < 10) && physicalKeys.Distinct().Count() == question.Distance,
        "Every keyboard response must wrap within exactly ten keys");
    Check(question.MovesLeft ? startKey - question.Distance == endKey : startKey + question.Distance == endKey, "Movement must land on the chosen destination");
}
Check(CustomArrowPracticePreferences.Load(otherUser).Kinds.Length == 6, "Custom setup must be user-specific");
Preferences.Default.Set($"custom_arrow_builder_v1_{user:D}", "broken");
Check(CustomArrowPracticePreferences.Load(user).ValidationError() == null, "Corrupt custom setup must fall back safely");
var movementRandom = new Random(48);
foreach (bool throughTen in new[] { false, true })
{
    int? previous = null;
    for (int i = 0; i < 100; i++)
    {
        var question = ArrowLabelMovement.Next(movementRandom, 1, 20, 4, !throughTen, throughTen, false, previous);
        Check(!previous.HasValue || question.Start == previous, "Arrow Label continuation must use the previous destination");
        Check(question.Distance is >= 1 and <= 4, "Arrow Label length choice must be respected");
        bool crosses = Math.Min(question.Start, question.End) < 10 && Math.Max(question.Start, question.End) > 10;
        Check(crosses == throughTen, "Arrow Label must honor the selected ten-boundary mode");
        Check(question.GetLabelAnswerKeys().Select(key => SpecificArrowQuestion.KeyNumber(key)).All(key => key is >= 1 and <= 10),
            "Arrow Label must always use a ten-key physical keyboard");
        previous = question.End;
    }
}
for (int i = 0; i < 100; i++)
{
    var question = ArrowLabelMovement.Next(movementRandom, 1, 20, 4, false, false, true, null);
    Check(SpecificArrowQuestion.KeyNumber(question.Start) is 1 or 10, "Edges selection must start at an edge of the ten-key keyboard");
}
Console.WriteLine($"Passed {checks} arrow preference, progression and customization checks.");
