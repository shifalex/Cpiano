namespace GestureSample.Maui.Models
{
    internal class PianoKeyboardHalfSync : PianoKeyboardSync
    {



        public enum KeyboardType
        {
            TwoAddens,
            TwoAddensWithRemoval,
            ThreeAddens,
            ThreeAddensWithRemoval
        }

        public KeyboardType Type { get; set; }

        private int _numbersChosen = 0;

        private int _seconds_pressedHS = 0;
        protected override int SECONDS_TO_ANSWER => Math.Abs(AnswerTimeSetting);
        protected virtual int SECONDS_TO_ANSWER_TOTAL => Math.Abs(AnswerTimeSetting);
        public override bool SupportsAnswerTimeTuner => true;

        private bool _withoutZero;

        public PianoKeyboardHalfSync(PPWGamePlay gamePlay, Label lblTimer, ProgressBar pressProgress, KeyboardConfig pianoConfig) : base(gamePlay, lblTimer, pressProgress, pianoConfig)
        {
            if (pianoConfig.AllowRemoval && pianoConfig.AddendsNum == 3)
                Type = KeyboardType.ThreeAddensWithRemoval;
            else
                Type = KeyboardType.TwoAddens;
            _withoutZero = pianoConfig.WithoutZero;
            _patterns = true;

        }

        public override string SecondsToEnd
        { //TODO: use some data structure for number/color/text
            get
            {
                string text = "No more";
                switch (_numbersChosen)
                {
                    case 0:
                        text = "First";
                        break;
                    case 1:
                        text = "Second";
                        break;
                    case 2:
                        text = "Third";
                        break;

                }
                if (AnswerTimeSetting == 0)
                    return string.Format("{0} number", text);

                int timeLeft = AnswerTimeSetting < 0
                    ? SECONDS_TO_ANSWER_TOTAL - _seconds_pressedHS
                    : SECONDS_TO_ANSWER - _seconds_pressed;

                if (timeLeft < 0)
                    timeLeft = 0;

                return string.Format("{0} number", text) + ((_withoutZero) ? "" : string.Format(".\nTime Left: {0} seconds", timeLeft));
            }
        }

        public override void UpdateAnswerTimeSetting(int secondsPressingToAnswer)
        {
            base.UpdateAnswerTimeSetting(secondsPressingToAnswer);
            _seconds_pressedHS = 0;
            _lblTimer.Text = SecondsToEnd;
        }

        protected override void TimerInit()
        {
            timer = Application.Current.Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {

                    if (AnswerTimeSetting == 0)
                    {
                        _seconds_pressed = 0;
                        _seconds_pressedHS = 0;
                        _lblTimer.Text = SecondsToEnd;
                        return;
                    }

                    _seconds_pressedHS++;

                    if ((_numbersChosen < 1 && _addend1 != 0) || (_numbersChosen >= 1 && _addend2 != 0))
                    {

                        if (!_withoutZero) _lblTimer.Text = Statement.Selecting;
                        _seconds_pressed++;
                    }
                    else
                    {
                        _seconds_pressed = 0;
                        _lblTimer.Text = SecondsToEnd;
                    }

                    bool shouldAdvance = AnswerTimeSetting < 0
                        ? _seconds_pressedHS >= SECONDS_TO_ANSWER_TOTAL
                        : _seconds_pressed >= SECONDS_TO_ANSWER;

                    if (shouldAdvance)
                    {
                        ImposeEdgesIfNeeded();
                        _seconds_pressed = 0;
                        _seconds_pressedHS = 0;
                        if (_numbersChosen == 0 || Type == KeyboardType.ThreeAddensWithRemoval)
                        {
                            _numbersChosen++;
                            if (Type == KeyboardType.ThreeAddensWithRemoval) { timer.Stop(); _lblTimer.Text = SecondsToEnd; }
                            return;
                        }

                        //if (Type == KeyboardType.ThreeAddensWithRemoval) return;
                        await PianoInitWithTimer();
                        /*
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


                        timer.Start();*/
                    }
                });
            };
        }

        protected override void ImposeEdgesIfNeeded()
        {
            if (_imposeEdges)
            {
                base.ImposeEdgesIfNeeded();
                if (_withoutZero && _numbersChosen == 1 &&
                    (btnKeys[NUMBER_OF_KEYS - 1].BackgroundColor == COLOR_FREE || btnKeys[0].BackgroundColor == COLOR_FREE))
                {
                    _addend1 = -1; _addend2 = -1;
                }
                _gamePlay.addend1 = _addend1; _gamePlay.addend2 = _addend2;
            }
        }

        public override void PianoInit()
        {
            _numbersChosen = 0;
            for (int i = 0; i < NUMBER_OF_KEYS; i++) colors[i] = COLOR_FREE;
            _lblTimer.Text = SecondsToEnd;
            base.PianoInit();
        }

        protected override void setAddendsByPattern()
        {
            _addend1 = 0; _addend2 = 0;
            for (int i = 0; i < NUMBER_OF_KEYS; i++)
            {
                if (btnKeys[i].BackgroundColor == COLOR_PRESSED)
                    _addend1++;
                else if (btnKeys[i].BackgroundColor == SECOND_COLOR)
                    _addend2++;
            }
        }

        protected override bool InnerKeyDown(MR.Gestures.Button sender)
        {
            if (sender.BackgroundColor == REMOVE_COLOR)
            {
                for (int i = 0; i < NUMBER_OF_KEYS; i++)
                    if (sender == btnKeys[i])
                        sender.BackgroundColor = colors[i];
                return true;
            }
            if (sender.BackgroundColor != COLOR_FREE && Type == KeyboardType.ThreeAddensWithRemoval)
            {
                sender.BackgroundColor = REMOVE_COLOR;
                return true;
            }
            if (_numbersChosen == 0)
                sender.BackgroundColor = COLOR_PRESSED;
            else if (_numbersChosen == 1 && sender.BackgroundColor == COLOR_FREE)
                sender.BackgroundColor = SECOND_COLOR;
            else if (_numbersChosen == 2 && sender.BackgroundColor == COLOR_FREE)
                sender.BackgroundColor = THIRD_COLOR;
            //_seconds_pressed = 0;

            SaveColors();
            if (Type == KeyboardType.ThreeAddensWithRemoval) timer.Start();
            return true;
        }



        protected override bool InnerKeyUp(MR.Gestures.Button sender)
        {
            /*if (sender.BackgroundColor == REMOVE_COLOR)
                for (int i = 0; i < NUMBER_OF_KEYS; i++) { 
                    if (sender==btnKeys[i])
                    {
                        if (colors[i] == COLOR_PRESSED && _secondNumChoosing) { }
                        else if (colors[i] == SECOND_COLOR && _thirdNumChoosing) { }
                        else
                        sender.BackgroundColor = colors[i];
                    }
                }
            else*/
            if ((_numbersChosen == 0 && sender.BackgroundColor == COLOR_PRESSED)
         || (_numbersChosen == 1 && sender.BackgroundColor == SECOND_COLOR)
         || (_numbersChosen == 2 && sender.BackgroundColor == THIRD_COLOR))
                sender.BackgroundColor = COLOR_FREE;


            /* setAddendsByPattern();
             if ((_secondNumChoosing && _addend2==0) || _addend1==0)
                 _seconds_pressed = -1;
             else
                 _seconds_pressed = 0;*/

            return true;

        }
    }
}
