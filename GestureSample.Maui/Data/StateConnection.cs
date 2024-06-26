using GestureSample.Maui.Models;

using SQLite;
using SQLitePCL;
using System;
using System.IO;
using System.Threading.Tasks;
//using Microsoft.Data.Sqlite;
//using Microsoft.EntityFrameworkCore.Sqlite;
using System.Data.Common;
using Microsoft.Maui.Controls.PlatformConfiguration;
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

                    _database.CreateTableAsync<State>().Wait();
                    Console.WriteLine("Table created successfully.");
                }
                catch (Exception ex)
                {
                    // Log or handle the exception as needed
                    Console.WriteLine($"Database initialization failed: {ex.Message}");
                }
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
