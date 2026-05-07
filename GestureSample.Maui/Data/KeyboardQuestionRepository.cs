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
            if (!selectedIdentifier.HasValue)
                return new List<KeyboardQuestion>();

            string gameId = selectedIdentifier.Value.ToString();
            return await _database.Table<KeyboardQuestion>()
                .Where(state => state.GameId == gameId)
                .ToListAsync();

        }

        public async Task<List<KeyboardQuestion>> GetByGameIdsAsync(IEnumerable<Guid> gameIds)
        {
            string[] gameIdTexts = gameIds?
                .Select(gameId => gameId.ToString())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray() ?? Array.Empty<string>();

            if (gameIdTexts.Length == 0)
                return new List<KeyboardQuestion>();

            string placeholders = string.Join(", ", gameIdTexts.Select(_ => "?"));
            string sql = $"SELECT * FROM KeyboardQuestion WHERE GameId IN ({placeholders})";
            return (await _database.QueryAsync<KeyboardQuestion>(sql, gameIdTexts.Cast<object>().ToArray()))
                .OrderBy(question => question.QuestionNumber)
                .ThenBy(question => question.AttemptNumber)
                .ThenBy(question => question.Time)
                .ThenBy(question => question.QuestionID)
                .ToList();
        }

        public async Task<KeyboardQuestion?> SaveSubmittedSnapshotAsync(string gameId, int questionNumber, bool[] submittedKeyboard, DateTime submittedTime, int resultStatus)
        {
            List<KeyboardQuestion> questions = await _database.Table<KeyboardQuestion>()
                .Where(state => state.GameId == gameId && state.QuestionNumber == questionNumber)
                .ToListAsync();

            if (questions.Count == 0)
                return null;

            int nextAttemptNumber = questions.Max(question => question.AttemptNumber) + 1;
            KeyboardQuestion pendingQuestion = questions
                .Where(question => question.AttemptNumber == 0)
                .OrderByDescending(question => question.QuestionID)
                .FirstOrDefault();

            if (pendingQuestion != null)
            {
                pendingQuestion.AttemptNumber = nextAttemptNumber;
                pendingQuestion.SubmittedKeyboard = submittedKeyboard?.ToArray();
                pendingQuestion.SubmittedTime = submittedTime;
                pendingQuestion.ResultStatus = resultStatus;
                await _database.UpdateAsync(pendingQuestion);
                return pendingQuestion;
            }

            KeyboardQuestion sourceQuestion = questions
                .OrderByDescending(question => question.QuestionID)
                .First();

            KeyboardQuestion attemptQuestion = CloneAttemptQuestion(sourceQuestion, nextAttemptNumber, submittedKeyboard, submittedTime, resultStatus, wasTutorialUsed: false);
            await _database.InsertAsync(attemptQuestion);
            return attemptQuestion;
        }

        public async Task MarkTutorialUsedAsync(string gameId, int questionNumber)
        {
            List<KeyboardQuestion> questions = await _database.Table<KeyboardQuestion>()
                .Where(state => state.GameId == gameId && state.QuestionNumber == questionNumber)
                .ToListAsync();

            if (questions.Count == 0)
                return;

            KeyboardQuestion pendingQuestion = questions
                .Where(question => question.AttemptNumber == 0)
                .OrderByDescending(question => question.QuestionID)
                .FirstOrDefault();

            if (pendingQuestion != null)
            {
                if (pendingQuestion.WasTutorialUsed)
                    return;

                pendingQuestion.WasTutorialUsed = true;
                await _database.UpdateAsync(pendingQuestion);
                return;
            }

            KeyboardQuestion sourceQuestion = questions
                .OrderByDescending(question => question.AttemptNumber)
                .ThenByDescending(question => question.QuestionID)
                .First();

            KeyboardQuestion pendingClone = CloneAttemptQuestion(
                sourceQuestion,
                attemptNumber: 0,
                submittedKeyboard: null,
                submittedTime: null,
                resultStatus: 0,
                wasTutorialUsed: true);

            await _database.InsertAsync(pendingClone);
        }

        public async Task UpdatePendingDisplayMetadataAsync(
            string gameId,
            int questionNumber,
            string? questionPromptText,
            bool showNumbersOnKeys,
            int[]? keyboardWeights,
            bool[]? initialKeyboardState,
            bool[]? questionKeyboard = null,
            int? keyboardRows = null,
            int? keyboardKeysInRow = null)
        {
            KeyboardQuestion? pendingQuestion = await _database.Table<KeyboardQuestion>()
                .Where(state => state.GameId == gameId && state.QuestionNumber == questionNumber)
                .OrderByDescending(state => state.AttemptNumber)
                .ThenByDescending(state => state.QuestionID)
                .FirstOrDefaultAsync();

            if (pendingQuestion == null)
                return;

            pendingQuestion.QuestionPromptText = string.IsNullOrWhiteSpace(questionPromptText)
                ? pendingQuestion.QuestionPromptText
                : questionPromptText;
            pendingQuestion.ShowNumbersOnKeys = showNumbersOnKeys;
            pendingQuestion.KeyboardWeights = keyboardWeights?.ToArray();
            pendingQuestion.InitialKeyboardState = initialKeyboardState?.ToArray();
            if (questionKeyboard != null && questionKeyboard.Length > 0)
                pendingQuestion.keyboard1 = questionKeyboard.ToArray();
            if (keyboardRows.HasValue && keyboardRows.Value > 0)
                pendingQuestion.KeyboardRows = keyboardRows.Value;
            if (keyboardKeysInRow.HasValue && keyboardKeysInRow.Value > 0)
                pendingQuestion.KeyboardKeysInRow = keyboardKeysInRow.Value;

            await _database.UpdateAsync(pendingQuestion);
        }

        public async Task ReplaceForGameAsync(string gameId, IEnumerable<KeyboardQuestion> questions)
        {
            await _database.ExecuteAsync("DELETE FROM KeyboardQuestion WHERE GameId = ?", gameId);

            if (questions == null)
                return;

            foreach (KeyboardQuestion question in questions.OrderBy(item => item.QuestionNumber).ThenBy(item => item.AttemptNumber).ThenBy(item => item.Time))
            {
                KeyboardQuestion localQuestion = CloneAttemptQuestion(
                    question,
                    question.AttemptNumber,
                    question.SubmittedKeyboard,
                    question.SubmittedTime,
                    question.ResultStatus,
                    question.WasTutorialUsed);

                localQuestion.QuestionID = 0;
                localQuestion.Time = question.Time;
                localQuestion.UserId = question.UserId;
                localQuestion.aboveNumber = question.aboveNumber;
                localQuestion.length = question.length;
                localQuestion.MoveByLength = question.MoveByLength;
                localQuestion.KeyboardRows = question.KeyboardRows;
                localQuestion.KeyboardKeysInRow = question.KeyboardKeysInRow;
                localQuestion.ShowNumbersOnKeys = question.ShowNumbersOnKeys;
                localQuestion.QuestionPromptText = question.QuestionPromptText;
                localQuestion.keyboard1 = question.keyboard1?.ToArray();
                localQuestion.keyboard2 = question.keyboard2?.ToArray();
                localQuestion.dir = question.dir;
                localQuestion.MoveByDirection = question.MoveByDirection;
                localQuestion.Op = question.Op;
                localQuestion.KeyboardWeights = question.KeyboardWeights?.ToArray();
                localQuestion.InitialKeyboardState = question.InitialKeyboardState?.ToArray();

                await _database.InsertAsync(localQuestion);
            }
        }

        private static KeyboardQuestion CloneAttemptQuestion(
            KeyboardQuestion sourceQuestion,
            int attemptNumber,
            bool[] submittedKeyboard,
            DateTime? submittedTime,
            int resultStatus,
            bool wasTutorialUsed)
        {
            return new KeyboardQuestion
            {
                QuestionNumber = sourceQuestion.QuestionNumber,
                AttemptNumber = attemptNumber,
                GameId = sourceQuestion.GameId,
                Time = sourceQuestion.Time,
                UserId = sourceQuestion.UserId,
                ResultStatus = resultStatus,
                WasTutorialUsed = wasTutorialUsed,
                aboveNumber = sourceQuestion.aboveNumber,
                length = sourceQuestion.length,
                MoveByLength = sourceQuestion.MoveByLength,
                KeyboardRows = sourceQuestion.KeyboardRows,
                KeyboardKeysInRow = sourceQuestion.KeyboardKeysInRow,
                ShowNumbersOnKeys = sourceQuestion.ShowNumbersOnKeys,
                QuestionPromptText = sourceQuestion.QuestionPromptText,
                keyboard1 = sourceQuestion.keyboard1?.ToArray(),
                keyboard2 = sourceQuestion.keyboard2?.ToArray(),
                dir = sourceQuestion.dir,
                MoveByDirection = sourceQuestion.MoveByDirection,
                Op = sourceQuestion.Op,
                KeyboardWeights = sourceQuestion.KeyboardWeights?.ToArray(),
                InitialKeyboardState = sourceQuestion.InitialKeyboardState?.ToArray(),
                SubmittedKeyboard = submittedKeyboard?.ToArray(),
                SubmittedTime = submittedTime
            };
        }

    }
}
