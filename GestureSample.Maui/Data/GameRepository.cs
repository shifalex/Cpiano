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

        internal async Task UpdateAsync(List<Game> unsyncedGames)
        {
            //TODO: run more synchroniusly
            //TODO: sync progress bar
            foreach(Game g in unsyncedGames) { await _database.UpdateAsync(g); }
        }
    }
}
