using GestureSample.Maui.Models.CustomStages;
using SQLite;

namespace GestureSample.Maui.Data.SQLite
{
    [Table("CustomStageDefinition")]
    public class CustomStageDefinition
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string StageKindName { get; set; } = CustomStageKind.PPWScheme.ToString();

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [Ignore]
        public CustomStageKind StageKind
        {
            get
            {
                if (Config?.KeyboardConfig?.UseWeightedCustomStageTargets == true)
                    return CustomStageKind.WeightedKeyboard;

                return Enum.TryParse(StageKindName, out CustomStageKind kind) ? kind : CustomStageKind.PPWScheme;
            }
            set => StageKindName = value.ToString();
        }

        [Ignore]
        public GameConfig Config { get; set; } = new();

        [Column("ConfigJson")]
        public string ConfigJson
        {
            get => GameConfigJson.Serialize(Config ?? new GameConfig());
            set => Config = string.IsNullOrWhiteSpace(value)
                ? new GameConfig()
                : GameConfigJson.Deserialize(value);
        }
    }
}
