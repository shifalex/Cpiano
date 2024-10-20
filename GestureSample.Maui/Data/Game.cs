//using SQLite;
//using Microsoft.Data.Sqlite;
using SQLite;
//using Realms;

namespace GestureSample.Maui.Data
{
    [Table("Game")]
    public class Game //: RealmObject

    {
        [PrimaryKey]
        public string Id { get; set; }
        public DateTimeOffset TimeStamp { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset TimeStampEnd { get; set; } = DateTimeOffset.Now;//TODO: excgange into the last endtime of the game by calculating
        public bool IsWin { get; set; }
        public int UserId { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }

        public GameConfig Config { get; set; }

        //public Color[] KeysPressed { get; set; }



    }
}
