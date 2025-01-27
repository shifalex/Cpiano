using SQLite;
using System.Data.Common;

//using Microsoft.Data.Sqlite;
//using Microsoft.EntityFrameworkCore.Sqlite;
using System.Net.Http.Headers;

namespace GestureSample.Maui.Data
{
    public class StateConnection
    {
        private static string _dbPath;
        //private static SQLiteAsyncConnection _database;
        private static readonly Lazy<StateConnection> lazy = new Lazy<StateConnection>(() => new StateConnection());

        public static StateConnection Instance => lazy.Value;


        public SQLiteAsyncConnection Database { get; private set; }

        private StateConnection()
        {
            Console.WriteLine("Creating Database");
            InitializeDatabase().Wait();
        }

        private async Task InitializeDatabase()
        {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MathOPiano.db");
#if WINDOWS || IOS
                _dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MathOPiano.db");
#endif
            Console.WriteLine($"Database path: {_dbPath}");
            Database = new SQLiteAsyncConnection(dbPath);
            Console.WriteLine($"Database created successfully");

            await CreateTableAsync<QuestionAnswer>();
            await CreateTableAsync<KeyboardQuestion>();
            await CreateTableAsync<Game>();
            await CreateTableAsync<KeyEvent>();
            await CreateTableAsync<User>();
            Console.WriteLine($"Tables created successfully");
        }

        private async Task CreateTableAsync<T>() where T : new()
        {
            try
            {
                //_database.DropTableAsync<QuestionAnswer>().Wait();
                Database.CreateTableAsync<T>().Wait();
                Console.WriteLine($"Table '{typeof(T).Name}' created successfully.");
            }
            catch (Exception ex)
            {
                // Drop the existing table
                Database.DropTableAsync<T>().Wait();
                Console.WriteLine($"Table '{typeof(T).Name}' dropped successfully.");

                // Recreate the table
                Database.CreateTableAsync<T>().Wait();
                Console.WriteLine($"Table '{typeof(T).Name}' created successfully.");
                Console.WriteLine($"Database initialization failed: {ex.Message}");
            }
        }
    }
}
