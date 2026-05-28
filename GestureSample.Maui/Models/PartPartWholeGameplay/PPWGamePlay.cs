using GestureSample.Maui.Data;
using GestureSample.Maui.Data.SQLite;
using GestureSample.Maui.Handlers;

namespace GestureSample.Maui.Models
{

    public class PPWGamePlay
    {



        public static readonly int NAN = -1111;
        public Guid GameId { get; set; } = Guid.NewGuid();
        public int addend1;
        public int addend2;
        public bool GameOver { get; set; }= false;
        QuestionAnswer qaState;

        public int _questionNumber = 0;
        protected int _questionsWrong = 0;
        protected bool _lastQuestionWrong = false;

        protected Data.SQLite.Game _gameData;
        public virtual int Sum { get; set; }

        public Operation CurrentOperation { get; set; }

        private int _guessNumber = 0;
        public int GuessNumber { get { return _guessNumber; } }

        protected string _status = Statement.Neutral;
        public string Status { get => _status; }

        public bool IsFirstGuess { get; set; } = true;

        protected int _currentTriadIndex = 0;
        protected int? _dynamicKeyboardWeightValue;
        protected int? _dynamicKeyboardExpectedPressCount;
        protected readonly HashSet<int> _weightedCustomReachableSums = new();
        protected readonly HashSet<int> _weightedCustomImpossibleSums = new();
        protected bool _currentWeightedTargetRequiresImpossibleAnswer;
        private string? _weightedTargetPoolCacheKey;

        public PPWObject GenerateSecondaryTriad(int sum, int? addend1Min=null, int? addend1Max = null)
        {
            List<PPWObject> possibleSums = PossibleTriads.Where(t => t.Sum == sum).ToList();
            if(addend1Min.HasValue && addend1Max.HasValue)
                possibleSums = possibleSums.Where(t => t.Addend1 >= addend1Min.Value && t.Addend1<= addend1Max.Value).ToList();
            else if (addend1Min.HasValue)
                possibleSums = possibleSums.Where(t => t.Addend1 >= addend1Min.Value).ToList();
            else if (addend1Max.HasValue)
                possibleSums = possibleSums.Where(t => t.Addend1 <= addend1Max.Value).ToList();
            if (possibleSums.Count > 0)
            {
                Random r = new();
                int index = r.Next(possibleSums.Count);
                return possibleSums[index];
            }
            return null;
        }

        public PPWObject GenerateTriadBySum(int sum, int? addend1Min = null, int? addend1Max = null)
        {
            Random r = new();
            int addend1 = addend1Min.HasValue && addend1Max.HasValue? r.Next((int)addend1Min,(int)addend1Max+1) :r.Next(1,sum);
            return new PPWObject(addend1, sum - addend1, sum);

        }

        public int _tasksMade = 0;
        public int _losesMade = 0;
        public DateTime StartTime = DateTime.Now;

        public GameConfig Config;

        private readonly GameRepository _gameRepository;
        private readonly QuestionAnswerRepository _questionAnswerRepository;
        private readonly KeyboardQuestionRepository _keyboardQuestionRepository;
        private readonly KeyEventRepository _keyEventRepository;
        private bool _gameInitialized = false;

        // plan runtime
        private int _planStepIndex = 0;
        private int _planStepRepeatLeft = 0;
        private int _planSeed;
        private Random _planRandom;

        // snapshot of last question (PPW form)
        private (int a1, int a2, int s, Operation op, VariableTypes vt)? _prevPPWQuestion;
        private PPWObject? _prevResolvedTriad;
        protected ExercisePlanStep? CurrentPlanStep
        {
            get
            {
                if (Config?.Plan?.Steps == null || Config.Plan.Steps.Count == 0) return null;
                if (_planStepIndex < 0 || _planStepIndex >= Config.Plan.Steps.Count) return null;
                return Config.Plan.Steps[_planStepIndex];
            }
        }

        protected ExercisePlanStep? AcquirePlanStep()
        {
            if (Config?.Plan?.Steps == null || Config.Plan.Steps.Count == 0) return null;

            if (_planStepRepeatLeft <= 0)
            {
                if (_planStepIndex >= Config.Plan.Steps.Count)
                {
                    if (Config.Plan.Loop) _planStepIndex = 0;
                    else return null;
                }

                ExercisePlanStep step = Config.Plan.Steps[_planStepIndex];
                _planStepRepeatLeft = Math.Max(1, step.Repeat);
                _planStepIndex++;
            }

            _planStepRepeatLeft--;
            return Config.Plan.Steps[Math.Max(0, _planStepIndex - 1)];
        }

        protected void ApplyOpMode(ExercisePlanStep step)
        {
            switch (step.OpMode)
            {
                case PlanOpMode.Keep:
                    return;
                case PlanOpMode.Fixed:
                    CurrentOperation = step.Operation;
                    return;
                case PlanOpMode.RandomFromConfigList:
                default:
                    if (Config.OperationList != null && Config.OperationList.Count > 0)
                        CurrentOperation = Config.OperationList[_planRandom.Next(Config.OperationList.Count)];
                    return;
            }
        }

        public PPWGamePlay(GameConfig config)
        {

            _gameRepository = ServiceHelper.GetService<GameRepository>();
            _questionAnswerRepository = ServiceHelper.GetService<QuestionAnswerRepository>();
            _keyboardQuestionRepository = ServiceHelper.GetService<KeyboardQuestionRepository>();
            _keyEventRepository = ServiceHelper.GetService<KeyEventRepository>();
            Config = config;
            CurrentOperation = Config.OperationList.Count > 0 ? Config.OperationList[0] : Operation.Sum;

             _gameData = new()
             {
                 UserId = (Guid)ServiceHelper.GetService<CurrentUserSession>().ActiveUser.Id,
                 Id = GameId,
                 GameName = config.GameName,
                 Config = config
            };
            _planSeed = Config?.Plan?.Seed ?? Environment.TickCount;
            _planRandom = new Random(_planSeed);


            if (!UsesWeightedKeyboardTargetGeneration())
                GeneratePossibleTriadsSet();

            EnsureWeightedTargetPools();


            //SaveState();
        }

        protected async Task EnsureGameInitializedAsync()
        {
            if (_gameInitialized)
                return;

            await _gameRepository.SaveAsync(_gameData);
            _gameInitialized = true;
        }

        protected async Task SaveState(int resultStatus =-1, bool syncAfterSave = true)
        {
            await EnsureGameInitializedAsync();
            await MarkGameAsDirtyAsync();

            int persistedAddend1 = GetPersistedQuestionAnswerAddend1();
            int persistedAddend2 = GetPersistedQuestionAnswerAddend2();
            int persistedSum = GetPersistedQuestionAnswerSum();
            Operation persistedOperation = GetPersistedQuestionAnswerOperation();

            QuestionAnswer s = new()
            {

                GameId = this.GameId.ToString(),
                QuestionNumber = _questionNumber,
                Time = DateTime.Now,
                Op = persistedOperation,
                Addend1 = persistedAddend1,
                Addend2 = persistedAddend2,
                Sum = persistedSum,
                ResultStatus = resultStatus
            };
            await _questionAnswerRepository.SaveAsync(s);
            qaState = s ;
            if (syncAfterSave)
                await TrySyncSupabaseStateAsync();
    }

        protected virtual int GetPersistedQuestionAnswerAddend1() => addend1;

        protected virtual int GetPersistedQuestionAnswerAddend2() => addend2;

        protected virtual int GetPersistedQuestionAnswerSum() => Sum;

        protected virtual Operation GetPersistedQuestionAnswerOperation() => CurrentOperation;

        protected async Task MarkGameAsDirtyAsync()
        {
            _gameData.WasSynced = false;
            await _gameRepository.UpdateAsync(_gameData);
        }

