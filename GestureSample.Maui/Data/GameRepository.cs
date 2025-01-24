using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureSample.Maui.Data
{
    internal class GameRepository : BaseRepository<QuestionAnswer>
    {
        public GameRepository(SQLiteAsyncConnection database) : base(database) { }

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

    }
}
