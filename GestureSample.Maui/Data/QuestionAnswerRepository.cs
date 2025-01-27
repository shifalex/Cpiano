using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureSample.Maui.Data
{
    internal class QuestionAnswerRepository : BaseRepository<QuestionAnswer>
    {
        public QuestionAnswerRepository() { }

        public async Task<List<QuestionAnswer>> GetAnswersByQueryAsync(string GameId)
        {
            //return await _database.QueryAsync<QuestionAnswer>("SELECT * FROM QuestionAnswer WHERE GameId = '{0}'", GameId);
            return await _database.Table<QuestionAnswer>().Where(state => state.GameId == GameId).ToListAsync();

        }
    }
}
