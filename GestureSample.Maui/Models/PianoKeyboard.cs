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

        private readonly int NUMBER_OF_KEYS;

        private readonly int FINGER_SEPERATOR = 5;

        private readonly PPWGamePlay _gamePlay;
        private readonly MR.Gestures.Button[] btnKeys;
        private readonly Microsoft.Maui.Controls.Label _lblTimer;

        private int _addent1 = 0;
        private int _addent2 = 0;
        private readonly bool _patterns;
        private readonly bool _imposeEdges = false;
        private readonly bool _fromNumToNum = false;
        private readonly bool _halfSync = false;
        private bool _secondNumChoosing = false;

        //TODO: add constructor for 20 keys and for keyboard questions
        //public BitArray Keys;

        public int Addent1 { get => _addent1; }
        public int Addent2 { get => _addent2; }
        public int Sum { get => _addent1+_addent2; }

        #region Timer
        private IDispatcherTimer timer;
        private int _seconds_pressed = -1;
        public string SecondsToEnd
        {
            get
            {
                return string.Format("{0}", 3 - _seconds_pressed);
            }
        }
        private void TimerInit()
        {
            timer = Application.Current.Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => {
                MainThread.BeginInvokeOnMainThread(async () =>
                {

                    _seconds_pressed++;
                    _lblTimer.Text = SecondsToEnd;

                    if (_seconds_pressed >= 3 )
                    {
                        _seconds_pressed = -1;
                        timer.Stop();

                        if (_halfSync && !_secondNumChoosing)
                        {
                            _secondNumChoosing = true; return;
                        }
                       

                            //OnPropertyChanged(nameof(Addent1)); OnPropertyChanged(nameof(Addent2)); OnPropertyChanged(nameof(Sum));
                            //_gamePlay.Addent1 = Addent1; _gamePlay.Addent2 = Addent2;
                        

                        if (_gamePlay.Check())
                        {
                            IsEnabled = false;
                            await Task.Delay(3000);
                            _gamePlay.GenerateExercise();
                            PianoInit();
                        }
                    }
                });
            };
        }
        #endregion

        public PianoKeyboard(PPWGamePlay gamePlay, Microsoft.Maui.Controls.Label lblTimer, bool isSync=false, int textBoxesQuantity=0, int rows=1, int keysInRow=10, bool imposeEdges =false, bool fromNumToNum = false, bool halfSync = false): base() {
            _patterns = keysInRow > 10 || imposeEdges || halfSync; _imposeEdges = imposeEdges; _fromNumToNum = fromNumToNum; _halfSync = halfSync;
            _gamePlay = gamePlay;
            _lblTimer = lblTimer;
             
            if (isSync) TimerInit();
            NUMBER_OF_KEYS = keysInRow * rows;
            this.ColumnSpacing = 5;
            this.BackgroundColor = Colors.Black;
            //this.Padding = textBoxesQuantity==0? new Thickness(0, 30, 0, 0):0;
            int handSeperator = 5; if(keysInRow>10) handSeperator = keysInRow+1;
            
            this.RowDefinitions.Add(new RowDefinition() { Height= new GridLength(55) });
            btnKeys = new MR.Gestures.Button[NUMBER_OF_KEYS];
            for (int i = 0; i < keysInRow+(handSeperator<keysInRow ? 1 : 0); i++)
                    this.ColumnDefinitions.Add((i == handSeperator) ? new ColumnDefinition { Width = new GridLength(5) } : new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            for (int r = 0; r < rows; r++)
                    this.RowDefinitions.Add(new RowDefinition{ Height = new GridLength(1, GridUnitType.Star) });

            for (int r = 0; r < rows; r++)
            {
                    //Keys = new BitArray(NUMBER_OF_KEYS);

                    for (int i = 0; i < keysInRow; i++)//TODO: enable 2 rows with 10 keys
                {
                    this.Add(
                    btnKeys[i+ keysInRow * r] = new()
                    {
                        Text = "Button " + (i + 1 + keysInRow * r).ToString(),
                        BackgroundColor = Colors.White,
                        CommandParameter = i + 1+ keysInRow * r,
                        Margin = new Thickness(0, 5, 0, 0),
                        DownCommand = isSync ? new Command<MR.Gestures.DownUpEventArgs>(OnDownSync) : new Command<MR.Gestures.DownUpEventArgs>(OnDown),
                        UpCommand = isSync ? new Command<MR.Gestures.DownUpEventArgs>(OnUpSync) : new Command<MR.Gestures.DownUpEventArgs>(OnUp)
                    }, (i < handSeperator) ? i : i + 1,/*r+1*/ rows-r 
                    );
                }

            }

            Microsoft.Maui.Controls.Button btnInit = new()
            {
                Text = "Init",
                Command = new Command(() => { PianoInit(); }),
                HorizontalOptions = LayoutOptions.Start,
            };

            if (textBoxesQuantity > 0)
            {
                Microsoft.Maui.Controls.Entry a1 = new()
                {
                    IsReadOnly = true,
                    FontSize = 14,
                    WidthRequest = 25,
                    HeightRequest = 16,
                    BackgroundColor = Colors.White,
                    HorizontalTextAlignment = TextAlignment.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    BindingContext = this,
                    IsVisible = textBoxesQuantity >= 2
                };

                Microsoft.Maui.Controls.Label spacing = new() { HorizontalOptions = LayoutOptions.Center, WidthRequest = 50, IsVisible = textBoxesQuantity >= 2 };

                Microsoft.Maui.Controls.Entry entrSum = new()
                {
                    IsReadOnly = true,
                    FontSize = 14,
                    WidthRequest = 25,
                    HeightRequest = 16,
                    BackgroundColor = Colors.White,
                    HorizontalTextAlignment = TextAlignment.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    BindingContext = this,
                    IsVisible = textBoxesQuantity == 1 || textBoxesQuantity == 3
                };

                Microsoft.Maui.Controls.Label spacing2 = new() { HorizontalOptions = LayoutOptions.Center, WidthRequest = 50, IsVisible = textBoxesQuantity == 3 };

                Microsoft.Maui.Controls.Entry a2 = new()
                {
                    IsReadOnly = true,
                    FontSize = 14,
                    WidthRequest = 25,
                    HeightRequest = 16,
                    BackgroundColor = Colors.White,
                    HorizontalTextAlignment = TextAlignment.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    BindingContext = this,
                    IsVisible = textBoxesQuantity >= 2
                };
                a1.SetBinding(Microsoft.Maui.Controls.Entry.TextProperty, nameof(Addent1));
                a2.SetBinding(Microsoft.Maui.Controls.Entry.TextProperty, nameof(Addent2));
                entrSum.SetBinding(Microsoft.Maui.Controls.Entry.TextProperty, nameof(Sum));
                Microsoft.Maui.Controls.HorizontalStackLayout hzl = new()
                {
                    btnInit, a1, spacing, entrSum, spacing2, a2
                };
                //hzl.HorizontalOptions = LayoutOptions.Center;
                this.SetColumnSpan(hzl, keysInRow + 1);
                this.Add(hzl, 0);
            }
            else
            {
                Microsoft.Maui.Controls.HorizontalStackLayout hzl = new()
                {
                    btnInit
                };
                //hzl.HorizontalOptions = LayoutOptions.Center;
                this.SetColumnSpan(hzl, keysInRow + 1);
                this.Add(hzl, 0);
            }
        }

        public void PianoInit()
        {
            IsEnabled = true;
            _secondNumChoosing = false;
            if (_fromNumToNum) return;

            for(int i=0; i < btnKeys.Length; i++)
            {
                btnKeys[i].BackgroundColor = Colors.White;
                //Keys[i] = false;
            }
            _addent1 = 0;
            _addent2=0;
            OnPropertyChanged(nameof(Addent1)); OnPropertyChanged(nameof(Addent2)); OnPropertyChanged(nameof(Sum));

            


        }

        private void setPatternAddents()
        {
            _addent1 = 0; _addent2 = 0; bool yellowStreak = false; int yellowStreakCount = 0;
            for (int i = 0; i < NUMBER_OF_KEYS; i++)
            {
                if (btnKeys[i].BackgroundColor == Colors.Yellow)
                {
                    if (!yellowStreak) { yellowStreak = true; yellowStreakCount++; }
                    if (_halfSync || yellowStreakCount == 1) _addent1++;
                    else if (!_halfSync && yellowStreakCount == 2) _addent2++;
                    else if (!_halfSync && yellowStreakCount > 2){ _addent1 = 0; _addent2 = -1; break; }
                }
                else if (btnKeys[i].BackgroundColor == Colors.LightGreen) { _addent2++; }
                else
                    yellowStreak = false;
            }
            if (!_halfSync && yellowStreakCount ==1)
            {
                for (int i = 0; i < NUMBER_OF_KEYS; i++)
                    if (btnKeys[i].BackgroundColor == Colors.Yellow && btnKeys[NUMBER_OF_KEYS-1-i].BackgroundColor == Colors.White) break;
                    else if (btnKeys[i].BackgroundColor == Colors.White && btnKeys[NUMBER_OF_KEYS - 1 - i].BackgroundColor == Colors.Yellow) { _addent2 = _addent1; _addent1 = 0; break; }
            }
            if(_imposeEdges && ((_addent1>0 && btnKeys[0].BackgroundColor == Colors.White) || (_addent2>0 && btnKeys[NUMBER_OF_KEYS - 1].BackgroundColor == Colors.White)))
            {
                _addent1 = 0; _addent2 = -1;
            }
        }

        #region Down/Up events
        protected void OnDown(MR.Gestures.DownUpEventArgs e)
        {
            if(!IsEnabled) { return; }
            MR.Gestures.Button sender = (MR.Gestures.Button)e.Sender;

            sender.BackgroundColor = (sender.BackgroundColor != Colors.Yellow) ? Colors.Yellow : Colors.White;

            OnPropertyChanged(nameof(Addent1)); OnPropertyChanged(nameof(Addent2)); OnPropertyChanged(nameof(Sum));
            _gamePlay.Addent1 = Addent1; _gamePlay.Addent2=Addent2;
            //SaveState();
        }

        protected void OnUp(MR.Gestures.DownUpEventArgs e)
        {

            if (!IsEnabled) { return; }
            MR.Gestures.Button sender = (MR.Gestures.Button)e.Sender;

            if (Convert.ToInt32(sender.CommandParameter) > 5)
                _addent2 = (sender.BackgroundColor != Colors.Yellow) ? _addent2 - 1 : _addent2 + 1;
            else
                _addent1 = (sender.BackgroundColor != Colors.Yellow) ? _addent1 - 1 : _addent1 + 1;


            if (_addent1 < 0) _addent1 = 0;
            if (_addent2 < 0) _addent2 = 0;


            if (_patterns) setPatternAddents();
            //SaveState();
            OnPropertyChanged(nameof(Addent1)); OnPropertyChanged(nameof(Addent2)); OnPropertyChanged(nameof(Sum));
            _gamePlay.Addent1 = Addent1; _gamePlay.Addent2 = Addent2;
        }

        protected void OnDownSync(MR.Gestures.DownUpEventArgs e)
        {

            if (!IsEnabled) { return; }
            MR.Gestures.Button sender = (MR.Gestures.Button)e.Sender;
            if (_halfSync)
            {
                if (!_secondNumChoosing)
                    sender.BackgroundColor = Colors.Yellow;
                else if (_secondNumChoosing && sender.BackgroundColor != Colors.Yellow)
                    sender.BackgroundColor = Colors.LightGreen;
            }
            else
               sender.BackgroundColor = Colors.Yellow;
            

            if (Convert.ToInt32(sender.CommandParameter) > 5)
               _addent2++;
            else
               _addent1++;
            
            if (_seconds_pressed==-1)
            {
                _seconds_pressed = 0;
                _lblTimer.Text = SecondsToEnd;
                timer.Start();
            }

            if (_patterns) setPatternAddents();

            OnPropertyChanged(nameof(Addent1)); OnPropertyChanged(nameof(Addent2)); OnPropertyChanged(nameof(Sum));
            _gamePlay.Addent1 = Addent1; _gamePlay.Addent2 = Addent2;
            //SaveState();
        }

        protected void OnUpSync(MR.Gestures.DownUpEventArgs e)
        {

            if (!IsEnabled) { return; }
            MR.Gestures.Button sender = (MR.Gestures.Button)e.Sender;

                _seconds_pressed = 0; OnPropertyChanged(nameof(SecondsToEnd));
                if (_halfSync)
                {
                    if (!_secondNumChoosing || sender.BackgroundColor == Colors.LightGreen)
                        sender.BackgroundColor = Colors.White;
                    else if (_secondNumChoosing && sender.BackgroundColor == Colors.Yellow)
                        sender.BackgroundColor = Colors.Yellow;
                }
                else
                    sender.BackgroundColor = Colors.Yellow;

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


            if (_patterns) setPatternAddents();

            //SaveState();
            OnPropertyChanged(nameof(Addent1)); OnPropertyChanged(nameof(Addent2)); OnPropertyChanged(nameof(Sum));
            _gamePlay.Addent1 = Addent1; _gamePlay.Addent2 = Addent2;
        }
        #endregion
    }
}
