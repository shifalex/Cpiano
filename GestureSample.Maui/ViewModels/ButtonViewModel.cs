
//using Foundation;
using GestureSample.Maui;
using GestureSample.Maui.Models;
//using Sentry;
using System.Collections;
using System.Windows.Input;

namespace GestureSample.ViewModels
{
    public partial class ButtonViewModel : CustomEventArgsViewModel
    {
        //private readonly bool ASSERT = false;
        private readonly int NAN = 0;

        private int _addent1;

        private int _addent2;

        private int _sum = 5;

        private string _gameType = "";

        public string SAddent1
        {
            get
            {
                if (_onlyOneAddent) return (_addent1 + _addent2).ToString();
                if (_addent1 == NAN && (!_isPiano)) return "";
                if (_addent1 == NAN) return "0";
                return _addent1.ToString();
            }
            set
            {   
                if (_onlyOneAddent) return;
                int _addent22;
                try { _addent22 = Int32.Parse(value); } catch { _addent22 = NAN; }
                SetProperty(ref _addent1, _addent22); 
                //OnPropertyChanged(nameof(Addent1)); OnPropertyChanged(nameof(TrueStatement));
            }
        }
        public string SAddent2
        {
            get
            {
                if (_addent2 == NAN && (!_isPiano)) return "";
                if (_addent2 == NAN) return "0";
                return _addent2.ToString();
            }
            set
            {
                int _addent22;
                try { _addent22 = Int32.Parse(value); } catch { _addent22 = NAN; }
                SetProperty(ref _addent2, _addent22); 
                //OnPropertyChanged(nameof(Addent2)); OnPropertyChanged(nameof(TrueStatement));
            }
        }
        public string SSum
        {
            get
            {
                if (_sum == NAN && (!_isPiano)) return "";
                if (_sum == NAN) return "0";             
                return _sum.ToString();                        
            }
            set
            {
                int _sum2;
                try { _sum2 = Int32.Parse(value); } catch { _sum2 = NAN; }
                SetProperty(ref _sum, _sum2);                            
                //OnPropertyChanged(nameof(Sum)); OnPropertyChanged(nameof(TrueStatement));
            }
        }

        private bool test=true;
            public string STest
        {
            get { if(test) return "true"; return "false"; }
            set { SetProperty(ref test, !test); }

        }

        public int Addent1
        {
            get { return _addent1; }
            set { SetProperty(ref _addent1, value); }
        }



        public int Addent2
        {
            get { return _addent2; }
            set { SetProperty(ref _addent2, value);  }
        }

        private Color _bgColor = Color.FromArgb("FFFFFF");
        public Color Color
        {
            get
            {
                return _bgColor;
            }
            set
            {
                SetProperty(ref _bgColor, Color.FromArgb("FFFFFF"));
                //_bgColor=value;
                NotifyPropertyChanged(nameof(Color));
            }
        }

        private void SaveState()
        {
             bool[] keysPressed= new bool[10];
            for(int i=0; i<10;i++) keysPressed[i]=false;
            if(_isPiano)
            {
                foreach (Button b in buttons)
                {
                    if (b.BackgroundColor == Colors.Yellow)
                        keysPressed[Convert.ToInt32( b.CommandParameter)] =true;
                }
            }
           
            /*App.CurrentDB.Add(new State
            {
                UserId = 1,
                TimeStamp = DateTime.Now,
                TypeName = _gameType,
                Addent1 = _addent1,
                Addent2 = _addent2,
                Sum = _sum, //TODO:make more elegant
                B1 = keysPressed[0],
                B2 = keysPressed[1],
                B3 = keysPressed[2],
                B4 = keysPressed[3],
                B5 = keysPressed[4],
                B6 = keysPressed[5],
                B7 = keysPressed[6],
                B8 = keysPressed[7],
                B9 = keysPressed[8],
                B10 = keysPressed[9]

            }) ; */
        }

