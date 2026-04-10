using System.Collections;

namespace GestureSample.Maui.Models
{
    internal class PianoKeyboardSync : PianoKeyboard
    {
        protected IDispatcherTimer timer;
        protected int _seconds_pressed = 0;
        protected virtual bool IS_WHOLE_TIMER { get; }
        protected virtual int SECONDS_TO_ANSWER { get; }



        int[] pressCounter;
        private readonly ProgressBar _pressProgress;
        private DateTime? _pressStartUtc;
        private CancellationTokenSource? _holdCts;
        private bool _isChecking = false;

        public PianoKeyboardSync(PPWGamePlay gamePlay, Label lblTimer, ProgressBar pressProgress, KeyboardConfig pianoConfig)
            : base(gamePlay, lblTimer, pianoConfig)
        {
            _pressProgress = pressProgress;
            _pressProgress.Progress = 0;
            //_pressProgress.Opacity = 0;
            //TODO:REALLY NOT SURE WHERE IS THE CORRECT PLACE FOR THIS - it should be in the gui - piano should give only text
            //_lblTimer.FontSize = 55;//(_seconds_pressed >= SECONDS_TO_ANSWER) ? 55 : 30;
            SECONDS_TO_ANSWER = pianoConfig.SecondsPressingToAnswer* (pianoConfig.SecondsPressingToAnswer>0? 1: -1);
            IS_WHOLE_TIMER = pianoConfig.SecondsPressingToAnswer < 0;
            pressCounter  = new int[NUMBER_OF_KEYS + 1];
            TimerInit();
            timer.Start();
        }

        public virtual string SecondsToEnd
        {
            get
            {
                return string.Format("00:0{0}", SECONDS_TO_ANSWER - _seconds_pressed);
            }
        }
        private bool AnyPressed()
        {
            return !(_addend1 == 0 && _addend2 == 0);
        }

        private void ResetProgressVisual()
        {
            _pressStartUtc = null;
            _seconds_pressed = 0;
            _pressProgress.Progress = 0;
            _pressProgress.Opacity = 0;
        }

        private void ShowProgressVisual()
        {

            Console.WriteLine("Before progress bar");
            _pressProgress.Opacity = 1;
        }
        protected virtual void TimerInit()
        {
            timer = Application.Current.Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromMilliseconds(33);

            timer.Tick += (s, e) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (_isChecking)
                        return;

                    //Debugging.DevLog.Line("What's going on?");

                    if (SECONDS_TO_ANSWER == 0)
                    {
                        ResetProgressVisual();
                        return;
                    }

                    if (!AnyPressed())
                    {
                        ResetProgressVisual();
                        return;
                    }

                    _pressStartUtc ??= DateTime.UtcNow;

                    TimeSpan elapsed = DateTime.UtcNow - _pressStartUtc.Value;
                    double progress = elapsed.TotalSeconds / SECONDS_TO_ANSWER;

                    if (progress < 0) progress = 0;
                    if (progress > 1) progress = 1;

                    ShowProgressVisual();
                    _pressProgress.Progress = progress;

                    if (progress >= 1.0)
                    {
                        await PianoInitWithTimer();
                    }
                });
            };
        }

        protected async Task PianoInitWithTimer()
        {
            if (_isChecking)
                return;

            _isChecking = true;
            timer.Stop();
            IsEnabled = false;

            // Hide progress immediately so feedback can appear on lblStatus
            ResetProgressVisual();

            bool isCorrect = await _gamePlay.CheckAsync(this);

            for (int i = 0; i < pressCounter.Length; i++)
                pressCounter[i] = 0;

            if (isCorrect && !_gamePlay.GameOver)
            {
                _gamePlay.GenerateExercise();
            }
            else
            {
                PianoInit();
            }

            _isChecking = false;
            timer.Start();
        }


        //ArrayList<MR.Gestures.Button> twicePressedKeys = new();

        protected override bool InnerKeyDown(MR.Gestures.Button sender)
        {
            if (sender.BackgroundColor == COLOR_PRESSED) { pressCounter[Convert.ToInt32(sender.CommandParameter) - 1]++; return true; }
            if(!IS_WHOLE_TIMER)
            {
                _seconds_pressed = 0;
                _pressStartUtc = null;
                _pressProgress.Progress = 0;
            }
            sender.BackgroundColor = COLOR_PRESSED;



            if (Convert.ToInt32(sender.CommandParameter) > 5)
                _addend2++;
            else
                _addend1++;

            if (!AnyPressed())
            {
                ResetProgressVisual();
            }

            return true;
        }
        protected override bool InnerKeyUp(MR.Gestures.Button sender)
        {
            if (sender.BackgroundColor == COLOR_FREE)
            {
               if(pressCounter[Convert.ToInt32(sender.CommandParameter)-1] > 0) pressCounter[Convert.ToInt32(sender.CommandParameter)-1]--; return true;
            }
            if (!IS_WHOLE_TIMER)
            {
                _seconds_pressed = 0;
                _pressStartUtc = null;
                _pressProgress.Progress = 0;
            }//OnPropertyChanged(nameof(SecondsToEnd));  
            sender.BackgroundColor = COLOR_FREE;

            if (Convert.ToInt32(sender.CommandParameter) > 5)
                _addend2--;
            else
                _addend1--;

            //TODO:make closingButonHandle private function  
            if (_addend1 < 0) _addend1 = 0;
            if (_addend2 < 0) _addend2 = 0;

            if (!AnyPressed())
            {
                ResetProgressVisual();
            }

            return true;
        }
    }
}
