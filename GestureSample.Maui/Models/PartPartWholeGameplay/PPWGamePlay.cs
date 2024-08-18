using GestureSample.Views.Tests;

namespace GestureSample.Maui.Models
{

    internal class PPWGamePlay
    {
        public static readonly int NAN = -1111;
        public int addend1;
        public int addend2;
        public virtual int Sum { get; set; }

        public Operation CurrentOperation { get; set; }

        private int _guessNumber = 0;
        public int GuessNumber { get { return _guessNumber; } }

        protected string _status = Statement.Neutral;
        public string Status { get => _status; }

        private bool _isFirstGuess = true;

        protected readonly SimpleViewCellsPage _view;

        public GameConfig Config;
        public PPWGamePlay(SimpleViewCellsPage view, GameConfig config)
        {
            _view = view; Config = config;
            CurrentOperation = Config.OperationList[0];
            GeneratePossibleTriadsSet();
        }

        private bool IsCorrectInput()
        {
            int minAddend2 = Config.MinAddend2 == NAN ? Config.MinAddend : Config.MinAddend2;
            int maxAddend2 = Config.MaxAddend2 == NAN ? Config.MaxAddend : Config.MaxAddend2;
            if (addend1 > Config.MaxAddend || addend1 < Config.MinAddend || addend2 > maxAddend2 || addend2 < minAddend2 || Sum > Config.MaxSum || Sum < Config.MinSum)
                return false;
            return true;
        }

        public virtual bool Check()
        {
            if (!IsCorrectInput())
                _status = Statement.WrongInput;
            else
            {
                _guessNumber++;
                _status = CurrentOperation switch
                {
                    Operation.Multiplication => (addend1 * addend2 == Sum) ? Statement.True : Statement.False,
                    //GameType.Logic => Statement.True,
                    Operation.Sum => (addend1 + addend2 == Sum) ? Statement.True : Statement.False,
                    _ => Statement.True
                };
                if (Config.IsHistory && _status == Statement.True)
                {
                    if (AllHistory.Where(item => item.Sum == Sum && item.Addend1 == addend1).Any() ||
                                (Config.IsHistorySymetrical && AllHistory.Where(item => item.Sum == Sum && item.Addend1 == addend1).Any()))
                        _status = Statement.New;
                    else
                        RemoveItemToHistory(addend1, addend2, Sum);
                    if (PossibleTriads.Count == 0)
                    {
                        _status = Statement.Win;
                        GeneratePossibleTriadsSet();
                        AllHistory.Clear(); Config.VariableTypes = (Config.VariableTypes == VariableTypes.OneNoSum) ? VariableTypes.TwoNoSum : VariableTypes.OneNoSum;

                    }
                }
            }
            _view.UpdateView();
            return _status == Statement.True;
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

                AllHistory.Add(new PPWObject(addend2, addend1, Sum));
            }
        }

        public virtual bool Check(int a1, int a2, int s)
        {
            addend1 = a1; addend2 = a2; Sum = s;
            return Check();
        }

        public virtual async Task<bool> CheckAsync(PianoKeyboard pianoKeyboard)
        {
            bool b = Check(pianoKeyboard.Addend1, pianoKeyboard.Addend2, Sum);
            await Task.Delay(Config.SecondsTillNextExercise * 1000);
            return b;
        }

        public virtual void GenerateExercise()
        {
            Random r = new();
            CurrentOperation = Config.OperationList[r.Next(Config.OperationList.Count)];

            int[] factors = Factors;
            if (CurrentOperation == Operation.Multiplication || CurrentOperation == Operation.Divide)
                factors = FactorsMultiplication;//TODO: Make a list for multiplication triads
            //else if (Config.OnlyThrougTen)
            //    factors = FactorsThroughTen;

            Console.WriteLine("Factors:{0}{1}{2}={3}", factors[0], CurrentOperation.ToDString(), factors[1], factors[2]);
            int n = (Config.VariableTypes == VariableTypes.OneCanBeSum || Config.VariableTypes == VariableTypes.TwoAny) ? r.Next(3) : r.Next(2);
            switch (Config.VariableTypes)
            {
                case VariableTypes.OneCanBeSum:
                case VariableTypes.OneNoSum:
                    factors[n] = NAN; break;
                case VariableTypes.SumOnly:
                    factors[2] = NAN; break;
                case VariableTypes.TwoNoSum:
                    factors[0] = NAN; factors[1] = NAN; break;
                case VariableTypes.TwoAny:
                default:
                    for (int i = 0; i < 3; i++)
                        if (i != n) factors[i] = NAN;
                    break;
            }

            addend1 = factors[0];
            addend2 = factors[1];
            Sum = factors[2];

            _status = Statement.Neutral;
            _guessNumber = 0;
            _view.UpdateView(true);
        }

        protected virtual int[] Factors
        {
            get
            {
                int[] factors = new int[3];

                if (_isFirstGuess && !Config.OnlyThrougTen)
                {
                    factors[0] = 2; factors[1] = 3; factors[2] = 5;
                    _isFirstGuess = false;
                    addend1 = 2; addend2 = 3; Sum = 5;
                    if (IsCorrectInput())
                        return factors;
                }

                Random r = new();
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
                int[] factors = new int[3];
                Random r = new();

                factors[0] = r.Next(Config.MinAddend, Config.MaxAddend + 1);
                factors[1] = r.Next(Config.MinAddend, Config.MaxAddend + 1);
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
            int minAddend2 = Config.MinAddend2 == NAN ? minAddend : Config.MinAddend2;
            int maxAddend2 = Config.MaxAddend2 == NAN ? maxAddend : Config.MaxAddend2;
            bool isSymetrical = Config.IsHistorySymetrical;

            for (int i = minAddend; i <= maxAddend; i++)
                for (int j = minAddend2; j <= (isSymetrical ? Math.Min(i, maxAddend2) : maxAddend2); j++)
                {
                    int sum = (CurrentOperation == Operation.Multiplication || CurrentOperation == Operation.Divide) ? i * j : (i + j);
                    if (sum >= minSum && sum <= maxSum)
                        if (!Config.OnlyThrougTen)
                        {
                            PossibleTriads.Add(new PPWObject(i, j, sum));
                            Console.WriteLine("{0} {1}= {2}", i, j, sum);
                        }
                        else if ((i / 10 + j / 10) < (i + j) / 10 && (i + j) % 10 != 0)
                            PossibleTriads.Add(new PPWObject(i, j, sum));
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
