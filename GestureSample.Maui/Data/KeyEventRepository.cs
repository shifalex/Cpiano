using GestureSample.Maui.Data.SQLite;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureSample.Maui.Data
{
    internal class KeyEventRepository : BaseRepository<KeyEvent>
    {
        public KeyEventRepository() { }

        public async Task<List<KeyEvent>> GetKeyEventsByQueryAsync(Guid GameId)
        {
            //return await _database.QueryAsync<QuestionAnswer>("SELECT * FROM KeyEvent WHERE GameId = '{0}'", GameId);
            return await _database.Table<KeyEvent>().Where(state => state.GameId == GameId.ToString()).ToListAsync();

        }

    }
}
