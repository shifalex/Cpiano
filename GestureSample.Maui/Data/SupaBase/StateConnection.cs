using SQLite;
using System.Data.Common;

//using Microsoft.Data.Sqlite;
//using Microsoft.EntityFrameworkCore.Sqlite;
using System.Net.Http.Headers;

namespace GestureSample.Maui.Data.SupaBase
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
            InitializeDatabase().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private async Task InitializeDatabase()
        {
            _dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MathOPiano.db");
#if WINDOWS || IOS
                _dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MathOPiano.db");
#endif
            Console.WriteLine($"Database path: {_dbPath}");
            Database = new SQLiteAsyncConnection(_dbPath);
            Console.WriteLine($"Database created successfully");

            await CreateTableAsync<QuestionAnswer>().ConfigureAwait(false);
            await CreateTableAsync<KeyboardQuestion>().ConfigureAwait(false);
            await CreateTableAsync<Game>().ConfigureAwait(false);
            await CreateTableAsync<KeyEvent>().ConfigureAwait(false);
            await CreateTableAsync<User>().ConfigureAwait(false);
            await EnsureColumnAsync("KeyEvent", "RelativeX", "REAL").ConfigureAwait(false);
            await EnsureColumnAsync("KeyEvent", "RelativeY", "REAL").ConfigureAwait(false);
            await EnsureColumnAsync("KeyEvent", "AttemptNumber", "INTEGER NOT NULL DEFAULT 0").ConfigureAwait(false);
            await EnsureColumnAsync("KeyboardQuestion", "SubmittedKeyboardJson", "TEXT").ConfigureAwait(false);
            await EnsureColumnAsync("KeyboardQuestion", "SubmittedTime", "TEXT").ConfigureAwait(false);
            await EnsureColumnAsync("KeyboardQuestion", "MoveByLength", "INTEGER").ConfigureAwait(false);
            await EnsureColumnAsync("KeyboardQuestion", "MoveByDirectionJson", "TEXT").ConfigureAwait(false);
            await EnsureColumnAsync("KeyboardQuestion", "KeyboardRows", "INTEGER NOT NULL DEFAULT 1").ConfigureAwait(false);
            await EnsureColumnAsync("KeyboardQuestion", "KeyboardKeysInRow", "INTEGER NOT NULL DEFAULT 10").ConfigureAwait(false);
            await EnsureColumnAsync("KeyboardQuestion", "AttemptNumber", "INTEGER NOT NULL DEFAULT 0").ConfigureAwait(false);
            await EnsureColumnAsync("KeyboardQuestion", "WasTutorialUsed", "INTEGER NOT NULL DEFAULT 0").ConfigureAwait(false);
            await EnsureColumnAsync("KeyboardQuestion", "ShowNumbersOnKeys", "INTEGER NOT NULL DEFAULT 0").ConfigureAwait(false);
            await EnsureColumnAsync("KeyboardQuestion", "QuestionPromptText", "TEXT").ConfigureAwait(false);
            await EnsureColumnAsync("KeyboardQuestion", "KeyboardWeightsJson", "TEXT").ConfigureAwait(false);
            await EnsureColumnAsync("KeyboardQuestion", "InitialKeyboardStateJson", "TEXT").ConfigureAwait(false);
            await EnsureColumnAsync("KeyboardQuestion", "QuestionKeyboardColorsJson", "TEXT").ConfigureAwait(false);
            await EnsureColumnAsync("KeyboardQuestion", "QuestionKeyboardColorsJson2", "TEXT").ConfigureAwait(false);
            await EnsureColumnAsync("KeyboardQuestion", "SubmittedKeyboardColorsJson", "TEXT").ConfigureAwait(false);
            await EnsureColumnAsync("KeyboardQuestion", "InitialKeyboardColorsJson", "TEXT").ConfigureAwait(false);
            Console.WriteLine($"Tables created successfully");
        }

        private static async Task CreateTableAsync<T>() where T : new()
        {
            try
            {
                //_database.DropTableAsync<QuestionAnswer>().Wait();
                await Instance.Database.CreateTableAsync<T>().ConfigureAwait(false);
                Console.WriteLine($"Table '{typeof(T).Name}' created successfully.");
            }
            catch (Exception ex)
            {
                // Drop the existing table
                //Database.DropTableAsync<T>().Wait();
                //Console.WriteLine($"Table '{typeof(T).Name}' dropped successfully.");

                // Recreate the table
               // Database.CreateTableAsync<T>().Wait();
                //Console.WriteLine($"Table '{typeof(T).Name}' created successfully.");
                Console.WriteLine($"table {typeof(T).Name} initialization failed: {ex.Message}");
            }
        }

        private static async Task EnsureColumnAsync(string tableName, string columnName, string columnDefinition)
        {
            try
            {
                List<SQLiteConnection.ColumnInfo> columns = await Instance.Database.GetTableInfoAsync(tableName).ConfigureAwait(false);
                if (columns.Any(column => column.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase)))
                    return;

                await Instance.Database.ExecuteAsync($"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDefinition};").ConfigureAwait(false);
                Console.WriteLine($"Column '{columnName}' added to '{tableName}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Column '{columnName}' on '{tableName}' initialization failed: {ex.Message}");
            }
        }
    }
}
