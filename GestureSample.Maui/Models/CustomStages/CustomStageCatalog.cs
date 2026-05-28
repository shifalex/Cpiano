using GestureSample.Maui.Data.SQLite;
namespace GestureSample.Maui.Models.CustomStages
{
    public enum CustomStageKind
    {
        PPWScheme,
        WeightedKeyboard,
        Arrow,
        Logical
    }

    public static class CustomStageCatalog
    {
        public static string GetDisplayName(CustomStageKind kind) => kind switch
        {
            CustomStageKind.PPWScheme => "PPW Scheme",
            CustomStageKind.WeightedKeyboard => "Weighted Keyboard",
            CustomStageKind.Arrow => "Arrow",
            CustomStageKind.Logical => "Logical",
            _ => kind.ToString()
        };

        public static GameConfig CreateDefaultConfig(CustomStageKind kind)
        {
            return kind switch
            {
                CustomStageKind.PPWScheme => new GameConfig
                {
                    GameName = "Custom PPW",
                    OperationList = new() { Operation.Sum },
                    UIQuestionType = UIQuestionType.ThreeTexts,
                    VariableTypes = VariableTypes.OneCanBeSum,
                    MinAddend = 1,
                    MaxAddend = 9,
                    MinSum = 2,
                    MaxSum = 10,
                    NumberOfTasksToWin = 20,
                    NumberOfMistakesToLose = 3,
                    NumericInputMode = NumericInputMode.AppKeypad
                },
                CustomStageKind.WeightedKeyboard => new GameConfig
                {
                    GameName = "Custom Weighted Keyboard",
                    OperationList = new() { Operation.Sum },
                    UIQuestionType = UIQuestionType.OneText,
                    VariableTypes = VariableTypes.TwoNoSum,
                    MinAddend = 1,
                    MaxAddend = 10,
                    MinSum = 1,
                    MaxSum = 10,
                    NumberOfTasksToWin = 20,
                    NumberOfMistakesToLose = 3,
                    NumericInputMode = NumericInputMode.AppKeypad,
                    KeyboardConfig = new KeyboardConfig
                    {
                        SyncType = SyncType.Sync,
                        TextBoxesQuantity = 1,
                        Rows = 1,
                        KeysInRow = 10,
                        SecondsPressingToAnswer = 2,
                        ShowNumbersOnKeys = true,
                        AllowSumHeaderVisibilityToggle = false,
                        UseWeightedCustomStageTargets = true
                    }
                },
                CustomStageKind.Arrow => new GameConfig
                {
                    GameName = "Custom Arrow",
                    UIQuestionType = UIQuestionType.OnlyKeyboard,
                    QuestionOrder = QuestionOrder.FromLeft,
                    MinAddend = 1,
                    MaxAddend = 9,
                    MinSum = 1,
                    MaxSum = 10,
                    OnlyToTen = true,
                    NumberOfTasksToWin = 20,
                    NumberOfMistakesToLose = 3,
                    KeyboardConfig = new KeyboardConfig
                    {
                        SyncType = SyncType.Sync,
                        IsArrow = true,
                        SecondsPressingToAnswer = 2,
                        ArrowDirectionMode = ArrowDirectionMode.LeftToRight,
                        AllowedArrowPromptKinds = ArrowPromptKindFlags.OnKeyboard,
                        AllowedArrowRouteKinds = ArrowRouteKindFlags.Cardinal,
                        SpecialArrowMissingTargets = MissingValueTargetFlags.Sum,
                        ArrowFeedbackMode = ArrowFeedbackMode.Icon
                    }
                },
                CustomStageKind.Logical => new GameConfig
                {
                    GameName = "Custom Logical",
                    UIQuestionType = UIQuestionType.LogicalKeyboards,
                    OperationList = new() { Operation.Copy },
                    OnlyToTen = true,
                    NumberOfTasksToWin = 20,
                    NumberOfMistakesToLose = 3,
                    KeyboardConfig = new KeyboardConfig
                    {
                        SyncType = SyncType.Sync,
                        KeysInRow = 6
                    }
                },
                _ => new GameConfig()
            };
        }

        public static GameConfig ClonePlayableConfig(CustomStageDefinition stage)
        {
            GameConfig config = DeepClone(stage.Config);
            config.GameName = stage.Name;
            return Normalize(stage.StageKind, config);
        }

        public static GameConfig DeepClone(GameConfig config)
        {
            string json = GameConfigJson.Serialize(config);
            return GameConfigJson.Deserialize(json);
        }