        protected async Task TrySyncSupabaseStateAsync()
        {
            try
            {
                var activeUser = ServiceHelper.GetService<CurrentUserSession>().ActiveUser;
                if (activeUser == null)
                    return;

                await GestureSample.Maui.Data.SupaBase.SupabaseService.SyncUnsyncedGamesAndRelatedDataAsync(activeUser);
                _gameData.WasSynced = true;
                await _gameRepository.UpdateAsync(_gameData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Supabase sync skipped: {ex.Message}");
            }
        }

    private bool IsCorrectInput()
        {
            int minAddend2 = Config.EffectiveMinAddend2;
            int maxAddend2 = Config.EffectiveMaxAddend2;
            if (addend1 > Config.MaxAddend || addend1 < Config.MinAddend || addend2 > maxAddend2 || addend2 < minAddend2 || Sum > Config.MaxSum || Sum < Config.MinSum)
                return false;
            return true;
        }

        protected void BeginExercise()
        {
            _status = Statement.Neutral;
            _guessNumber = 0;
            _questionNumber++;
        }

        public void ResetStatusToNeutral()
        {
            _status = Statement.Neutral;
        }

        protected void IncrementGuessNumber()
        {
            _guessNumber++;
        }

        protected virtual ExerciseGenerationResult CreateGeneratedExerciseResult()
        {
            return new ExerciseGenerationResult
            {
                ActionText = CurrentOperation.ToDString()
            };
        }

        protected virtual async Task PersistGeneratedExerciseAsync()
        {
            await SaveState(syncAfterSave: false);

            if (ShouldPersistKeyboardQuestion())
                await SaveKeyboardQuestionToDbAsync();
        }

        public virtual Color[]? GetInitialKeyboardColors()
        {
            if (Config?.KeyboardConfig == null)
                return null;

            int keyCount = Math.Max(
                1,
                (Config.KeyboardConfig.KeysInRow > 0 ? Config.KeyboardConfig.KeysInRow : 10) *
                Math.Max(1, Config.KeyboardConfig.Rows));

            Color[] keyboardColors = Enumerable.Repeat(Colors.White, keyCount).ToArray();

            switch (Config.KeyboardConfig.PpwKeyboardSeedMode)
            {
                case PpwKeyboardSeedMode.VisiblePartPressed:
                    FillColorRange(keyboardColors, GetVisibleKnownAddendValue(), Colors.Yellow);
                    return keyboardColors;

                case PpwKeyboardSeedMode.WholePressed:
                    FillColorRange(keyboardColors, Sum == NAN ? 0 : Sum, Colors.Yellow);
                    return keyboardColors;

                case PpwKeyboardSeedMode.VisiblePartsColored:
                    if (addend1 != NAN)
                        FillColorRange(keyboardColors, addend1, Colors.Yellow);
                    if (addend2 != NAN)
                        FillColorRange(keyboardColors, addend2, Colors.LightGreen, addend1 == NAN ? 0 : addend1);
                    return keyboardColors;

                default:
                    return null;
            }
        }

        public virtual bool[]? GetInitialKeyboardState()
        {
            Color[]? initialColors = GetInitialKeyboardColors();
            if (initialColors == null || initialColors.Length == 0)
                return null;

            bool[] state = new bool[initialColors.Length];
            for (int i = 0; i < initialColors.Length; i++)
                state[i] = initialColors[i] != Colors.White && initialColors[i] != Colors.Transparent;

            return state.Any(bit => bit) ? state : null;
        }

        public virtual Color[]? GetQuestionKeyboardColors()
        {
            return null;
        }

        public virtual Color[]? GetSecondQuestionKeyboardColors()
        {
            return null;
        }

        public virtual string? GetKeyboardQuestionPromptText()
        {
            if (Config?.UIQuestionType == UIQuestionType.OneText)
            {
                if (Sum != NAN)
                    return Sum.ToString();
                if (addend1 != NAN)
                    return addend1.ToString();
                if (addend2 != NAN)
                    return addend2.ToString();
            }

            if (addend1 != NAN || addend2 != NAN || Sum != NAN)
            {
                string left = addend1 == NAN ? "?" : addend1.ToString();
                string right = addend2 == NAN ? "?" : addend2.ToString();
                string total = Sum == NAN ? "?" : Sum.ToString();
                string op = CurrentOperation.ToDString();
                return $"{left} {op} {right} = {total}";
            }

            return null;
        }

        protected virtual bool ShouldPersistKeyboardQuestion()
        {
            return Config?.KeyboardConfig != null &&
                   !Config.KeyboardConfig.KeyboardOnlyForHelp;
        }

        protected virtual async Task SaveKeyboardQuestionToDbAsync()
        {
            if (_keyboardQuestionRepository == null || Config?.KeyboardConfig == null)
                return;

            int keyCount = Math.Max(
                1,
                (Config.KeyboardConfig.KeysInRow > 0 ? Config.KeyboardConfig.KeysInRow : 10) *
                Math.Max(1, Config.KeyboardConfig.Rows));

            var question = new Data.SQLite.KeyboardQuestion
            {
                GameId = GameId.ToString(),
                QuestionNumber = _questionNumber,
                Time = DateTime.Now,
                Op = CurrentOperation,
                KeyboardRows = Math.Max(1, Config.KeyboardConfig.Rows),
                KeyboardKeysInRow = Config.KeyboardConfig.KeysInRow > 0 ? Config.KeyboardConfig.KeysInRow : keyCount,
                ShowNumbersOnKeys = Config.KeyboardConfig.ShowNumbersOnKeys,
                KeyboardWeights = Config.KeyboardConfig.WeightsArray?.ToArray(),
                InitialKeyboardState = GetInitialKeyboardState(),
                InitialKeyboardColors = GetInitialKeyboardColors(),
                QuestionKeyboardColors = GetQuestionKeyboardColors(),
                QuestionKeyboardColors2 = GetSecondQuestionKeyboardColors(),
                QuestionPromptText = GetKeyboardQuestionPromptText()
            };

            await _keyboardQuestionRepository.SaveAsync(question);
        }

        private static void FillColorRange(Color[] keyboardColors, int count, Color color, int startIndex = 0)
        {
            int safeStart = Math.Max(0, startIndex);
            int safeEnd = Math.Min(keyboardColors.Length, safeStart + Math.Max(0, count));

            for (int i = safeStart; i < safeEnd; i++)
            {
                keyboardColors[i] = color;
            }
        }

        protected ExerciseCheckResult CreateCheckResult(bool isCorrect, bool isWrongInput = false, GameCompletionResult? completion = null, bool refreshCurrentQuestion = false)
        {
            return new ExerciseCheckResult
            {
                IsCorrect = isCorrect,
                IsWrongInput = isWrongInput,
                Status = _status,
                Completion = completion,
                RefreshCurrentQuestion = refreshCurrentQuestion
            };
        }

        protected async Task<GameCompletionResult?> RegisterSuccessfulAttemptAsync(int resultStatus = 1, Func<Task>? onSuccess = null)
        {
            _tasksMade++;
            await SaveState(resultStatus, syncAfterSave: false);
            _lastQuestionWrong = false;

            if (onSuccess != null)
                await onSuccess();

            return await PersistGameProgressAsync();
        }

        protected async Task<GameCompletionResult?> RegisterFailedAttemptAsync(int resultStatus = 0, Func<Task>? onFailure = null)
        {
            _losesMade++;
            await SaveState(resultStatus, syncAfterSave: false);
            if (!_lastQuestionWrong)
            {
                _questionsWrong++;
                _lastQuestionWrong = true;
            }

            if (onFailure != null)
                await onFailure();

            return await PersistGameProgressAsync();
        }

        protected async Task<GameCompletionResult?> PersistGameProgressAsync()
        {
            await EnsureGameInitializedAsync();

            bool isWin = Config.NumberOfTasksToWin == _tasksMade || (Config.IsHistory && PossibleTriads.Count == 0);
            bool isLose = Config.NumberOfMistakesToLose == _losesMade;
            bool isGameOver = isWin || isLose;

            if (isGameOver)
            {
                _gameData.FinalStatus = isWin ? 1 : 0;
                _gameData.TimeEnd = DateTime.Now;
                _gameData.Wins = _questionNumber;
                _gameData.Losses = _questionsWrong;
                GameOver = true;
                await _gameRepository.UpdateAsync(_gameData);

                CurrentUserSession? currentUserSession = ServiceHelper.GetService<CurrentUserSession>();
                BackgroundSyncService? backgroundSyncService = ServiceHelper.GetService<BackgroundSyncService>();
                backgroundSyncService?.TryStartSync(currentUserSession?.ActiveUser);

                return new GameCompletionResult
                {
                    GameId = GameId,
                    IsWin = isWin,
                    Duration = DateTime.Now.Subtract(StartTime)
                };
            }

            _gameData.TimeEnd = DateTime.Now;
            _gameData.Wins = _tasksMade;
            _gameData.Losses = _losesMade;
            await _gameRepository.UpdateAsync(_gameData);
            return null;
        }



        public virtual async Task<ExerciseCheckResult> EvaluateAsync()
        {
            if (!IsCorrectInput())
            {
                _status = Statement.WrongInput;
                addend1 = oldA1;
                addend2 = oldA2;
                Sum = oldS;
                return CreateCheckResult(isCorrect: false, isWrongInput: true);
            }

            IncrementGuessNumber();
            _status = CurrentOperation switch
            {
                Operation.Multiplication => (addend1 * addend2 == Sum) ? Statement.True : Statement.False,
                Operation.Sum => (addend1 + addend2 == Sum) ? Statement.True : Statement.False,
                _ => Statement.True
            };

            if (Config.IsHistory && _status == Statement.True &&
                (AllHistory.Where(item => item.Sum == Sum && item.Addend1 == addend1).Any() ||
                (Config.IsHistorySymetrical && AllHistory.Where(item => item.Sum == Sum && item.Addend1 == addend2).Any())))
            {
                _status = Statement.New;
            }

            GameCompletionResult? completion = null;
            if (_status == Statement.True)
            {
                completion = await RegisterSuccessfulAttemptAsync(onSuccess: async () =>
                {
                    if (Config.IsHistory)
                    {
                        RemoveItemToHistory(addend1, addend2, Sum);
                    }

                    await Task.CompletedTask;
                });
            }

            if (_status == Statement.False || _status == Statement.New)
            {
                completion = await RegisterFailedAttemptAsync(_status == Statement.New ? 2 : 0, onFailure: async () =>
                {
                    addend1 = oldA1;
                    addend2 = oldA2;
                    Sum = oldS;
                    await Task.CompletedTask;
                });
            }

            return CreateCheckResult(isCorrect: _status == Statement.True, completion: completion);
        }

        public virtual async Task<bool> CheckAsync()
        {
            return (await EvaluateAsync()).IsCorrect;
        }

        public virtual bool IsCloseEnough(PianoKeyboard keyboard, int allowedDifferences = 1)
        {
            return IsCloseEnough(keyboard.Addend1, keyboard.Addend2, Sum, allowedDifferences);
        }

        public bool IsCloseEnough(int addend1, int addend2, int sum, int allowedDifferences = 1)
        {
            // returns true when the addends' sum differs from 'sum' by at most 1
            return Math.Abs(addend1 + addend2 - sum) <= allowedDifferences;
        }
        private void RemoveItemToHistory(int addend1, int addend2, int sum)
        {
            if (PossibleTriads.Where(item => item.Sum == Sum && item.Addend1 == addend1).Any())
                PossibleTriads.Remove(PossibleTriads.Where(item => item.Sum == Sum && item.Addend1 == addend1).ToList()[0]);
            //else { Console.WriteLine("{0} {1}= {2}", addend1, addend2, Sum); }
            AllHistory.Add(new PPWObject(addend1, addend2, Sum));
            if (Config.IsHistorySymetrical)
            {
                if (PossibleTriads.Where(item => item.Sum == Sum && item.Addend2 == addend1).Any())
                    PossibleTriads.Remove(PossibleTriads.Where(item => item.Sum == Sum && item.Addend2 == addend1).ToList()[0]);
                if(addend1!=addend2)
                    AllHistory.Add(new PPWObject(addend2, addend1, Sum));
            }
        }

        int oldA1, oldA2, oldS;
        private bool _isCurrentPairedBenchmarkAnchor;

        public virtual async Task<ExerciseCheckResult> EvaluateAsync(int a1, int a2, int s)
        {
            oldA1 = addend1; oldA2 = addend2; oldS = Sum;
            if(addend1 == NAN) addend1 = a1; 
            if(addend2 == NAN) addend2 = a2;
            if (Sum == NAN) Sum = s;
            return await EvaluateAsync();
        }

        public virtual async Task<bool> Check(int a1, int a2, int s)
        {
            return (await EvaluateAsync(a1, a2, s)).IsCorrect;
        }

        public void SetCurrentBenchmarkAnchor(int a1, int a2, int s)
        {
            if (!IsTriadWithinConfig(a1, a2, s))
                return;

            addend1 = a1;
            addend2 = a2;
            Sum = s;
            _prevResolvedTriad = new PPWObject(a1, a2, s);
            SnapshotPrevPPWQuestion();
        }

        public bool IsCurrentPairedBenchmarkAnchor =>
            Config?.UsePairedCloseTriadBenchmark == true &&
            _isCurrentPairedBenchmarkAnchor &&
            addend1 != NAN &&
            addend2 != NAN &&
            Sum != NAN;

        public virtual async Task<ExerciseCheckResult> EvaluateAsync(PianoKeyboard pianoKeyboard)
        {
            if (UsesSeededPpwKeyboardStage())
            {
                return await EvaluateSeededPpwKeyboardStageAsync(pianoKeyboard);
            }

            if (UsesDynamicKeyboardMultiplication())
            {
                return await EvaluateDynamicKeyboardMultiplicationAsync(pianoKeyboard);
            }

            if (UsesWeightedSingleTargetKeyboardStage())
            {
                return await EvaluateWeightedSingleTargetKeyboardStageAsync(pianoKeyboard);
            }

            if (UsesWeightedCustomStageTargets())
            {
                return await EvaluateWeightedCustomStageAsync(pianoKeyboard);
            }

            int keyboardSum = Sum;
            if (Sum == NAN && (pianoKeyboard.Addend1>=0 && pianoKeyboard.Addend2>=0)) {
               if( pianoKeyboard.Addend1 == addend1 && pianoKeyboard.Addend2 == addend2)
                 keyboardSum = pianoKeyboard.Sum; //if the sum is not set, it is set to the sum of addends
                else
                    keyboardSum = GetAlternateValidSum(pianoKeyboard.Sum);
            }
            ExerciseCheckResult result = await EvaluateAsync(pianoKeyboard.Addend1, pianoKeyboard.Addend2, keyboardSum);
            await SaveKeyboardAttemptSnapshotAsync(pianoKeyboard.ToBitArray(), result.IsCorrect, DateTime.Now, pianoKeyboard.GetCurrentColors());
            Console.WriteLine("CheckAsync(Enabled returned): {0} {1}={2}", pianoKeyboard.Addend1, pianoKeyboard.Addend2, Sum);
            return result;
        }

        private bool UsesSeededPpwKeyboardStage()
        {
            return Config?.KeyboardConfig != null &&
                   Config.KeyboardConfig.PpwKeyboardSeedMode != PpwKeyboardSeedMode.None;
        }

        private bool HasSingleMissingAddendQuestion()
        {
            return Sum != NAN &&
                   ((addend1 == NAN && addend2 != NAN) ||
                    (addend2 == NAN && addend1 != NAN));
        }

        private int GetVisibleKnownAddendValue()
        {
            if (addend1 != NAN && addend2 == NAN)
                return addend1;

            if (addend2 != NAN && addend1 == NAN)
                return addend2;

            return 0;
        }

        private int GetMissingAddendValue()
        {
            if (!HasSingleMissingAddendQuestion())
                return 0;

            return Sum - GetVisibleKnownAddendValue();
        }

        private async Task<ExerciseCheckResult> EvaluateSeededPpwKeyboardStageAsync(PianoKeyboard pianoKeyboard)
        {
            IncrementGuessNumber();

            bool isCorrect = Config.KeyboardConfig.PpwKeyboardSeedMode switch
            {
                PpwKeyboardSeedMode.VisiblePartPressed => EvaluateVisiblePartKeyboardStage(pianoKeyboard),
                PpwKeyboardSeedMode.WholePressed => EvaluateWholePressedKeyboardStage(pianoKeyboard),
                _ => false
            };

            _status = isCorrect ? Statement.True : Statement.False;
            await SaveKeyboardAttemptSnapshotAsync(pianoKeyboard.ToBitArray(), isCorrect, DateTime.Now, pianoKeyboard.GetCurrentColors());

            GameCompletionResult? completion = isCorrect
                ? await RegisterSuccessfulAttemptAsync()
                : await RegisterFailedAttemptAsync();

            return CreateCheckResult(isCorrect, completion: completion);
        }

        private bool EvaluateVisiblePartKeyboardStage(PianoKeyboard pianoKeyboard)
        {
            if (!HasSingleMissingAddendQuestion())
                return false;

            int visiblePart = GetVisibleKnownAddendValue();
            int missingPart = GetMissingAddendValue();

            if (Config.KeyboardConfig.ColorInteractionMode == KeyboardColorInteractionMode.AddSecondColor)
            {
                return pianoKeyboard.GetColorCount(Colors.Yellow) == visiblePart &&
                       pianoKeyboard.GetColorCount(Colors.LightGreen) == missingPart;
            }

            int totalPressed = pianoKeyboard.GetNonFreeColorCount();
            return totalPressed == Sum && Math.Max(0, totalPressed - visiblePart) == missingPart;
        }

        private bool EvaluateWholePressedKeyboardStage(PianoKeyboard pianoKeyboard)
        {
            if (!HasSingleMissingAddendQuestion())
                return false;

            int visiblePart = GetVisibleKnownAddendValue();
            int missingPart = GetMissingAddendValue();

            if (Config.KeyboardConfig.ColorInteractionMode != KeyboardColorInteractionMode.RemoveWithRed)
            {
                return pianoKeyboard.GetNonFreeColorCount() == Sum &&
                       pianoKeyboard.GetColorCount(Colors.Yellow) == visiblePart;
            }

            return pianoKeyboard.GetColorCount(Colors.Red) == missingPart &&
                   pianoKeyboard.GetColorCount(Colors.Yellow) == visiblePart;
        }

        private bool UsesDynamicKeyboardMultiplication()
        {
            return CurrentOperation == Operation.Multiplication &&
                   Config?.KeyboardConfig?.UseDynamicMultiplicationWeights == true &&
                   _dynamicKeyboardWeightValue.HasValue &&
                   _dynamicKeyboardExpectedPressCount.HasValue;
        }

        private bool UsesWeightedSingleTargetKeyboardStage()
        {
            return Config?.KeyboardConfig?.WeightsArray != null &&
                   Config.KeyboardConfig.WeightsArray.Length > 0 &&
                   Config.UIQuestionType == UIQuestionType.OneText &&
                   !UsesDynamicKeyboardMultiplication() &&
                   !UsesWeightedCustomStageTargets() &&
                   CurrentOperation == Operation.Sum;
        }

        private bool UsesWeightedCustomStageTargets()
        {
            return Config?.KeyboardConfig?.UseWeightedCustomStageTargets == true &&
                   Config.KeyboardConfig.WeightsArray != null &&
                   Config.KeyboardConfig.WeightsArray.Length > 0 &&
                   Config.UIQuestionType == UIQuestionType.OneText;
        }

        private bool UsesWeightedKeyboardTargetGeneration()
        {
            return UsesWeightedSingleTargetKeyboardStage() || UsesWeightedCustomStageTargets();
        }

        public bool SupportsImpossibleWeightedAnswer =>
            UsesWeightedCustomStageTargets() &&
            Config?.KeyboardConfig?.AllowImpossibleWeightedAnswer == true &&
            _weightedCustomImpossibleSums.Count > 0;

        public bool CurrentWeightedTargetRequiresImpossibleAnswer => _currentWeightedTargetRequiresImpossibleAnswer;

        private void EnsureWeightedTargetPools()
        {
            if (!UsesWeightedKeyboardTargetGeneration())
                return;

            int[] weights = Config.KeyboardConfig.WeightsArray
                .Where(weight => weight > 0)
                .Take(10)
                .ToArray();

            if (weights.Length == 0)
                return;

            int minTarget = Config.MinSum;
            int maxTarget = Config.MaxSum;
            bool allowImpossible = UsesWeightedCustomStageTargets() && Config.KeyboardConfig.AllowImpossibleWeightedAnswer;
            string cacheKey = $"{minTarget}:{maxTarget}:{allowImpossible}:{string.Join(",", weights)}";

            if (string.Equals(_weightedTargetPoolCacheKey, cacheKey, StringComparison.Ordinal))
                return;

            _weightedTargetPoolCacheKey = cacheKey;
            _weightedCustomReachableSums.Clear();
            _weightedCustomImpossibleSums.Clear();
            _currentWeightedTargetRequiresImpossibleAnswer = false;

            HashSet<int> rollingSums = new() { 0 };

            foreach (int weight in weights)
            {
                HashSet<int> nextSums = new(rollingSums);
                foreach (int existingSum in rollingSums)
                {
                    int nextSum = existingSum + weight;
                    if (nextSum <= maxTarget)
                        nextSums.Add(nextSum);
                }

                rollingSums = nextSums;
            }

            foreach (int reachableSum in rollingSums)
            {
                if (reachableSum >= minTarget && reachableSum <= maxTarget)
                    _weightedCustomReachableSums.Add(reachableSum);
            }

            if (allowImpossible)
            {
                for (int target = minTarget; target <= maxTarget; target++)
                {
                    if (!_weightedCustomReachableSums.Contains(target))
                        _weightedCustomImpossibleSums.Add(target);
                }
            }
        }

        private int[] BuildWeightedCustomStageFactors(Random r)
        {
            EnsureWeightedTargetPools();

            List<int> weightedTargets = _weightedCustomReachableSums
                .Concat(_weightedCustomImpossibleSums)
                .Distinct()
                .OrderBy(value => value)
                .ToList();

            if (weightedTargets.Count == 0)
            {
                int fallback = Math.Max(1, Config.MinSum);
                _currentWeightedTargetRequiresImpossibleAnswer = false;
                return new[] { NAN, NAN, fallback };
            }

            int chosenTarget = weightedTargets[r.Next(weightedTargets.Count)];
            _currentWeightedTargetRequiresImpossibleAnswer = _weightedCustomImpossibleSums.Contains(chosenTarget);
            return new[] { NAN, NAN, chosenTarget };
        }

        private int[] BuildWeightedSingleTargetFactors(Random r)
        {
            EnsureWeightedTargetPools();

            if (_weightedCustomReachableSums.Count == 0)
            {
                int fallback = Math.Max(1, Config.MinSum);
                _currentWeightedTargetRequiresImpossibleAnswer = false;
                return new[] { NAN, NAN, fallback };
            }

            List<int> weightedTargets = _weightedCustomReachableSums.OrderBy(value => value).ToList();
            int chosenTarget = weightedTargets[r.Next(weightedTargets.Count)];
            _currentWeightedTargetRequiresImpossibleAnswer = false;
            return new[] { NAN, NAN, chosenTarget };
        }

        private async Task<ExerciseCheckResult> EvaluateDynamicKeyboardMultiplicationAsync(PianoKeyboard pianoKeyboard)
        {
            IncrementGuessNumber();

            int pressedKeysCount = pianoKeyboard.ToBitArray().Count(bit => bit);
            bool isCorrect =
                pianoKeyboard.Sum == Sum &&
                pressedKeysCount == _dynamicKeyboardExpectedPressCount.Value;

            _status = isCorrect ? Statement.True : Statement.False;
            await SaveKeyboardAttemptSnapshotAsync(pianoKeyboard.ToBitArray(), isCorrect, DateTime.Now, pianoKeyboard.GetCurrentColors());

            GameCompletionResult? completion = isCorrect
                ? await RegisterSuccessfulAttemptAsync()
                : await RegisterFailedAttemptAsync();

            return CreateCheckResult(isCorrect, completion: completion);
        }

        private async Task<ExerciseCheckResult> EvaluateWeightedSingleTargetKeyboardStageAsync(PianoKeyboard pianoKeyboard)
        {
            IncrementGuessNumber();

            bool isCorrect = pianoKeyboard.Sum == Sum;

            _status = isCorrect ? Statement.True : Statement.False;
            await SaveKeyboardAttemptSnapshotAsync(pianoKeyboard.ToBitArray(), isCorrect, DateTime.Now, pianoKeyboard.GetCurrentColors());

            GameCompletionResult? completion = isCorrect
                ? await RegisterSuccessfulAttemptAsync()
                : await RegisterFailedAttemptAsync();

            return CreateCheckResult(isCorrect, completion: completion);
        }

        private async Task<ExerciseCheckResult> EvaluateWeightedCustomStageAsync(PianoKeyboard pianoKeyboard)
        {
            IncrementGuessNumber();

            bool isCorrect = !_currentWeightedTargetRequiresImpossibleAnswer &&
                             pianoKeyboard.Sum == Sum;

            _status = isCorrect ? Statement.True : Statement.False;
            await SaveKeyboardAttemptSnapshotAsync(pianoKeyboard.ToBitArray(), isCorrect, DateTime.Now, pianoKeyboard.GetCurrentColors());

            GameCompletionResult? completion = isCorrect
                ? await RegisterSuccessfulAttemptAsync()
                : await RegisterFailedAttemptAsync();

            return CreateCheckResult(isCorrect, completion: completion);
        }

        public virtual async Task<ExerciseCheckResult> EvaluateImpossibleWeightedAnswerAsync()
        {
            if (!SupportsImpossibleWeightedAnswer)
                return CreateCheckResult(isCorrect: false);

            IncrementGuessNumber();

            bool isCorrect = _currentWeightedTargetRequiresImpossibleAnswer;
            _status = isCorrect ? Statement.True : Statement.False;

            GameCompletionResult? completion = isCorrect
                ? await RegisterSuccessfulAttemptAsync()
                : await RegisterFailedAttemptAsync();

            return CreateCheckResult(isCorrect, completion: completion);
        }

        private int GetAlternateValidSum(int excludedSum)
        {
            if (Config.MinSum != excludedSum)
                return Config.MinSum;

            if (Config.MaxSum != excludedSum)
                return Config.MaxSum;

            return excludedSum;
        }

        public virtual async Task<bool> CheckAsync(PianoKeyboard pianoKeyboard)
        {
            return (await EvaluateAsync(pianoKeyboard)).IsCorrect;
        }

        private async Task SaveKeyboardAttemptSnapshotAsync(bool[] submittedKeyboard, bool isCorrect, DateTime submittedTime, Color[]? submittedKeyboardColors = null)
        {
            if (!ShouldPersistKeyboardQuestion() || _keyboardQuestionRepository == null || _keyEventRepository == null)
                return;

            var savedAttempt = await _keyboardQuestionRepository.SaveSubmittedSnapshotAsync(
                GameId.ToString(),
                _questionNumber,
                submittedKeyboard,
                submittedTime,
                isCorrect ? 1 : 0,
                submittedKeyboardColors);

            if (savedAttempt == null)
                return;

            await FinalizeKeyboardAttemptAsync(savedAttempt, submittedTime);
        }

        protected async Task FinalizeKeyboardAttemptAsync(Data.SQLite.KeyboardQuestion savedAttempt, DateTime submittedTime)
        {
            if (_keyEventRepository == null || _keyboardQuestionRepository == null)
                return;

            await _keyEventRepository.AssignPendingEventsToAttemptAsync(GameId.ToString(), _questionNumber, savedAttempt.AttemptNumber);
            await _keyEventRepository.SaveCheckEventAsync(GameId.ToString(), _questionNumber, savedAttempt.AttemptNumber, submittedTime);

            List<KeyEvent> attemptEvents = await _keyEventRepository.GetAttemptEventsAsync(
                GameId.ToString(),
                _questionNumber,
                savedAttempt.AttemptNumber);

            KeyboardAttemptTimingMetrics metrics = KeyboardTimingAnalyzer.AnalyzeAttempt(attemptEvents, submittedTime);
            savedAttempt.KeyDownCount = metrics.KeyDownCount;
            savedAttempt.DistinctKeyCount = metrics.DistinctKeyCount;
            savedAttempt.PressClusterCount = metrics.PressClusterCount;
            savedAttempt.LargestPressClusterSize = metrics.LargestPressClusterSize;
            savedAttempt.MaxInterKeyGapMs = metrics.MaxInterKeyGapMs;
            savedAttempt.AverageInterKeyGapMs = metrics.AverageInterKeyGapMs;
            savedAttempt.FirstKeyToSubmitMs = metrics.FirstKeyToSubmitMs;
            savedAttempt.LastKeyToSubmitMs = metrics.LastKeyToSubmitMs;
            savedAttempt.PressPatternKind = (int)metrics.PressPatternKind;

            await _keyboardQuestionRepository.UpdateAsync(savedAttempt);
        }

        public virtual Task<ExerciseGenerationResult> GenerateExerciseAsync()
        {
            Random r = new();

            ExercisePlanStep? step = AcquirePlanStep();

            ResolveOperation(r, step);
            ResolveQuestionSource(r, step);

            // runtime bookkeeping (same as your current pattern)
            BeginExercise();
            ExerciseGenerationResult generatedExercise = CreateGeneratedExerciseResult();
            return Task.FromResult(new ExerciseGenerationResult
            {
                ActionText = generatedExercise.ActionText,
                PersistenceTask = PersistGeneratedExerciseAsync()
            });
        }

        private void ResolveOperation(Random r, ExercisePlanStep? step)
        {
            if (step != null)
            {
                // Plan decides op (Fixed/Keep/RandomFromConfigList)
                ApplyOpMode(step);
                return;
            }

            // Legacy behavior: keep your triad logic
            if (_currentTriadIndex == 0 || _currentTriadIndex >= Config.RepeatingTimesOfTriad - 1)
            {
                CurrentOperation = Config.OperationList[r.Next(Config.OperationList.Count)];
            }
        }

        private void ResolveQuestionSource(Random r, ExercisePlanStep? step)
        {
            if (step != null)
            {
                if (step.Kind == PlanStepKind.RepeatQuestion && _prevPPWQuestion.HasValue)
                {
                    RestorePrevPPWQuestion();
                    if (step.OpMode == PlanOpMode.Fixed)
                        CurrentOperation = step.Operation;
                    return;
                }

                // NewQuestion (or Repeat requested but no prev yet)
                GenerateNewPPWQuestion(r);

                SnapshotPrevPPWQuestion();
                return;
            }

            // Legacy mode: your existing generation logic
            GenerateNewPPWQuestion(r);

            // your existing triad bookkeeping likely happens elsewhere;
            // if you handle it here, keep it:
            _currentTriadIndex = (_currentTriadIndex + 1) % (Config.RepeatingTimesOfTriad>1? Config.RepeatingTimesOfTriad: Config.RepeatingTimesOfSum);
            SnapshotPrevPPWQuestion();
        }

        private void GenerateNewPPWQuestion(Random r)
        {
            _isCurrentPairedBenchmarkAnchor = IsPairedCloseTriadBenchmarkAnchorQuestion();

            // pick factor set depending on operation
            int[] factors = UsesWeightedCustomStageTargets()
                ? BuildWeightedCustomStageFactors(r)
                : UsesWeightedSingleTargetKeyboardStage()
                    ? BuildWeightedSingleTargetFactors(r)
                    : (CurrentOperation == Operation.Multiplication || CurrentOperation == Operation.Divide ? FactorsMultiplication : Factors);

            ConfigureDynamicKeyboardMultiplicationWeights(r, factors);

            if (Config.isLargerAddend1 && factors[0] < factors[1])
            {
                int temp = factors[0];
                factors[0] = factors[1];
                factors[1] = temp;
            }

            _prevResolvedTriad = new PPWObject(factors[0], factors[1], factors[2]);

            if (!_isCurrentPairedBenchmarkAnchor && !TryApplyDistortedRepeatVariant(factors))
            {
                foreach (int index in ChooseHiddenValueIndexes(r, factors))
                    factors[index] = NAN;
            }

            addend1 = factors[0];
            addend2 = factors[1];
            Sum = factors[2];
        }

        private bool IsPairedCloseTriadBenchmarkAnchorQuestion()
        {
            return Config?.UsePairedCloseTriadBenchmark == true &&
                   _questionNumber % 2 == 0;
        }

        private bool TryApplyDistortedRepeatVariant(int[] factors)
        {
            if (!Config.UseDistortedVariantInRepeatSequence ||
                factors == null ||
                factors.Length < 3 ||
                Config.RepeatingTimesOfTriad <= 1 ||
                _currentTriadIndex <= 0 ||
                _currentTriadIndex >= Config.RepeatingTimesOfTriad ||
                CurrentOperation != Operation.Sum ||
                !_prevPPWQuestion.HasValue)
            {
                return false;
            }

            if (!TryBuildDistortedDisplayedQuestion(_prevPPWQuestion.Value, out int displayA1, out int displayA2, out int displaySum))
                return false;

            factors[0] = displayA1;
            factors[1] = displayA2;
            factors[2] = displaySum;
            return true;
        }

        private bool TryBuildDistortedDisplayedQuestion((int a1, int a2, int s, Operation op, VariableTypes vt) previousQuestion, out int displayA1, out int displayA2, out int displaySum)
        {
            displayA1 = NAN;
            displayA2 = NAN;
            displaySum = NAN;

            if (previousQuestion.a1 == NAN)
            {
                int resolvedA1 = previousQuestion.s;
                int resolvedA2 = previousQuestion.a2;
                int resolvedSum = resolvedA1 + resolvedA2;
                if (!IsTriadWithinConfig(resolvedA1, resolvedA2, resolvedSum))
                    return false;

                displayA1 = resolvedA1;
                displayA2 = resolvedA2;
                displaySum = NAN;
                return true;
            }

            if (previousQuestion.a2 == NAN)
            {
                int resolvedA1 = previousQuestion.a1;
                int resolvedA2 = previousQuestion.s;
                int resolvedSum = resolvedA1 + resolvedA2;
                if (!IsTriadWithinConfig(resolvedA1, resolvedA2, resolvedSum))
                    return false;

                displayA1 = resolvedA1;
                displayA2 = resolvedA2;
                displaySum = NAN;
                return true;
            }

            if (previousQuestion.s == NAN)
            {
                if (previousQuestion.a1 >= previousQuestion.a2)
                {
                    int resolvedA1 = previousQuestion.a1 - previousQuestion.a2;
                    int resolvedA2 = previousQuestion.a2;
                    int resolvedSum = previousQuestion.a1;
                    if (resolvedA1 < 0 || !IsTriadWithinConfig(resolvedA1, resolvedA2, resolvedSum))
                        return false;

                    displayA1 = NAN;
                    displayA2 = resolvedA2;
                    displaySum = resolvedSum;
                    return true;
                }

                int altResolvedA1 = previousQuestion.a1;
                int altResolvedA2 = previousQuestion.a2 - previousQuestion.a1;
                int altResolvedSum = previousQuestion.a2;
                if (altResolvedA2 < 0 || !IsTriadWithinConfig(altResolvedA1, altResolvedA2, altResolvedSum))
                    return false;

                displayA1 = altResolvedA1;
                displayA2 = NAN;
                displaySum = altResolvedSum;
                return true;
            }

            return false;
        }

        private bool IsTriadWithinConfig(int candidateA1, int candidateA2, int candidateSum)
        {
            return candidateA1 >= Config.MinAddend &&
                   candidateA1 <= Config.MaxAddend &&
                   candidateA2 >= Config.EffectiveMinAddend2 &&
                   candidateA2 <= Config.EffectiveMaxAddend2 &&
                   candidateSum >= Config.MinSum &&
                   candidateSum <= Config.MaxSum;
        }

        private void ConfigureDynamicKeyboardMultiplicationWeights(Random r, int[] factors)
        {
            _dynamicKeyboardWeightValue = null;
            _dynamicKeyboardExpectedPressCount = null;

            if (CurrentOperation != Operation.Multiplication ||
                Config?.KeyboardConfig?.UseDynamicMultiplicationWeights != true ||
                factors == null ||
                factors.Length < 3)
            {
                return;
            }

            bool useFirstFactorAsWeight = r.Next(2) == 0;
            _dynamicKeyboardWeightValue = useFirstFactorAsWeight ? factors[0] : factors[1];
            _dynamicKeyboardExpectedPressCount = useFirstFactorAsWeight ? factors[1] : factors[0];

            int keyCount = Math.Max(
                1,
                (Config.KeyboardConfig.KeysInRow > 0 ? Config.KeyboardConfig.KeysInRow : 10) *
                Math.Max(1, Config.KeyboardConfig.Rows));

            Config.KeyboardConfig.WeightsArray = Enumerable.Repeat(_dynamicKeyboardWeightValue.Value, keyCount).ToArray();
            Config.KeyboardConfig.ShowNumbersOnKeys = true;
        }

        private List<int> ChooseHiddenValueIndexes(Random r, int[]? currentFactors = null)
        {
            List<int[]> candidates = new();
            int hiddenCount = Math.Clamp(Config.HiddenValueCount, 0, 3);

            if (hiddenCount == 0)
                return new List<int>();

            for (int mask = 1; mask < 8; mask++)
            {
                List<int> indexes = new();
                for (int i = 0; i < 3; i++)
                {
                    if ((mask & (1 << i)) != 0)
                        indexes.Add(i);
                }

                if (indexes.Count != hiddenCount)
                    continue;

                if (!indexes.All(IsHiddenTargetAllowed))
                    continue;

                if (Config.KeepsSumVisible && indexes.Contains(2))
                    continue;

                if (Config.KeepsAtLeastOneAddendVisible && indexes.Contains(0) && indexes.Contains(1))
                    continue;

                candidates.Add(indexes.ToArray());
            }

            if (Config.UseDistortedVariantInRepeatSequence &&
                CurrentOperation == Operation.Sum &&
                Config.RepeatingTimesOfTriad > 1 &&
                _currentTriadIndex == 0 &&
                hiddenCount == 1 &&
                currentFactors != null &&
                currentFactors.Length >= 3)
            {
                List<int[]> distortionFriendlyCandidates = candidates
                    .Where(indexes => CanDistortDisplayedQuestion(currentFactors[0], currentFactors[1], currentFactors[2], indexes[0]))
                    .ToList();

                if (distortionFriendlyCandidates.Count > 0)
                    candidates = distortionFriendlyCandidates;
            }

            if (candidates.Count == 0)
            {
                return Config.VariableTypes switch
                {
                    VariableTypes.SumOnly => new List<int> { 2 },
                    VariableTypes.TwoNoSum => new List<int> { 0, 1 },
                    VariableTypes.Three => new List<int> { r.Next(2) == 0 ? 1 : 0, 2 },
                    VariableTypes.OneCanBeSum => new List<int> { r.Next(3) },
                    _ => new List<int> { r.Next(2) }
                };
            }

            return candidates[r.Next(candidates.Count)].ToList();
        }

        private bool CanDistortDisplayedQuestion(int a1, int a2, int sum, int hiddenIndex)
        {
            (int displayA1, int displayA2, int displaySum) previousDisplay = hiddenIndex switch
            {
                0 => (NAN, a2, sum),
                1 => (a1, NAN, sum),
                2 => (a1, a2, NAN),
                _ => (a1, a2, sum)
            };

            return TryBuildDistortedDisplayedQuestion(
                (previousDisplay.displayA1, previousDisplay.displayA2, previousDisplay.displaySum, CurrentOperation, Config.VariableTypes),
                out _,
                out _,
                out _);
        }

        private bool IsHiddenTargetAllowed(int index)
        {
            MissingValueTargetFlags target = index switch
            {
                0 => MissingValueTargetFlags.Addend1,
                1 => MissingValueTargetFlags.Addend2,
                _ => MissingValueTargetFlags.Sum
            };

            return Config.AllowedMissingValueTargets.HasFlag(target);
        }

        private void RestorePrevPPWQuestion()
        {
            var q = _prevPPWQuestion.Value;

            addend1 = q.a1;
            addend2 = q.a2;
            Sum = q.s;

            CurrentOperation = q.op;

            // Optional: if you want repeats to preserve which-variable-hidden mode:
            // Config.VariableTypes = q.vt;
        }

        private void SnapshotPrevPPWQuestion()
        {
            _prevPPWQuestion = (addend1, addend2, Sum, CurrentOperation, Config.VariableTypes);
        }

        protected virtual int[] Factors
        {
            get
            {
                Random r = new();
                int[] factors = new int[3];
                if ((Config.RepeatingTimesOfTriad > 1 || Config.RepeatingTimesOfSum > 1) && _currentTriadIndex>0 &&
                    !(_currentTriadIndex >= Config.RepeatingTimesOfTriad && _currentTriadIndex>= Config.RepeatingTimesOfSum))
                {
                    PPWObject repeatSource = _prevResolvedTriad ?? new PPWObject(this.addend1, this.addend2, this.Sum);
                    factors[2] = repeatSource.Sum;
                    if (Config.RepeatingTimesOfTriad > 1)
                    {
                        factors[0] = repeatSource.Addend1;
                        factors[1] = repeatSource.Addend2;
                        if (r.Next(2) == 1)
                        {
                            factors[0] = repeatSource.Addend2;
                            factors[1] = repeatSource.Addend1;
                        }
                    }
                    else if (Config.RepeatingTimesOfSum > 1)
                    {
                        PPWObject newTriad = GenerateSecondaryTriad(factors[2]);
                        if (newTriad != null)
                        {
                            factors[0] = newTriad.Addend1;
                            factors[1] = newTriad.Addend2;
                        }
                        else
                        {
                            factors[0] = repeatSource.Addend1;
                            factors[1] = repeatSource.Addend2;
                        }
                    }
                    return factors;
                }   
                
                

                if (    IsFirstGuess /*&& !Config.OnlyThrougTen*/)
                {
                        factors[0] = Config.DefaultTriad.Addend1; factors[1] = Config.DefaultTriad.Addend2; factors[2] = Config.DefaultTriad.Sum; 
               
                        IsFirstGuess = false;
                        addend1 = factors[0]; addend2 = factors[1]; Sum = factors[2];
                        if (IsCorrectInput())
                            return factors;
                }

                if (Config.OnlyCloseTriad)
                {
                    if (Config.UsePairedCloseTriadBenchmark &&
                        _questionNumber > 1 &&
                        _questionNumber % 2 == 0)
                    {
                        int randomTriadIndex = r.Next(PossibleTriads.Count);
                        factors[2] = PossibleTriads[randomTriadIndex].Sum;
                        factors[0] = PossibleTriads[randomTriadIndex].Addend1;
                        factors[1] = PossibleTriads[randomTriadIndex].Addend2;
                        return factors;
                    }

                    int[] closeTriadMoveOptions = Config.AllowCloseTriadSumChange
                        ? new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }
                        : new[] { 0, 5, 6, 7, 8, 9, 10, 11 };
                    int chosenClosedTriad;
                    do
                    {
                        factors[2] = this.Sum;
                        factors[0] = this.addend1;
                        factors[1] = this.addend2;
                        chosenClosedTriad = closeTriadMoveOptions[r.Next(closeTriadMoveOptions.Length)];
                        switch (chosenClosedTriad)
                        {
                            case 0: case 7: case 8: 
                                factors[0]++; factors[1]--;
                                break;
                            case 9:
                            case 10:
                            case 11:
                                factors[0]++; factors[1]--;
                                factors[0]++; factors[1]--;
                                break;
                            case 1:
                                factors[0]++; factors[2]++;
                                break;
                            case 2:
                                factors[1]++; factors[2]++;
                                break;
                            case 3:
                                factors[0]--; factors[2]--;
                                break;
                            case 4:
                                factors[1]--; factors[2]--;
                                break;
                            case 5:
                                factors[0]--; factors[1]++;
                                break;
                             case 6:
                                break;
                        }
                    } 
                    while (!PossibleTriads.Contains(new PPWObject(factors[0], factors[1], factors[2])));
                    return factors;
                }

                if (PossibleTriads.Count == 0)
                {
                    PPWObject fallbackTriad = Config.DefaultTriad;
                    factors[0] = fallbackTriad.Addend1;
                    factors[1] = fallbackTriad.Addend2;
                    factors[2] = fallbackTriad.Sum;
                    return factors;
                }

                int currentTriadIndex = r.Next(PossibleTriads.Count);
                factors[2] = PossibleTriads[currentTriadIndex].Sum;//r.Next(Config.MinSum, Config.MaxSum + 1);
                factors[0] = PossibleTriads[currentTriadIndex].Addend1;//GenerateNewAddend(factors[2]);
                factors[1] = PossibleTriads[currentTriadIndex].Addend2;//factors[2] - factors[0];
                return factors;
                /*
                //factors[2] = r.Next(Config.MinAddend * 2, Config.MaxAddend*2 + 1);//This is instead MinSum MaxSum for negative numbers
                //Console.WriteLine("rand from {0}=>min({1},{2})", Config.MinAddend, Config.MaxAddend, factors[2] - Config.MinAddend);
                //if (_fInsisitentOnOne) factors[2] = _lastNum;
                factors[0] = r.Next(Config.MinAddend, Config.MaxAddend + 1);
                factors[1] = r.Next(Config.MinAddend, Config.MaxAddend + 1); 
                factors[2] = factors[0]+factors[1];
                while (factors[2] < Config.MinSum || factors[2]>Config.MaxSum )
                {
                    factors[0] = r.Next(Config.MinAddend, Config.MaxAddend + 1);
                    factors[1] = r.Next(Config.MinAddend, Config.MaxAddend + 1);
                    factors[2] = factors[0] + factors[1];
                }

                return factors;*/
            }
        }

