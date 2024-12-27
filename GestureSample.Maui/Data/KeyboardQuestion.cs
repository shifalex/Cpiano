using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GestureSample.Maui.Models;
using SQLite;

namespace GestureSample.Maui.Data
{
    [Table("KeyboardQuestion")]
    public class KeyboardQuestion
    {
        [PrimaryKey, AutoIncrement]
        public int QuestionID { get; set; }
        public int QuestionNumber { get; set; }
        public string GameId { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;
        public string UserId { get; set; } = "1";
        public int ResultStatus { get; set; } = 0;


        public int? aboveNumber { get; set; }
        public int? length { get; set; }

        [Ignore]
        public Color RowBackgroundColor { get; set; } = Colors.White;

        [Ignore]
        public Color RowBackgroundColor { get; set; } = Colors.White;

        //public string Op { get; set; } = Operation.Sum.ToString();

        // Ignore GameConfig during table creation
        [Ignore]
        public bool[] keyboard1 { get; set; }
        [Ignore]
        public bool[] keyboard2 { get; set; }
        [Ignore]
        public Direction dir { get; set; } 

        // Serialize GameConfig as JSON for storage
        [Column("ConfigJson")]
        public string ConfigJson
        {
            get => JsonSerializer.Serialize(keyboard1);
            set => keyboard1 = value != null ? JsonSerializer.Deserialize<bool[]>(value) : null;
        }

        // Serialize GameConfig as JSON for storage
        [Column("ConfigJson2")]
        public string ConfigJson2
        {
            get => JsonSerializer.Serialize(keyboard2);
            set => keyboard2 = value != null ? JsonSerializer.Deserialize<bool[]>(value) : null;
        }
        [Ignore]
        public Operation Op { get; set; } = Operation.Sum;

        // Serialize GameConfig as JSON for storage
        [Column("ConfigJson4")]
        public string ConfigJson4
        {
            get => JsonSerializer.Serialize(Op);
            set => Op = value != null ? JsonSerializer.Deserialize<Operation>(value) : Operation.Copy;
        }

        // Serialize GameConfig as JSON for storage
        [Column("ConfigJson3")]
        public string ConfigJson3
        {
            get => JsonSerializer.Serialize(dir);
            set => dir = value != null ? JsonSerializer.Deserialize<Direction>(value) : Direction.Right;
        }


    }
}