        public static GameConfig Normalize(CustomStageKind kind, GameConfig config)
        {
            config.GameName = string.IsNullOrWhiteSpace(config.GameName)
                ? $"Custom {GetDisplayName(kind)}"
                : config.GameName;

            switch (kind)
            {
                case CustomStageKind.PPWScheme:
                    config.UIQuestionType = config.UIQuestionType switch
                    {
                        UIQuestionType.ThreeTexts or
                        UIQuestionType.ThreeAddends or
                        UIQuestionType.SimpleEquation or
                        UIQuestionType.TwoLinesTwoAddends or
                        UIQuestionType.OneText => config.UIQuestionType,
                        _ => UIQuestionType.ThreeTexts
                    };
                    config.OperationList ??= new List<Operation>();
                    if (config.OperationList.Count == 0)
                        config.OperationList.Add(Operation.Sum);
                    config.NumericInputMode = config.NumericInputMode == NumericInputMode.Auto
                        ? NumericInputMode.AppKeypad
                        : config.NumericInputMode;
                    break;
                case CustomStageKind.WeightedKeyboard:
                    config.OperationList = new List<Operation> { Operation.Sum };
                    config.UIQuestionType = UIQuestionType.OneText;
                    config.VariableTypes = VariableTypes.TwoNoSum;
                    config.NumericInputMode = NumericInputMode.AppKeypad;
                    config.KeyboardConfig ??= new KeyboardConfig();
                    config.KeyboardConfig.UseWeightedCustomStageTargets = true;
                    config.KeyboardConfig.SyncType = SyncType.Sync;
                    config.KeyboardConfig.TextBoxesQuantity = 1;
                    config.KeyboardConfig.Rows = 1;
                    if (config.KeyboardConfig.WeightsArray?.Length > 0)
                        config.KeyboardConfig.KeysInRow = config.KeyboardConfig.WeightsArray.Length;
                    config.KeyboardConfig.ShowNumbersOnKeys = true;
                    config.KeyboardConfig.KeyLabelVerticalPosition = KeyLabelVerticalPosition.Middle;
                    config.KeyboardConfig.AllowSumHeaderVisibilityToggle = false;
                    break;
                case CustomStageKind.Arrow:
                    config.UIQuestionType = UIQuestionType.OnlyKeyboard;
                    config.KeyboardConfig ??= new KeyboardConfig();
                    if (config.KeyboardConfig.ArrowDirectionMode == ArrowDirectionMode.Auto)
                    {
                        config.KeyboardConfig.ArrowDirectionMode = config.QuestionOrder switch
                        {
                            QuestionOrder.ToLeft => ArrowDirectionMode.RightToLeft,
                            QuestionOrder.BackAndForth => ArrowDirectionMode.Alternating,
                            QuestionOrder.Random => ArrowDirectionMode.Random,
                            _ => ArrowDirectionMode.LeftToRight
                        };
                    }
                    config.QuestionOrder = config.KeyboardConfig.ArrowDirectionMode switch
                    {
                        ArrowDirectionMode.RightToLeft => QuestionOrder.ToLeft,
                        ArrowDirectionMode.Alternating => QuestionOrder.BackAndForth,
                        ArrowDirectionMode.Random => QuestionOrder.Random,
                        _ => QuestionOrder.FromLeft
                    };
                    if (config.KeyboardConfig.AllowedArrowPromptKinds == ArrowPromptKindFlags.None)
                        config.KeyboardConfig.AllowedArrowPromptKinds = config.KeyboardConfig.ArrowLabelExerciseMode == ArrowLabelExerciseMode.None
                            ? ArrowPromptKindFlags.OnKeyboard
                            : ArrowPromptKindFlags.SpecialPrompt;
                    if (config.KeyboardConfig.AllowedArrowRouteKinds == ArrowRouteKindFlags.None)
                        config.KeyboardConfig.AllowedArrowRouteKinds = config.KeyboardConfig.ArrowType == ArrowType.Rounded ||
                                                                      config.KeyboardConfig.ArrowLabelExerciseMode == ArrowLabelExerciseMode.OrdinalStartAndLength
                            ? ArrowRouteKindFlags.Ordinal
                            : ArrowRouteKindFlags.Cardinal;
                    if (config.KeyboardConfig.SpecialArrowMissingTargets == MissingValueTargetFlags.None)
                    {
                        config.KeyboardConfig.SpecialArrowMissingTargets = config.KeyboardConfig.ArrowLabelExerciseMode switch
                        {
                            ArrowLabelExerciseMode.StartAndEndWithMissingLength => MissingValueTargetFlags.Addend2,
                            ArrowLabelExerciseMode.EndAndLengthWithMissingStart => MissingValueTargetFlags.Addend1,
                            _ => MissingValueTargetFlags.Sum
                        };
                    }
                    if (config.KeyboardConfig.AllowedArrowPromptKinds.HasFlag(ArrowPromptKindFlags.SpecialPrompt) &&
                        config.KeyboardConfig.AllowedArrowRouteKinds == ArrowRouteKindFlags.Ordinal &&
                        !config.KeyboardConfig.SpecialArrowMissingTargets.HasFlag(MissingValueTargetFlags.Sum))
                    {
                        config.KeyboardConfig.SpecialArrowMissingTargets |= MissingValueTargetFlags.Sum;
                    }
                    config.KeyboardConfig.IsArrow = config.KeyboardConfig.AllowedArrowPromptKinds.HasFlag(ArrowPromptKindFlags.OnKeyboard);
                    if (config.KeyboardConfig.SecondsPressingToAnswer == 0)
                        config.KeyboardConfig.SecondsPressingToAnswer = 2;
                    config.OperationList ??= new List<Operation> { Operation.Sum };
                    break;
                case CustomStageKind.Logical:
                    config.UIQuestionType = UIQuestionType.LogicalKeyboards;
                    config.KeyboardConfig ??= new KeyboardConfig();
                    if (config.KeyboardConfig.KeysInRow <= 0)
                        config.KeyboardConfig.KeysInRow = 6;
                    config.OperationList ??= new List<Operation>();
                    if (config.OperationList.Count == 0)
                        config.OperationList.Add(Operation.Copy);
                    break;
            }

            return config;
        }

