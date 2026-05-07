using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SupabaseClient = Supabase.Client;
using Supabase.Postgrest.Models;
using SQLite = GestureSample.Maui.Data.SQLite;
using SupaBase = GestureSample.Maui.Data.SupaBase;
using Microsoft.IdentityModel.Tokens;
using static GestureSample.Maui.Data.GameRepository;
using System.ComponentModel;
using System.Reflection;
using EnumsNET;
using Supabase.Postgrest.Attributes;
using static Supabase.Postgrest.Constants;
using System.Threading;

namespace GestureSample.Maui.Data.SupaBase
{
    public static class SupabaseService
    {
        private sealed class LocalRelatedSyncBatch
        {
            public Dictionary<Guid, List<SQLite.QuestionAnswer>> QuestionAnswersByGameId { get; init; } = new();
            public Dictionary<Guid, List<SQLite.KeyboardQuestion>> KeyboardQuestionsByGameId { get; init; } = new();
            public Dictionary<Guid, List<SQLite.KeyEvent>> KeyEventsByGameId { get; init; } = new();
        }

        private readonly record struct RelatedSyncSummary(string Label, int GamesTouched, int RecordsInserted);

        private static readonly Lazy<SupabaseLocalConfig> _config = new(SupabaseLocalConfig.LoadOrThrow);
        private static readonly Lazy<SupabaseClient> _supabase = new(() =>
            new SupabaseClient(
                _config.Value.Url,
                _config.Value.AnonKey
            ));
        // In-memory storage of the current session's JWT
        private static string? _currentJwt;
        private static SupabaseClient Client => _supabase.Value;

       
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
        /// Signs in a user with email and password, storing the JWT in the client session.
        /// </summary>
        /// <param name="email">User's email.</param>
        /// <param name="password">User's password.</param>
        /// <exception cref="Exception">Thrown if sign-in fails.</exception>
        public static async Task SignInWithPasswordAsync(string email, string password)
        {
            // SignIn returns a Session where session.User is null if credentials are invalid
            var session = await Client.Auth.SignIn(email, password);
            if (session?.User == null)
                throw new Exception("Sign in failed: invalid credentials");
            else
            {
                _currentJwt = Client.Auth.CurrentSession?.AccessToken;
                LogInfo($"Supabase JWT: {_currentJwt}");
            }
        }

        /// <summary>
        /// Signs out the current user, clearing the session.
        /// </summary>
        public static async Task SignOutAsync()
        {
            await Client.Auth.SignOut();
        }

        /// <summary>
        /// Indicates whether a user is currently signed in.
        /// </summary>
        public static bool IsSignedIn => Client.Auth.CurrentSession != null;

        /// <summary>
        /// Retrieves the current session's access token (JWT), or null if not signed in.
        /// </summary>
        public static string? AccessToken => Client.Auth.CurrentSession?.AccessToken;


        public static async Task<List<User>> GetUsersOfUser(SQLite.User user)
        {
            try
            {


                //if (!user.IsTeacher) return null;


                var parameters = new Dictionary<string, object>
{
    { "user_id", user.Id } // Replace userId with the actual UUID value
};

                var users = await Client.Rpc<List<User>>("get_users_by_classroom", parameters);
                //var users = response.Model;

                //var users = await _supabase.Functions.Invoke<List<User>>("select-users-by-classroom", options: options);
                return users;
            }
            catch (Exception ex)
            {
                LogError("Error in GetUsersOfUser", ex);
                throw;
            }
        }

        public static async Task<List<SQLite.Game>> GetAllByUserAsync(Guid? userID)
        {
            if (userID == null)
            {
                return new List<SQLite.Game>();
            }

            //string userIdString = userID.Value.ToString("D"); // Use the "D" format for consistent GUID string representation
            var result = await Client
        .From<Game>()
                .Where(game => game.UserId == (Guid)userID)
        .Get();

            return result.Models.Select(game => ConvertFrom<SupaBase.Game,SQLite.Game>(game))
                           .OrderBy(game => game.TimeStart)
                .ToList();
        }
        public static async Task<List<SQLite.Game>> GetRecordsByGameNamesAsync(Guid? userID, string gameName)
        {
           
            var result = await Client
        .From<Game>()
        .Where(g => g.UserId == userID && g.FinalStatus==1)
        .Get();

            return result.Models.Select(game => ConvertFrom<SupaBase.Game, SQLite.Game>(game))
                .OrderBy(game => game.TimeEnd.Subtract(game.TimeStart)).Take<SQLite.Game>(20).ToList();
        }

