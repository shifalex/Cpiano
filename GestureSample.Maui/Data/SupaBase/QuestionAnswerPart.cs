using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace GestureSample.Maui.Data.SupaBase
{
    [Table("QuestionAnswerPart")]
    public class QuestionAnswerPart : BaseModel
    {
        [PrimaryKey("Id", false)]
        public int Id { get; set; }

        public string GameId { get; set; } = string.Empty;
        public int QuestionNumber { get; set; }
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public string ValueText { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
        public string PartKind { get; set; } = "Visible";
        public string EntryName { get; set; } = string.Empty;
        public int AttemptNumber { get; set; } = 0;
        public bool? IsCorrect { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.Now;
    }
}
