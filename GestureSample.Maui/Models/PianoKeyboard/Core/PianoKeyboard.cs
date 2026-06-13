using MR.Gestures;
using MvvmCross.Base;
using GestureSample.Maui.Data;
using GestureSample.Maui.Data.SQLite;
using Microsoft.Maui.Layouts;


namespace GestureSample.Maui.Models
{
    public class PianoKeyboard : PianoKeyboardReadOnly//, IDisposable
    {

        protected readonly SoundService _soundService;

        protected override int heading_height { get; set; } = 55;
        public bool[] initColors ;

        protected int _addend1 = 0;
        protected int _addend2 = 0;

        public Microsoft.Maui.Controls.Button BtnInit { get; private set; }


        //TODO: Let user define Statement.Neutral as NeutralText
        //TODO: Give the option to save to database the keypress and timestamp and keyboard ID and the new color(which is made when it is created. And a database to work with..
        //TODO: Make an interface
        protected readonly PPWGamePlay _gamePlay;


        protected readonly KeyboardConfig _pianoConfig;
        public event Action? HeadingTapped;

        protected readonly Microsoft.Maui.Controls.Label _lblTimer;
        protected bool _patterns;
        protected readonly bool _imposeEdges = false;
        private readonly KeyEventRepository _keyEventRepository;
        private readonly KeyboardQuestionRepository _keyboardQuestionRepository;
        private readonly VisibilityChangeEventRepository _visibilityChangeEventRepository;
        private int? _draggingKeyIndex;
        private Color _draggingKeyColor = Colors.Transparent;
        protected virtual Color TraceSecondColor => SECOND_COLOR.WithAlpha(0.82f);
        protected virtual Color TraceThirdColor => THIRD_COLOR.WithAlpha(0.7f);
        private Microsoft.Maui.Controls.Entry? _headerResultEntry;
        private View? _headerResultVisibilityTarget;
        private bool _headerResultInitiallyVisible;
        private bool _headerResultVisible;
        public bool SupportsExternalHeaderResultVisibilityToggle =>
            _headerResultInitiallyVisible && _headerResultVisibilityTarget != null;

        private void EnsureAllKeyTextIsBlack()
        {
            if (btnKeys == null)
                return;

            for (int i = 0; i < btnKeys.Length; i++)
            {
                btnKeys[i].TextColor = Colors.Black;
                btnKeys[i].Opacity = 1;
            }

            NormalizeAllPianoKeyVisuals();
        }

        protected virtual void AddDummies()
        {
            int[] dummiesArray = _pianoConfig.DummiesArray;
            for (int i = 0; i < btnKeys.Length; i++)
            {

                if (_pianoConfig.DummiesArray != null && dummiesArray.Length > i && dummiesArray[i] > -1)
                {
                    //btnKeys[i].Opacity = 0.5;
                    btnKeys[i].IsEnabled = true;
                    btnKeys[i].InputTransparent = true;
                    btnKeys[i].TextColor = Colors.Black;
                    btnKeys[i].BackgroundColor = dummiesArray[i] == 1 ? COLOR_PRESSED : COLOR_FREE;
                }
                else
                {
                    btnKeys[i].Opacity = 1;
                    btnKeys[i].IsEnabled = true;
                    btnKeys[i].InputTransparent = false;
                    btnKeys[i].TextColor = Colors.Black;
                    btnKeys[i].BackgroundColor = colors[i];
                }
                // btnKeys[i].DownCommand = new Command<MR.Gestures.DownUpEventArgs>(OnDown);
                // btnKeys[i].UpCommand = new Command<MR.Gestures.DownUpEventArgs>(OnUp);

            }
            EnsureAllKeyTextIsBlack();
        }

        //TODO: add constructor for 20 keys and for keyboard questions
        //public BitArray Keys;

        public int Addend1 { get => _addend1; }
        public int Addend2 { get => _addend2; }
        public override int Sum { get => _addend1 + _addend2; }

        
        //private readonly Data.RealmService _realmService;

