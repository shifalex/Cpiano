//using SQLite;
//using Microsoft.Data.Sqlite;
//using MongoDB.Bson.IO;
using Microsoft.Maui.Platform;
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
        public DateTime TimeStart { get; set; } = DateTime.Now;
        public DateTime TimeEnd { get; set; } = DateTime.Now;//TODO: excgange into the last endtime of the game by calculating
        public int FinalStatus { get; set; } = -1;
        //public TimeSpan FinalTime { get; set; } = TimeSpan.Zero;
        public string UserId { get; set; }
        public int Wins { get; set; } = 0;
        public int Losses { get; set; } = 0;

        public override string ToString()
        {
            string status =  FinalStatus switch { 0=>"Lose", 1=>"WIN!", _ => ""};
            string time = ((TimeSpan)(TimeEnd-TimeStart)).ToFormattedString("mm:ss");
            return $"{ TimeStart:t} {status} {time} Minutes {Wins}/{(Wins+Losses)}";
        }

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
