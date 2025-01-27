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


    }
}