        /* protected int[] FactorsThroughTen
         {
             get
             {
                 int[] factors = new int[3];
                 Random r = new();

                 factors[2] = r.Next(Math.Max(Config.MinAddend, Config.MinSum), Config.MaxSum);
                 while (factors[2] % 10 == 9 || factors[2] / 10 == 0) factors[2] = r.Next(Math.Max(Config.MinAddend, Config.MinSum), Config.MaxSum);
                 if (factors[2] % 10 == 0) factors[0] = r.Next(Config.MinAddend, Math.Min(Config.MaxAddend + 1, factors[2]));
                 else
                 {
                     int tens = r.Next(Math.Max(Config.MinAddend / 10, 0), factors[2] / 10 - 1);
                     int ones = r.Next(factors[2] % 10 + 1, 10);
                     factors[0] = tens * 10 + ones;
                 }
                 factors[1] = factors[2] - factors[0];

                 return factors;
             }
         }
 */
        private int[] FactorsMultiplication
        {
            get
            {
                Random r = new();
                int[] factors = new int[3];
                if ((Config.RepeatingTimesOfTriad > 1 || Config.RepeatingTimesOfSum > 1) && _currentTriadIndex > 0 &&
                    !(_currentTriadIndex >= Config.RepeatingTimesOfTriad && _currentTriadIndex >= Config.RepeatingTimesOfSum))
                {
                    PPWObject repeatSource = _prevResolvedTriad ?? new PPWObject(this.addend1, this.addend2, this.Sum);
                    factors[2] = repeatSource.Sum;
                    if (Config.RepeatingTimesOfTriad > 1)
                    {
                        factors[0] = repeatSource.Addend1;
                        factors[1] = repeatSource.Addend2;
                        if (r.Next(2) == 1)
                        {
                            factors[0] = repeatSource.Addend2;
                            factors[1] = repeatSource.Addend1;
                        }
                    }
                    else if (Config.RepeatingTimesOfSum > 1)
                    {
                        PPWObject newTriad = GenerateSecondaryTriad(factors[2]);
                        if (newTriad != null)
                        {
                            factors[0] = newTriad.Addend1;
                            factors[1] = newTriad.Addend2;
                        }
                        else
                        {
                            factors[0] = repeatSource.Addend1;
                            factors[1] = repeatSource.Addend2;
                        }
                    }
                    return factors;
                }

                if (Config.IsHistory)
                {
                    int currentTriadIndex = r.Next(PossibleTriads.Count);
                    factors[2] = PossibleTriads[currentTriadIndex].Sum;//r.Next(Config.MinSum, Config.MaxSum + 1);
                    factors[0] = PossibleTriads[currentTriadIndex].Addend1;//GenerateNewAddend(factors[2]);
                    factors[1] = PossibleTriads[currentTriadIndex].Addend2;//factors[2] - factors[0];
                    return factors;
                }

                if (IsFirstGuess)
                {
                    if (Config.DefaultTriad != null &&
                        Config.DefaultTriad.Addend1 * Config.DefaultTriad.Addend2 == Config.DefaultTriad.Sum)
                    {
                        factors[0] = Config.DefaultTriad.Addend1;
                        factors[1] = Config.DefaultTriad.Addend2;
                        factors[2] = Config.DefaultTriad.Sum;

                        IsFirstGuess = false;
                        addend1 = factors[0];
                        addend2 = factors[1];
                        Sum = factors[2];

                        if (IsCorrectInput())
                            return factors;
                    }

                    IsFirstGuess = false;
                }

                if (Config.OnlyCloseTriad && !IsFirstGuess)
                {
                    PPWObject? benchmarkTriad = IsMultiplicationBenchmarkSequenceMode()
                        ? GetPreferredBenchmarkMultiplicationTriad()
                        : null;

                    if (benchmarkTriad != null)
                    {
                        factors[0] = benchmarkTriad.Addend1;
                        factors[1] = benchmarkTriad.Addend2;
                        factors[2] = benchmarkTriad.Sum;
                        return factors;
                    }

                    List<PPWObject> closeTriads = PossibleTriads
                        .Where(item =>
                            item.Addend1 == this.addend1 &&
                            Math.Abs(item.Addend2 - this.addend2) <= 2 &&
                            !(item.Addend1 == this.addend1 && item.Addend2 == this.addend2))
                        .ToList();

                    if (closeTriads.Count > 0)
                    {
                        PPWObject chosenTriad = closeTriads[r.Next(closeTriads.Count)];
                        factors[0] = chosenTriad.Addend1;
                        factors[1] = chosenTriad.Addend2;
                        factors[2] = chosenTriad.Sum;
                        return factors;
                    }
                }

                if (PossibleTriads.Count > 0)
                {
                    PPWObject chosenTriad = PossibleTriads[r.Next(PossibleTriads.Count)];
                    factors[0] = chosenTriad.Addend1;
                    factors[1] = chosenTriad.Addend2;
                    factors[2] = chosenTriad.Sum;
                    return factors;
                }

                factors[0] = r.Next(Config.MinAddend, Config.MaxAddend + 1);
                factors[1] = r.Next(Config.MinAddend2, Config.MaxAddend2 + 1);
                factors[2] = factors[0] * factors[1];

                while (factors[2] < Config.MinSum || factors[2] > Config.MaxSum)
                {
                    factors[0] = r.Next(Config.MinAddend, Config.MaxAddend + 1);
                    factors[1] = r.Next(Config.MinAddend2, Config.MaxAddend2 + 1);
                    factors[2] = factors[0] * factors[1];
                }

                return factors;
            }
        }