        private bool _isFirstGuess = true;
        public bool IsNotFirstGuess
        {
            get { return !_isFirstGuess; }
        }
        private bool _isEnabledTotal = true;
        public bool IsEnabledTotal {
        get{ return _isEnabledTotal; }

            set { SetProperty(ref _isEnabledTotal, value); }
        }
        public String TrueStatement
        {
            get
            {


                if (_seconds_pressed > 0 && _seconds_pressed < 3)
                    return SecondsToEnd;
                if (_isFirstGuess) {
                    return "| |";
                }
                else if (_mult)
                {
                    if(_addent1 > _maxAddent || _addent1 < _minAddent || _addent2 > _maxAddent || _addent2 < _minAddent || _sum > 100 || _sum < _minAddent) return "wrong input!";
                    else if (_sum == _addent1 * _addent2)
                    {
                        return "CORRECT :D";
                    }
                    else
                    {
                        return "WRONG :(";
                    }
                }
                else if (!_isPiano && (_addent1 > _maxAddent || _addent1 < _minAddent || _addent2 > _maxAddent || _addent2 < _minAddent || _sum > _maxSum || _sum < _minAddent)) return "wrong input!";
                else if (_sum == _addent1 + _addent2)
                {
                    if (_requireNewAddents)
                    {
                        foreach (PPWObject ppw in _allHistory)
                            if (ppw.Sum == _sum && ppw.Addent1 == _addent1)
                            {

                                //Addent1 = NAN; Addent2 = NAN;
                                return "Find NEW combination";
                            }

                    }
                    NotifyPropertyChanged(nameof(SumEnabled)); NotifyPropertyChanged(nameof(Addent1Enabled)); NotifyPropertyChanged(nameof(Addent2Enabled));
                    _allHistory.Add(new PPWObject(_addent1, _addent2, _sum));

                    if (_decompositionLevel > 0)
                    {
                        StreakCorrect++; StreakWrong = 0;
                        if (StreakCorrect >= 20)
                        {
                            DecompositionLevel++; StreakCorrect = 0;
                            if (_decompositionLevel > 3)
                            {

                                //if(ASSERT) SentrySdk.CaptureMessage("Win");
                                Application.Current.MainPage.DisplayAlert("Win", "You Won!!", "OK");
                                return "YOU WON!!!!!!";
                            }
                        }
                    }
                    //if (ASSERT) SentrySdk.CaptureMessage("Correct");
                    //Sentry.SentrySdk.CaptureMessage(string.Format("  Correct: {0}={1}+{2}", _sum, _addent1, _addent2));

                    IsEnabledTotal = false;
                    NotifyPropertyChanged(nameof(History));
                    return "CORRECT :D";
                }
                else
                {

                    if (_decompositionLevel > 0)
                    {
                        StreakWrong++;
                        if (StreakWrong > 5)
                        {
                            DecompositionLevel--; StreakCorrect = 0; StreakWrong = 0;
                            if (_decompositionLevel == 0)
                            {
                                //if (ASSERT) SentrySdk.CaptureMessage("Lose");

                                Application.Current.MainPage.DisplayAlert("Lose", "You Lost!!", "OK");
                                return "YOU LOST!!!!!!";
                            }
                        }
                    }
                    //if (ASSERT) SentrySdk.CaptureMessage("Incorrect");
                    //SentrySdk.CaptureMessage(string.Format("Incorrect: {0}={1}+{2}", _sum, _addent1, _addent2));

                    return "WRONG :(";
                }
            }
        }




        //TODO:Add long press

        public ICommand CheckCommand { get; private set; }
        public ICommand NextCommand { get; private set; }
        readonly MR.Gestures.Button[] _keys;
        readonly private bool _isSync;
        readonly private bool _isPiano;
        readonly private bool _isNotBlind;
        readonly private bool _mult = false;
        public bool IsReadOnly { get { return _isPiano; } }
        public bool IsNotReadOnly { get { return !_isPiano; } }
        //public int PianoRowHeight { get { return 0; } }// _isPiano ? "2*" : "0"; } }

