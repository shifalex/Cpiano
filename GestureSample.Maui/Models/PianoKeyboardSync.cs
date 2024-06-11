using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MR.Gestures;

namespace GestureSample.Maui.Models
{
    internal class PianoKeyboardSync : PianoKeyboard
    {
        protected IDispatcherTimer timer;
        protected int _seconds_pressed = 0;
        protected virtual int SECONDS_TO_ANSWER { get; } = 3;

        public PianoKeyboardSync(PPWGamePlay gamePlay, Microsoft.Maui.Controls.Label lblTimer, int textBoxesQuantity = 0, int rows = 1, int keysInRow = 10, bool imposeEdges = false, bool fromNumToNum = false) : base(gamePlay, lblTimer, textBoxesQuantity, rows, keysInRow, imposeEdges, fromNumToNum)
        {
            TimerInit();
            timer.Start();
        }

        public virtual string SecondsToEnd
        {
            get
            {
                return string.Format("{0}", SECONDS_TO_ANSWER - _seconds_pressed);
            }
        }
        protected virtual void TimerInit()
        {
            timer = Application.Current.Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    _seconds_pressed = (_addent1 == 0 && _addent2 == 0) ? 0 : (_seconds_pressed + 1);
                    _lblTimer.Text = (_addent1 == 0 && _addent2 == 0)?Statement.Neutral :SecondsToEnd;

                    if (_seconds_pressed >= SECONDS_TO_ANSWER)
                    {
                        _seconds_pressed = 0;
                        PianoInitWithTimer();
                    }
                });
            };
        }

        protected async void PianoInitWithTimer()
        {
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
                _lblTimer.Text = Statement.Neutral;
            }
            timer.Start();
        }

        protected override bool InnerKeyDown(MR.Gestures.Button sender)
        {
            _seconds_pressed = 0;
            sender.BackgroundColor = COLOR_PRESSED;

            if (Convert.ToInt32(sender.CommandParameter) > 5)
                _addent2++;
            else
                _addent1++;

            return true;
        }

        protected override bool InnerKeyUp(MR.Gestures.Button sender)
        {
            _seconds_pressed = 0; //OnPropertyChanged(nameof(SecondsToEnd));
            sender.BackgroundColor = COLOR_FREE;

            if (Convert.ToInt32(sender.CommandParameter) > 5)
                _addent2--;
            else
                _addent1--;

            //TODO:make closingButonHandle private function
            if (_addent1 < 0) _addent1 = 0;
            if (_addent2 < 0) _addent2 = 0;

            return true;
        }


    }
}