        protected async Task SaveStateAsync(int eventType, int key, int row = 0, double? relativeX = null, double? relativeY = null)
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
                RelativeX = relativeX,
                RelativeY = relativeY,
                GameId= _gamePlay.GameId.ToString(),
                QuestionNumber = _gamePlay._questionNumber
            };// = new ();
            await _keyEventRepository.SaveAsync(keyEvent);
        }

        private static double Clamp01(double value)
        {
            if (value < 0)
                return 0;
            if (value > 1)
                return 1;
            return value;
        }

        private static (double? relativeX, double? relativeY) GetRelativeTouch(MR.Gestures.DownUpEventArgs e, MR.Gestures.Button sender)
        {
            double width = e.ViewPosition.Width > 0 ? e.ViewPosition.Width : sender.Width;
            double height = e.ViewPosition.Height > 0 ? e.ViewPosition.Height : sender.Height;

            if (width <= 0 || height <= 0)
                return (null, null);

            if (e.Touches == null || e.Touches.Length == 0)
                return (null, null);

            int touchIndex = 0;
            if (e.TriggeringTouches != null && e.TriggeringTouches.Length > 0)
            {
                touchIndex = Math.Clamp(e.TriggeringTouches[0], 0, e.Touches.Length - 1);
            }

            var touch = e.Touches[touchIndex];
            double x = touch.X;
            double y = touch.Y;

            if (e.ViewPosition.X > 0 && x >= e.ViewPosition.X)
            {
                x -= e.ViewPosition.X;
            }
            if (e.ViewPosition.Y > 0 && y >= e.ViewPosition.Y)
            {
                y -= e.ViewPosition.Y;
            }
            return (Clamp01(x / width), Clamp01(y / height));
        }

        public PianoKeyboard(PPWGamePlay gamePlay, Microsoft.Maui.Controls.Label lblTimer,
            KeyboardConfig pianoConfig) : base(pianoConfig)
        {

            _soundService =  ServiceHelper.GetService<SoundService>();
            _soundService.Mode = pianoConfig.IsVoice?2:1;//TODO:make an enum
            _keyEventRepository = ServiceHelper.GetService<KeyEventRepository>();
            _keyboardQuestionRepository = ServiceHelper.GetService<KeyboardQuestionRepository>();
            _visibilityChangeEventRepository = ServiceHelper.GetService<VisibilityChangeEventRepository>();
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
            Panning += OnKeyboardPanning;
            Panned += OnKeyboardPanned;


            // Add an image to your Resources/Images folder, e.g., "reset.png" (ensure Build Action: MauiImage)

            initColors = new bool[btnKeys.Length];
            for (int i = 0; i < btnKeys.Length; i++)
            {
                initColors[i] = false;// btnKeys[i].BackgroundColor = COLOR_FREE;
            }

            BtnInit = new()
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

            VisualStateManager.SetVisualStateGroups(BtnInit, new VisualStateGroupList
{
    new VisualStateGroup
    {
        Name = "CommonStates",
        States =
        {
            new VisualState { Name = "Normal" },
            new VisualState
            {
                Name = "Disabled",
                Setters =
                {
                    new Setter { Property = Microsoft.Maui.Controls.Button.OpacityProperty, Value = 0.6 },
                    new Setter { Property = Microsoft.Maui.Controls.Button.BackgroundColorProperty, Value = Colors.Transparent }
                }
            }
        }
    }
});

            if (textBoxesQuantity > 0)
            {
                Microsoft.Maui.Controls.Entry[] a_array = new Microsoft.Maui.Controls.Entry[3];
                bool canToggleSumHeaderVisibility = CanToggleSumHeaderVisibility(textBoxesQuantity);
                bool canToggleFromHeader = CanToggleAnswerTimePanelFromHeader() && !canToggleSumHeaderVisibility;
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

                    if (canToggleFromHeader)
                        a_array[i].GestureRecognizers.Add(CreateHeaderTapGesture());
                }
                a_array[0].IsVisible = textBoxesQuantity >= 2; a_array[0].SetBinding(Microsoft.Maui.Controls.Entry.TextProperty, nameof(Addend1));
                a_array[1].IsVisible = textBoxesQuantity >= 2; a_array[1].SetBinding(Microsoft.Maui.Controls.Entry.TextProperty, nameof(Addend2));
                a_array[2].IsVisible = textBoxesQuantity == 1 || textBoxesQuantity == 3;
                a_array[2].SetBinding(Microsoft.Maui.Controls.Entry.TextProperty, nameof(Sum));
                _headerResultInitiallyVisible = a_array[2].IsVisible;
                _headerResultVisible = _headerResultInitiallyVisible;
                _headerResultEntry = a_array[2];
                View headerResultView = a_array[2];
                if (canToggleSumHeaderVisibility)
                {
                    _headerResultEntry.InputTransparent = true;
                    Microsoft.Maui.Controls.Grid resultTapHost = new()
                    {
                        WidthRequest = a_array[2].WidthRequest,
                        HeightRequest = a_array[2].HeightRequest,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        InputTransparent = false
                    };
                    Microsoft.Maui.Controls.Button resultTapOverlay = new()
                    {
                        BackgroundColor = Colors.Transparent,
                        Opacity = 0.01,
                        Padding = 0,
                        CornerRadius = 0,
                        BorderWidth = 0,
                        HorizontalOptions = LayoutOptions.Fill,
                        VerticalOptions = LayoutOptions.Fill
                    };
                    resultTapOverlay.Clicked += async (_, _) => await ToggleHeaderResultVisibilityAsync("HeaderResultOverlayButton");
                    resultTapHost.Add(a_array[2]);
                    resultTapHost.Add(resultTapOverlay);
                    headerResultView = resultTapHost;
                }
                _headerResultVisibilityTarget = headerResultView;

                Microsoft.Maui.Controls.HorizontalStackLayout hzl = new()
                {
                    a_array[0],
                    new Microsoft.Maui.Controls.Label(){ HorizontalOptions = LayoutOptions.Center, WidthRequest = 50, IsVisible = textBoxesQuantity >= 2 },
                    headerResultView,
                    new Microsoft.Maui.Controls.Label(){ HorizontalOptions = LayoutOptions.Center, WidthRequest = 50, IsVisible = textBoxesQuantity == 3 },
                    a_array[1]
                };
                hzl.HorizontalOptions = LayoutOptions.Center;
                hzl.VerticalOptions = LayoutOptions.Center;
                if (canToggleFromHeader)
                    hzl.GestureRecognizers.Add(CreateHeaderTapGesture());
                Microsoft.Maui.Controls.Grid g = new();
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(heading_height) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                g.Add(hzl, 0, 0);
                g.HorizontalOptions = LayoutOptions.Fill;
                g.VerticalOptions = LayoutOptions.Start;
                g.InputTransparent = !(canToggleFromHeader || canToggleSumHeaderVisibility);
                if (canToggleFromHeader)
                    g.GestureRecognizers.Add(CreateHeaderTapGesture());
                g.ZIndex = 50;
                if (canToggleFromHeader)
                    g.SetBinding(Microsoft.Maui.Controls.Grid.PaddingProperty, new Binding(nameof(Padding), source: this));
                Microsoft.Maui.Controls.Grid.SetRow(g, 0);
                Microsoft.Maui.Controls.Grid.SetColumn(g, 0);
                Microsoft.Maui.Controls.Grid.SetColumnSpan(g, ColumnDefinitions.Count);
                Children.Add(g);
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
                g.HorizontalOptions = LayoutOptions.Fill;
                g.VerticalOptions = LayoutOptions.Start;
                Microsoft.Maui.Controls.Grid.SetRow(g, 0);
                Microsoft.Maui.Controls.Grid.SetColumn(g, 0);
                Microsoft.Maui.Controls.Grid.SetColumnSpan(g, ColumnDefinitions.Count);
                Children.Add(g);

            }
        }

        private bool CanToggleAnswerTimePanelFromHeader()
        {
            return _pianoConfig.AllowAnswerTimePanelToggleFromKeyboardHeader;
        }

        private bool CanToggleSumHeaderVisibility(int textBoxesQuantity)
        {
            return textBoxesQuantity == 1 || textBoxesQuantity == 3;
        }

        private TapGestureRecognizer CreateHeaderTapGesture()
        {
            TapGestureRecognizer tapRecognizer = new();
            tapRecognizer.Tapped += (_, _) => HeadingTapped?.Invoke();
            return tapRecognizer;
        }

        private TapGestureRecognizer CreateHeaderResultVisibilityTapGesture()
        {
            TapGestureRecognizer tapRecognizer = new();
            tapRecognizer.Tapped += async (_, _) => await ToggleHeaderResultVisibilityAsync("HeaderResultTap");
            return tapRecognizer;
        }

        private async Task ToggleHeaderResultVisibilityAsync(string source)
        {
            if (!_headerResultInitiallyVisible || _headerResultVisibilityTarget == null)
                return;

            bool previousVisibility = _headerResultVisible;
            bool nextVisibility = !previousVisibility;
            _headerResultVisible = nextVisibility;
            _headerResultVisibilityTarget.IsVisible = nextVisibility;

            if (string.Equals(source, "ExternalButton", StringComparison.Ordinal))
            {
                await _keyboardQuestionRepository.MarkHeaderResultToggleUsedAsync(
                    _gamePlay.GameId.ToString(),
                    _gamePlay._questionNumber);
            }

            if (_visibilityChangeEventRepository == null)
                return;

            VisibilityChangeEvent visibilityEvent = new()
            {
                GameId = _gamePlay.GameId.ToString(),
                QuestionNumber = _gamePlay._questionNumber,
                EventTime = DateTime.Now,
                Target = "HeaderResult",
                WasVisible = previousVisibility,
                IsVisible = nextVisibility,
                WasInitiallyVisible = _headerResultInitiallyVisible,
                Source = source
            };

            await _visibilityChangeEventRepository.SaveAsync(visibilityEvent);
        }

        public Task ToggleHeaderResultVisibilityFromExternalButtonAsync()
        {
            return ToggleHeaderResultVisibilityAsync("ExternalButton");
        }

        public virtual void PianoInit()
        {
            IsEnabled = true;
            _addend1 = 0;
            _addend2 = 0;
            if(_soundService!=null)
                _soundService.StopAllVoices();

            for (int i = 0; i < btnKeys.Length; i++)
            {
                btnKeys[i].TextColor = Colors.Black;
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
            EnsureAllKeyTextIsBlack();
        }

        public new void PianoInit(bool[] array)
        {
            base.PianoInit(array);
            RecalculateKeyboardStateFromColors();
            AddDummies();
        }

        public new void PianoInit(Color[] array)
        {
            base.PianoInit(array);
            RecalculateKeyboardStateFromColors();
            AddDummies();
        }

        private void RecalculateKeyboardStateFromColors()
        {
            _addend1 = 0;
            _addend2 = 0;

            if (_patterns)
            {
                setAddendsByPattern();
            }
            else
            {
                for (int i = 0; i < btnKeys.Length; i++)
                {
                    if (btnKeys[i].BackgroundColor != COLOR_FREE)
                    {
                        if (i >= btnKeys.Length / 2)
                            _addend2++;
                        else
                            _addend1++;
                    }
                }
            }

            OnPropertyChanged(nameof(Addend1));
            OnPropertyChanged(nameof(Addend2));
            OnPropertyChanged(nameof(Sum));
            SaveColors();
            EnsureAllKeyTextIsBlack();
        }

        //Spatial
        protected virtual void setAddendsByPattern()
        {
            if(Config.WeightsArray != null)
            {
                _addend1 = 0; _addend2 = 0;
                int weightsCount = Config.WeightsArray.Length;
                for (int i = 0; i < NUMBER_OF_KEYS && i < weightsCount; i++)
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

        protected bool UsePermutationTraceColors()
        {
            return false;
        }

        private bool UsesSecondColorEntryMode()
        {
            return _pianoConfig.ColorInteractionMode == KeyboardColorInteractionMode.AddSecondColor;
        }

        private bool UsesThreeColorGroupByColorCycle()
        {
            return Config.IsMulticolor &&
                   _gamePlay is BitArrayGamePlay bitArrayGamePlay &&
                   bitArrayGamePlay.CurrentOperation == Operation.GroupByColor &&
                   (Config.GroupByColorColorCount >= 3 ||
                    (Config.GroupByColorCounts?.Length ?? 0) >= 3);
        }

        private bool UsesRedRemovalMode()
        {
            return _pianoConfig.ColorInteractionMode == KeyboardColorInteractionMode.RemoveWithRed;
        }

        private bool EnablesColorDrag()
        {
            return _pianoConfig.EnableColorDrag;
        }

        private bool TryBeginColorDrag(MR.Gestures.Button sender)
        {
            if (!EnablesColorDrag())
                return false;

            Color color = sender.BackgroundColor;
            if (color == COLOR_FREE || color == REMOVE_COLOR)
                return false;

            int keyIndex = Array.IndexOf(btnKeys, sender);
            if (keyIndex < 0)
                return false;

            _draggingKeyIndex = keyIndex;
            _draggingKeyColor = color;
            return true;
        }

        protected bool ColorsMatch(Color a, Color b)
        {
            return Math.Abs(a.Red - b.Red) < 0.01f &&
                   Math.Abs(a.Green - b.Green) < 0.01f &&
                   Math.Abs(a.Blue - b.Blue) < 0.01f &&
                   Math.Abs(a.Alpha - b.Alpha) < 0.01f;
        }

        protected bool IsPermutationSecondTrace(Color color)
        {
            return ColorsMatch(color, TraceSecondColor);
        }

        protected bool IsPermutationThirdTrace(Color color)
        {
            return ColorsMatch(color, TraceThirdColor);
        }

        protected void AdvancePermutationTraceColors(MR.Gestures.Button? exempt = null)
        {
            for (int i = 0; i < btnKeys.Length; i++)
            {
                MR.Gestures.Button button = btnKeys[i];
                if (button == exempt)
                    continue;

                if (button.BackgroundColor == COLOR_PRESSED)
                {
                    button.BackgroundColor = TraceSecondColor;
                }
                else if (IsPermutationSecondTrace(button.BackgroundColor))
                {
                    button.BackgroundColor = TraceThirdColor;
                }
                else if (IsPermutationThirdTrace(button.BackgroundColor))
                {
                    button.BackgroundColor = COLOR_FREE;
                }
            }
        }

        protected void SetPermutationTracePressed(MR.Gestures.Button sender)
        {
            AdvancePermutationTraceColors(sender);
            sender.BackgroundColor = COLOR_PRESSED;
        }

        protected void ReleasePermutationTracePressed(MR.Gestures.Button sender)
        {
            if (sender.BackgroundColor != COLOR_PRESSED)
                return;

            sender.BackgroundColor = TraceSecondColor;
        }

        protected virtual bool InnerKeyDown(MR.Gestures.Button sender)
        {
            if (TryBeginColorDrag(sender))
            {
                return false;
            }

            if (UsePermutationTraceColors())
            {
                SetPermutationTracePressed(sender);
                return false;
            }

            if (UsesRedRemovalMode())
            {
                if (sender.BackgroundColor == COLOR_PRESSED)
                {
                    sender.BackgroundColor = REMOVE_COLOR;
                }
                else if (sender.BackgroundColor == REMOVE_COLOR)
                {
                    sender.BackgroundColor = COLOR_PRESSED;
                }

                return false;
            }

            if (UsesSecondColorEntryMode())
            {
                if (sender.BackgroundColor == COLOR_FREE)
                {
                    sender.BackgroundColor = SECOND_COLOR;
                }
                else if (sender.BackgroundColor == SECOND_COLOR)
                {
                    sender.BackgroundColor = COLOR_FREE;
                }

                return false;
            }

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
                else if (UsesThreeColorGroupByColorCycle() && sender.BackgroundColor == SECOND_COLOR)
                {
                    sender.BackgroundColor = THIRD_COLOR;
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
            if (UsesRedRemovalMode() || UsesSecondColorEntryMode() || _draggingKeyIndex.HasValue)
            {
                return true;
            }

            if (UsePermutationTraceColors())
            {
                ReleasePermutationTracePressed(sender);
            }

            if (Convert.ToInt32(sender.CommandParameter) > 5)
                _addend2 = (sender.BackgroundColor != COLOR_PRESSED) ? _addend2 - 1 : _addend2 + 1;
            else
                _addend1 = (sender.BackgroundColor != COLOR_PRESSED) ? _addend1 - 1 : _addend1 + 1;


            if (_addend1 < 0) _addend1 = 0;
            if (_addend2 < 0) _addend2 = 0;


            return true;
        }

        private void OnKeyboardPanning(object? sender, MR.Gestures.PanEventArgs e)
        {
            if (!_draggingKeyIndex.HasValue || !EnablesColorDrag() || e.Touches == null || e.Touches.Length == 0)
                return;

            int targetIndex = GetKeyIndexAt(e.Touches[0]);
            if (targetIndex < 0 || targetIndex == _draggingKeyIndex.Value)
                return;

            if (btnKeys[targetIndex].BackgroundColor != COLOR_FREE)
                return;

            btnKeys[targetIndex].BackgroundColor = _draggingKeyColor;
            btnKeys[_draggingKeyIndex.Value].BackgroundColor = COLOR_FREE;
            _draggingKeyIndex = targetIndex;
            RecalculateKeyboardStateFromColors();
        }

        private void OnKeyboardPanned(object? sender, MR.Gestures.PanEventArgs e)
        {
            _draggingKeyIndex = null;
            _draggingKeyColor = Colors.Transparent;
        }

        private int GetKeyIndexAt(Point touch)
        {
            for (int i = 0; i < btnKeys.Length; i++)
            {
                if (IsOver(touch, btnKeys[i]))
                    return i;
            }

            return -1;
        }

        private static (double x, double y) GetAbsolutePosition(View view)
        {
            double x = view.X;
            double y = view.Y;

            while (view.Parent is View parentView)
            {
                view = parentView;
                x += view.X;
                y += view.Y;
            }

            return (x, y);
        }

        private static bool IsOver(Point touch, View view)
        {
            (double viewX, double viewY) = GetAbsolutePosition(view);
            return new Rect(viewX, viewY, view.Width, view.Height).Contains(touch);
        }

        private async void OnDown(MR.Gestures.DownUpEventArgs e)
        {
            await OnKey(e, true);

            if (Config.IsNumberVoice || Config.IsVoice)
            {
                Console.WriteLine(Convert.ToInt32(((MR.Gestures.Button)e.Sender).CommandParameter));
                if(_soundService != null && Config.IsNumberVoice)
                    await _soundService.PlayNumberAsync(Convert.ToInt32(((MR.Gestures.Button)e.Sender).CommandParameter));
                if (_soundService != null && Config.IsVoice)
                    await _soundService.PlayVoiceAsync(Convert.ToInt32(((MR.Gestures.Button)e.Sender).CommandParameter));
                else
                    Console.WriteLine("Sound service is null");
            }
        }
        private async void OnUp(MR.Gestures.DownUpEventArgs e)
        {
            await OnKey(e, false);
            if (Config.IsNumberVoice || Config.IsVoice)
            {
                Console.WriteLine(Convert.ToInt32(((MR.Gestures.Button)e.Sender).CommandParameter));
                if (_soundService != null && Config.IsVoice)
                    _soundService.StopVoiceAsync(Convert.ToInt32(((MR.Gestures.Button)e.Sender).CommandParameter));
                else
                    Console.WriteLine("Sound service is null");
            }
        }

        private void RefreshSecondArrowLeftTraceOverlay()
        {
            if (!_pianoConfig.EnableSecondArrowLeftTrace ||
                _gamePlay is not BitArrayGamePlay bitArrayGamePlay ||
                btnKeys == null)
            {
                return;
            }

            ClearTraceOverlay();
            ClearSecondArrowTraceKeyBackgrounds();

            bool[] pressedKeys = ToBitArray();
            int leftmostPressedIndex = Array.FindIndex(pressedKeys, pressed => pressed);
            if (leftmostPressedIndex < 0)
            {
                return;
            }

            bool isRtl = bitArrayGamePlay.dir == Direction.Left;
            Color traceColor = SECOND_ARROW_TRACE_YELLOW;

            for (int i = 0; i < pressedKeys.Length; i++)
            {
                if (pressedKeys[i])
                {
                    btnKeys[i].BackgroundColor = isRtl
                        ? REMOVE_COLOR
                        : COLOR_PRESSED;
                }
            }

            if (leftmostPressedIndex == 0)
            {
                return;
            }

            for (int i = 0; i < leftmostPressedIndex; i++)
            {
                if (!pressedKeys[i])
                {
                    btnKeys[i].BackgroundColor = traceColor;
                }
            }
        }

        private void ClearSecondArrowTraceKeyBackgrounds()
        {
            if (btnKeys == null)
                return;

            for (int i = 0; i < btnKeys.Length; i++)
            {
                if (IsSecondArrowTraceColor(btnKeys[i].BackgroundColor))
                {
                    btnKeys[i].BackgroundColor = COLOR_FREE;
                }
            }
        }

        private async Task OnKey(MR.Gestures.DownUpEventArgs e, bool isDown)
        {
            
            if (!IsEnabled) { return; }
            MR.Gestures.Button sender = (MR.Gestures.Button)e.Sender;
            sender.TextColor = Colors.Black;
            sender.Opacity = 1;
            NormalizePianoKeyVisual(sender);
            Color prevColor = sender.BackgroundColor;
            if ((isDown ? InnerKeyDown(sender) : InnerKeyUp(sender)) && _patterns)
            {
                setAddendsByPattern();
            }
               // Console.WriteLine("Special sounds");
                if (_soundService != null && (Config.IsVoice || Config.IsVoices))
                {
                bool stopCorrectSound = true;
                
                if (isDown && BitArrayHelper.IsSequential(this.ToBitArray()) && BitArrayHelper.CountSetBits(this.ToBitArray()) >= 2)
                    {
                        await _soundService.PlayCustomVoiceAsync(11, 4, "Voices");
                        if (_gamePlay.IsCloseEnough(this))
                        {
                            await _soundService.PlayCustomVoiceAsync(12, 6, "Voices");
                            stopCorrectSound=false;
                        }
                    }
                    else if (isDown && !BitArrayHelper.IsSequential(this.ToBitArray()) && BitArrayHelper.CountSetBits(this.ToBitArray()) >= 2)
                    {
                        await _soundService.PlayCustomVoiceAsync(11, 3, "Voices");
                    }
                    else
                    { 
                        _soundService.StopVoiceAsync(11);
                        
                    }
                if (stopCorrectSound)
                        _soundService.StopVoiceAsync(12);
            }


            OnPropertyChanged(nameof(Addend1)); OnPropertyChanged(nameof(Addend2)); OnPropertyChanged(nameof(Sum));
            EnsureAllKeyTextIsBlack();
            RefreshSecondArrowLeftTraceOverlay();
            //_gamePlay.addend1 = Addend1; _gamePlay.addend2 = Addend2;
            int keyNumber = 0;
            int row = 0;
            int keyIndex = Array.IndexOf(btnKeys, sender);
            if (keyIndex >= 0)
            {
                keyNumber = keyIndex + 1;
                row = _pianoConfig.KeysInRow > 0 ? keyIndex / _pianoConfig.KeysInRow : 0;
            }

            (double? relativeX, double? relativeY) = GetRelativeTouch(e, sender);
            if (prevColor != sender.BackgroundColor)
            {
                await SaveStateAsync(isDown ? 1 : 0, keyNumber, row, relativeX, relativeY);
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                NormalizePianoKeyVisual(sender);
                EnsureAllKeyTextIsBlack();
            });

            ScheduleNormalizeAllPianoKeyVisuals();
        }
    }
}
