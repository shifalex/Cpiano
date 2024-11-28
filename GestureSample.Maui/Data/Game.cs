//using SQLite;
//using Microsoft.Data.Sqlite;
//using MongoDB.Bson.IO;
using SQLite;
using System.Text.Json;
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

        // Ignore GameConfig during table creation
        [Ignore]
        public GameConfig Config { get; set; }

        // Serialize GameConfig as JSON for storage
        [Column("ConfigJson")]
        public string ConfigJson
        {
            get => Config != null ? JsonSerializer.Serialize(Config) : null;
            set => Config = value != null ? JsonSerializer.Deserialize<GameConfig>(value) : null;
        }

        //public Color[] KeysPressed { get; set; }



    }
}
