using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections;
using GestureSample.Maui.ViewModels;

namespace GestureSample.ViewModels
{
    public partial class ButtonViewModel2 : CustomEventArgsViewModel
    {

        public ICommand CheckCommand { get; private set; }
        public ICommand NextCommand { get; private set; }
        readonly MR.Gestures.Button[] _keys;
        readonly private bool _isSync;
        readonly private bool _isPiano;
        readonly private bool _isNotBlind;
        readonly private bool _mult = false;
        //private readonly bool ASSERT = false;
        private readonly int NAN = 0;

        int _minaddend = 0;
        int _maxaddend = 5;
        int _minSum = 1;
        int _maxSum = 10;


        private int _addend1;

        private int _addend2;

        private int _sum = 5;

        public string Saddend1
        {
            get
            {
                if (_onlyOneaddend) return (_addend1 + _addend2).ToString();
                if (_addend1 == NAN && (!_isPiano)) return "";
                if (_addend1 == NAN) return "0";
                return _addend1.ToString();
            }
            set
            {
                if (_onlyOneaddend) return;
                int _addend22;
                try { _addend22 = Int32.Parse(value); } catch { _addend22 = NAN; }
                SetProperty(ref _addend1, _addend22);
                //OnPropertyChanged(nameof(Addend1)); OnPropertyChanged(nameof(TrueStatement));
            }
        }
        public string Saddend2
        {
            get
            {
                if (_addend2 == NAN && (!_isPiano)) return "";
                if (_addend2 == NAN) return "0";
                return _addend2.ToString();
            }
            set
            {
                int _addend22;
                try { _addend22 = Int32.Parse(value); } catch { _addend22 = NAN; }
                SetProperty(ref _addend2, _addend22);
                //OnPropertyChanged(nameof(Addend2)); OnPropertyChanged(nameof(TrueStatement));
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

        private bool test = true;
        public string STest
        {
            get { if (test) return "true"; return "false"; }
            set { SetProperty(ref test, !test); }

        }

        public int addend1
        {
            get { return _addend1; }
            set { SetProperty(ref _addend1, value); }
        }



        public int addend2
        {
            get { return _addend2; }
            set { SetProperty(ref _addend2, value); }
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

        private bool _isFirstGuess = true;
        public bool IsNotFirstGuess
        {
            get { return !_isFirstGuess; }
        }
        private bool _isEnabledTotal = true;
        public bool IsEnabledTotal
        {
            get { return _isEnabledTotal; }

            set { SetProperty(ref _isEnabledTotal, value); }
        }
        public bool IsReadOnly { get { return _isPiano; } }
        public bool IsNotSync { get { return !_isSync; } }
        public bool IsNotBlind { get { return _isNotBlind; } }
        //public bool ShowSecondsToEnd { get { return (_seconds_pressed > 0 && _seconds_pressed < 3); } }
        readonly private bool _onlyOneaddend;
        public bool HasTwoaddends { get { return (!_onlyOneaddend && _isNotBlind); } }


        public ButtonViewModel2()
        {
            SentrySdk.CaptureMessage("page build started");
            CheckCommand = new Command(() => Check());
            NextCommand = new Command(() => GenerateExercise());

            this._isPiano = true;
            NAN = -1111;
            _addend1 = NAN;
            _addend2 = NAN;
            NotifyPropertyChanged(nameof(Saddend1));
            NotifyPropertyChanged(nameof(Saddend2));
            this._isSync = false;
            this._onlyOneaddend = false;
            //this._requireNewaddends = false;
            this._isNotBlind = true;

            _keys = new MR.Gestures.Button[10];


            SentrySdk.CaptureMessage("page build ended");


        }
        public String TrueStatement
        {
            get
            { return ""; }

        }

        public void Check() { _isFirstGuess = false; NotifyPropertyChanged(nameof(IsNotFirstGuess)); NotifyPropertyChanged(nameof(TrueStatement)); }

        private List<VisualElement> buttons = new();
        private int _seconds_pressed = 0;
        public string SecondsToEnd
        {
            get
            {
                return string.Format("{0}", 3 - _seconds_pressed);
            }
        }

        protected override void OnDown(MR.Gestures.DownUpEventArgs e)
        {

            if (!IsEnabledTotal) return;
            //AddText2("{0} was clicked.", ((Button)e.Sender).CommandParameter);
            base.OnDown(e);
            buttons.Add((VisualElement)e.Sender);
            if (_isSync)
            {
                ((VisualElement)e.Sender).BackgroundColor = Colors.Yellow;
                if (Convert.ToInt32(((Button)e.Sender).CommandParameter) > 4)
                    _addend2++;
                else
                    _addend1++;
                //if(_addend1==0 && _addend2==0) { _isTimerWorking = false; _waiting_check = false; _seconds_pressed = 0; return; }
                /*_waiting_check = true;
                _seconds_pressed = 0;
                if (!_isTimerWorking)
                {
                    timer.Start();
                    _isTimerWorking = true;
                }*/
            }
            else
            {
                if (((VisualElement)e.Sender).BackgroundColor != Colors.Yellow)

                    ((VisualElement)e.Sender).BackgroundColor = Colors.Yellow;

                else
                    ((VisualElement)e.Sender).BackgroundColor = Color.FromArgb("FFFFFF");

            }

            NotifyPropertyChanged(nameof(Saddend1)); NotifyPropertyChanged(nameof(Saddend2)); NotifyPropertyChanged(nameof(SecondsToEnd));
        }

        protected override void OnUp(MR.Gestures.DownUpEventArgs e)
        {


            if (!IsEnabledTotal) return;
            base.OnUp(e);
            if (_isSync)
            {
                _seconds_pressed = 0; NotifyPropertyChanged(nameof(SecondsToEnd));
                ((VisualElement)e.Sender).BackgroundColor = Color.FromArgb("FFFFFF");
                if (Convert.ToInt32(((Button)e.Sender).CommandParameter) > 4)
                    _addend2--;
                else
                    _addend1--;
                //if (_addend1 == 0 && _addend2 == 0) { _isTimerWorking = false; _waiting_check = false; _seconds_pressed = 0; timer.Stop(); NotifyPropertyChanged(nameof(TrueStatement)); }

            }
            else
            {

                if (Convert.ToInt32(((Button)e.Sender).CommandParameter) > 4)
                    if (((VisualElement)e.Sender).BackgroundColor != Colors.Yellow)
                        _addend2--;
                    else
                        _addend2++;
                else
                    if (((VisualElement)e.Sender).BackgroundColor != Colors.Yellow)
                    _addend1--;
                else
                    _addend1++;
            }
            if (_addend1 < 0) _addend1 = 0;
            if (_addend2 < 0) _addend2 = 0;
            //AddText2("{0} {1}", _addend1, _addend2);
            NotifyPropertyChanged(nameof(Saddend1)); NotifyPropertyChanged(nameof(Saddend2));
        }


        public void GenerateExercise()
        {
            _isFirstGuess = true;
            //if (ASSERT)
            //    SentrySdk.CaptureMessage("Hello Sentry");

            //TODO: validation also in the form with Binding
            if (_minaddend < 0) _minaddend = 0;
            if (_maxaddend < _minaddend + 3) _maxaddend = _minaddend + 2;
            if (_maxSum > 2 * _maxaddend || _maxSum <= 2 * _minaddend) _maxSum = 2 * _maxaddend;


            int[] factors = new int[3];
            Random r = new();
            factors[2] = r.Next(_minSum, _maxSum + 1);
            //if (_fInsisitentOnOne) factors[2] = _lastNum;
            factors[0] = r.Next(_minaddend, Math.Min(_maxaddend, factors[2]) + 1);
            factors[1] = factors[2] - factors[0];

            if (_mult)
            {
                factors[0] = r.Next(_minaddend, Math.Min(_maxaddend, factors[2]) + 1);
                factors[1] = r.Next(_minaddend, Math.Min(_maxaddend, factors[2]) + 1);
                factors[2] = factors[0] * factors[1];

            }

            //if (ASSERT)
            //    SentrySdk.CaptureMessage("First factors success");



            /*if (_decompositionLevel > 1)
            {

                if (_sum != _addend1 + _addend2) StreakWrong++;//you moved next without solving
                int minSum = (_decompositionLevel >= 3) ? 20 : 10;
                factors[2] = r.Next(Math.Max(_minaddend, minSum), _maxSum);
                while (factors[2] % 10 == 9) factors[2] = r.Next(Math.Max(_minaddend, minSum), _maxSum);
                if (factors[2] % 10 == 0) factors[0] = r.Next(_minaddend, Math.Min(_maxaddend + 1, factors[2]));
                else
                {

                    int tens = r.Next(Math.Max(_minaddend / 10, 0), factors[2] / 10 - 1);
                    int ones = r.Next(factors[2] % 10 + 1, 10);
                    factors[0] = tens * 10 + ones;
                }
                factors[1] = factors[2] - factors[0];
            }            //if (ASSERT)
            //    SentrySdk.CaptureMessage("Second factors success");
            */

            int questionType; questionType = 2;

            int n = r.Next(3); n = 2;
            for (int i = 0; i < 3; i++)
                if (i != n) factors[i] = NAN;
            //if (ASSERT)
            //    SentrySdk.CaptureMessage("Xs success");





            //Saddend1 = factors[0].ToString();
            //Saddend2 = factors[1].ToString();
            SSum = factors[2].ToString(); //NotifyPropertyChanged(nameof(History));

            NotifyPropertyChanged(nameof(TrueStatement));
            IsEnabledTotal = true;

            NotifyPropertyChanged(nameof(IsNotFirstGuess));
            //NotifyPropertyChanged(SSum);
            if (_isPiano)
            {
                _addend1 = NAN;
                _addend2 = NAN;
            }
            else
            {
                _addend1 = factors[0];
                _addend2 = factors[1];
                _sum = factors[2];
            }
            NotifyPropertyChanged(nameof(Saddend1));
            NotifyPropertyChanged(nameof(Saddend2));
            NotifyPropertyChanged(nameof(SSum));
            //Color = 
            //Color = Color.FromArgb("FFFFFF");
            STest = "";
            foreach (VisualElement ve in buttons) ve.BackgroundColor = Color.FromArgb("FFFFFF");
            buttons.Clear();


        }
    }
}
    

