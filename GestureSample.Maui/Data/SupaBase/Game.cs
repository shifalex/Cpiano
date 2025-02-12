
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.Text.Json;

namespace GestureSample.Maui.Data.SupaBase
{
    [Table("Games")]
    public class Game : BaseModel

    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }
        [Column("index")]
        public int index { get; set; }
        [Column("timeStart")]
        public DateTime TimeStart { get; set; } = DateTime.Now;
        [Column("timeEnd")]
        public DateTime TimeEnd { get; set; } = DateTime.Now;//TODO: excgange into the last endtime of the game by calculating
        [Column("finalStatus")]
        public int FinalStatus { get; set; } = -1;
        //public TimeSpan FinalTime { get; set; } = TimeSpan.Zero;
        [Column("userId")]
        public Guid UserId { get; set; }
        [Column("wins")]
        public int Wins { get; set; } = 0;
        [Column("losses")]
        public int Losses { get; set; } = 0;
        [Column("gameName")]
        public string GameName { get; set; }

       [Column("configJson")]
        public string ConfigJson { get; set; }




    }
}