        public static async Task<List<string>> GetDistinctGameNamesAsync(Guid? userID)
        {
            if(userID == null)
            {
                return new List<string>();
            }
            // SELECT DISTINCT with an alias matching the property name in your class
            var result = await Client
        .From<Game>()
        .Where(g => g.UserId == userID)
        .Get();

            // Extract distinct game names
            return result.Models
                .Select(g => g.GameName)
                .Where(name => !string.IsNullOrWhiteSpace(name)).
                OrderBy(g => g)
                .Distinct()
                .ToList();

        }

        public static async Task<List<SQLite.QuestionAnswer>> GetAnswersByQueryAsync(Guid GameId)
        {
            var result = await Client.From<QuestionAnswer>().Where(state => state.GameId == GameId).Get();
            return result.Models.Select(QA => ConvertFrom<SupaBase.QuestionAnswer, SQLite.QuestionAnswer>(QA)).ToList();
        }

        public static async Task<List<SQLite.KeyboardQuestion>> GetKeyboardQuestionByQueryAsync(Guid? selectedIdentifier)
        {
            if (!selectedIdentifier.HasValue)
                return new List<SQLite.KeyboardQuestion>();

            string gameId = selectedIdentifier.Value.ToString();
            var result = await Client.From<KeyboardQuestion>().Where(state => state.GameId == gameId).Get();

            LogInfo("Retrived states");
            return result.Models.Select(q => ConvertFrom<SupaBase.KeyboardQuestion, SQLite.KeyboardQuestion>(q)).ToList();
            
        }

        public static async Task<List<SQLite.KeyEvent>> GetKeyEventsByQueryAsync(Guid GameId)
        {
            var result = await Client.From<KeyEvent>().Where(state => state.GameId == GameId.ToString()).Get();
            return result.Models.Select(keyEvent => ConvertFrom<SupaBase.KeyEvent, SQLite.KeyEvent>(keyEvent)).ToList(); 
        }

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
                throw;
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
                var existingResponse = await Client
                    .From<User>()
                    .Where(u => u.Id == user.Id)
                    .Get();

                bool isNewUser = existingResponse.Models == null || !existingResponse.Models.Any();

                LogInfo(string.Format( "isNewUser? {0}", isNewUser));

                // Use Upsert so that if the record exists it will be updated,
                // and if not, it will be inserted.
                var SupabaseUser = ConvertFrom<SQLite.User, SupaBase.User>(user);
                await Client.From<User>().Upsert(SupabaseUser);
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
                await Client.From<SupaBase.Game>().Upsert(remoteGameBatch);
                LogInfo($"Upserted {remoteGameBatch.Count} games to SupaBase.");

                // 4. Replace related game rows so repeated syncs stay safe.
                var newGameIds = unsyncedGames.Select(g => g.Id).ToList();
                var relatedData = await LoadRelatedDataForGamesAsync(newGameIds);
                await ReplaceRelatedDataForGamesAsync(newGameIds, relatedData);

                LogInfo("Finished SyncUnsyncedGamesAndRelatedDataAsync successfully.");

                // 5. Mark local games as synced.
                foreach (var localGame in unsyncedGames)
                {
                    localGame.WasSynced = true;
                }
                var gameRepo = ServiceHelper.GetService<GameRepository>();
                int markedCount = await gameRepo.MarkSyncedAsync(user.Id, unsyncedGames.Select(game => game.Id));
                LogInfo($"Marked {markedCount} local games as synced.");
                int unsyncedRemaining = await gameRepo.CountUnsyncedByUserAsync(user.Id);
                LogInfo($"Unsynced local games remaining for user {user.Id}: {unsyncedRemaining}.");

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
            var unsynced = await gameRepo.GetUnsyncedByUserAsync(user.Id);
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

                var response = await Client.From<KeyboardQuestion>().Get();
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

                var response = await Client.From<KeyboardAnswer>().Get();
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

                var response = await Client.From<KeyEvent>().Get();
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

        private static async Task<LocalRelatedSyncBatch> LoadRelatedDataForGamesAsync(List<Guid> gameIds)
        {
            var qaRepo = ServiceHelper.GetService<QuestionAnswerRepository>();
            var keyboardQuestionRepo = ServiceHelper.GetService<KeyboardQuestionRepository>();
            var keyEventRepo = ServiceHelper.GetService<KeyEventRepository>();

            var allQuestionAnswersTask = qaRepo.GetByGameIdsAsync(gameIds);
            var allKeyboardQuestionsTask = keyboardQuestionRepo.GetByGameIdsAsync(gameIds);
            var allKeyEventsTask = keyEventRepo.GetByGameIdsAsync(gameIds);

            await Task.WhenAll(allQuestionAnswersTask, allKeyboardQuestionsTask, allKeyEventsTask);

            Dictionary<Guid, List<SQLite.QuestionAnswer>> questionAnswersByGameId = allQuestionAnswersTask.Result
                .Where(qa => !string.IsNullOrWhiteSpace(qa.GameId))
                .GroupBy(qa => Guid.Parse(qa.GameId))
                .ToDictionary(group => group.Key, group => group.OrderBy(item => item.Time).ToList());

            Dictionary<Guid, List<SQLite.KeyboardQuestion>> keyboardQuestionsByGameId = allKeyboardQuestionsTask.Result
                .Where(question => !string.IsNullOrWhiteSpace(question.GameId))
                .GroupBy(question => Guid.Parse(question.GameId))
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderBy(item => item.QuestionNumber)
                        .ThenBy(item => item.AttemptNumber)
                        .ThenBy(item => item.Time)
                        .ToList());