        public Color PianoBG { get { return _isPiano ? Colors.Black: Colors.AntiqueWhite; } }
        private bool _zeroToFive;
        public bool ZeroToFive { get { return _zeroToFive; } }
        public bool IsNotSync { get { return !_isSync; } }
        public bool IsNotBlind { get { return _isNotBlind; } }
        public bool ShowSecondsToEnd { get { return (_seconds_pressed>0 && _seconds_pressed <3); } }
        readonly private bool _onlyOneAddent;
        public bool HasTwoAddents { get { return (!_onlyOneAddent && _isNotBlind); } }

        public bool SumEnabled { get { return (_sum == NAN || _isPiano); } }
        public bool Addent1Enabled { get { return (_addent1 == NAN || _isPiano); } }
        public bool Addent2Enabled { get { return (_addent2 == NAN || _isPiano); } }


        public ButtonViewModel()
        {
            //SentrySdk.CaptureMessage("page build started");
            CheckCommand = new Command(() => Check());
            NextCommand = new Command(() => GenerateExercise());

            this._isPiano = true;
            NAN = -1111;
            _addent1 = NAN;
            _addent2 = NAN;
            NotifyPropertyChanged(nameof(SAddent1));
            NotifyPropertyChanged(nameof(SAddent2));
            this._isSync = false;
            this._onlyOneAddent = false;
            this._requireNewAddents = false;
            this._isNotBlind = true;
            
            _keys = new MR.Gestures.Button[10];

            SaveState();

            //SentrySdk.CaptureMessage("page build ended");


        }


        public ButtonViewModel(bool isPiano,bool isSync,bool onlyOneAddent, bool requireNewCombinations, bool isNotBlind=true) {
           // SentrySdk.CaptureMessage("page build started");
            CheckCommand = new Command(() => Check());
            NextCommand = new Command(() => GenerateExercise());
            _zeroToFive= !isPiano && !isSync && !onlyOneAddent;
            _gameType = string.Format("{0} {1} {2} {3} {4}", isPiano ? "Keyboard" : "number", isSync ? "Sync" : "Async", isNotBlind ? "SeeAll" : "Blind", onlyOneAddent ? "Combination" : "Solution", requireNewCombinations ? "New" : "Old");
            this._isPiano = isPiano;
            if (!isPiano) NAN = -1111;
            _addent1 = NAN;
            _addent2 = NAN;
            NotifyPropertyChanged(nameof(SAddent1));
            NotifyPropertyChanged(nameof(SAddent2));
            this._isSync = isSync;
            this._onlyOneAddent = onlyOneAddent;
            this._requireNewAddents = requireNewCombinations;
            this._isNotBlind = isNotBlind;
            if (isPiano == false && isSync == true && onlyOneAddent == false && requireNewCombinations == true)
            {
                IsDecomposition = true;
                _isSync = false;
                GenerateExercise();
                return;
            }
            if (isPiano == false && isSync == true && onlyOneAddent == true && requireNewCombinations == true)
            {
                _mult = true;
                _isSync = false;
                _onlyOneAddent = false;
                _requireNewAddents = false;
                _fMustFindOneTwoBoth = 1;
                _fMustFindTheSum = false;
                _maxAddent = 10;
                GenerateExercise();
                return;
            }

            SaveState();
            _keys = new MR.Gestures.Button[10];
            
                timer = Application.Current.Dispatcher.CreateTimer();
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += (s, e) => {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {

                        _seconds_pressed++;
                        //NotifyPropertyChanged(nameof(ShowSecondsToEnd));
                        if (_seconds_pressed >= 3 && _waiting_check)
                        {
                            _isTimerWorking = false; _waiting_check = false; _seconds_pressed = 0; 
                            Check();
                            timer.Stop();
                            
                            if (!IsEnabledTotal)
                            {   await Task.Delay(3000);
                                GenerateExercise();
                            }
                           
                        }
                        NotifyPropertyChanged(nameof(SecondsToEnd));
                        NotifyPropertyChanged(nameof(TrueStatement));
                    });
                };
           // SentrySdk.CaptureMessage("page build ended");


        }

