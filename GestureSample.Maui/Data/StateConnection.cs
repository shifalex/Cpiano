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
                _dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MathOPiano.db");

#if WINDOWS || IOS
                _dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MathOPiano.db");
#endif
                Console.WriteLine($"Database path: {_dbPath}");

                _database = new SQLiteAsyncConnection(_dbPath);
                Console.WriteLine("Database created successfully.");
                try
                {
                    //_database.DropTableAsync<QuestionAnswer>().Wait();
                    _database.CreateTableAsync<QuestionAnswer>().Wait();
                    Console.WriteLine($"Table '{typeof(QuestionAnswer).Name}' created successfully.");
                }
                catch (Exception ex) {
                    // Drop the existing table
                    _database.DropTableAsync<QuestionAnswer>().Wait();
                    Console.WriteLine($"Table '{typeof(QuestionAnswer).Name}' dropped successfully.");

                    // Recreate the table
                    _database.CreateTableAsync<QuestionAnswer>().Wait();
                    Console.WriteLine($"Table '{typeof(QuestionAnswer).Name}' created successfully.");
                    Console.WriteLine($"Database initialization failed: {ex.Message}");
                }
            }
            try { 
                    _database.CreateTableAsync<Game>().Wait();
                    Console.WriteLine($"Table '{typeof(Game).Name}' created successfully.");
                }
                catch (Exception ex)
                {
                // Drop the existing table
                _database.DropTableAsync<Game>();
                Console.WriteLine($"Table '{typeof(Game).Name}' dropped successfully.");

                // Recreate the table
                _database.CreateTableAsync<Game>();
                Console.WriteLine($"Table '{typeof(Game).Name}' created successfully.");
                    // Log or handle the exception as needed
                    Console.WriteLine($"Database initialization failed: {ex.Message}");
                }
            try
            {
                _database.CreateTableAsync<KeyEvent>().Wait();
                Console.WriteLine($"Table '{typeof(KeyEvent).Name}' created successfully.");
            }
            catch (Exception ex)
            {
                // Drop the existing table
                _database.DropTableAsync<KeyEvent>();
                Console.WriteLine($"Table '{typeof(KeyEvent).Name}' dropped successfully.");

                // Recreate the table
                _database.CreateTableAsync<KeyEvent>();
                Console.WriteLine($"Table '{typeof(KeyEvent).Name}' created successfully.");
                // Log or handle the exception as needed
                Console.WriteLine($"Database initialization failed: {ex.Message}");
            }
        }
        

        public Task<int> SaveStateAsync(QuestionAnswer state)
        {
            return _database.InsertAsync(state);
        }

        public Task<List<QuestionAnswer>> GetStatesAsync()
        {
            return _database.Table<QuestionAnswer>().ToListAsync();
        }
        public async Task<List<QuestionAnswer>> GetStatesByQueryAsync(string GameId)
        {
            //return await _database.QueryAsync<QuestionAnswer>("SELECT * FROM QuestionAnswer WHERE GameId = '{0}'", GameId);
            return await _database.Table<QuestionAnswer>().Where(state => state.GameId==GameId).ToListAsync();

        }
        public Task<int> SaveKeyEventAsync(KeyEvent kevent)
        {
            return _database.InsertAsync(kevent);
        }

        public Task<List<KeyEvent>> GetKeyEventsAsync()
        {
            return _database.Table<KeyEvent>().ToListAsync();
        }
        public async Task<List<KeyEvent>> GetKeyEventsByQueryAsync(string GameId)
        {
            //return await _database.QueryAsync<QuestionAnswer>("SELECT * FROM KeyEvent WHERE GameId = '{0}'", GameId);
            return await _database.Table<KeyEvent>().Where(state => state.GameId == GameId).ToListAsync();

        }

        public Task<int> SaveKeyboardQuestionAsync(KeyboardQuestion kQuestion)
        {
            return _database.InsertAsync(kQuestion);
        }

        public Task<int> SaveGameAsync(Game game)
        {
            return _database.InsertAsync(game);
        }

        public Task<List<Game>> GetGamesAsync()
        {
            return _database.Table<Game>().OrderBy(game => game.TimeStart).ToListAsync();
        }

        public async Task<int> UpdateGameAsync(Game game)
        {
            try
            {
                return await _database.UpdateAsync(game);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating game: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> UpdateStateAsync(QuestionAnswer state)
        {
            try
            {
                return await _database.UpdateAsync(state);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating game: {ex.Message}");
                return 0;
            }
        }

        public async Task UploadDatabaseAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                    var uri = new Uri("https://mathopiano.herokuapp.com/upload");

                    using (var form = new MultipartFormDataContent())
                    {
                        var fileContent = new StreamContent(File.OpenRead(_dbPath));
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                        form.Add(fileContent, "file", "MathOPiano.db3");

                        var response = await client.PostAsync(uri, form);

                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine("Database uploaded successfully.");
                        }
                        else
                        {
                            Console.WriteLine($"Failed to upload database. Status code: {response.StatusCode}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading database: {ex.Message}");
            }
        }

        internal async Task Execute(String v)
        {
            await _database.ExecuteAsync(v);
            //throw new NotImplementedException();
        }

        /*

        string _dbPath;
        private DbConnection conn;
        public DbSet<QuestionAnswer> States { get; set; }
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
            conn.CreateTable<QuestionAnswer>();
        }

        public List<QuestionAnswer> GetStates()
        {
            Init();
            return conn.Table<QuestionAnswer>().ToList();
        }

        public void Add(QuestionAnswer s)
        {
            Init();
            conn.Insert(s);
        }*/
    }
}