        private bool IsMultiplicationBenchmarkSequenceMode()
        {
            return string.Equals(Config?.GameName, "Multiplication Benchmarks", StringComparison.Ordinal);
        }

        private PPWObject? GetPreferredBenchmarkMultiplicationTriad()
        {
            int benchmarkStepIndex = Math.Max(0, _questionNumber - 2);
            bool changeFirstMultiplier = benchmarkStepIndex % 2 == 0;
            int preferredDelta = GetBenchmarkPreferredDelta(benchmarkStepIndex);

            IEnumerable<PPWObject> candidates = PossibleTriads.Where(item =>
                !(item.Addend1 == this.addend1 && item.Addend2 == this.addend2));

            if (changeFirstMultiplier)
            {
                candidates = candidates.Where(item =>
                    item.Addend2 == this.addend2 &&
                    Math.Abs(item.Addend1 - this.addend1) <= 2);
            }
            else
            {
                candidates = candidates.Where(item =>
                    item.Addend1 == this.addend1 &&
                    Math.Abs(item.Addend2 - this.addend2) <= 2);
            }

            return OrderBenchmarkCandidates(
                    candidates,
                    candidate => changeFirstMultiplier
                        ? candidate.Addend1 - this.addend1
                        : candidate.Addend2 - this.addend2,
                    preferredDelta)
                .FirstOrDefault();
        }