        public void Check() { _isFirstGuess = false; NotifyPropertyChanged(nameof(IsNotFirstGuess)); NotifyPropertyChanged(nameof(TrueStatement)); SaveState(); }

        int _minAddent = 0;
        int _maxAddent = 5;
        int _minSum = 1;
        int _maxSum = 10;

        private int _streakCorrect = 0;
        private int _streakWrong = 0;


        public int StreakCorrect { get => _streakCorrect; set => SetProperty(ref _streakCorrect, value); }
        public int StreakWrong { get => _streakWrong; set => SetProperty(ref _streakWrong, value); }

        private int _decompositionLevel = -1;
        public int DecompositionLevel { get { return _decompositionLevel; }
            set { SetProperty(ref _decompositionLevel, value); }
        }
        private int _oldlevel = 2;
        public bool IsDecomposition
        {
            get { return _decompositionLevel != -1; }
            set
            {
                if (!value)
                {
                    _oldlevel = _decompositionLevel;
                    DecompositionLevel = -1;
                }
                else
                {
                    if (_decompositionLevel == -1 || _decompositionLevel == 4)
                        DecompositionLevel = _oldlevel;
                    if (_decompositionLevel == -1) DecompositionLevel = 2;
                    _fMustFindOneTwoBoth = 1;
                    _fMustFindTheSum = false;
                    _maxAddent = 20;
                    _minAddent = 1;
                    _maxSum = 20;
                    _requireNewAddents = false;
                    //FInsisitentOnOne = false;

                }

                //OnPropertyChanged();
                NotifyPropertyChanged(nameof(IsNotDecomposition));
            }
        }
        public bool IsNotDecomposition { get { return !IsDecomposition; } }
        private int _fMustFindOneTwoBoth = 2;

        private bool _fMustFindTheSum = true;
        private bool _requireNewAddents = true;
        private bool _freeCombination = true;


        private List<PPWObject> _allHistory = new();
        private List<int> _impossibleSums = new();
        public String History
        {
            get
            {
                String s = "";
                if (_requireNewAddents && _sum != NAN)
                {
                    s = "HISTORY:\n";
                    foreach (PPWObject ppw in _allHistory)
                        if (ppw.Sum == _sum)
                            s += ppw.Addent1 + "\t" + ppw.Addent2 + "\n";
                }

                return s;
                //return allHistory;
            }
        }

        private int GenerateNewAddent(int newSum)
        {
            ArrayList possibleAddents = new();
            for (int i = Math.Max(_minAddent, newSum - _maxAddent); i <= Math.Min(_maxAddent, newSum - _minAddent); i++)
            {
                bool isExist = false;
                foreach (PPWObject ppw in _allHistory)
                    if (ppw.Sum == newSum && ppw.Addent1 == i) isExist = true;
                if (!isExist)
                    possibleAddents.Add(i);
            }
            if (possibleAddents.Count > 0) { Random r = new(); return (int)possibleAddents[r.Next(possibleAddents.Count)]; }

            if (!_impossibleSums.Contains(newSum)) _impossibleSums.Add(newSum);
            return NAN;
        }

        

