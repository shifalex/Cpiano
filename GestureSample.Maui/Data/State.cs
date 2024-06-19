//using SQLite;
//using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
//using Realms;

namespace GestureSample.Maui.Data
{
    [Table("State")]
    public class State //: RealmObject

    {
        [PrimaryKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset TimeStamp { get; set; } = DateTimeOffset.Now;
        public int UserId { get; set; }
        public string TypeName { get; set; }
        public int Sum { get; set; }
        public int Addent1 { get; set; }
        public int Addent2 { get; set; }





    }
}
