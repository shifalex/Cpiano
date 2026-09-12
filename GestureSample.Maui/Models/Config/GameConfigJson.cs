using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace GestureSample.Maui;

public static class GameConfigJson
{
    private static readonly JsonSerializerOptions WriteOptions = CreateWriteOptions();

    public static string Serialize(GameConfig config)
    {
        return JsonSerializer.Serialize(config ?? new GameConfig(), WriteOptions);
    }

    public static GameConfig Deserialize(string? json)
    {
        return string.IsNullOrWhiteSpace(json)
            ? new GameConfig()
            : JsonSerializer.Deserialize<GameConfig>(json) ?? new GameConfig();
    }

    private static JsonSerializerOptions CreateWriteOptions()
    {
        DefaultJsonTypeInfoResolver resolver = new();
        resolver.Modifiers.Add(typeInfo =>
        {
            if (typeInfo.Type == typeof(GameConfig))
            {
                IgnoreProperties(typeInfo,
                    nameof(GameConfig.VariableTypes),
                    nameof(GameConfig.PreferLargerAddend1),
                    nameof(GameConfig.EnforceOperationLabel),
                    nameof(GameConfig.FromNumToNum),
                    nameof(GameConfig.ShowPrev),
                    nameof(GameConfig.IncludeTutorials),
                    nameof(GameConfig.isHelpEntries),
                    nameof(GameConfig.isHelpThroughTen),
                    nameof(GameConfig.isOnlySequence),
                    nameof(GameConfig.isOnlyKeyboard),
                    nameof(GameConfig.OnlyThrougTen),
                    nameof(GameConfig.OnlyToTen),
                    nameof(GameConfig.OnlyThroughTen),
                    nameof(GameConfig.HelpEntries),
                    nameof(GameConfig.HelpThroughTen),
                    nameof(GameConfig.OnlySequence),
                    nameof(GameConfig.KeyboardOnly),
                    nameof(GameConfig.DenyStrangeOrSameGroups),
                    nameof(GameConfig.TwoKeybordsOnOne),
                    nameof(GameConfig.IsOnlyOneHand),
                    nameof(GameConfig.IsSpecialColor),
                    nameof(GameConfig.HasExercisePlan),
                    nameof(GameConfig.HasKeyboard),
                    nameof(GameConfig.HasTaskGoal),
                    nameof(GameConfig.HasMistakeLimit),
                    nameof(GameConfig.UsesQuestionPreview),
                    nameof(GameConfig.DelaysInput),
                    nameof(GameConfig.EffectiveMinAddend2),
                    nameof(GameConfig.EffectiveMaxAddend2),
                    nameof(GameConfig.KeepsSumVisible),
                    nameof(GameConfig.KeepsAtLeastOneAddendVisible),
                    nameof(GameConfig.UsesCombinedLogicalKeyboard),
                    nameof(GameConfig.RestrictsLogicalKeyboardToOneHand),
                    nameof(GameConfig.UsesSpecialLogicalKeyboardColors),
                    nameof(GameConfig.RequiresBothAddendsInput));
            }

            if (typeInfo.Type == typeof(KeyboardConfig))
            {
                IgnoreProperties(typeInfo,
                    nameof(KeyboardConfig.ShowNumbersOnKeys),
                    nameof(KeyboardConfig.ImposeEdges),
                    nameof(KeyboardConfig.ImposeSerealization),
                    nameof(KeyboardConfig.WithoutZero),
                    nameof(KeyboardConfig.AllowRemoval),
                    nameof(KeyboardConfig.KeyboardOnlyForHelp),
                    nameof(KeyboardConfig.KeyboardAsAQuestion),
                    nameof(KeyboardConfig.IsArrow),
                    nameof(KeyboardConfig.IsMulticolor),
                    nameof(KeyboardConfig.ArrowType),
                    nameof(KeyboardConfig.IsArrowLengthDynamic),
                    nameof(KeyboardConfig.IsNumberVoice),
                    nameof(KeyboardConfig.IsVoice),
                    nameof(KeyboardConfig.IsVoices),
                    nameof(KeyboardConfig.IsHelpNeeded));
            }
        });

        return new JsonSerializerOptions
        {
            TypeInfoResolver = resolver
        };
    }

    private static void IgnoreProperties(JsonTypeInfo typeInfo, params string[] names)
    {
        foreach (JsonPropertyInfo property in typeInfo.Properties)
        {
            if (names.Contains(property.Name, StringComparer.Ordinal))
            {
                property.ShouldSerialize = static (_, _) => false;
            }
        }
    }
}
