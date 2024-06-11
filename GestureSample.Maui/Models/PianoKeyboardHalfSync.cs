using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureSample.Maui.Models
{
    internal class PianoKeyboardHalfSync : PianoKeyboardSync
    {
        private readonly Color SECOND_COLOR = Colors.LightGreen;

        private bool _secondNumChoosing = false;
        private int _seconds_pressedHS = 0;
        protected override int SECONDS_TO_ANSWER { get; } = 2;
        protected virtual int SECONDS_TO_ANSWER_TOTAL { get; } = 10;

        private bool _withoutZero;

        public PianoKeyboardHalfSync(PPWGamePlay gamePlay, Label lblTimer, int textBoxesQuantity = 0, int rows = 1, int keysInRow = 10, bool imposeEdges = false, bool fromNumToNum = false, bool withoutZero = false) : base(gamePlay, lblTimer, textBoxesQuantity, rows, keysInRow, imposeEdges, fromNumToNum)
        {
            _withoutZero = withoutZero;
            _patterns = true;
        }

        public override string SecondsToEnd
        {
            get
            {
                return string.Format("{0} number", _secondNumChoosing ? "Second" : "First") + ((_withoutZero)?"":string.Format( ".\nTime Left: {0} seconds", SECONDS_TO_ANSWER_TOTAL - (_seconds_pressedHS)));
            }
        }
        protected override void TimerInit()
        {
            timer = Application.Current.Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => {
                MainThread.BeginInvokeOnMainThread(async () =>
                {

                    _seconds_pressedHS++;

                    if ((!_secondNumChoosing && _addent1 != 0) || (_secondNumChoosing && _addent2 != 0))
                    {

                        if(!_withoutZero) _lblTimer.Text = Statement.Selecting;
                        _seconds_pressed++;
                    }
                    else
                    {
                        _seconds_pressed = 0;
                        _lblTimer.Text = SecondsToEnd;
                    }

                    if (_seconds_pressed >= 1 || (_seconds_pressedHS >= SECONDS_TO_ANSWER_TOTAL && !_withoutZero) )
                    {
                        ImposeEdgesIfNeeded();
                        _seconds_pressed = 0;
                        _seconds_pressedHS = 0;
                        if (!_secondNumChoosing)
                        {
                            _secondNumChoosing = true;
                            return;
                        }

                        //PianoInitWithTimer();

                        timer.Stop();


                        IsEnabled = false;
                        bool isCorrect = _gamePlay.Check();
                        await Task.Delay(3000);
                        PianoInit();
                        if (isCorrect)
                        {
                            _gamePlay.GenerateExercise();
                        }
                        else
                        {
                            _lblTimer.Text = SecondsToEnd;
                        }


                        timer.Start();
                    }
                });
            };
        }

        protected override void ImposeEdgesIfNeeded()
        {
            if (_imposeEdges)
            {
                base.ImposeEdgesIfNeeded();
                if (_withoutZero && _secondNumChoosing &&
                    (btnKeys[NUMBER_OF_KEYS - 1].BackgroundColor == COLOR_FREE || btnKeys[0].BackgroundColor == COLOR_FREE))
                {
                    _addent1 = -1; _addent2 = -1;
                }
                _gamePlay.Addent1 = _addent1; _gamePlay.Addent2 = _addent2;
            }
        }

        public override void PianoInit()
        {
            _secondNumChoosing = false;
            base.PianoInit();
        }

        protected override void setAddentsByPattern()
        {
            _addent1 = 0; _addent2 = 0;
            for (int i = 0; i < NUMBER_OF_KEYS; i++)
            {
                if (btnKeys[i].BackgroundColor ==COLOR_PRESSED)
                    _addent1++;
                else if (btnKeys[i].BackgroundColor == SECOND_COLOR) 
                    _addent2++;
            }
        }

        protected override bool InnerKeyDown(MR.Gestures.Button sender)
        {

            if (!_secondNumChoosing)
                sender.BackgroundColor = COLOR_PRESSED;
            else if (_secondNumChoosing && sender.BackgroundColor != COLOR_PRESSED)
                sender.BackgroundColor = SECOND_COLOR;
            
            //_seconds_pressed = 0;
            
            return true;
        }

        protected override bool InnerKeyUp(MR.Gestures.Button sender)
        {
            if (!_secondNumChoosing || sender.BackgroundColor == SECOND_COLOR)
                sender.BackgroundColor = COLOR_FREE;
            else if (_secondNumChoosing && sender.BackgroundColor == COLOR_PRESSED)
                sender.BackgroundColor = COLOR_PRESSED;

           /* setAddentsByPattern();
            if ((_secondNumChoosing && _addent2==0) || _addent1==0)
                _seconds_pressed = -1;
            else
                _seconds_pressed = 0;*/

            return true;

        }
    }
}
