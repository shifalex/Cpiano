using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SupabaseClient = Supabase.Client;
using Supabase.Postgrest.Models;
using SQLite = GestureSample.Maui.Data.SQLite;
using SupaBase = GestureSample.Maui.Data.SupaBase;

namespace GestureSample.Maui.Data.SupaBase
{
    public static class SupabaseService
    {
        private static readonly SupabaseClient _supabase = new(
            "https://njsspracfpbyozvandph.supabase.co",
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im5qc3NwcmFjZnBieW96dmFuZHBoIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzYwMTg5MzcsImV4cCI6MjA1MTU5NDkzN30.yrk-QUINVC1rR4km1dO0X5OaMEdZbmGUGtgExTcxOiA"
        );

        #region Logging Helpers

        private static void LogInfo(string message)
        {
            Console.WriteLine($"[INFO] {DateTime.Now}: {message}");
        }

        private static void LogError(string message, Exception ex)
        {
            Console.WriteLine($"[ERROR] {DateTime.Now}: {message} - Exception: {ex.Message}");
        }

        #endregion

        /// <summary>
        /// Syncs all data related to the current user.
        /// 
        /// New Requirements:
        /// - If the user already exists (UserId exists), do not sync its games.
        /// - For tables that rely on GameId, if the game record exists, do not sync (insert/update) that data.
        /// </summary>
        public static async Task SyncUserDataAsync(SQLite.User user)
        {
            if (user == null)
            {
                LogInfo("SyncUserDataAsync called with null user.");
                return;
            }

            try
            {
               
                LogInfo("Starting SyncGamesAsync.");
                await SyncUnsyncedGamesAndRelatedDataAsync(user);
                //Sync the user record first and capture whether the user is new.
                
                LogInfo("Starting SyncUserDataAsync.");
                bool isNewUser = await SyncUserAsync(user);
                
                
                // Only sync games if the user is new.
                /*if (isNewUser)
                {
                    LogInfo("User is new, syncing games.");
                }
                else
                {
                    LogInfo("User already exists, skipping game sync.");
                }

                // Sync other data concurrently.
                await Task.WhenAll(
                    SyncKeyboardQuestionsAsync(user),
                    SyncKeyboardAnswersAsync(user),
                    SyncKeyEventsAsync(user)
                );
                */
                LogInfo("Completed SyncUserDataAsync successfully.");
            }
            catch (Exception ex)
            {
                LogError("Error in SyncUserDataAsync", ex);
            }
        }

        /// <summary>
        /// Syncs the user record.
        /// Returns true if the user was newly inserted; false if the user already existed.
        /// </summary>
        private static async Task<bool> SyncUserAsync(SQLite.User user)
        {
            try
            {
                LogInfo("Starting SyncUserAsync.");

                // Check if the user exists remotely.
                var existingResponse = await _supabase
                    .From<User>()
                    .Where(u => u.Id == user.Id)
                    .Get();

                bool isNewUser = existingResponse.Models == null || !existingResponse.Models.Any();

                LogInfo(string.Format( "isNewUser? {0}", isNewUser));

                // Use Upsert so that if the record exists it will be updated,
                // and if not, it will be inserted.
                var SupabaseUser = ConvertFrom<SQLite.User, SupaBase.User>(user);
                await _supabase.From<User>().Upsert(SupabaseUser);
                LogInfo("User record upserted.");

                LogInfo("Completed SyncUserAsync.");
                return isNewUser;
            }
            catch (Exception ex)
            {
                LogError("Error in SyncUserAsync", ex);
                throw;
            }
        }

        /// <summary>
        /// Main sync method that upserts unsynced local games and inserts related QuestionAnswer records.
        /// </summary>
        public static async Task SyncUnsyncedGamesAndRelatedDataAsync(SQLite.User user)
        {
            try
            {
                LogInfo("Starting SyncUnsyncedGamesAndRelatedDataAsync.");

                // 1. Fetch unsynced local games for the user.
                var unsyncedGames = await GetLocalUnsyncedGamesAsync(user);
                if (!unsyncedGames.Any())
                {
                    LogInfo("No unsynced local games found.");
                    return;
                }

                // 2. Convert local games (SQLite.Game) to remote games (SupaBase.Game) using the ConvertFrom method.
                var remoteGameBatch = unsyncedGames
                    .Select(localGame => ConvertFrom<SQLite.Game, SupaBase.Game>(localGame))
                    .ToList();

                // 3. Batch upsert all remote games.
                // Note: Batch upsert minimizes the number of HTTP calls.
                await _supabase.From<SupaBase.Game>().Upsert(remoteGameBatch);
                LogInfo($"Upserted {remoteGameBatch.Count} games to SupaBase.");

                // 4. Insert related QuestionAnswer records for each newly upserted game.
                var newGameIds = unsyncedGames.Select(g => g.Id).ToList();
                await InsertQuestionAnswersForNewGamesAsync(newGameIds);

                LogInfo("Finished SyncUnsyncedGamesAndRelatedDataAsync successfully.");

                // 5. Mark local games as synced.
                foreach (var localGame in unsyncedGames)
                {
                    localGame.WasSynced = true;
                    LogInfo("Chaged Asynced status synced.");
                }
                var gameRepo = ServiceHelper.GetService<GameRepository>();
                await gameRepo.UpdateAsync(unsyncedGames);
                LogInfo("Marked local games as synced.");

            }
            catch (Exception ex)
            {
                LogError("Unexpected error in SyncUnsyncedGamesAndRelatedDataAsync", ex);
                throw;
            }
        }

