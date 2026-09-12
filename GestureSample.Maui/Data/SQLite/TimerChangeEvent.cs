using SQLite;

namespace GestureSample.Maui.Data.SQLite
{
    [Table("TimerChangeEvent")]
    public class TimerChangeEvent
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string GameId { get; set; } = string.Empty;

        public int QuestionNumber { get; set; }

        public DateTime EventTime { get; set; } = DateTime.Now;

        public int OldSetting { get; set; }

        public int NewSetting { get; set; }

        public string Source { get; set; } = string.Empty;
    }
}
