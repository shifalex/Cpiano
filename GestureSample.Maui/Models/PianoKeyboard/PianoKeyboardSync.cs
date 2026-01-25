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
        public PianoKeyboardSync(PPWGamePlay gamePlay, Microsoft.Maui.Controls.Label lblTimer, KeyboardConfig pianoConfig) : base(gamePlay, lblTimer, pianoConfig)
        {
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
        bool isKeyPressedTwice = false;
        protected virtual void TimerInit()
        {
            timer = Application.Current.Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (SECONDS_TO_ANSWER == 0) { _lblTimer.Text = Statement.Neutral; return; }

                    isKeyPressedTwice = false;
                    for (int i = 0; i< pressCounter.Length; i++) { if (pressCounter[i] > 0) isKeyPressedTwice = true; }
                    if (isKeyPressedTwice)
                    {
                        for (int i = 0; i < pressCounter.Length; i++)
                            if (pressCounter[i] > 0)
                            { }// btnKeys[i].BackgroundColor = _seconds_pressed % 2 == 0 ? Colors.Red : COLOR_PRESSED;
                    }

                    _seconds_pressed = (_addend1 == 0 && _addend2 == 0) ? 0 : (_seconds_pressed + 1);
                    
                    _lblTimer.Text = SecondsToEnd;// (_addend1 == 0 && _addend2 == 0) ? Statement.Neutral : SecondsToEnd;

                    // Removed the problematic line as 'Stroke' is not a valid property for Label.  
                    // Instead, you can use other properties like BorderColor or BackgroundColor if applicable.  

                    //_lblTimer.BackgroundColor = (_seconds_pressed >= SECONDS_TO_ANSWER || (_addend1 == 0 && _addend2 == 0)) ? Colors.Transparent : Colors.Red;

                    if (_seconds_pressed >= SECONDS_TO_ANSWER)
                    {
                        _seconds_pressed = 0;
                       //if(!isKeyPressedTwice) 
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

            if (isCorrect )
            {
                _gamePlay.GenerateExercise();
            }
            else
            {
                _lblTimer.Text = Statement.Neutral;
            }
            for (int i = 0; i < pressCounter.Length; i++)
                pressCounter[i] = 0;
            PianoInit();
            timer.Start();
        }


        //ArrayList<MR.Gestures.Button> twicePressedKeys = new();

        protected override bool InnerKeyDown(MR.Gestures.Button sender)
        {
            if (sender.BackgroundColor == COLOR_PRESSED) { pressCounter[Convert.ToInt32(sender.CommandParameter) - 1]++; return true; }
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
            if (sender.BackgroundColor == COLOR_FREE)
            {
               if(pressCounter[Convert.ToInt32(sender.CommandParameter)-1] > 0) pressCounter[Convert.ToInt32(sender.CommandParameter)-1]--; return true;
            }
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
