using GestureSample.Maui.Models;
using Microsoft.EntityFrameworkCore;

using SQLite;
using System;
using System.IO;
using System.Threading.Tasks;
//using Microsoft.Data.Sqlite;
//using Microsoft.EntityFrameworkCore.Sqlite;
using System.Data.Common;

namespace GestureSample.Maui.Data
{
    public class StateConnection
    {

        private static SQLiteAsyncConnection _database;
        private static readonly Lazy<StateConnection> lazy = new Lazy<StateConnection>(() => new StateConnection());

        public static StateConnection Instance => lazy.Value;

        private StateConnection()
        {
            InitializeDatabase().Wait();
        }

        private async Task InitializeDatabase()
        {
            if (_database == null)
            {
                var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "StatesDocumentation.db3");
                _database = new SQLiteAsyncConnection(dbPath);
                await _database.CreateTableAsync<State>();
            }
        }

        public Task<int> SaveStateAsync(State state)
        {
            return _database.InsertAsync(state);
        }

        public Task<List<State>> GetStatesAsync()
        {
            return _database.Table<State>().ToListAsync();
        }

        /*

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
