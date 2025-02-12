using GestureSample.Maui.Data.SQLite;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureSample.Maui.Data
{
    internal class KeyboardQuestionRepository : BaseRepository<KeyboardQuestion>
    {
        public KeyboardQuestionRepository() :base() { }

        public async Task<List<KeyboardQuestion>> GetKeyboardQuestionByQueryAsync(Guid? selectedIdentifier)
        {
            return await _database.Table<KeyboardQuestion>().Where(state => state.GameId == selectedIdentifier.ToString()).ToListAsync();

        }

    }
}
