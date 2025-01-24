using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureSample.Maui.Data
{
    internal class KeyboardQuestionRepository : BaseRepository<QuestionAnswer>
    {
        public KeyboardQuestionRepository(SQLiteAsyncConnection database) : base(database) { }

        public Task<int> SaveKeyboardQuestionAsync(KeyboardQuestion kQuestion)
        {
            return _database.InsertAsync(kQuestion);
        }

        public async Task<List<KeyboardQuestion>> GetKeyboardQuestionByQueryAsync(string selectedIdentifier)
        {
            return await _database.Table<KeyboardQuestion>().Where(state => state.GameId == selectedIdentifier).ToListAsync();

        }

    }
}
