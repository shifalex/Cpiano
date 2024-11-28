//using SQLite;
//using Microsoft.Data.Sqlite;
using SQLite;
//using Realms;

namespace GestureSample.Maui.Data
{
    [Table("State")]
    public class State //: RealmObject

    {

        public string Id { get; set; }
        public DateTimeOffset TimeStamp { get; set; } = DateTimeOffset.Now;
        public int UserId { get; set; }
        public int Sum { get; set; }
        public int Addend1 { get; set; }
        public int Addend2 { get; set; }

        public string Op { get; set; } = Operation.Sum.ToString();

        //public Color[] KeysPressed { get; set; }



    }

    public class ShowState : State
    {

        public ShowState(State state)
        {
            TimeStamp = state.TimeStamp;
            UserId = state.UserId;
            Sum = state.Sum;
            Addend1 = state.Addend1;
            Addend2 = state.Addend2;
            Op = state.Op;
        }

        public Color Addend1Color { get; set; } = Colors.White;
        public Color Addend2Color { get; set; } = Colors.White;
        public Color SumColor { get; set; } = Colors.White;
        public Color TimeColor { get { return TimeOnTask > TimeSpan.FromSeconds(10) ? Colors.Yellow : Colors.White; } }  
        public DateTimeOffset? StartTime { get; set; } = null;

        public TimeSpan? TimeOnTask { get { if (StartTime == null) return null; return (TimeStamp - StartTime); } }
    }
}
