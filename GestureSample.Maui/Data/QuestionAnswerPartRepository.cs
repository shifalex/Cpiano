using GestureSample.Maui.Data.SQLite;

namespace GestureSample.Maui.Data
{
    internal class QuestionAnswerPartRepository : BaseRepository<QuestionAnswerPart>
    {
        public async Task<List<QuestionAnswerPart>> GetByGameAsync(Guid gameId)
        {
            string storedGameId = gameId.ToString();
            return await _database.Table<QuestionAnswerPart>()
                .Where(item => item.GameId == storedGameId)
                .OrderBy(item => item.QuestionNumber)
                .ThenBy(item => item.RowIndex)
                .ThenBy(item => item.ColumnIndex)
                .ToListAsync();
        }

        public async Task ReplaceForQuestionAsync(string gameId, int questionNumber, IEnumerable<QuestionAnswerPart> parts)
        {
            await _database.ExecuteAsync(
                "DELETE FROM QuestionAnswerPart WHERE GameId = ? AND QuestionNumber = ?",
                gameId,
                questionNumber);

            foreach (QuestionAnswerPart part in parts.OrderBy(item => item.RowIndex).ThenBy(item => item.ColumnIndex))
            {
                part.GameId = gameId;
                part.QuestionNumber = questionNumber;
                await _database.InsertAsync(part);
            }
        }
    }
}
