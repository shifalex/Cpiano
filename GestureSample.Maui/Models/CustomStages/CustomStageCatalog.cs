using GestureSample.Maui.Data.SQLite;
namespace GestureSample.Maui.Models.CustomStages
{
    public enum CustomStageKind
    {
        PPWScheme,
        Arrow,
        Logical
    }

    public static class CustomStageCatalog
    {
        public static string GetDisplayName(CustomStageKind kind) => kind switch
        {
            CustomStageKind.PPWScheme => "PPW Scheme",
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
                        SecondsPressingToAnswer = 2
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
                case CustomStageKind.Arrow:
                    config.UIQuestionType = UIQuestionType.OnlyKeyboard;
                    config.KeyboardConfig ??= new KeyboardConfig();
                    config.KeyboardConfig.IsArrow = true;
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
                CustomStageKind.PPWScheme =>
                    $"{config.OperationList.FirstOrDefault().ToDString()}  {config.MinAddend}-{config.MaxAddend}  sum {config.MinSum}-{config.MaxSum}  {config.UIQuestionType}",
                CustomStageKind.Arrow =>
                    $"{config.QuestionOrder}  {config.MinAddend}-{config.MaxAddend}  sum {config.MinSum}-{config.MaxSum}  {config.KeyboardConfig?.SyncType}",
                CustomStageKind.Logical =>
                    $"{config.OperationList.FirstOrDefault().ToDString()}  {config.KeyboardConfig?.SyncType}  {config.KeyboardConfig?.KeysInRow} keys",
                _ => stage.Name
            };
        }
    }
}
