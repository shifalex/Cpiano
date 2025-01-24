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
        public QuestionAnswerRepository(SQLiteAsyncConnection database) : base(database) { }

        public Task<List<QuestionAnswer>> GetQuestionAnswerByGameIdAsync(string gameId)
        {
            return _database.Table<QuestionAnswer>().Where(q => q.GameId == gameId).ToListAsync();
        }

        public Task<int> SaveAnswerAsync(QuestionAnswer answer)
        {
            return _database.InsertAsync(answer);
        }


        public Task<List<QuestionAnswer>> GetAnswersAsync()
        {
            return _database.Table<QuestionAnswer>().ToListAsync();
        }
        public async Task<List<QuestionAnswer>> GetAnswersByQueryAsync(string GameId)
        {
            //return await _database.QueryAsync<QuestionAnswer>("SELECT * FROM QuestionAnswer WHERE GameId = '{0}'", GameId);
            return await _database.Table<QuestionAnswer>().Where(state => state.GameId == GameId).ToListAsync();

        }
    }
}
