//using SQLite;
//using Microsoft.Data.Sqlite;
using SQLite;
using System.Text.Json;
//using Realms;

namespace GestureSample.Maui.Data
{
    [Table("QuestionAnswer")]
    public class QuestionAnswer //: RealmObject

    {
        [PrimaryKey,AutoIncrement]
        public int QuestionID { get; set; }
        public int QuestionNumber { get; set; }
        public string GameId { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;
        public string UserId { get; set; }
        public int Sum { get; set; }
        public int Addend1 { get; set; }
        public int Addend2 { get; set; }

        public int ResultStatus { get; set; } = 0;

        //public string Op { get; set; } = Operation.Sum.ToString();

        // Ignore GameConfig during table creation
        [Ignore]
        public Operation Op { get; set; } = Operation.Sum;

        // Serialize GameConfig as JSON for storage
        [Column("ConfigJson")]
        public string ConfigJson
        {
            get =>  JsonSerializer.Serialize(Op);
            set => Op = value != null ? JsonSerializer.Deserialize<Operation>(value) : Operation.Sum;
        }

        //public Color[] KeysPressed { get; set; }



    }

    public class ShowState : QuestionAnswer
    {

        public ShowState(QuestionAnswer state)
        {
            QuestionNumber = state.QuestionNumber;
            Time = state.Time;
            UserId = state.UserId;
            Sum = state.Sum;
            Addend1 = state.Addend1;
            Addend2 = state.Addend2;
            Op = state.Op;
            ResultStatus = state.ResultStatus;
            OpDString = state.Op.ToDString();
        }

        public Color Addend1Color { get; set; } = Colors.White;
        public Color Addend2Color { get; set; } = Colors.White;
        public Color SumColor { get; set; } = Colors.White;
        public Color TimeColor { get { return TimeOnTask > 10 ? Colors.Yellow : Colors.White; } }  
        public DateTimeOffset? StartTime { get; set; } = null;

        public Color RowBackgroundColor { get; set; }
        public double? TimeOnTask { get { if (StartTime == null) return null; return ((TimeSpan)(Time - StartTime)).TotalSeconds; } }
        public int SerialNumber { get; set; }
        public string OpDString { get; set; }
    }
}
