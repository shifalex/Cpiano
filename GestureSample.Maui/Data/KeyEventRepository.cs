using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureSample.Maui.Data
{
    internal class KeyEventRepository : BaseRepository<QuestionAnswer>
    {
        public KeyEventRepository(SQLiteAsyncConnection database) : base(database) { }

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

    }
}
