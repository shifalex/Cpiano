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

    }
}
