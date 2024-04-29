using GestureSample.Maui.Models;
using Microsoft.EntityFrameworkCore;

//using SQLite;
//using Microsoft.Data.Sqlite;
//using Microsoft.EntityFrameworkCore.Sqlite;
using System.Data.Common;

namespace GestureSample.Maui.Data
{
    public class StateConnection : DbContext
    {
        string _dbPath;
        private DbConnection conn;
        public DbSet<State> States { get; set; }
        public StateConnection(DbContextOptions<StateConnection> options) : base(options)
        { 
        }

        /*
        public StateConnection(string dbPath)
        {
            _dbPath = dbPath;
        }

        public void Init()
        {
            if (conn is not null) return;
            conn= new SQLiteConnection(_dbPath);
            conn.CreateTable<State>();
        }

        public List<State> GetStates()
        {
            Init();
            return conn.Table<State>().ToList();
        }

        public void Add(State s)
        {
            Init();
            conn.Insert(s);
        }*/
    }
}
