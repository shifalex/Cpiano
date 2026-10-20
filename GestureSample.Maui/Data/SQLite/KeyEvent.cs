using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Platform;
using SQLite;
using Supabase.Postgrest.Models;

namespace GestureSample.Maui.Data.SQLite
{
    [Table("KeyEvent")]
    public class KeyEvent
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }

        public string GameId { get; set; }

        public int QuestionNumber { get; set; }
        public int AttemptNumber { get; set; } = 0;
        public int EventType { get; set; }
        public int KeyNumber { get; set; }
        public int Row { get; set; } = 0;
        public DateTime EventTime { get; set; }
        public double? RelativeX { get; set; }
        public double? RelativeY { get; set; }

        [Ignore]
        public string EventTypeText => EventType switch
        {
            0 => "Up",
            1 => "Down",
            2 => "Check",
            3 => "Reset",
            _ => EventType.ToString()
        };

        [Ignore]
        public string RelativeXText => RelativeX.HasValue ? RelativeX.Value.ToString("0.00") : "-";

        [Ignore]
        public string RelativeYText => RelativeY.HasValue ? RelativeY.Value.ToString("0.00") : "-";

        [Ignore]
        public string EventTimeText => EventTime.ToString("HH:mm:ss.fff");
    }
}
