using GestureSample.Maui.Data.SQLite;

namespace GestureSample.Maui.Data
{
    internal class QuestionAnswerPartRepository : BaseRepository<QuestionAnswerPart>
    {
        public async Task<List<QuestionAnswerPart>> GetByGameAsync(Guid gameId)
        {
            string storedGameId = gameId.ToString();
            return await _database.Table<QuestionAnswerPart>()
                .Where(item => item.GameId == storedGameId && item.PartKind == "Visible")
                .OrderBy(item => item.QuestionNumber)
                .ThenBy(item => item.RowIndex)
                .ThenBy(item => item.ColumnIndex)
                .ToListAsync();
        }

        public async Task<List<QuestionAnswerPart>> GetByGameIdsAsync(IEnumerable<Guid> gameIds)
        {
            string[] gameIdTexts = gameIds?
                .Select(gameId => gameId.ToString())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray() ?? Array.Empty<string>();

            if (gameIdTexts.Length == 0)
                return new List<QuestionAnswerPart>();

            string placeholders = string.Join(", ", gameIdTexts.Select(_ => "?"));
            string sql = $"SELECT * FROM QuestionAnswerPart WHERE GameId IN ({placeholders})";
            return (await _database.QueryAsync<QuestionAnswerPart>(sql, gameIdTexts.Cast<object>().ToArray()))
                .OrderBy(item => item.QuestionNumber)
                .ThenBy(item => item.AttemptNumber)
                .ThenBy(item => item.RecordedAt)
                .ThenBy(item => item.Id)
                .ToList();
        }

        public async Task ReplaceForQuestionAsync(string gameId, int questionNumber, IEnumerable<QuestionAnswerPart> parts)
        {
            await _database.ExecuteAsync(
                "DELETE FROM QuestionAnswerPart WHERE GameId = ? AND QuestionNumber = ? AND PartKind = 'Visible'",
                gameId,
                questionNumber);

            foreach (QuestionAnswerPart part in parts.OrderBy(item => item.RowIndex).ThenBy(item => item.ColumnIndex))
            {
                part.GameId = gameId;
                part.QuestionNumber = questionNumber;
                part.PartKind = "Visible";
                part.RecordedAt = DateTime.Now;
                await _database.InsertAsync(part);
            }
        }

        public async Task AddComplexArrowAttemptAsync(
            string gameId,
            int questionNumber,
            int attemptNumber,
            string entryName,
            int columnIndex,
            string valueText,
            bool isCorrect)
        {
            await _database.InsertAsync(new QuestionAnswerPart
            {
                GameId = gameId,
                QuestionNumber = questionNumber,
                RowIndex = attemptNumber,
                ColumnIndex = columnIndex,
                ValueText = valueText,
                IsEnabled = true,
                PartKind = "Attempt",
                EntryName = entryName,
                AttemptNumber = attemptNumber,
                IsCorrect = isCorrect,
                RecordedAt = DateTime.Now
            });
        }
    }
}
