using GestureSample.Maui.Data.SQLite;
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

        public async Task<List<QuestionAnswer>> GetAnswersByQueryAsync(Guid GameId)
        {
            string gID = GameId.ToString();
            //return await _database.QueryAsync<QuestionAnswer>("SELECT * FROM QuestionAnswer WHERE GameId = '{0}'", GameId);
            return await _database.Table<QuestionAnswer>().Where(state => state.GameId == gID).ToListAsync();

        }

        public async Task<List<QuestionAnswer>> GetByGameIdsAsync(IEnumerable<Guid> gameIds)
        {
            string[] gameIdTexts = gameIds?
                .Select(gameId => gameId.ToString())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray() ?? Array.Empty<string>();

            if (gameIdTexts.Length == 0)
                return new List<QuestionAnswer>();

            string placeholders = string.Join(", ", gameIdTexts.Select(_ => "?"));
            string sql = $"SELECT * FROM QuestionAnswer WHERE GameId IN ({placeholders})";
            return (await _database.QueryAsync<QuestionAnswer>(sql, gameIdTexts.Cast<object>().ToArray()))
                .OrderBy(answer => answer.Time)
                .ThenBy(answer => answer.QuestionID)
                .ToList();
        }

        public async Task UpdateSecondaryPpwAsync(
            string gameId,
            int questionNumber,
            int secondaryAddend1,
            int secondaryAddend2,
            int secondarySum,
            bool secondaryAddend1Enabled,
            bool secondaryAddend2Enabled,
            bool secondarySumEnabled)
        {
            QuestionAnswer? question = await _database.Table<QuestionAnswer>()
                .Where(state => state.GameId == gameId && state.QuestionNumber == questionNumber)
                .OrderByDescending(state => state.QuestionID)
                .FirstOrDefaultAsync();

            if (question == null)
                return;

            question.SecondaryAddend1 = secondaryAddend1;
            question.SecondaryAddend2 = secondaryAddend2;
            question.SecondarySum = secondarySum;
            question.SecondaryAddend1Enabled = secondaryAddend1Enabled;
            question.SecondaryAddend2Enabled = secondaryAddend2Enabled;
            question.SecondarySumEnabled = secondarySumEnabled;
            await _database.UpdateAsync(question);
        }
    }
}
