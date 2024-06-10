using Microsoft.Maui;
using MR.Gestures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureSample.Maui.Models
{
    internal class PianoKeyboard: MR.Gestures.Grid
    {

        protected readonly int NUMBER_OF_KEYS;
        protected readonly int FINGER_SEPERATOR = 5;
        protected readonly MR.Gestures.Button[] btnKeys;
        protected readonly Color COLOR_PRESSED = Colors.Yellow;
        protected readonly Color COLOR_FREE = Colors.White;
        protected readonly int heading_height = 55;

        protected int _addent1 = 0;
        protected int _addent2 = 0;

        protected readonly PPWGamePlay _gamePlay;
        

        protected readonly Microsoft.Maui.Controls.Label _lblTimer;
        protected bool _patterns;
        protected readonly bool _imposeEdges = false;
        protected readonly bool _fromNumToNum = false;
        

        //TODO: add constructor for 20 keys and for keyboard questions
        //public BitArray Keys;

        public int Addent1 { get => _addent1; }
        public int Addent2 { get => _addent2; }
        public int Sum { get => _addent1+_addent2; }


        protected void SaveState()
        {
            Data.State s = new()
            {
                UserId = 1,
                TimeStamp = DateTime.Now,
                TypeName = "test",
                Addent1 = this.Addent1,
                Addent2 = this.Addent2,
                Sum = this.Sum, //TODO:make more elegant
                B1 = btnKeys[0].BackgroundColor== COLOR_PRESSED?true:false,
                B2 = btnKeys[1].BackgroundColor == COLOR_PRESSED ? true : false,
                B3 = btnKeys[2].BackgroundColor == COLOR_PRESSED ? true : false,
                B4 = btnKeys[3].BackgroundColor == COLOR_PRESSED ? true : false,
                B5 = btnKeys[4].BackgroundColor == COLOR_PRESSED ? true : false,
                B6 = btnKeys[5].BackgroundColor == COLOR_PRESSED ? true : false,
                B7 = btnKeys[6].BackgroundColor == COLOR_PRESSED ? true : false,
                B8 = btnKeys[7].BackgroundColor == COLOR_PRESSED ? true : false,
                B9 = btnKeys[8].BackgroundColor == COLOR_PRESSED ? true : false,
                B10 = btnKeys[9].BackgroundColor == COLOR_PRESSED ? true : false,

            } ; 
            Data.StateConnection.Instance.SaveStateAsync(s);
        }

        public PianoKeyboard(PPWGamePlay gamePlay, Microsoft.Maui.Controls.Label lblTimer, int textBoxesQuantity=0, int rows=1, int keysInRow=10, bool imposeEdges =false, bool fromNumToNum = false): base() {
            _patterns = keysInRow > 10 || imposeEdges; _imposeEdges = imposeEdges; _fromNumToNum = fromNumToNum;
            _gamePlay = gamePlay;
            _lblTimer = lblTimer;

            
            NUMBER_OF_KEYS = keysInRow * rows;
            this.ColumnSpacing = 5;
            this.BackgroundColor = Colors.Black;
            //this.Padding = textBoxesQuantity==0? new Thickness(0, 30, 0, 0):0;
            int handSeperator = 5; if(keysInRow>10) handSeperator = keysInRow+1;
            
            this.RowDefinitions.Add(new RowDefinition() { Height= new GridLength(heading_height) });
            btnKeys = new MR.Gestures.Button[NUMBER_OF_KEYS];
            for (int i = 0; i < keysInRow+(handSeperator<keysInRow ? 1 : 0); i++)
                    this.ColumnDefinitions.Add((i == handSeperator) ? new ColumnDefinition { Width = new GridLength(5) } : new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            for (int r = 0; r < rows; r++)
                    this.RowDefinitions.Add(new RowDefinition{ Height = new GridLength(1, GridUnitType.Star) });

            for (int r = 0; r < rows; r++)
            {
                    for (int i = 0; i < keysInRow; i++)//TODO: enable 2 rows with 10 keys
                {
                    this.Add(
                    btnKeys[i+ keysInRow * r] = new()
                    {
                        Text = "Button " + (i + 1 + keysInRow * r).ToString(),
                        BackgroundColor = COLOR_FREE,
                        CommandParameter = i + 1+ keysInRow * r,
                        Margin = new Thickness(0, 5, 0, 0),
                        DownCommand = new Command<MR.Gestures.DownUpEventArgs>(OnDown), 
                        UpCommand =  new Command<MR.Gestures.DownUpEventArgs>(OnUp), 
                    }, (i < handSeperator) ? i : i + 1,/*r+1*/ rows-r 
                    );
                }
            }

            Microsoft.Maui.Controls.Button btnInit = new()
            {
                Text = "Reset",
                Command = new Command(() => { PianoInit(); }),
                HorizontalOptions = LayoutOptions.Start,
                WidthRequest = 80,
                HeightRequest = 16
            };

            if (textBoxesQuantity > 0)
            {
                Microsoft.Maui.Controls.Entry[] a_array= new Microsoft.Maui.Controls.Entry[3];
                for (int i = 0; i < a_array.Length; i++)
                {
                    a_array[i] = new()
                    {
                        IsReadOnly = true,
                        FontSize = 14,
                        WidthRequest = 25,
                        HeightRequest = 16,
                        BackgroundColor = Colors.White,
                        HorizontalTextAlignment = TextAlignment.Center,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        BindingContext = this
                    };
                }
                a_array[0].IsVisible = textBoxesQuantity >= 2; a_array[0].SetBinding(Microsoft.Maui.Controls.Entry.TextProperty, nameof(Addent1)); 
                a_array[1].IsVisible = textBoxesQuantity >= 2; a_array[1].SetBinding(Microsoft.Maui.Controls.Entry.TextProperty, nameof(Addent2));
                a_array[2].IsVisible = textBoxesQuantity == 1 || textBoxesQuantity == 3;
                a_array[2].SetBinding(Microsoft.Maui.Controls.Entry.TextProperty, nameof(Sum));

                Microsoft.Maui.Controls.HorizontalStackLayout hzl = new()
                {
                    a_array[0], 
                    new Microsoft.Maui.Controls.Label(){ HorizontalOptions = LayoutOptions.Center, WidthRequest = 50, IsVisible = textBoxesQuantity >= 2 }, 
                    a_array[2],
                    new Microsoft.Maui.Controls.Label(){ HorizontalOptions = LayoutOptions.Center, WidthRequest = 50, IsVisible = textBoxesQuantity == 3 }, 
                    a_array[1]
                };
                hzl.HorizontalOptions = LayoutOptions.Center;
                Microsoft.Maui.Controls.Grid g = new();
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(55) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(85) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(85) });
                g.Add(hzl,1); g.Add(btnInit,0);
                this.SetColumnSpan(g, keysInRow + 1);
                g.HorizontalOptions = LayoutOptions.Fill;
                this.Add(g, 0);
            }
            else
            {
                Microsoft.Maui.Controls.HorizontalStackLayout hzl = new()
                {
                    btnInit
                };
                this.SetColumnSpan(hzl, keysInRow + 1);
                this.Add(hzl, 0);
            }
        }

        public void PianoInit()
        {
            IsEnabled = true;
            if (_fromNumToNum) return;

            for(int i=0; i < btnKeys.Length; i++)
            {
                btnKeys[i].BackgroundColor = COLOR_FREE;
            }
            _addent1 = 0;
            _addent2=0;
            OnPropertyChanged(nameof(Addent1)); OnPropertyChanged(nameof(Addent2)); OnPropertyChanged(nameof(Sum));
        }

        //Spatial
        protected void setAddentsByPattern()
        {
            _addent1 = 0; _addent2 = 0; bool isNowYellowStreak = false; int yellowStreaksTillNowIncluding = 0;
            for (int i = 0; i < NUMBER_OF_KEYS; i++)
            {
                if (btnKeys[i].BackgroundColor == COLOR_PRESSED)
                {
                    if (!isNowYellowStreak) { isNowYellowStreak = true; yellowStreaksTillNowIncluding++; }
                    if (yellowStreaksTillNowIncluding == 1) _addent1++;
                    else if (yellowStreaksTillNowIncluding == 2) _addent2++;
                    else if (yellowStreaksTillNowIncluding > 2) { _addent1 = 0; _addent2 = -1; break; }
                }
                else
                    isNowYellowStreak = false;
            }
            if (yellowStreaksTillNowIncluding == 1 /*one addent is 0 - which one? if most keys in the left - addent2, if most keys in the right - addent1*/)
            {
                for (int i = 0; i < NUMBER_OF_KEYS; i++)
                    if (btnKeys[i].BackgroundColor == COLOR_PRESSED && btnKeys[NUMBER_OF_KEYS - 1 - i].BackgroundColor == COLOR_FREE)
                        break;
                    else if (btnKeys[i].BackgroundColor == COLOR_FREE && btnKeys[NUMBER_OF_KEYS - 1 - i].BackgroundColor == COLOR_PRESSED)
                    {
                        _addent2 = _addent1; _addent1 = 0;
                        break;
                    }
            }
            if (_imposeEdges /*Make wrong input if edges weren't imposed*/)
                if ((_addent1>0 && btnKeys[0].BackgroundColor == COLOR_FREE) || (_addent2>0 && btnKeys[NUMBER_OF_KEYS - 1].BackgroundColor == COLOR_FREE))
                {
                    _addent1 = 0; _addent2 = -1;
                }
        }

        protected bool InnerKeyDown (MR.Gestures.Button sender) {
            sender.BackgroundColor = (sender.BackgroundColor != COLOR_PRESSED) ? COLOR_PRESSED : COLOR_FREE;
            return false;
        }

        protected bool InnerKeyUp(MR.Gestures.Button sender)
        {
            if (Convert.ToInt32(sender.CommandParameter) > 5)
                _addent2 = (sender.BackgroundColor != COLOR_PRESSED) ? _addent2 - 1 : _addent2 + 1;
            else
                _addent1 = (sender.BackgroundColor != COLOR_PRESSED) ? _addent1 - 1 : _addent1 + 1;


            if (_addent1 < 0) _addent1 = 0;
            if (_addent2 < 0) _addent2 = 0;


            return true;
        }

        private void OnDown(MR.Gestures.DownUpEventArgs e)
        {
            OnKey(e, true);
        }
        private void OnUp(MR.Gestures.DownUpEventArgs e){
            OnKey(e, false);
        }
        private void OnKey (MR.Gestures.DownUpEventArgs e, bool isDown)
        {
            if (!IsEnabled) { return; }
            MR.Gestures.Button sender = (MR.Gestures.Button)e.Sender;

            if ((isDown?InnerKeyDown(sender):InnerKeyUp(sender)) && _patterns)
                setAddentsByPattern();

            OnPropertyChanged(nameof(Addent1)); OnPropertyChanged(nameof(Addent2)); OnPropertyChanged(nameof(Sum));
            _gamePlay.Addent1 = Addent1; _gamePlay.Addent2 = Addent2;
            SaveState();
        }
    }
}