            Dictionary<Guid, List<SQLite.KeyEvent>> keyEventsByGameId = allKeyEventsTask.Result
                .Where(keyEvent => !string.IsNullOrWhiteSpace(keyEvent.GameId))
                .GroupBy(keyEvent => Guid.Parse(keyEvent.GameId))
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderBy(item => item.EventTime)
                        .ThenBy(item => item.id)
                        .ToList());

            return new LocalRelatedSyncBatch
            {
                QuestionAnswersByGameId = questionAnswersByGameId,
                KeyboardQuestionsByGameId = keyboardQuestionsByGameId,
                KeyEventsByGameId = keyEventsByGameId
            };
        }

        private static async Task ReplaceRelatedDataForGamesAsync(List<Guid> gameIds, LocalRelatedSyncBatch relatedData)
        {
            var gameIdsWithQuestionAnswers = gameIds
                .Where(gameId => relatedData.QuestionAnswersByGameId.ContainsKey(gameId))
                .ToList();
            var gameIdsWithKeyboardQuestions = gameIds
                .Where(gameId => relatedData.KeyboardQuestionsByGameId.ContainsKey(gameId))
                .ToList();
            var gameIdsWithKeyEvents = gameIds
                .Where(gameId => relatedData.KeyEventsByGameId.ContainsKey(gameId))
                .ToList();

            LogInfo($"QuestionAnswer sync will touch {gameIdsWithQuestionAnswers.Count} game(s).");
            LogInfo($"KeyboardQuestion sync will touch {gameIdsWithKeyboardQuestions.Count} game(s).");
            LogInfo($"KeyEvent sync will touch {gameIdsWithKeyEvents.Count} game(s).");

            RelatedSyncSummary questionAnswerSummary = await ProcessGamesInParallelAsync(
                gameIdsWithQuestionAnswers,
                async gameId => await ReplaceQuestionAnswersForGameAsync(gameId, relatedData.QuestionAnswersByGameId[gameId]),
                "QuestionAnswer");

            RelatedSyncSummary keyboardQuestionSummary = await ProcessGamesInParallelAsync(
                gameIdsWithKeyboardQuestions,
                async gameId => await ReplaceKeyboardQuestionsForGameAsync(gameId, relatedData.KeyboardQuestionsByGameId[gameId]),
                "KeyboardQuestion");

            RelatedSyncSummary keyEventSummary = await ProcessGamesInParallelAsync(
                gameIdsWithKeyEvents,
                async gameId => await ReplaceKeyEventsForGameAsync(gameId, relatedData.KeyEventsByGameId[gameId]),
                "KeyEvent");

            LogInfo($"{questionAnswerSummary.Label} sync summary: {questionAnswerSummary.GamesTouched} game(s), {questionAnswerSummary.RecordsInserted} record(s).");
            LogInfo($"{keyboardQuestionSummary.Label} sync summary: {keyboardQuestionSummary.GamesTouched} game(s), {keyboardQuestionSummary.RecordsInserted} record(s).");
            LogInfo($"{keyEventSummary.Label} sync summary: {keyEventSummary.GamesTouched} game(s), {keyEventSummary.RecordsInserted} record(s).");
        }

        private static async Task<RelatedSyncSummary> ProcessGamesInParallelAsync(
            IReadOnlyList<Guid> gameIds,
            Func<Guid, Task<int>> action,
            string label,
            int maxDegreeOfParallelism = 6)
        {
            int processedGames = 0;
            int insertedRecords = 0;

            await Parallel.ForEachAsync(
                gameIds,
                new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
                async (gameId, _) =>
                {
                    int records = await action(gameId);
                    Interlocked.Add(ref insertedRecords, records);

                    int processed = Interlocked.Increment(ref processedGames);
                    if (processed % 25 == 0 || processed == gameIds.Count)
                        LogInfo($"{label} sync progress: {processed}/{gameIds.Count} game(s).");
                });

            return new RelatedSyncSummary(label, gameIds.Count, insertedRecords);
        }

        private static async Task<int> ReplaceQuestionAnswersForGameAsync(Guid gameId, List<SQLite.QuestionAnswer> localQAs)
        {
            if (localQAs == null || localQAs.Count == 0)
                return 0;

            await Client.From<SupaBase.QuestionAnswer>()
                .Where(state => state.GameId == gameId)
                .Delete();

            var supabaseQAs = localQAs
                .Select(qa => ConvertFrom<SQLite.QuestionAnswer, SupaBase.QuestionAnswer>(qa))
                .ToList();

            await Client.From<SupaBase.QuestionAnswer>().Insert(supabaseQAs);
            return supabaseQAs.Count;
        }

        private static async Task<int> ReplaceKeyboardQuestionsForGameAsync(Guid gameId, List<SQLite.KeyboardQuestion> localQuestions)
        {
            if (localQuestions == null || localQuestions.Count == 0)
                return 0;

            string gameIdText = gameId.ToString();

            await Client.From<SupaBase.KeyboardQuestion>()
                .Where(state => state.GameId == gameIdText)
                .Delete();

            var supabaseQuestions = localQuestions
                .Select(question => ConvertFrom<SQLite.KeyboardQuestion, SupaBase.KeyboardQuestion>(question))
                .ToList();

            await Client.From<SupaBase.KeyboardQuestion>().Insert(supabaseQuestions);
            return supabaseQuestions.Count;
        }

        private static async Task<int> ReplaceKeyEventsForGameAsync(Guid gameId, List<SQLite.KeyEvent> localEvents)
        {
            if (localEvents == null || localEvents.Count == 0)
                return 0;

            string gameIdText = gameId.ToString();

            await Client.From<SupaBase.KeyEvent>()
                .Where(state => state.GameId == gameIdText)
                .Delete();

            var supabaseEvents = localEvents
                .Select(keyEvent => ConvertFrom<SQLite.KeyEvent, SupaBase.KeyEvent>(keyEvent))
                .ToList();

            await Client.From<SupaBase.KeyEvent>().Insert(supabaseEvents);
            return supabaseEvents.Count;
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
                await Client.From<T>().Insert(toInsert);
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
                await Client.From<T>().Upsert(list);
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
            //LogInfo("Converting..");
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var source = new TSource();
            var sourceProps = typeof(TSource).GetProperties();
            var targetProps = typeof(TTarget).GetProperties();

            foreach (var targetProp in targetProps)
            {
                //LogInfo($"Converting {targetProp}");
                // Skip QuestionID (auto incremented field not present in Supabase)
                //if (targetProp.Name.Equals("QuestionID", StringComparison.OrdinalIgnoreCase))
                //    continue;
                // Look for a readable source property with the same name
                var sourceProp = sourceProps.FirstOrDefault(sp =>
                    sp.Name.Equals(targetProp.Name, StringComparison.InvariantCultureIgnoreCase) &&
                    sp.CanWrite);
                if (sourceProp != null)
                {
                    bool isRemoteAutoGeneratedIntegerPrimaryKey =
                        typeof(TSource).Namespace?.Contains(".Data.SupaBase", StringComparison.Ordinal) == true &&
                        sourceProp.GetCustomAttribute<PrimaryKeyAttribute>() != null &&
                        (sourceProp.PropertyType == typeof(int) || sourceProp.PropertyType == typeof(long));

                    if (isRemoteAutoGeneratedIntegerPrimaryKey)
                        continue;

                    var value = targetProp.GetValue(target);
                    if (value != null && sourceProp.PropertyType != targetProp.PropertyType)
                    {
                        try
                        {
                            if (sourceProp.PropertyType == typeof(Guid) && value is string s)
                            {
                                value = Guid.Parse(s);
                            }
                            else if (sourceProp.PropertyType == typeof(string) && value is Guid g)
                            {
                                value = g.ToString();
                            }
                            // Handle string -> enum
                            else if (sourceProp.PropertyType.IsEnum && value is string enumString)
                            {
                                //LogInfo($"Converting {targetProp.Name} from string {value} to enum");
                                value = EnumsNET.Enums.Parse<Operation>(enumString, true, EnumsNET.EnumFormat.Name);
                            }
                            else
                            {
                                value = System.Convert.ChangeType(value, sourceProp.PropertyType);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogError($"Failed to convert property '{targetProp.Name}' from {targetProp.PropertyType} to {sourceProp.PropertyType}", ex);
                            //throw new InvalidCastException($"Failed to convert property '{targetProp.Name}' from {targetProp.PropertyType} to {sourceProp.PropertyType}", ex);
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