        public void GenerateExercise()
        {
            _isFirstGuess = true;
            //if (ASSERT)
            //    SentrySdk.CaptureMessage("Hello Sentry");
            if (_decompositionLevel == 1) { _minAddent = 0; _maxAddent = 10; _maxSum = 10; /*FInsisitentOnOne = true;*/ }
            if (_decompositionLevel == 2) { _minAddent = 0; _maxAddent = 20; _maxSum = 20; /*FInsisitentOnOne = false;*/ }
            if (_decompositionLevel == 3) { _minAddent = 0; _maxAddent = 100; _maxSum = 100; }
            //TODO: validation also in the form with Binding
            if (_minAddent < 0) _minAddent = 0;
            if (_maxAddent < _minAddent + 3) _maxAddent = _minAddent + 2;
            if (_maxSum > 2 * _maxAddent || _maxSum <= 2 * _minAddent) _maxSum = 2 * _maxAddent;
            

            int[] factors = new int[3];
            Random r = new();
            factors[2] = r.Next(_minSum, _maxSum + 1);
            //if (_fInsisitentOnOne) factors[2] = _lastNum;
            factors[0] = r.Next(_minAddent, Math.Min(_maxAddent, factors[2]) + 1);
            factors[1] = factors[2] - factors[0];

            if(_mult)
            {
                factors[0] = r.Next(_minAddent, Math.Min(_maxAddent, factors[2]) + 1);
                factors[1] = r.Next(_minAddent, Math.Min(_maxAddent, factors[2]) + 1);
                factors[2] = factors[0] * factors[1];

            }

            //if (ASSERT)
            //    SentrySdk.CaptureMessage("First factors success");



            if (_decompositionLevel > 1)
            {

                if (_sum != _addent1 + _addent2) StreakWrong++;//you moved next without solving
                int minSum = (_decompositionLevel >= 3) ? 20 : 10;
                factors[2] = r.Next(Math.Max(_minAddent, minSum), _maxSum);
                while (factors[2] % 10 == 9) factors[2] = r.Next(Math.Max(_minAddent, minSum), _maxSum);
                if (factors[2] % 10 == 0) factors[0] = r.Next(_minAddent, Math.Min(_maxAddent + 1, factors[2]));
                else
                {

                    int tens = r.Next(Math.Max(_minAddent / 10, 0), factors[2] / 10 - 1);
                    int ones = r.Next(factors[2] % 10 + 1, 10);
                    factors[0] = tens * 10 + ones;
                }
                factors[1] = factors[2] - factors[0];
            }            //if (ASSERT)
            //    SentrySdk.CaptureMessage("Second factors success");


            int questionType;
            if (_fMustFindOneTwoBoth == 1) questionType = 1;
            else if (_fMustFindOneTwoBoth == 2) questionType = 2;
            else questionType = r.Next(2);
            int n = r.Next(3);
            if (_fMustFindTheSum) n = 2;
            if (questionType == 1)
                factors[n] = NAN;
            else
                for (int i = 0; i < 3; i++)
                    if (i != n) factors[i] = NAN;
            //if (ASSERT)
            //    SentrySdk.CaptureMessage("Xs success");

            if (_requireNewAddents)
            {
                //make some win message before arriving to it
                if (_impossibleSums.Count >= _maxSum - 2 * _minAddent - 1)
                {
                    Application.Current.MainPage.DisplayAlert("Win", "You Won!!", "OK");
                    _impossibleSums.Clear(); _allHistory.Clear(); _freeCombination = !_freeCombination;
                }
                factors[0] = GenerateNewAddent(factors[2]);
                if (_freeCombination) factors[0] = NAN;
                while (_impossibleSums.Contains(factors[2]))
                {
                    if (_impossibleSums.Count >= (_maxSum - 2 * _minAddent) - 1)
                    {
                        Application.Current.MainPage.DisplayAlert("Win", "You Won!!", "OK");
                        _impossibleSums.Clear(); _allHistory.Clear(); _freeCombination = !_freeCombination;
                    }
                    factors[2] = r.Next(_minSum, _maxSum + 1);
                    factors[0] = GenerateNewAddent(factors[2]);
                    if (_freeCombination) factors[0] = NAN;
                }


            }


            //SAddent1 = factors[0].ToString();
            //SAddent2 = factors[1].ToString();
            SSum = factors[2].ToString();NotifyPropertyChanged(nameof(History));
            
            NotifyPropertyChanged(nameof(TrueStatement));
            IsEnabledTotal = true;

            NotifyPropertyChanged(nameof(IsNotFirstGuess));
            //NotifyPropertyChanged(SSum);
            if (_isPiano)
            {
                _addent1 = NAN;
                _addent2 = NAN;
            }
            else
            {
                _addent1 = factors[0];
                _addent2 = factors[1];
                _sum = factors[2];
            }
            NotifyPropertyChanged(nameof(SAddent1));
            NotifyPropertyChanged(nameof(SAddent2));
            NotifyPropertyChanged(nameof(SSum));
            NotifyPropertyChanged(nameof(Addent1Enabled));
            NotifyPropertyChanged(nameof(Addent2Enabled));
            NotifyPropertyChanged(nameof(SumEnabled));
            //Color = 
            //Color = Color.FromArgb("FFFFFF");
            STest = "";
            foreach(Button b in buttons) b.BackgroundColor= Color.FromArgb("FFFFFF");
            buttons.Clear();

            //Button[] buttons = this.Controls.OfType<Button>().ToArray();


            SaveState();
            //SentrySdk.CaptureMessage(string.Format("Question:{0}={1}+{2}", SSum, SAddent1, SAddent2));

            //if (ASSERT)
            //    SentrySdk.CaptureMessage("Pulling the entries success");



        }

