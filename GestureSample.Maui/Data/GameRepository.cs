using GestureSample.Maui.Data.SQLite;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureSample.Maui.Data
{
    internal class GameRepository : BaseRepository<Game>
    {
        public GameRepository() : base() { }

        public new Task<List<Game>> GetAllAsync()
        {
            return _database.Table<Game>().OrderBy(game => game.TimeStart).ToListAsync();
        }

        public Task<List<Game>> GetAllByUserAsync(Guid? userID)
        {
            if (userID == null)
            {
                return Task.FromResult<List<Game>>(null);
            }

            //string userIdString = userID.Value.ToString("D"); // Use the "D" format for consistent GUID string representation
            return _database.Table<Game>()
                .Where(game => game.UserId == (Guid)userID)
                .OrderBy(game => game.TimeStart)
                .ToListAsync();
        }

        public class GameNameResult
        {
            public string GameName { get; set; }
        }

        public async Task<List<Game>> GetRecordsByGameNamesAsync(Guid? userID, string gameName)
        {
            string query = String.Format("SELECT * FROM Game Where GameName='{0}' and FinalStatus=1", gameName);
            Console.WriteLine(query);
            var results = await _database.QueryAsync<Game>(query
 );/*WHERE UserID='{0}'*/
            return results.OrderBy(g => g.TimeEnd.Subtract(g.TimeStart)).Take<Game>(20).ToList();

        }

        public async Task<List<string>> GetDistinctGameNamesAsync(Guid? userID)
        {
            // SELECT DISTINCT with an alias matching the property name in your class
            var results = await _database.QueryAsync<GameNameResult>(
    "SELECT DISTINCT GameName FROM Game ", userID?.ToString());/*WHERE UserID='{0}'*/


            // Extract the string property from your wrapper
            return results.Select(r => r.GameName).OrderBy(r => r).ToList();
        }

        public async Task UpdateAllToNotSynced()
        {
            var results = await _database.QueryAsync<GameNameResult>(
    "UPDATE Game SET WasSynced=0");


        }

        internal async Task UpdateAsync(List<Game> unsyncedGames)
        {
            //TODO: run more synchroniusly
            //TODO: sync progress bar
            foreach(Game g in unsyncedGames) { await _database.UpdateAsync(g); }
        }
    }
}
