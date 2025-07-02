namespace GestureSample.Maui.Models
{
    internal class PianoKeyboardSync : PianoKeyboard
    {
        protected IDispatcherTimer timer;
        protected int _seconds_pressed = 0;
        protected virtual bool IS_WHOLE_TIMER { get; }
        protected virtual int SECONDS_TO_ANSWER { get; }

        public PianoKeyboardSync(PPWGamePlay gamePlay, Microsoft.Maui.Controls.Label lblTimer, KeyboardConfig pianoConfig) : base(gamePlay, lblTimer, pianoConfig)
        {
            SECONDS_TO_ANSWER = pianoConfig.SecondsPressingToAnswer* (pianoConfig.SecondsPressingToAnswer>0? 1: -1);
            IS_WHOLE_TIMER = pianoConfig.SecondsPressingToAnswer < 0;
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
        protected virtual void TimerInit()
        {
            timer = Application.Current.Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (SECONDS_TO_ANSWER == 0) { _lblTimer.Text = Statement.Neutral; return; }
                
                    _seconds_pressed = (_addend1 == 0 && _addend2 == 0) ? 0 : (_seconds_pressed + 1);
                    _lblTimer.Text = SecondsToEnd;// (_addend1 == 0 && _addend2 == 0) ? Statement.Neutral : SecondsToEnd;

                    // Removed the problematic line as 'Stroke' is not a valid property for Label.  
                    // Instead, you can use other properties like BorderColor or BackgroundColor if applicable.  

                    //_lblTimer.BackgroundColor = (_seconds_pressed >= SECONDS_TO_ANSWER || (_addend1 == 0 && _addend2 == 0)) ? Colors.Transparent : Colors.Red;
                    _lblTimer.FontSize = (_seconds_pressed >= SECONDS_TO_ANSWER) ? 55 : 30;

                    if (_seconds_pressed >= SECONDS_TO_ANSWER)
                    {
                        _seconds_pressed = 0;
                        await PianoInitWithTimer();
                    }
                });
            };
        }

        protected async Task PianoInitWithTimer()
        {
            timer.Stop();
            IsEnabled = false;
            bool isCorrect = await _gamePlay.CheckAsync(this);

            if (isCorrect)
            {
                _gamePlay.GenerateExercise();
            }
            else
            {
                _lblTimer.Text = Statement.Neutral;
            }
            PianoInit();
            timer.Start();
        }

        protected override bool InnerKeyDown(MR.Gestures.Button sender)
        {
            if (sender.BackgroundColor == COLOR_PRESSED) return true;
            if(!IS_WHOLE_TIMER) _seconds_pressed = 0;
            _lblTimer.Text = SecondsToEnd;
            sender.BackgroundColor = COLOR_PRESSED;

            if (Convert.ToInt32(sender.CommandParameter) > 5)
                _addend2++;
            else
                _addend1++;

            return true;
        }

        protected override bool InnerKeyUp(MR.Gestures.Button sender)
        {
            if (sender.BackgroundColor == COLOR_FREE) return true;
            if (!IS_WHOLE_TIMER) { _seconds_pressed = 0;  } //OnPropertyChanged(nameof(SecondsToEnd));  
            _lblTimer.Text = SecondsToEnd;
            sender.BackgroundColor = COLOR_FREE;

            if (Convert.ToInt32(sender.CommandParameter) > 5)
                _addend2--;
            else
                _addend1--;

            //TODO:make closingButonHandle private function  
            if (_addend1 < 0) _addend1 = 0;
            if (_addend2 < 0) _addend2 = 0;

            return true;
        }
    }
}
