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
            string gameId = GameId.ToString();
            return (await _database.Table<KeyEvent>().Where(state => state.GameId == gameId).ToListAsync())
                .OrderBy(state => state.EventTime)
                .ThenBy(state => state.id)
                .ToList();

        }

        public async Task<List<KeyEvent>> GetByGameIdsAsync(IEnumerable<Guid> gameIds)
        {
            string[] gameIdTexts = gameIds?
                .Select(gameId => gameId.ToString())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray() ?? Array.Empty<string>();

            if (gameIdTexts.Length == 0)
                return new List<KeyEvent>();

            string placeholders = string.Join(", ", gameIdTexts.Select(_ => "?"));
            string sql = $"SELECT * FROM KeyEvent WHERE GameId IN ({placeholders})";
            return (await _database.QueryAsync<KeyEvent>(sql, gameIdTexts.Cast<object>().ToArray()))
                .OrderBy(keyEvent => keyEvent.EventTime)
                .ThenBy(keyEvent => keyEvent.id)
                .ToList();
        }

        public async Task AssignPendingEventsToAttemptAsync(string gameId, int questionNumber, int attemptNumber)
        {
            List<KeyEvent> pendingEvents = await _database.Table<KeyEvent>()
                .Where(state => state.GameId == gameId && state.QuestionNumber == questionNumber && state.AttemptNumber == 0)
                .ToListAsync();

            foreach (KeyEvent keyEvent in pendingEvents.OrderBy(state => state.EventTime).ThenBy(state => state.id))
            {
                keyEvent.AttemptNumber = attemptNumber;
                await _database.UpdateAsync(keyEvent);
            }
        }

        public Task SaveCheckEventAsync(string gameId, int questionNumber, int attemptNumber, DateTime eventTime)
        {
            KeyEvent keyEvent = new()
            {
                GameId = gameId,
                QuestionNumber = questionNumber,
                AttemptNumber = attemptNumber,
                EventType = 2,
                KeyNumber = 0,
                Row = 0,
                EventTime = eventTime
            };

            return _database.InsertAsync(keyEvent);
        }

        public async Task ReplaceForGameAsync(string gameId, IEnumerable<KeyEvent> keyEvents)
        {
            await _database.ExecuteAsync("DELETE FROM KeyEvent WHERE GameId = ?", gameId);

            if (keyEvents == null)
                return;

            foreach (KeyEvent keyEvent in keyEvents.OrderBy(item => item.EventTime).ThenBy(item => item.id))
            {
                KeyEvent localKeyEvent = new()
                {
                    GameId = keyEvent.GameId,
                    QuestionNumber = keyEvent.QuestionNumber,
                    AttemptNumber = keyEvent.AttemptNumber,
                    EventType = keyEvent.EventType,
                    KeyNumber = keyEvent.KeyNumber,
                    Row = keyEvent.Row,
                    EventTime = keyEvent.EventTime,
                    RelativeX = keyEvent.RelativeX,
                    RelativeY = keyEvent.RelativeY
                };

                await _database.InsertAsync(localKeyEvent);
            }
        }

    }
}