        private static int GetBenchmarkPreferredDelta(int benchmarkStepIndex)
        {
            int[] preferredDeltas = { 1, 1, -1, 2, 2 };
            return preferredDeltas[benchmarkStepIndex % preferredDeltas.Length];
        }

        private static IEnumerable<PPWObject> OrderBenchmarkCandidates(
            IEnumerable<PPWObject> candidates,
            Func<PPWObject, int> getDelta,
            int preferredDelta)
        {
            return candidates
                .OrderBy(candidate => getDelta(candidate) == preferredDelta ? 0 : 1)
                .ThenBy(candidate => Math.Sign(getDelta(candidate)) == Math.Sign(preferredDelta) ? 0 : 1)
                .ThenBy(candidate => Math.Abs(getDelta(candidate) - preferredDelta))
                .ThenBy(candidate => Math.Abs(getDelta(candidate)));
        }


        #region History
        public List<PPWObject> AllHistory = new();

        public List<PPWObject> PossibleTriads = new();
        //private int _currentTriadIndex = -1;


        public void GeneratePossibleTriadsSet()
        {
            PossibleTriads.Clear();
            int minAddend = Config.MinAddend, maxAddend = Config.MaxAddend, minSum = Config.MinSum, maxSum = Config.MaxSum;
            int minAddend2 = Config.EffectiveMinAddend2;
            int maxAddend2 = Config.EffectiveMaxAddend2;

            for (int i = minAddend; i <= maxAddend; i++)
                for (int j = minAddend2; j <= (Config.IsHistorySymetrical ? Math.Min(i, maxAddend2) : maxAddend2); j++)
                {
                    int sum = (CurrentOperation == Operation.Multiplication || CurrentOperation == Operation.Divide) ? (i * j) : (i + j);
                    if (sum >= minSum && sum <= maxSum)
                        if (!Config.OnlyThrougTen && !Config.OnlyToTen)
                        {
                            PossibleTriads.Add(new PPWObject(i, j, sum));
                            Console.WriteLine("{0} {1}= {2}", i, j, sum);
                        }
                        else if (!Config.OnlyToTen && (i / 10 + j / 10) < (i + j) / 10 && (i + j) % 10 != 0)
                        {
                            PossibleTriads.Add(new PPWObject(i, j, sum));
                            Console.WriteLine("{0} {1}= {2}", i, j, sum);
                        }
                        else if (!Config.OnlyThrougTen && i + j <= 10)
                        {
                            PossibleTriads.Add(new PPWObject(i, j, sum));
                            Console.WriteLine("{0} {1}= {2}", i, j, sum);
                        }
                }
        }

        /*
                private List<int> _impossibleSums = new();
                private int GenerateNewAddend(int newSum)
                {
                    ArrayList possibleAddends = new();
                    for (int i = Math.Max(Config.MinAddend, newSum - Config.MaxAddend); i <= Math.Min(Config.MaxAddend, newSum - Config.MinAddend); i++)
                    {
                        bool isExist = false;
                        foreach (PPWObject ppw in AllHistory)
                            if (ppw.Sum == newSum && ppw.Addend1 == i) isExist = true;
                        if (!isExist)
                            possibleAddends.Add(i);
                    }
                    if (possibleAddends.Count > 0) { Random r = new(); return (int)possibleAddends[r.Next(possibleAddends.Count)]; }

                    if (!_impossibleSums.Contains(newSum)) _impossibleSums.Add(newSum);
                    return NAN;
                }*/

        #endregion
    }
}
