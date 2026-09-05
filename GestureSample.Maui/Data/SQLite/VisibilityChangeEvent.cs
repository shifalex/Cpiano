using SQLite;

namespace GestureSample.Maui.Data.SQLite
{
    [Table("VisibilityChangeEvent")]
    public class VisibilityChangeEvent
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string GameId { get; set; } = string.Empty;

        public int QuestionNumber { get; set; }

        public DateTime EventTime { get; set; } = DateTime.Now;

        public string Target { get; set; } = string.Empty;

        public bool WasVisible { get; set; }

        public bool IsVisible { get; set; }

        public bool WasInitiallyVisible { get; set; }

        public string Source { get; set; } = string.Empty;
    }
}