        /// <summary>
        /// Retrieves unsynced local games for the given user.
        /// </summary>
        private static async Task<List<SQLite.Game>> GetLocalUnsyncedGamesAsync(SQLite.User user)
        {
            var gameRepo = ServiceHelper.GetService<GameRepository>();
            var allGames = await gameRepo.GetAllByUserAsync(user.Id);
            //return allGames;
            // Filter by WasSynced == false.
            var unsynced = allGames.Where(g => g.WasSynced == false && g.UserId == user.Id).ToList();
            LogInfo($"Found {unsynced.Count} unsynced local games for user {user.Id}.");
            return unsynced;
        }

       /* /// <summary>
        /// Syncs Keyboard Questions.
        /// </summary>
        private static async Task SyncKeyboardQuestionsAsync(SQLite.User user)
        {
            try
            {
                LogInfo("Starting SyncKeyboardQuestionsAsync.");
                var repo = ServiceHelper.GetService<KeyboardQuestionRepository>();
                var localQuestions = await repo.GetAllAsync();

                // Additional filtering can be applied here.
                var userQuestions = localQuestions
                    .Where(q => q.UserId == user.Id && IsQuestionEligibleForSync(q))
                    .ToList();

                var response = await _supabase.From<KeyboardQuestion>().Get();
                var remoteQuestions = response.Models;

                await SyncEntitiesAsync(
                    userQuestions,
                    remoteQuestions,
                    keySelector: q => q.QuestionID,
                    matchBuilder: q => new { q.QuestionID },
                    skipIfExists: false
                );

                LogInfo("Completed SyncKeyboardQuestionsAsync.");
            }
            catch (Exception ex)
            {
                LogError("Error in SyncKeyboardQuestionsAsync", ex);
            }
        }

        /// <summary>
        /// Syncs Keyboard Answers.
        /// Since they are keyed by GameId, if a record exists, we skip syncing.
        /// </summary>
        private static async Task SyncKeyboardAnswersAsync(User user)
        {
            try
            {
                LogInfo("Starting SyncKeyboardAnswersAsync.");
                var repo = ServiceHelper.GetService<KeyboardAnswerRepository>();
                var localAnswers = await repo.GetAllAsync();

                // Additional filtering: Only sync answers that meet your criteria.
                var userAnswers = localAnswers
                    .Where(a => a.GameId == user.Id.ToString() && IsAnswerEligibleForSync(a))
                    .ToList();

                var response = await _supabase.From<KeyboardAnswer>().Get();
                var remoteAnswers = response.Models;

                await SyncEntitiesAsync(
                    userAnswers,
                    remoteAnswers,
                    keySelector: a => a.GameId,
                    matchBuilder: a => new { a.GameId },
                    skipIfExists: true
                );

                LogInfo("Completed SyncKeyboardAnswersAsync.");
            }
            catch (Exception ex)
            {
                LogError("Error in SyncKeyboardAnswersAsync", ex);
            }
        }

        /// <summary>
        /// Syncs Key Events.
        /// These are keyed by GameId; if a record exists, we skip syncing.
        /// </summary>
        private static async Task SyncKeyEventsAsync(User user)
        {
            try
            {
                LogInfo("Starting SyncKeyEventsAsync.");
                var repo = ServiceHelper.GetService<KeyEventRepository>();
                var localEvents = await repo.GetAllAsync();

                // Additional filtering: e.g., only sync events with valid timestamps.
                var userEvents = localEvents
                    .Where(e => e.GameId == user.Id.ToString() && IsKeyEventEligibleForSync(e))
                    .ToList();

                var response = await _supabase.From<KeyEvent>().Get();
                var remoteEvents = response.Models;

                await SyncEntitiesAsync(
                    userEvents,
                    remoteEvents,
                    keySelector: e => e.id,
                    matchBuilder: e => new { e.id },
                    skipIfExists: true
                );

                LogInfo("Completed SyncKeyEventsAsync.");
            }
            catch (Exception ex)
            {
                LogError("Error in SyncKeyEventsAsync", ex);
            }
        }*/

