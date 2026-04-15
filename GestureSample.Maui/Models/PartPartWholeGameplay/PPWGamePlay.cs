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
        private bool _gameInitialized = false;

        // plan runtime
        private int _planStepIndex = 0;
        private int _planStepRepeatLeft = 0;
        private int _planSeed;
        private Random _planRandom;

        // snapshot of last question (PPW form)
        private (int a1, int a2, int s, Operation op, VariableTypes vt)? _prevPPWQuestion;
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


            GeneratePossibleTriadsSet();


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

            QuestionAnswer s = new()
            {

                GameId = this.GameId.ToString(),
                QuestionNumber = _questionNumber,
                Time = DateTime.Now,
                Op = CurrentOperation,
                Addend1 = this.addend1,
                Addend2 = this.addend2,
                Sum = this.Sum, //TODO:make more elegant
                ResultStatus = resultStatus
            };
            await _questionAnswerRepository.SaveAsync(s);
            qaState = s ;
            if (syncAfterSave)
                await TrySyncSupabaseStateAsync();
    }

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

        protected virtual Task PersistGeneratedExerciseAsync()
        {
            return SaveState(syncAfterSave: false);
        }

        protected ExerciseCheckResult CreateCheckResult(bool isCorrect, bool isWrongInput = false, GameCompletionResult? completion = null)
        {
            return new ExerciseCheckResult
            {
                IsCorrect = isCorrect,
                IsWrongInput = isWrongInput,
                Status = _status,
                Completion = completion
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
                await TrySyncSupabaseStateAsync();

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

        public virtual async Task<ExerciseCheckResult> EvaluateAsync(PianoKeyboard pianoKeyboard)
        {
            int keyboardSum = Sum;
            if (Sum == NAN && (pianoKeyboard.Addend1>=0 && pianoKeyboard.Addend2>=0)) {
               if( pianoKeyboard.Addend1 == addend1 && pianoKeyboard.Addend2 == addend2)
                 keyboardSum = pianoKeyboard.Sum; //if the sum is not set, it is set to the sum of addends
                else
                    keyboardSum = GetAlternateValidSum(pianoKeyboard.Sum);
            }
            ExerciseCheckResult result = await EvaluateAsync(pianoKeyboard.Addend1, pianoKeyboard.Addend2, keyboardSum);
            Console.WriteLine("CheckAsync(Enabled returned): {0} {1}={2}", pianoKeyboard.Addend1, pianoKeyboard.Addend2, Sum);
            return result;
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
            // pick factor set depending on operation
            int[] factors = (CurrentOperation == Operation.Multiplication || CurrentOperation == Operation.Divide)?factors = FactorsMultiplication: factors = Factors;

            if (Config.isLargerAddend1 && factors[0] < factors[1])
            {
                int temp = factors[0];
                factors[0] = factors[1];
                factors[1] = temp;
            }
            // Decide which value becomes NAN based on Config.VariableTypes
            int n = (Config.VariableTypes == VariableTypes.OneCanBeSum) ? r.Next(3) : r.Next(2);

            switch (Config.VariableTypes)
            {
                case VariableTypes.OneCanBeSum:
                case VariableTypes.OneNoSum:
                    factors[n] = NAN;
                    break;

                case VariableTypes.SumOnly:
                    factors[2] = NAN;
                    break;

                case VariableTypes.TwoNoSum:
                    factors[0] = NAN;
                    factors[1] = NAN;
                    break;

                default:
                    for (int i = 0; i < 3; i++)
                        if (i != n) factors[i] = NAN;
                    break;
            }

            addend1 = factors[0];
            addend2 = factors[1];
            Sum = factors[2];
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
                    factors[2] = this.Sum;
                    if (Config.RepeatingTimesOfTriad > 1)
                    {
                        factors[0] = this.addend1;
                        factors[1] = this.addend2;
                        if (r.Next(2) == 1)
                        {
                            factors[0] = this.addend2;
                            factors[1] = this.addend1;
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
                            factors[0] = this.addend1;
                            factors[1] = this.addend2;
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
                    int chosenClosedTriad;
                    do
                    {
                        factors[2] = this.Sum;
                        factors[0] = this.addend1;
                        factors[1] = this.addend2;
                        chosenClosedTriad = r.Next(12);
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
                    factors[2] = this.Sum;
                    if (Config.RepeatingTimesOfTriad > 1)
                    {
                        factors[0] = this.addend1;
                        factors[1] = this.addend2;
                        if (r.Next(2) == 1)
                        {
                            factors[0] = this.addend2;
                            factors[1] = this.addend1;
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
                            factors[0] = this.addend1;
                            factors[1] = this.addend2;
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

                if (Config.OnlyCloseTriad && !IsFirstGuess)
                {
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

                factors[0] = r.Next(Config.MinAddend, Config.MaxAddend + 1);
                factors[1] = r.Next(Config.MinAddend2, Config.MaxAddend2 + 1);
                factors[2] = factors[0] * factors[1];


                return factors;
            }
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
