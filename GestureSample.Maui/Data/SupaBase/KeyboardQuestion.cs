using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GestureSample.Maui.Handlers;
using GestureSample.Maui.Models;
using SQLite;
using Supabase.Postgrest.Models;
using NewtonsoftJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace GestureSample.Maui.Data.SupaBase
{
    [Table("KeyboardQuestion")]
    public class KeyboardQuestion : BaseModel
    {
        [PrimaryKey, AutoIncrement]
        public int QuestionID { get; set; }
        public int QuestionNumber { get; set; }
        public int AttemptNumber { get; set; } = 0;
        public string GameId { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;
        public Guid UserId { get; set; } = Guid.Empty;
        public int ResultStatus { get; set; } = 0;
        public bool WasTutorialUsed { get; set; } = false;

        public int? aboveNumber { get; set; }
        public int? length { get; set; }
        public int? MoveByLength { get; set; }
        public int KeyboardRows { get; set; } = 1;
        public int KeyboardKeysInRow { get; set; } = 10;
        public bool ShowNumbersOnKeys { get; set; } = false;
        public string? QuestionPromptText { get; set; }

        [NewtonsoftJsonIgnore]
        public Color RowBackgroundColor { get; set; } = Colors.White;


        //public string Op { get; set; } = Operation.Sum.ToString();

        // Ignore GameConfig during table creation
        [NewtonsoftJsonIgnore]
        public bool[] keyboard1 { get; set; }
        [NewtonsoftJsonIgnore]
        public bool[] keyboard2 { get; set; }
        [NewtonsoftJsonIgnore]
        public Direction dir { get; set; } 
        [NewtonsoftJsonIgnore]
        public Direction MoveByDirection { get; set; } = Direction.Right;
        [NewtonsoftJsonIgnore]
        public bool[] SubmittedKeyboard { get; set; }
        [NewtonsoftJsonIgnore]
        public int[]? KeyboardWeights { get; set; }
        [NewtonsoftJsonIgnore]
        public bool[]? InitialKeyboardState { get; set; }

        // Serialize GameConfig as JSON for storage
        public string ConfigJson
        {
            get => JsonSerializer.Serialize(keyboard1);
            set => keyboard1 = value != null ? JsonSerializer.Deserialize<bool[]>(value) : null;
        }

        // Serialize GameConfig as JSON for storage
        public string ConfigJson2
        {
            get => JsonSerializer.Serialize(keyboard2);
            set => keyboard2 = value != null ? JsonSerializer.Deserialize<bool[]>(value) : null;
        }
        [NewtonsoftJsonIgnore]
        public Operation Op { get; set; } = Operation.Sum;

        // Serialize GameConfig as JSON for storage
        public string ConfigJson4
        {
            get => JsonSerializer.Serialize(Op);
            set => Op = value != null ? JsonSerializer.Deserialize<Operation>(value) : Operation.Copy;
        }

        // Serialize GameConfig as JSON for storage
        public string ConfigJson3
        {
            get => JsonSerializer.Serialize(dir);
            set => dir = value != null ? JsonSerializer.Deserialize<Direction>(value) : Direction.Right;
        }

        public string SubmittedKeyboardJson
        {
            get => JsonSerializer.Serialize(SubmittedKeyboard);
            set => SubmittedKeyboard = value != null ? JsonSerializer.Deserialize<bool[]>(value) : null;
        }

        public string KeyboardWeightsJson
        {
            get => JsonSerializer.Serialize(KeyboardWeights);
            set => KeyboardWeights = value != null ? JsonSerializer.Deserialize<int[]>(value) : null;
        }

        public string InitialKeyboardStateJson
        {
            get => JsonSerializer.Serialize(InitialKeyboardState);
            set => InitialKeyboardState = value != null ? JsonSerializer.Deserialize<bool[]>(value) : null;
        }

        public string MoveByDirectionJson
        {
            get => JsonSerializer.Serialize(MoveByDirection);
            set => MoveByDirection = value != null ? JsonSerializer.Deserialize<Direction>(value) : Direction.Right;
        }

        public DateTime? SubmittedTime { get; set; }


    }
}
