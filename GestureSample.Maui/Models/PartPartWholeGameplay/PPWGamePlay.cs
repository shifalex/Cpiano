using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using GestureSample.Maui;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using GestureSample.Views.Tests;
using MvvmCross.Binding.Extensions;

namespace GestureSample.Maui.Models
{

    internal class PPWGamePlay
    {
        public static readonly int NAN = -1111;
        public int addend1;
        public int addend2;
        public int Sum;

        
        public Operation CurrentOperation { get; set; }

        private int _guessNumber = 0;
        public int GuessNumber { get { return _guessNumber; } }

        protected int _minSum = 1, _maxSum = 10, _minAddend = 1, _maxAddend = 5;
        protected VariableTypes _numberOfVariables = VariableTypes.TwoNoSum;//TODO: now it is only in the "history games. Maybe I will have to change it
        


        protected string _status = Statement.Neutral;
        public string Status { get => _status; }

        private readonly bool _isHistory = false;
        private bool _isFirstGuess = true;

        protected readonly SimpleViewCellsPage _view;

        public GameConfig Config;
        public PPWGamePlay(SimpleViewCellsPage view, GameConfig config)
        {
             _view = view; Config = config;
            CurrentOperation = Config.OperationList[0];
            
            _isHistory = config.IsHistory; _maxAddend = config.MaxAddend; _maxSum = config.MaxSum; _numberOfVariables = config.VariableTypes;
            _minAddend = config.MinAddend; _minSum = config.MinSum;
            
            //TODO: Move these to MainPage
            //if (_isHistory) _minSum = 1;
        }


        private bool IsCorrectInput()
        {
            if(addend1 > _maxAddend || addend1 < _minAddend || addend2 > _maxAddend || addend2 < _minAddend || Sum > _maxSum || Sum < _minAddend)
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
                if (_isHistory && _status==Statement.True)
                {
                    if(AllHistory.Where(item => item.Sum == Sum && item.Addend1 == addend1).Any())
                            _status = Statement.New;
                     else AllHistory.Add(new PPWObject(addend1, addend2, Sum));
                }
            }
                

            _view.UpdateView();

            return _status == Statement.True;
        }

        public virtual bool Check(int a1, int a2, int s)
        {
            if ((a1 > _maxAddend || a1 < _minAddend || a2 > _maxAddend || a2 < _minAddend || s > _maxSum || s < _minAddend))
            {
                _status = Statement.WrongInput;
                _view.UpdateView();
                return false;
            }
            addend1 = a1; addend2= a2; Sum = s; return Check();
        }

        public virtual bool Check(PianoKeyboard pianoKeyboard)
        { 
            return Check(pianoKeyboard.Addend1, pianoKeyboard.Addend2, Sum);
        }

        public virtual void GenerateExercise()
        {

            Random r = new();
            CurrentOperation = Config.OperationList[r.Next(Config.OperationList.Count())];

            int[] factors;
            if (CurrentOperation == Operation.Multiplication) 
                factors = FactorsMultiplication;
            else if(Config.OnlyThrougTen)
                factors = FactorsThroughTen;
            else if(_isHistory) 
                factors = FactorsByHistory;
            else
                factors= Factors;
            

            int n = (_numberOfVariables == VariableTypes.OneCanBeSum || _numberOfVariables == VariableTypes.TwoAny) ? r.Next(3) : r.Next(2);
            switch (_numberOfVariables)   {
                case VariableTypes.OneCanBeSum:
                case VariableTypes.OneNoSum:
                    factors[n] = NAN;  break;
                case VariableTypes.SumOnly:
                    factors[2] = NAN;  break;
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
            CurrentOperation = Config.OperationList[r.Next(Config.OperationList.Count())];
            _status = Statement.Neutral;
            _guessNumber = 0;
            _view.UpdateView(true);
        }



        protected virtual int[] Factors
        {
            get
            {
                int[] factors = new int[3];
                Random r = new();

                if (_isFirstGuess)
                {
                    factors[0] = 2; factors[1] = 3; factors[2] = 5;
                    _isFirstGuess = false;
                    return factors;
                }
                factors[2] = r.Next(_minSum, _maxSum + 1);
                //if (_fInsisitentOnOne) factors[2] = _lastNum;
                factors[0] = r.Next(_minAddend, Math.Min(_maxAddend, factors[2]) + 1);
                factors[1] = factors[2] - factors[0];

                return factors;
            }
        }

        protected int[] FactorsThroughTen
        {
            get
            {
                int[] factors = new int[3];
                Random r = new();

                factors[2] = r.Next(Math.Max(_minAddend, _minSum), _maxSum);
                while (factors[2] % 10 == 9 || factors[2] / 10 == 0) factors[2] = r.Next(Math.Max(_minAddend, _minSum), _maxSum);
                if (factors[2] % 10 == 0) factors[0] = r.Next(_minAddend, Math.Min(_maxAddend + 1, factors[2]));
                else
                {
                    int tens = r.Next(Math.Max(_minAddend / 10, 0), factors[2] / 10 - 1);
                    int ones = r.Next(factors[2] % 10 + 1, 10);
                    factors[0] = tens * 10 + ones;
                }
                factors[1] = factors[2] - factors[0];

                return factors;
            }
        }

        private int[] FactorsMultiplication
        {
            get
            {
                int[] factors = new int[3];
                Random r = new();

                factors[0] = r.Next(_minAddend, _maxAddend + 1);
                factors[1] = r.Next(_minAddend, _maxAddend + 1);
                factors[2] = factors[0] * factors[1];

                return factors;
            }
        }


        #region History

        public List<PPWObject> AllHistory = new();
        private List<int> _impossibleSums = new();
        
        private int GenerateNewAddend(int newSum)
        {
            ArrayList possibleAddends = new();
            for (int i = Math.Max(_minAddend, newSum - _maxAddend); i <= Math.Min(_maxAddend, newSum - _minAddend); i++)
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
        }

        
        private int[] FactorsByHistory
        {
            get
            {
                int[] factors = new int[3];
                Random r = new();
                factors[2] = r.Next(_minSum, _maxSum + 1);
                factors[0] = GenerateNewAddend(factors[2]);
                factors[1] = factors[2] - factors[0];
                while (_impossibleSums.Contains(factors[2]) || _impossibleSums.Count >= _maxSum - 2 * _minAddend - 1)
                {
                    if (_impossibleSums.Count >= _maxSum - 2 * _minAddend - 1)
                    {
                        _status = Statement.Win;
                        _impossibleSums.Clear(); AllHistory.Clear(); _numberOfVariables = (_numberOfVariables == VariableTypes.OneNoSum) ? VariableTypes.TwoNoSum : VariableTypes.OneNoSum;
                    }
                    factors[2] = r.Next(_minSum, _maxSum + 1);
                    factors[0] = GenerateNewAddend(factors[2]);
                    factors[1] = factors[2] - factors[0];
                    //What about multiplicaiton with history?
                }
                return factors;
            }
        }

        #endregion
    }
}
