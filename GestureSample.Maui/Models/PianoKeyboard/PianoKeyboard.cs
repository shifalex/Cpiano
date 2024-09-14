using DevExpress.Data.Utils;
using MR.Gestures;

namespace GestureSample.Maui.Models
{
    internal class PianoKeyboard : PianoKeyboardReadOnly//, IDisposable
    {


        protected override int heading_height { get; } = 55;

        protected int _addend1 = 0;
        protected int _addend2 = 0;

        //TODO: Let user define Statement.Neutral as NeutralText
        //TODO: Give the option to save to database the keypress and timestamp and keyboard ID and the new color(which is made when it is created. And a database to work with..
        //TODO: Make an interface
        protected readonly PPWGamePlay _gamePlay;

        protected readonly KeyboardConfig _pianoConfig;

        protected readonly Microsoft.Maui.Controls.Label _lblTimer;
        protected bool _patterns;
        protected readonly bool _imposeEdges = false;

        protected virtual void AddDummies()
        {
            int[] dummiesArray = _pianoConfig.DummiesArray;
            for (int i = 0; i < btnKeys.Length; i++)
            {

                if (_pianoConfig.DummiesArray != null && dummiesArray.Length > i && dummiesArray[i] > -1)
                {
                    //btnKeys[i].Opacity = 0.5;
                    btnKeys[i].IsEnabled = false;
                    btnKeys[i].BackgroundColor = dummiesArray[i] == 1 ? COLOR_PRESSED : COLOR_FREE;
                }
                else
                {
                    btnKeys[i].Opacity = 1;
                    btnKeys[i].IsEnabled = true;
                    btnKeys[i].BackgroundColor = colors[i];
                }
                // btnKeys[i].DownCommand = new Command<MR.Gestures.DownUpEventArgs>(OnDown);
                // btnKeys[i].UpCommand = new Command<MR.Gestures.DownUpEventArgs>(OnUp);

            }
        }

        //TODO: add constructor for 20 keys and for keyboard questions
        //public BitArray Keys;

        public int Addend1 { get => _addend1; }
        public int Addend2 { get => _addend2; }
        public override int Sum { get => _addend1 + _addend2; }

        //private readonly Data.RealmService _realmService;

        protected async void SaveState()
        {
            Data.State s = new()
            {
                UserId = 1,
                //TimeStamp = DateTime.Now,
                //TypeName = _gamePlay.GameType.ToString(),
                Addend1 = this.Addend1,
                Addend2 = this.Addend2,
                Sum = this.Sum, //TODO:make more elegant
                /*
B1 = btnKeys[0].BackgroundColor== COLOR_PRESSED,
B2 = btnKeys[1].BackgroundColor == COLOR_PRESSED ,
B3 = btnKeys[2].BackgroundColor == COLOR_PRESSED ,
B4 = btnKeys[3].BackgroundColor == COLOR_PRESSED,
B5 = btnKeys[4].BackgroundColor == COLOR_PRESSED,
B6 = btnKeys[5].BackgroundColor == COLOR_PRESSED,
B7 = btnKeys[6].BackgroundColor == COLOR_PRESSED,
B8 = btnKeys[7].BackgroundColor == COLOR_PRESSED,
B9 = btnKeys[8].BackgroundColor == COLOR_PRESSED,
B10 = btnKeys[9].BackgroundColor == COLOR_PRESSED
*/
            };
            //await Data.StateConnection.Instance.SaveStateAsync(s);
            //await _realmService.AddStateAsync(s);
        }

        public PianoKeyboard(PPWGamePlay gamePlay, Microsoft.Maui.Controls.Label lblTimer,
            KeyboardConfig pianoConfig) : base(pianoConfig)
        {

            _patterns = pianoConfig.SyncType == SyncType.Spatial || pianoConfig.ImposeEdges;
            _imposeEdges = pianoConfig.ImposeEdges;
            int textBoxesQuantity = pianoConfig.TextBoxesQuantity;
            _gamePlay = gamePlay;
            _lblTimer = lblTimer;
            _pianoConfig = pianoConfig;
            for (int i = 0; i < NUMBER_OF_KEYS; i++) colors[i] = COLOR_FREE;

            //_realmService = new();

            for (int i = 0; i < btnKeys.Length; i++)
            {
                //var wEHD= new WeakEventHandler<MR.Gestures.DownUpEventArgs>(OnDown);
                //var wEHU = new WeakEventHandler<MR.Gestures.DownUpEventArgs>(OnUp);
                btnKeys[i].DownCommand = new Command<MR.Gestures.DownUpEventArgs>(OnDown);
                btnKeys[i].UpCommand = new Command<MR.Gestures.DownUpEventArgs>(OnUp);

            }
            AddDummies();


            Microsoft.Maui.Controls.Button btnInit = new()
            {
                Text = "Reset",
                Command = new Command(() =>
                {
                    PianoInit();
                    for (int i = 0; i < btnKeys.Length; i++)
                    {
                        btnKeys[i].DownCommand = null;
                        btnKeys[i].UpCommand = null;
                    }
                    for (int i = 0; i < btnKeys.Length; i++)
                    {
                        btnKeys[i].DownCommand = new Command<MR.Gestures.DownUpEventArgs>(OnDown);
                        btnKeys[i].UpCommand = new Command<MR.Gestures.DownUpEventArgs>(OnUp);

                    }
                }),
                HorizontalOptions = LayoutOptions.Start,
                WidthRequest = 80,
                HeightRequest = 16
            };

            if (textBoxesQuantity > 0)
            {
                Microsoft.Maui.Controls.Entry[] a_array = new Microsoft.Maui.Controls.Entry[3];
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
                a_array[0].IsVisible = textBoxesQuantity >= 2; a_array[0].SetBinding(Microsoft.Maui.Controls.Entry.TextProperty, nameof(Addend1));
                a_array[1].IsVisible = textBoxesQuantity >= 2; a_array[1].SetBinding(Microsoft.Maui.Controls.Entry.TextProperty, nameof(Addend2));
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
                g.Add(hzl, 1); g.Add(btnInit, 0);
                this.SetColumnSpan(g, pianoConfig.KeysInRow + 1);
                g.HorizontalOptions = LayoutOptions.Fill;
                this.Add(g, 0);
            }
            else
            {
                Microsoft.Maui.Controls.HorizontalStackLayout hzl = new()
                {
                    btnInit
                };
                this.SetColumnSpan(hzl, pianoConfig.KeysInRow + 1);
                this.Add(hzl, 0);
            }
        }