        /// <summary>
        /// Syncs Question Answers.
        /// </summary>
        private static async Task InsertQuestionAnswersForNewGamesAsync(List<Guid> gameIds)
        {
            var qaRepo = ServiceHelper.GetService<QuestionAnswerRepository>();
            foreach (var gameId in gameIds)
            {
                try
                {
                    var localQAs = await qaRepo.GetAnswersByQueryAsync(gameId);
                    if (localQAs.Any())
                    {
                        // Convert each local QuestionAnswer (SQLite.QuestionAnswer) to SupaBase.QuestionAnswer.
                        var supabaseQAs = localQAs
                            .Select(qa => ConvertFrom<SQLite.QuestionAnswer, SupaBase.QuestionAnswer>(qa))
                            .ToList();

                        // Batch insert the converted QuestionAnswer records.
                        await _supabase.From<SupaBase.QuestionAnswer>().Insert(supabaseQAs);
                        LogInfo($"Inserted {supabaseQAs.Count} SupaBase QuestionAnswer records for Game {gameId}.");
                    }
                    else
                    {
                        LogInfo($"No local QuestionAnswer records for Game {gameId}.");
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Error inserting QuestionAnswer records for Game {gameId}.", ex);
                }
            }
        }

        /// <summary>
        /// Generic helper to sync local data with remote data.
        /// 
        /// If skipIfExists is true:
        ///   - Only insert records that do not exist remotely.
        ///   - Do not update existing records.
        /// If skipIfExists is false:
        ///   - Insert missing records and update those that exist.
        /// </summary>
        private static async Task SyncEntitiesAsync<T, TKey>(
             IEnumerable<T> localData,
             IEnumerable<T> remoteData,
             Func<T, TKey> keySelector,
             Func<T, object> matchBuilder,
             bool skipIfExists = false)
             where T : BaseModel, new()
        {
            var remoteKeys = remoteData.Select(keySelector).ToHashSet();

            if (skipIfExists)
            {
                // Only insert new records.
                var toInsert = localData.Where(x => !remoteKeys.Contains(keySelector(x))).ToList();
                if (toInsert.Any())
                {
                    await _supabase.From<T>().Insert(toInsert);
                    LogInfo($"{typeof(T).Name}: Inserted {toInsert.Count} new record(s).");
                }
                else
                {
                    LogInfo($"{typeof(T).Name}: No new records to insert.");
                }
            }
            else
            {
                // Use Upsert to insert and update all records in localData.
                // (localData should be pre-filtered to the relevant records for this user)
                try
                {
                    var list = localData.ToList();
                    if (list.Any())
                    {
                        await _supabase.From<T>().Upsert(list);
                        LogInfo($"{typeof(T).Name}: Upserted {list.Count} record(s).");
                    }
                    else
                    {
                        LogInfo($"{typeof(T).Name}: No records to upsert.");
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Error during Upsert for {typeof(T).Name}", ex);
                    throw;
                }
            }
        }

        public static TSource ConvertFrom<TTarget, TSource>(TTarget target)
    where TSource : new()
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var source = new TSource();
            var sourceProps = typeof(TSource).GetProperties();
            var targetProps = typeof(TTarget).GetProperties();

            foreach (var targetProp in targetProps)
            {
                // Look for a readable source property with the same name
                var sourceProp = sourceProps.FirstOrDefault(sp =>
                    sp.Name.Equals(targetProp.Name, StringComparison.InvariantCultureIgnoreCase) &&
                    sp.CanWrite);
                if (sourceProp != null)
                {
                    var value = targetProp.GetValue(target);
                    if (value != null && sourceProp.PropertyType != targetProp.PropertyType)
                    {
                        // Optionally add custom type conversion logic here.
                        try
                        {
                            value = System.Convert.ChangeType(value, sourceProp.PropertyType);
                        }
                        catch
                        {
                            value = Guid.Parse((string)value);//TODO: Change this ugliness. Somehow change the QuestionAnswer column to Guid.. or work with string and Guid
                        }
                    }
                    sourceProp.SetValue(source, value);
                }
            }
            return source;
        }

        #region Additional Filtering Helpers
        // Customize these methods to apply any extra filtering rules.

        private static bool IsGameEligibleForSync(SQLite.Game game)
        {
            // Example: only sync games that are active.
            // return game.IsActive;
            return true; // default to no extra filtering.
        }

        private static bool IsQuestionEligibleForSync(KeyboardQuestion question)
        {
            // Add additional filtering logic for KeyboardQuestion here.
            return true;
        }

        private static bool IsAnswerEligibleForSync(KeyboardAnswer answer)
        {
            // Add additional filtering logic for KeyboardAnswer here.
            return true;
        }

        private static bool IsKeyEventEligibleForSync(KeyEvent keyEvent)
        {
            // Example: only sync key events with a valid timestamp.
            // return keyEvent.Timestamp > DateTime.MinValue;
            return true;
        }

        private static bool IsQuestionAnswerEligibleForSync(QuestionAnswer answer)
        {
            // Add any extra filtering logic for QuestionAnswer.
            return true;
        }
        #endregion
    }
}