        private List<Button> buttons = new();
        private int _seconds_pressed = 0;
        public string SecondsToEnd
        {
            get
            {
                return string.Format("{0}",3 - _seconds_pressed);
            }
        }


        private bool _waiting_check = false;
        private bool _isTimerWorking = false;
        private IDispatcherTimer timer;

        protected override void OnDown(MR.Gestures.DownUpEventArgs e)
		{
            
            if (!IsEnabledTotal) return;
            //AddText2("{0} was clicked.", ((Button)e.Sender).CommandParameter);
            base.OnDown(e);
            if(!buttons.Contains((Button)e.Sender)) buttons.Add((Button)e.Sender);
            if (_isSync)
            {
                ((VisualElement)e.Sender).BackgroundColor = Colors.Yellow;
                if (Convert.ToInt32(((Button)e.Sender).CommandParameter) > 4)
                    _addent2++;
                else
                    _addent1++;
                //if(_addent1==0 && _addent2==0) { _isTimerWorking = false; _waiting_check = false; _seconds_pressed = 0; return; }
                _waiting_check = true;
                _seconds_pressed = 0;
                if (!_isTimerWorking)
                {
                    timer.Start();
                    _isTimerWorking = true;
                }
             }
            else
            {
                if (((VisualElement)e.Sender).BackgroundColor != Colors.Yellow)

                    ((VisualElement)e.Sender).BackgroundColor = Colors.Yellow;

                else
                    ((VisualElement)e.Sender).BackgroundColor = Color.FromArgb("FFFFFF");
                
            }
            
            NotifyPropertyChanged(nameof(SAddent1)); NotifyPropertyChanged(nameof(SAddent2)); NotifyPropertyChanged(nameof(SecondsToEnd));

            SaveState();
        }

		protected override void OnUp(MR.Gestures.DownUpEventArgs e)
        {

            
            if (!IsEnabledTotal) return;
            base.OnUp(e);
            if (_isSync)
            {
                _seconds_pressed = 0;NotifyPropertyChanged(nameof(SecondsToEnd));
                ((VisualElement)e.Sender).BackgroundColor = Color.FromArgb("FFFFFF");
                if (Convert.ToInt32(((Button)e.Sender).CommandParameter) > 4)
                    _addent2--;
                else
                    _addent1--;
                if (_addent1 == 0 && _addent2 == 0) { _isTimerWorking = false; _waiting_check = false; _seconds_pressed = 0; timer.Stop(); NotifyPropertyChanged(nameof(TrueStatement)); }

            }
            else
            {

                if (Convert.ToInt32(((Button)e.Sender).CommandParameter) > 4)
                    if (((VisualElement)e.Sender).BackgroundColor != Colors.Yellow)
                        _addent2--;
                    else
                        _addent2++;
                else
                    if (((VisualElement)e.Sender).BackgroundColor != Colors.Yellow)
                    _addent1--;
                else
                    _addent1++;
            }
            if (_addent1 < 0) _addent1 = 0;
            if (_addent2 < 0) _addent2 = 0;
            //AddText2("{0} {1}", _addent1, _addent2);
            SaveState();
            NotifyPropertyChanged(nameof(SAddent1)); NotifyPropertyChanged(nameof(SAddent2)); 
        }
    }
}
