using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Platform;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace GestureSample.Maui.Data.SupaBase
{
    [Table("KeyEvent")]
    public class KeyEvent : BaseModel
    {
        [PrimaryKey("id", false)]
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
    }
}
