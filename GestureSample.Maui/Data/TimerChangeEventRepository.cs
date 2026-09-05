using GestureSample.Maui.Data.SQLite;

namespace GestureSample.Maui.Data
{
    public class TimerChangeEventRepository : BaseRepository<TimerChangeEvent>
    {
        public TimerChangeEventRepository()
        {
        }

        public Task<List<TimerChangeEvent>> GetByGameAsync(Guid gameId)
        {
            string gameIdString = gameId.ToString();
            return _database.Table<TimerChangeEvent>()
                .Where(item => item.GameId == gameIdString)
                .OrderBy(item => item.EventTime)
                .ThenBy(item => item.Id)
                .ToListAsync();
        }

        public async Task EnsureInitialEventAsync(string gameId, int initialSetting, DateTime eventTime, string source)
        {
            if (string.IsNullOrWhiteSpace(gameId))
                return;

            int existingCount = await _database.Table<TimerChangeEvent>()
                .Where(item => item.GameId == gameId && item.QuestionNumber == 1)
                .CountAsync();

            if (existingCount > 0)
                return;

            await SaveAsync(new TimerChangeEvent
            {
                GameId = gameId,
                QuestionNumber = 1,
                EventTime = eventTime,
                OldSetting = initialSetting,
                NewSetting = initialSetting,
                Source = source
            });
        }

        public async Task EnsureInitialEventsAsync(IEnumerable<Game> games)
        {
            foreach (Game game in games ?? Enumerable.Empty<Game>())
            {
                int? initialSetting = game.Config?.KeyboardConfig?.SecondsPressingToAnswer;
                if (!initialSetting.HasValue)
                    continue;

                await EnsureInitialEventAsync(
                    game.Id.ToString(),
                    initialSetting.Value,
                    game.TimeStart,
                    "InitialConfigBackfill");
            }
        }
    }
}
