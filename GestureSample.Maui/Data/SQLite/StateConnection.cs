using SQLite;
using System.Data.Common;

//using Microsoft.Data.Sqlite;
//using Microsoft.EntityFrameworkCore.Sqlite;
using System.Net.Http.Headers;
using static SQLite.SQLiteConnection;

namespace GestureSample.Maui.Data.SQLite
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
            await CreateTableAsync<QuestionAnswerPart>().ConfigureAwait(false);
            await CreateTableAsync<KeyboardQuestion>().ConfigureAwait(false);
            await CreateTableAsync<Game>().ConfigureAwait(false);
            await CreateTableAsync<KeyEvent>().ConfigureAwait(false);
            await CreateTableAsync<TimerChangeEvent>().ConfigureAwait(false);
            await CreateTableAsync<VisibilityChangeEvent>().ConfigureAwait(false);
            await CreateTableAsync<CustomStageDefinition>().ConfigureAwait(false);
            await CreateTableAsync<CustomStageFlowDefinition>().ConfigureAwait(false);
            await CreateTableAsync<User>().ConfigureAwait(false);
            await EnsureColumnAsync("Game", "WasSynced", "INTEGER NOT NULL DEFAULT 0").ConfigureAwait(false);
            await EnsureColumnAsync("QuestionAnswer", "SecondarySum", "INTEGER NOT NULL DEFAULT -1111").ConfigureAwait(false);
            await EnsureColumnAsync("QuestionAnswer", "SecondaryAddend1", "INTEGER NOT NULL DEFAULT -1111").ConfigureAwait(false);
            await EnsureColumnAsync("QuestionAnswer", "SecondaryAddend2", "INTEGER NOT NULL DEFAULT -1111").ConfigureAwait(false);
            await EnsureColumnAsync("QuestionAnswer", "SecondarySumEnabled", "INTEGER NOT NULL DEFAULT 0").ConfigureAwait(false);
            await EnsureColumnAsync("QuestionAnswer", "SecondaryAddend1Enabled", "INTEGER NOT NULL DEFAULT 0").ConfigureAwait(false);
            await EnsureColumnAsync("QuestionAnswer", "SecondaryAddend2Enabled", "INTEGER NOT NULL DEFAULT 0").ConfigureAwait(false);
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
            await EnsureColumnAsync("KeyboardQuestion", "WasHeaderResultToggleUsed", "INTEGER NOT NULL DEFAULT 0").ConfigureAwait(false);
            await EnsureColumnAsync("KeyboardQuestion", "ShowNumbersOnKeys", "INTEGER NOT NULL DEFAULT 0").ConfigureAwait(false);
            await EnsureColumnAsync("KeyboardQuestion", "QuestionPromptText", "TEXT").ConfigureAwait(false);
            await EnsureColumnAsync("KeyboardQuestion", "KeyboardWeightsJson", "TEXT").ConfigureAwait(false);
            await EnsureColumnAsync("KeyboardQuestion", "InitialKeyboardStateJson", "TEXT").ConfigureAwait(false);
            await EnsureColumnAsync("KeyboardQuestion", "QuestionKeyboardColorsJson", "TEXT").ConfigureAwait(false);
            await EnsureColumnAsync("KeyboardQuestion", "QuestionKeyboardColorsJson2", "TEXT").ConfigureAwait(false);
            await EnsureColumnAsync("KeyboardQuestion", "SubmittedKeyboardColorsJson", "TEXT").ConfigureAwait(false);
            await EnsureColumnAsync("KeyboardQuestion", "InitialKeyboardColorsJson", "TEXT").ConfigureAwait(false);
            Console.WriteLine($"Tables created successfully");
            //await MigrateAndResetWasSyncedAsync();
        }

        private  async Task CreateTableAsync<T>() where T : new()
        {
            try
            {
                //_database.DropTableAsync<QuestionAnswer>().Wait();
                await Database.CreateTableAsync<T>().ConfigureAwait(false);
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

        private async Task EnsureColumnAsync(string tableName, string columnName, string columnDefinition)
        {
            try
            {
                List<ColumnInfo> columns = await Database.GetTableInfoAsync(tableName).ConfigureAwait(false);
                if (columns.Any(column => column.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase)))
                    return;

                await Database.ExecuteAsync($"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDefinition};").ConfigureAwait(false);
                Console.WriteLine($"Column '{columnName}' added to '{tableName}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Column '{columnName}' on '{tableName}' initialization failed: {ex.Message}");
            }
        }

        public async Task MigrateAndResetWasSyncedAsync()
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            try
            {
                // Check if the WasSynced column exists by querying PRAGMA table_info.

                Console.WriteLine("Column 'WasSynced' adding... 1");
                //var columns = await connection.QueryAsync<ColumnInfo>("PRAGMA table_info(Game);");
                Console.WriteLine("Column 'WasSynced' adding... 2");
                //bool columnExists = columns.Any(c => c.Name.Equals("WasSynced", StringComparison.InvariantCultureIgnoreCase));
                Console.WriteLine("Column 'WasSynced' adding... 3");

                // Add the column since it doesn't exist.
                await connection.ExecuteAsync("ALTER TABLE Game ADD COLUMN WasSynced BOOLEAN NOT NULL DEFAULT 0;");
                Console.WriteLine("Column 'WasSynced' added successfully.");
                // Update all rows so that WasSynced is false (0)
                await connection.ExecuteAsync("UPDATE Game SET WasSynced = 0;");
                Console.WriteLine("Updated all 'Game' records: WasSynced set to false (0).");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during migration: {ex.Message}");
            }
        }
    }
}
