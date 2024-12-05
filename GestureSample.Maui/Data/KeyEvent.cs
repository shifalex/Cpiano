using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Platform;
using SQLite;

namespace GestureSample.Maui.Data
{
    [Table("KeyEvent")]
    public class KeyEvent
    {
        [PrimaryKey, AutoIncrement]
        public int id {  get; }
        public string GameId { get; set; }
        public int EventType { get; set; }
        public int KeyNumber { get; set; }
        public int Row { get; set; } = 0;
        public DateTime EventTime { get; set; }
    }
}
