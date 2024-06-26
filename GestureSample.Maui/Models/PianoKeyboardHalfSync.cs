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
        private readonly Color THIRD_COLOR = Colors.Blue;
        private readonly Color REMOVE_COLOR = Colors.Red;

        private Color[] colors;

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
        protected override int SECONDS_TO_ANSWER { get; } = 2;
        protected virtual int SECONDS_TO_ANSWER_TOTAL { get; } = 10;

        private bool _withoutZero;

        public PianoKeyboardHalfSync(PPWGamePlay gamePlay, Label lblTimer, int textBoxesQuantity = 0, int rows = 1, int keysInRow = 10, bool imposeEdges = false, bool fromNumToNum = false, bool withoutZero = false, int addentsNum=2, bool allowRemoval=false) : base(gamePlay, lblTimer, textBoxesQuantity, rows, keysInRow, imposeEdges, fromNumToNum)
        {
            if (allowRemoval && addentsNum == 3)
                Type = KeyboardType.ThreeAddensWithRemoval;
            else
                Type = KeyboardType.TwoAddens;
            _withoutZero = withoutZero;
            _patterns = true;
            colors = new Color[NUMBER_OF_KEYS];
            for (int i = 0; i < NUMBER_OF_KEYS; i++) colors[i] = COLOR_FREE;
        }

        public override string SecondsToEnd
        { //TODO: use some data structure for number/color/text
            get
            {
                string text = "No more";
                switch(_numbersChosen)
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
                return string.Format("{0} number", text) + ((_withoutZero)?"":string.Format( ".\nTime Left: {0} seconds", SECONDS_TO_ANSWER_TOTAL - (_seconds_pressedHS)));
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

                    if ((_numbersChosen<1 && _addent1 != 0) || (_numbersChosen>=1 && _addent2 != 0))
                    {

                        if(!_withoutZero) _lblTimer.Text = Statement.Selecting;
                        _seconds_pressed++;
                    }
                    else
                    {
                        _seconds_pressed = 0;
                        _lblTimer.Text = SecondsToEnd;
                    }

                    if ((_seconds_pressed >= 1 || (_seconds_pressedHS >= SECONDS_TO_ANSWER_TOTAL && !_withoutZero))  )
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
                if (_withoutZero && _numbersChosen==1 &&
                    (btnKeys[NUMBER_OF_KEYS - 1].BackgroundColor == COLOR_FREE || btnKeys[0].BackgroundColor == COLOR_FREE))
                {
                    _addent1 = -1; _addent2 = -1;
                }
                _gamePlay.Addent1 = _addent1; _gamePlay.Addent2 = _addent2;
            }
        }

        public override void PianoInit()
        {
            _numbersChosen = 0;
            for (int i = 0;i< NUMBER_OF_KEYS; i++) colors[i]= COLOR_FREE;
            _lblTimer.Text = SecondsToEnd;
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
            if (_numbersChosen==0)
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

        private void SaveColors()
        {

            for (int i = 0; i < NUMBER_OF_KEYS; i++) colors[i] = btnKeys[i].BackgroundColor;
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
            else*/ if ((_numbersChosen ==0 && sender.BackgroundColor ==COLOR_PRESSED)
                || (_numbersChosen == 1 && sender.BackgroundColor == SECOND_COLOR)
                || (_numbersChosen == 2 && sender.BackgroundColor == THIRD_COLOR))
                sender.BackgroundColor = COLOR_FREE;


            /* setAddentsByPattern();
             if ((_secondNumChoosing && _addent2==0) || _addent1==0)
                 _seconds_pressed = -1;
             else
                 _seconds_pressed = 0;*/

            return true;

        }
    }
}
