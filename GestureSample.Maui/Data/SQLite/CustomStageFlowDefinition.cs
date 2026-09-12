using SQLite;
using System.Text.Json;

namespace GestureSample.Maui.Data.SQLite
{
    public sealed class CustomStageFlowItem
    {
        public Guid StageId { get; set; }
    }

    [Table("CustomStageFlowDefinition")]
    public class CustomStageFlowDefinition
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [Ignore]
        public List<CustomStageFlowItem> Items { get; set; } = new();

        [Column("ItemsJson")]
        public string ItemsJson
        {
            get => JsonSerializer.Serialize(Items ?? new List<CustomStageFlowItem>());
            set => Items = string.IsNullOrWhiteSpace(value)
                ? new List<CustomStageFlowItem>()
                : JsonSerializer.Deserialize<List<CustomStageFlowItem>>(value) ?? new List<CustomStageFlowItem>();
        }
    }
}
