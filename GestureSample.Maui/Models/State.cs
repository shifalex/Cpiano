//using SQLite;
//using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureSample.Maui.Models
{
    //[Table("State")]
    public class State

    {
        //[PrimaryKey, AutoIncrement, Column("Id")]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string TypeName { get; set; }
        public DateTime TimeStamp { get; set; }
        public int Sum { get; set; }
        public int Addent1 { get; set; }
        public int Addent2 { get; set; }

        //TODO: change to byte array
        public bool B1 { get; set; } = false;
        public bool B2 { get; set; } = false;
        public bool B3 { get; set; }
        = false;
        public bool B4 { get; set; } = false;
        public bool B5 { get; set; } = false;
        public bool B6 { get; set; } = false;
        public bool B7 { get; set; } = false;
        public bool B8 { get; set; } = false;
        public bool B9 { get; set; } = false;
        public bool B10 { get; set; } = false;
        public bool B11 { get; set; } = false;
        public bool B12 { get; set; } = false;
        public bool B13 { get; set; } = false;
        public bool B14 { get; set; } = false;
        public bool B15 { get; set; } = false;
        public bool B16 { get; set; } = false;
        public bool B17 { get; set; } = false;
        public bool B18 { get; set; } = false;
        public bool B19 { get; set; } = false;
        public bool B20 { get; set; } = false;




    }
}
