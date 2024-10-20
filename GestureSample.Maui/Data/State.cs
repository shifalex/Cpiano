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


}
