using MR.Gestures;
using MvvmCross.Base;
using GestureSample.Maui.Data;
using Microsoft.Maui.Layouts;


namespace GestureSample.Maui.Models
{
    internal class PianoKeyboard : PianoKeyboardReadOnly//, IDisposable
    {


        protected override int heading_height { get; set; } = 55;
        public bool[] initColors ;

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
        private readonly KeyEventRepository _keyEventRepository;

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

        protected async void SaveState(int eventType, int key, int row = 0)
        {

/* Unmerged change from project 'GestureSample.Maui (net7.0-ios)'
Before:
            Data.KeyEvent keyEvent = new()
            {
After:
            KeyEvent keyEvent = new()
            {
*/
            Data.SQLite.KeyEvent keyEvent = new()
            {
                EventTime = DateTime.Now,
                KeyNumber = key,
                Row = row,
                EventType = eventType,
                GameId= _gamePlay.GameId.ToString(),
                QuestionNumber = _gamePlay._questionNumber
            };// = new ();
            await _keyEventRepository.SaveAsync(keyEvent);
        }

        public PianoKeyboard(PPWGamePlay gamePlay, Microsoft.Maui.Controls.Label lblTimer,
            KeyboardConfig pianoConfig) : base(pianoConfig)
        {
            _keyEventRepository = ServiceHelper.GetService<KeyEventRepository>();
            _patterns = pianoConfig.SyncType == SyncType.Spatial || pianoConfig.ImposeEdges || pianoConfig.IsMulticolor || pianoConfig.WeightsArray!=null;
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


            // Add an image to your Resources/Images folder, e.g., "reset.png" (ensure Build Action: MauiImage)

            initColors = new bool[btnKeys.Length];
            for (int i = 0; i < btnKeys.Length; i++)
            {
                initColors[i] = false;// btnKeys[i].BackgroundColor = COLOR_FREE;
            }

            Microsoft.Maui.Controls.Button btnInit = new()
            {
                ImageSource = "reset.png", // Use your professional icon here
                BackgroundColor = Colors.Transparent,
                Padding = new Thickness(2),
                WidthRequest = 40,
                HeightRequest = 40,
                ZIndex = 100, // Ensure it appears above other elements
                Command = new Command(() =>
                {
                    // Reset the keyboard state
                    Console.WriteLine("Resetting Piano Keyboard...");
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
                HorizontalOptions = LayoutOptions.Start
                
               
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
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(heading_height) });
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
                /*
                Microsoft.Maui.Controls.HorizontalStackLayout hzl = new()
                {
                    btnInit
                };
                this.SetColumnSpan(hzl, pianoConfig.KeysInRow + 1);
                this.Add(hzl, 0);
                // 1. phantom row
                this.RowDefinitions.Insert(
    0,
    new RowDefinition { Height = new GridLength(1, GridUnitType.Absolute) } // ABSOLUTE 0 px
);

                // 2. add reset button
                //btnInit.Margin = new Thickness(12);   // visible padding
                btnInit.TranslationY = 20;     // pull into visible area
                btnInit.ZIndex = 99;
                Microsoft.Maui.Controls.Grid.SetRow(btnInit, 0);
                Microsoft.Maui.Controls.Grid.SetColumnSpan(btnInit, _pianoConfig.KeysInRow + 1);
                //Microsoft.Maui.Controls.Grid.SetZIndex(btnInit, 99);
                this.Children.Add(btnInit);

                // 3. allow overflow
                this.IsClippedToBounds = false;


                btnInit.WidthRequest = 40;
                btnInit.HeightRequest = 40;
                btnInit.ZIndex = 100;     // paint above everything
                */

                Console.WriteLine("Heading height for btnInit: " + heading_height);
                Microsoft.Maui.Controls.Grid g = new();
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(heading_height) });
                // ── 2. create an overlay layer ----------------------------------
                /*var overlay = new Microsoft.Maui.Controls.AbsoluteLayout
                {
                    InputTransparent = true      // taps go through, except on btnInit
                };

                // place the button in the overlay, top-left corner
                Microsoft.Maui.Controls.AbsoluteLayout.SetLayoutBounds(btnInit, new Rect(0, 0, 20, 20));
                Microsoft.Maui.Controls.AbsoluteLayout.SetLayoutFlags(btnInit, AbsoluteLayoutFlags.PositionProportional);
                //btnInit.TranslationX = 12;       // visual padding
                //btnInit.TranslationY = 12;
                btnInit.InputTransparent = false; // button itself must receive taps
                overlay.Children.Add(btnInit);

                // ── 3. pin the overlay on top of the whole keyboard grid --------
                Children.Add(overlay);               // “this” is the grid you inherit

                Microsoft.Maui.Controls.Grid.SetRow(overlay, 0);        // any existing cell is fine
                Microsoft.Maui.Controls.Grid.SetColumn(overlay, 0);
                Microsoft.Maui.Controls.Grid.SetRowSpan(overlay, RowDefinitions.Count);
                Microsoft.Maui.Controls.Grid.SetColumnSpan(overlay, ColumnDefinitions.Count);

                // the grid must allow children to spill out (needed only once)
                IsClippedToBounds = false;*/
                g.Add(btnInit, 0);
                this.SetColumnSpan(g, pianoConfig.KeysInRow + 1);
                g.HorizontalOptions = LayoutOptions.Fill;
                this.Add(g, 0);

            }
        }

        public virtual void PianoInit()
        {
            IsEnabled = true;
            _addend1 = 0;
            _addend2 = 0;

            for (int i = 0; i < btnKeys.Length; i++)
            {
                btnKeys[i].BackgroundColor = initColors[i]?COLOR_PRESSED:COLOR_FREE;
                if (btnKeys[i].BackgroundColor == COLOR_PRESSED)
                {
                    if (i > btnKeys.Length/2)
                        _addend2++;
                    else
                        _addend1++;
                }
            }
            if (_patterns) setAddendsByPattern();
            OnPropertyChanged(nameof(Addend1)); OnPropertyChanged(nameof(Addend2)); OnPropertyChanged(nameof(Sum));
            SaveColors();
            AddDummies();
        }

        //Spatial
        protected virtual void setAddendsByPattern()
        {
            if(Config.WeightsArray != null)
            {
                _addend1 = 0; _addend2 = 0;
                for (int i = 0; i < NUMBER_OF_KEYS; i++)
                {
                    if (btnKeys[i].BackgroundColor == COLOR_PRESSED)
                    {
                        if (i < NUMBER_OF_KEYS / 2)
                            _addend1 += Config.WeightsArray[i];
                        else
                            _addend2 += Config.WeightsArray[i];
                    }
                }
                return;
            }

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
            if (Config.IsMulticolor)
            {
                if (sender.BackgroundColor == COLOR_FREE)
                {
                    sender.BackgroundColor = COLOR_PRESSED;
                }
                else if (sender.BackgroundColor == COLOR_PRESSED)
                {
                    sender.BackgroundColor = SECOND_COLOR;
                }
                else
                {
                    sender.BackgroundColor = COLOR_FREE;
                }
            }
            else
            {
                sender.BackgroundColor = (sender.BackgroundColor != COLOR_PRESSED) ? COLOR_PRESSED : COLOR_FREE;
            }
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
            Color prevColor = sender.BackgroundColor;
            if ((isDown ? InnerKeyDown(sender) : InnerKeyUp(sender)) && _patterns)
                setAddendsByPattern();

            OnPropertyChanged(nameof(Addend1)); OnPropertyChanged(nameof(Addend2)); OnPropertyChanged(nameof(Sum));
            //_gamePlay.addend1 = Addend1; _gamePlay.addend2 = Addend2;
            int keyNumber = 0;
            int row = 0;
            for(int j=0; j<_pianoConfig.Rows; j++)
            for (int i = 0; i < btnKeys.Length; i++)
            {
                    if (btnKeys[i] == sender)
                    {
                        keyNumber = i + 1;
                        row = j;
                    }
            }
            if (prevColor != sender.BackgroundColor)
            {
                //TODO: add all the x and y of the touches on the keyboard to different db table using e.Touches[0] - will be needed for the touching patterns. Make it a seperate event of touch the grid which doesn't interfere
                SaveState(isDown ? 1 : 0, keyNumber, row);
            }
        }
    }
}
