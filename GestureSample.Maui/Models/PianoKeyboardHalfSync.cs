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

        public PianoKeyboardHalfSync(PPWGamePlay gamePlay, Label lblTimer, int textBoxesQuantity = 0, int rows = 1, int keysInRow = 10, bool imposeEdges = false, bool fromNumToNum = false) : base(gamePlay, lblTimer, textBoxesQuantity, rows, keysInRow, imposeEdges, fromNumToNum)
        {
            _patterns = true;
            timer.Start();

        }

        public new string SecondsToEnd
        {
            get
            {
                return string.Format("{0} number.\nTime Left: {1} seconds", _secondNumChoosing ? "Second" : "First", 10 - (_seconds_pressedHS / 4));
            }
        }
        protected new void TimerInit()
        {
            timer = Application.Current.Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000 / 4);
            timer.Tick += (s, e) => {
                MainThread.BeginInvokeOnMainThread(async () =>
                {

                    if ((!_secondNumChoosing && _addent1 != 0) || (_secondNumChoosing && _addent2 != 0))
                    {

                        _lblTimer.Text = Statement.Selecting;
                        _seconds_pressed++;
                    }
                    else
                    {
                        _seconds_pressedHS++;
                        _lblTimer.Text = SecondsToEnd;
                    }

                    if (_seconds_pressed >= 1 || _seconds_pressedHS >= (10 * 4))
                    {
                        _seconds_pressed = -1;
                        _seconds_pressedHS = 0;
                        if (!_secondNumChoosing)
                        {
                            _secondNumChoosing = true;
                            return;
                        }
                        
                        PianoInitWithTimer();
                        timer.Start();
                    }
                });
            };
        }

        public new void PianoInit()
        {
            _secondNumChoosing = false;
            base.PianoInit();
        }

        protected new void setAddentsByPattern()
        {
            _addent1 = 0; _addent2 = 0;
            for (int i = 0; i < NUMBER_OF_KEYS; i++)
            {
                if (btnKeys[i].BackgroundColor == Colors.Yellow)
                    _addent1++;
                else if (btnKeys[i].BackgroundColor == SECOND_COLOR) 
                    _addent2++;
            }
        }

        protected new bool InnerKeyDown(MR.Gestures.Button sender)
        {

            if (!_secondNumChoosing)
                sender.BackgroundColor = Colors.Yellow;
            else if (_secondNumChoosing && sender.BackgroundColor != Colors.Yellow)
                sender.BackgroundColor = SECOND_COLOR;
            if (_seconds_pressed <= 0)
            {
                _seconds_pressed = -1;
            }
            return true;
        }

        protected new bool InnerKeyUp(MR.Gestures.Button sender)
        {
            if (!_secondNumChoosing || sender.BackgroundColor == SECOND_COLOR)
                sender.BackgroundColor = COLOR_FREE;
            else if (_secondNumChoosing && sender.BackgroundColor == COLOR_PRESSED)
                sender.BackgroundColor = COLOR_PRESSED;

            _seconds_pressed = -1; //OnPropertyChanged(nameof(SecondsToEndHS));

            return true;

        }
    }
}
