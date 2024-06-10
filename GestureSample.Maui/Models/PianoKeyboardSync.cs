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

        public PianoKeyboardSync(PPWGamePlay gamePlay, Microsoft.Maui.Controls.Label lblTimer, int textBoxesQuantity = 0, int rows = 1, int keysInRow = 10, bool imposeEdges = false, bool fromNumToNum = false) : base(gamePlay, lblTimer, textBoxesQuantity, rows, keysInRow, imposeEdges, fromNumToNum)
        {
            TimerInit();
        }

        public string SecondsToEnd
        {
            get
            {
                return string.Format("{0}", 3 - _seconds_pressed);
            }
        }
        protected void TimerInit()
        {
            timer = Application.Current.Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => {
                MainThread.BeginInvokeOnMainThread(async () =>
                {

                    _seconds_pressed++;
                    _lblTimer.Text = SecondsToEnd;

                    if (_seconds_pressed >= 3)
                    {
                        _seconds_pressed = -1;
                        PianoInitWithTimer();
                    }
                });
            };
        }

        protected async void PianoInitWithTimer()
        {
            timer.Stop();


            IsEnabled = false;
            if (_gamePlay.Check())
            {
                await Task.Delay(3000);
                _gamePlay.GenerateExercise();

            }
            else
            {
                await Task.Delay(3000);
            }
            PianoInit();
        }

        protected new bool InnerKeyDown(MR.Gestures.Button sender)
        {
            sender.BackgroundColor = COLOR_PRESSED;

            if (Convert.ToInt32(sender.CommandParameter) > 5)
                _addent2++;
            else
                _addent1++;

            if (_seconds_pressed == -1)
            {
                _seconds_pressed = 0;
                _lblTimer.Text = SecondsToEnd;
                timer.Start();
            }

            return true;
        }

        protected new bool InnerKeyUp(MR.Gestures.Button sender)
        {
            _seconds_pressed = 0; OnPropertyChanged(nameof(SecondsToEnd));
            sender.BackgroundColor = COLOR_FREE;

            if (Convert.ToInt32(sender.CommandParameter) > 5)
                _addent2--;
            else
                _addent1--;
            if (_addent1 == 0 && _addent2 == 0)
            {
                _seconds_pressed = -1; timer.Stop();
                _lblTimer.Text = Statement.Neutral;
            }
            else
                _lblTimer.Text = SecondsToEnd;

            //TODO:make closingButonHandle private function
            if (_addent1 < 0) _addent1 = 0;
            if (_addent2 < 0) _addent2 = 0;

            return true;
        }


    }
}