        public virtual void PianoInit()
        {
            IsEnabled = true;

            for (int i = 0; i < btnKeys.Length; i++)
            {
                btnKeys[i].BackgroundColor = COLOR_FREE;
            }
            _addend1 = 0;
            _addend2 = 0;
            OnPropertyChanged(nameof(Addend1)); OnPropertyChanged(nameof(Addend2)); OnPropertyChanged(nameof(Sum));
            SaveColors();
            AddDummies();
        }

        public void Dispose()
        {
            /*for (int i = 0; i < btnKeys.Length; i++)
            {
                btnKeys[i].DownCommand = null;
                btnKeys[i].UpCommand = null;
                btnKeys[i] = null;
            }*/

        }

        //Spatial
        protected virtual void setAddendsByPattern()
        {
            _addend1 = 0; _addend2 = 0; bool isNowYellowStreak = false; int yellowStreaksTillNowIncluding = 0;
            for (int i = 0; i < NUMBER_OF_KEYS; i++)
            {
                if (btnKeys[i].BackgroundColor == COLOR_PRESSED || btnKeys[i].BackgroundColor == COLOR_PRESSED)
                {
                    if (!isNowYellowStreak) { isNowYellowStreak = true; yellowStreaksTillNowIncluding++; }
                    if (yellowStreaksTillNowIncluding == 1) _addend1++;
                    else if (yellowStreaksTillNowIncluding == 2) _addend2++;
                    else if (yellowStreaksTillNowIncluding > 2) { _addend1 = 0; _addend2 = -1; break; }
                }
                else
                    isNowYellowStreak = false;
            }
            if (yellowStreaksTillNowIncluding == 1 /*one addend is 0 - which one? if most keys in the left - Addend2, if most keys in the right - Addend1*/)
            {
                for (int i = _pianoConfig.LeftAddendIndex; i < NUMBER_OF_KEYS; i++)
                    if (btnKeys[i].BackgroundColor == COLOR_PRESSED && btnKeys[NUMBER_OF_KEYS - 1 - i + _pianoConfig.LeftAddendIndex].BackgroundColor == COLOR_FREE)
                        break;
                    else if (btnKeys[i].BackgroundColor == COLOR_FREE && btnKeys[NUMBER_OF_KEYS - 1 - i + _pianoConfig.LeftAddendIndex].BackgroundColor == COLOR_PRESSED)
                    {
                        _addend2 = _addend1; _addend1 = 0;
                        break;
                    }
            }
            ImposeEdgesIfNeeded();
        }

        protected virtual void ImposeEdgesIfNeeded()
        {
            bool begin = true; bool end = true;
            if (_imposeEdges /*Make wrong input if edges weren't imposed*/)
            {
                for (int i = 0; i < NUMBER_OF_KEYS; i++)
                    if (begin && btnKeys[i].BackgroundColor == COLOR_FREE) begin = false;
                    else if (!begin && btnKeys[i].BackgroundColor != COLOR_FREE) end = false;
                    else if (!end && btnKeys[i].BackgroundColor == COLOR_FREE)
                    {
                        _addend1 = -1; _addend2 = -1; return;
                    }
            }
        }
        protected virtual bool InnerKeyDown(MR.Gestures.Button sender)
        {
            sender.BackgroundColor = (sender.BackgroundColor != COLOR_PRESSED) ? COLOR_PRESSED : COLOR_FREE;
            return false;
        }

        protected virtual bool InnerKeyUp(MR.Gestures.Button sender)
        {
            if (Convert.ToInt32(sender.CommandParameter) > 5)
                _addend2 = (sender.BackgroundColor != COLOR_PRESSED) ? _addend2 - 1 : _addend2 + 1;
            else
                _addend1 = (sender.BackgroundColor != COLOR_PRESSED) ? _addend1 - 1 : _addend1 + 1;


            if (_addend1 < 0) _addend1 = 0;
            if (_addend2 < 0) _addend2 = 0;


            return true;
        }

        private void OnDown(MR.Gestures.DownUpEventArgs e)
        {
            OnKey(e, true);
        }
        private void OnUp(MR.Gestures.DownUpEventArgs e)
        {
            OnKey(e, false);
        }
        private void OnKey(MR.Gestures.DownUpEventArgs e, bool isDown)
        {
            if (!IsEnabled) { return; }
            MR.Gestures.Button sender = (MR.Gestures.Button)e.Sender;

            if ((isDown ? InnerKeyDown(sender) : InnerKeyUp(sender)) && _patterns)
                setAddendsByPattern();

            OnPropertyChanged(nameof(Addend1)); OnPropertyChanged(nameof(Addend2)); OnPropertyChanged(nameof(Sum));
            //_gamePlay.addend1 = Addend1; _gamePlay.addend2 = Addend2;
            SaveState();
        }
    }
}