        public static string BuildSummary(CustomStageDefinition stage)
        {
            GameConfig config = stage.Config ?? CreateDefaultConfig(stage.StageKind);

            return stage.StageKind switch
            {
                CustomStageKind.WeightedKeyboard =>
                    $"Weighted  {string.Join(",", config.KeyboardConfig.WeightsArray ?? Array.Empty<int>())}  sum {config.MinSum}-{config.MaxSum}" +
                    (config.KeyboardConfig.AllowImpossibleWeightedAnswer ? "  + XXX" : string.Empty),
                CustomStageKind.PPWScheme =>
                    $"{config.OperationList.FirstOrDefault().ToDString()}  {config.MinAddend}-{config.MaxAddend}  sum {config.MinSum}-{config.MaxSum}  {config.UIQuestionType}",
                CustomStageKind.Arrow =>
                    $"{config.KeyboardConfig?.ArrowDirectionMode}  {config.MinAddend}-{config.MaxAddend}  sum {config.MinSum}-{config.MaxSum}  {config.KeyboardConfig?.SyncType}  {DescribeArrowPromptFamily(config.KeyboardConfig)}  {DescribeArrowRouteFamily(config.KeyboardConfig)}  {config.KeyboardConfig?.ArrowFeedbackMode}",
                CustomStageKind.Logical =>
                    $"{config.OperationList.FirstOrDefault().ToDString()}  {config.KeyboardConfig?.SyncType}  {config.KeyboardConfig?.KeysInRow} keys",
                _ => stage.Name
            };
        }

        private static string DescribeArrowPromptFamily(KeyboardConfig? keyboardConfig)
        {
            ArrowPromptKindFlags flags = keyboardConfig?.AllowedArrowPromptKinds ?? ArrowPromptKindFlags.None;
            return flags switch
            {
                ArrowPromptKindFlags.OnKeyboard => "On keyboard",
                ArrowPromptKindFlags.SpecialPrompt => "Special prompt",
                ArrowPromptKindFlags.OnKeyboard | ArrowPromptKindFlags.SpecialPrompt => "Mixed prompt",
                _ => "On keyboard"
            };
        }

        private static string DescribeArrowRouteFamily(KeyboardConfig? keyboardConfig)
        {
            ArrowRouteKindFlags flags = keyboardConfig?.AllowedArrowRouteKinds ?? ArrowRouteKindFlags.None;
            return flags switch
            {
                ArrowRouteKindFlags.Cardinal => "Cardinal",
                ArrowRouteKindFlags.Ordinal => "Ordinal",
                ArrowRouteKindFlags.Cardinal | ArrowRouteKindFlags.Ordinal => "Mixed route",
                _ => "Cardinal"
            };
        }
    }
}
