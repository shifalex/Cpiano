using Microsoft.Maui.Controls.Shapes;
using MongoDB.Bson;
using static Supabase.Postgrest.Constants;
#if IOS
using UIKit;
#endif

namespace GestureSample.Maui.Models
{


    public class PianoKeyboardReadOnly : MR.Gestures.Grid
    {
        public event EventHandler? LayoutReady;
        public event EventHandler? KeysRebuilt;
        GraphicsView? _overlayView;
        IDrawable? _overlayDrawable;
        Microsoft.Maui.Controls.BoxView[]? _traceOverlayViews;
        Microsoft.Maui.Controls.BoxView[]? _traceOverlaySecondaryViews;
        public GraphicsView? OverlayView => _overlayView;

        public Grid Arrow1; // The combined object containing the number and the arrow
        public Grid Arrow2;


        protected readonly int NUMBER_OF_KEYS;
        protected readonly int FINGER_SEPERATOR = 5;
        int STROKE_THICKNESS = 6;
        int _layoutVersion;

        protected MR.Gestures.Button[] btnKeys;
        public int Length => btnKeys.Length;
        protected virtual int heading_height { get; set; } = 5;


        protected readonly Color COLOR_PRESSED = Colors.Yellow;
        protected readonly Color COLOR_FREE = Colors.White;

        protected readonly Color SECOND_COLOR = Colors.LightGreen;
        protected readonly Color THIRD_COLOR = Colors.Blue;
        protected readonly Color REMOVE_COLOR = Colors.Red;
        public Color[] colors;


        public static readonly BindableProperty KeysProperty =
        BindableProperty.Create(
            nameof(colors),
            typeof(bool[]),
            typeof(PianoKeyboardReadOnly),
            default(bool[]),
            propertyChanged: OnKeysChanged);

        public bool[] Keys
        {
            get => (bool[])GetValue(KeysProperty);
            set => SetValue(KeysProperty, value);
        }

        public readonly double MAX_KEY_WIDTH = 105;
        public double ActualKeyWidth { get; private set; }

        public IReadOnlyList<MR.Gestures.Button> KeyButtons => btnKeys;
        public int KeyCount => btnKeys?.Length ?? 0;



        public void SetNoBorderBetweenRows()
        {
            for (int i = 0; i < RowDefinitions.Count; i++)
            {
                if (RowDefinitions[i].Height.IsStar)
                {
                    //RowDefinitions[i] = null;
                    //RowDefinitions[i].Height = new GridLength(0, GridUnitType.Star);
                }
            }
        }

        public void InvalidateOverlay() => _overlayView?.Invalidate();

        public void ClearTraceOverlay()
        {
            ClearOverlayViews(_traceOverlayViews);
            ClearOverlayViews(_traceOverlaySecondaryViews);
        }

        public void SetTraceOverlayColors(Color?[]? traceOverlayColors, Color?[]? secondaryTraceOverlayColors = null)
        {
            ApplyOverlayColors(_traceOverlayViews, traceOverlayColors);
            ApplyOverlayColors(_traceOverlaySecondaryViews, secondaryTraceOverlayColors);
        }

        private static void ClearOverlayViews(Microsoft.Maui.Controls.BoxView[]? overlayViews)
        {
            if (overlayViews == null)
                return;

            for (int i = 0; i < overlayViews.Length; i++)
            {
                overlayViews[i].IsVisible = false;
                overlayViews[i].BackgroundColor = Colors.Transparent;
            }
        }

        private static void ApplyOverlayColors(Microsoft.Maui.Controls.BoxView[]? overlayViews, Color?[]? overlayColors)
        {
            if (overlayViews == null)
                return;

            for (int i = 0; i < overlayViews.Length; i++)
            {
                Color? overlayColor = overlayColors != null && i < overlayColors.Length
                    ? overlayColors[i]
                    : null;

                if (overlayColor is Color visibleColor)
                {
                    overlayViews[i].BackgroundColor = visibleColor.WithAlpha(1f);
                    overlayViews[i].Opacity = 1;
                    overlayViews[i].IsVisible = true;
                }
                else
                {
                    overlayViews[i].IsVisible = false;
                    overlayViews[i].BackgroundColor = Colors.Transparent;
                }
            }
        }

        public void FixOverlaySpan()
        {
            if (_overlayView == null) return;

            Grid.SetRow(_overlayView, 0);
            Grid.SetColumn(_overlayView, 0);
            Grid.SetRowSpan(_overlayView, Math.Max(1, RowDefinitions.Count));
            Grid.SetColumnSpan(_overlayView, Math.Max(1, ColumnDefinitions.Count));

            _overlayView.Invalidate();
        }
        

        public void InstallOverlay(IDrawable drawable, int zIndex = 1000)
        {
            _overlayDrawable = drawable;

            if (_overlayView == null)
            {
                _overlayView = new GraphicsView
                {
                    InputTransparent = true,
                    ZIndex = zIndex,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill
                };

                _overlayView.Drawable = _overlayDrawable;

                // IMPORTANT: put overlay in same grid cell
                Grid.SetRowSpan(_overlayView, 1000);
                Grid.SetColumnSpan(_overlayView, 1000);

                Children.Add(_overlayView);
            }

            _overlayView.Drawable = drawable;
            _overlayView.Invalidate();
        }

        private static void OnKeysChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is PianoKeyboardReadOnly control)
            {
                if (newValue is bool[] boolKeys)
                    control.PianoInit(boolKeys);
                else if (newValue is Color[] colorKeys)
                    control.PianoInit(colorKeys);
            }
        }

        public static readonly BindableProperty IsArrowByLengthProperty =
        BindableProperty.Create(
            nameof(IsArrowByLength),
            typeof(bool),
            typeof(PianoKeyboardReadOnly),
            default(bool));

        public bool IsArrowByLength
        {
            get => (bool)GetValue(IsArrowByLengthProperty);
            set => SetValue(IsArrowByLengthProperty, value);
        }



        private static void OnIsArrowByLengthChanged(BindableObject bindable, object oldValue, object newValue)
        {
            
        }


        public static readonly BindableProperty DirectionProperty =
        BindableProperty.Create(
            nameof(Direction),
            typeof(Direction),
            typeof(PianoKeyboardReadOnly),
            Direction.Right, // default value can be whatever makes sense
            propertyChanged: OnDirectionChanged);

        public Direction Direction
        {
            get => (Direction)GetValue(DirectionProperty);
            set => SetValue(DirectionProperty, value);
        }

        private static void OnDirectionChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is PianoKeyboardReadOnly control && newValue is Direction dir)
            {

                control.RemoveArrows();
                if (control.ArrowLength != null && control.AboveNumber != null)
                { }//control.AddArrow(control.Direction, (int)control.AboveNumber, (int)control.ArrowLength,1,8);

            }
        }

        // 2. AboveNumber Property
        public static readonly BindableProperty AboveNumberProperty =
            BindableProperty.Create(
                nameof(AboveNumber),
                typeof(int?),
                typeof(PianoKeyboardReadOnly),
                default(int?), // null by default
                propertyChanged: OnAboveNumberChanged);

        public int? AboveNumber
        {
            get => (int?)GetValue(AboveNumberProperty);
            set => SetValue(AboveNumberProperty, value);
        }

        private static void OnAboveNumberChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is PianoKeyboardReadOnly control && newValue != null && newValue is int number)
            {
                control.RemoveArrows();
                if (control.ArrowLength != null && control.AboveNumber != null)
                { }//control.AddArrow(control.Direction, (int)control.AboveNumber, (int)control.ArrowLength, 1);
            }
        }


        // 3. Length Property
        public static readonly BindableProperty ArrowLengthProperty =
            BindableProperty.Create(
                nameof(ArrowLength),
                typeof(int?),
                typeof(PianoKeyboardReadOnly),
                default(int?),
                propertyChanged: OnLengthChanged);

        public int? ArrowLength
        {
            get => (int?)GetValue(ArrowLengthProperty);
            set => SetValue(ArrowLengthProperty, value);
        }

        private static void OnLengthChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is PianoKeyboardReadOnly control && newValue != null && newValue is int number)
            {
                control.RemoveArrows();
                if (control.ArrowLength != null && control.AboveNumber != null)
                { }// control.AddArrow(control.Direction, (int)control.AboveNumber, (int)control.ArrowLength, 1);
            }
        }


        public void AddArrow(Direction direction, int aboveKeyNumber, int numberAbove = -1, int row = 1, double columnWidth=102.5, int columnspan=2, bool isSecondArrow=false, string? labelTextOverride = null)
        {
            Console.WriteLine("Adding arrow: {0} {1} {2} {3}", direction, aboveKeyNumber, numberAbove, row);
            if (!isSecondArrow) { AboveNumber =aboveKeyNumber ; ArrowLength = numberAbove; Direction = direction; } // Set the properties for the first arrow
            columnWidth = ActualKeyWidth > 0 ? ActualKeyWidth : columnWidth; // Use ActualKeyWidth if available, otherwise use provided columnWidth
            if (columnWidth > MAX_KEY_WIDTH) columnWidth = MAX_KEY_WIDTH;
            double border_width = 5;
            double seperator_width = 5+ border_width;
            double arrow_reduction = 13;
            Grid Arrow = new()
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = 30 }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star }
                },
                FlowDirection = FlowDirection.LeftToRight // << no mirroring for arrows
            };

            var request = btnKeys[0].Measure(double.PositiveInfinity, double.PositiveInfinity);
            //double columnWidth = request.Request.Width;

            //double columnWidth = btnKeys[0].Width;


            int colSpan;
            double arrowStart, arrowEnd, arrowEdgeX;
            bool toAddSeperator = false;
            if (direction == Direction.Right)
            {
                arrowStart = 0;
                colSpan = aboveKeyNumber switch
                {
                    10 => 1,
                    _ => 2
                };
                arrowEnd = 0;// arrowStart + columnWidth * Math.Min(colSpan, 2);// - (aboveKeyNumber == 10 ? columnWidth / 2 + 10 : 0) + 10;
                if (aboveKeyNumber == FINGER_SEPERATOR) { toAddSeperator = true; } 
                if (IsArrowByLength)
                {
                    colSpan = numberAbove;
                    Console.WriteLine("{0} {1}",aboveKeyNumber, numberAbove);
                    toAddSeperator = false;
                    arrowEnd = 0;
                    if (!isSecondArrow)
                    {
                        
                        if (aboveKeyNumber -1 + numberAbove > NUMBER_OF_KEYS)//aboveKeyNumber is the number that it is to it's left
                        {
                            colSpan = NUMBER_OF_KEYS - aboveKeyNumber + 1;
                            int secondArrowColSpan = aboveKeyNumber + numberAbove - NUMBER_OF_KEYS - 1;
                            //secondArrowColSpan = (secondArrowColSpan>FINGER_SEPERATOR)? secondArrowColSpan+1 : secondArrowColSpan;
                            Console.WriteLine("Adding second arrow with colSpan {0}", secondArrowColSpan);
                            AddArrow(Direction.Right, 1,-1,1, columnWidth, secondArrowColSpan, true);
                            arrowEnd += 3*arrow_reduction;
                        }
                        if (aboveKeyNumber <= FINGER_SEPERATOR && aboveKeyNumber -1 + numberAbove > FINGER_SEPERATOR)
                        {
                            toAddSeperator = true;
                            
                        }
                        
                    }
                    else 
                    {
                        colSpan = columnspan;
                        if(colSpan>FINGER_SEPERATOR) toAddSeperator = true;
                        Console.WriteLine("Colspan second arrow: {0}", colSpan);
                        arrowStart -= 2* STROKE_THICKNESS;
                        arrowEnd += 2* STROKE_THICKNESS;
                    }
                    
                 }
                arrowEnd += arrowStart + (columnWidth + border_width) * colSpan+(toAddSeperator? seperator_width : 0) -border_width- arrow_reduction ;

                arrowEdgeX = arrowEnd - arrow_reduction;
                //numberLabel.HorizontalOptions = LayoutOptions.Start;
            }
            else
            {
                arrowStart = 0;
                colSpan = aboveKeyNumber switch
                {
                    1 => 1,
                    _ => 2
                };
                if (aboveKeyNumber == (FINGER_SEPERATOR + 1)) toAddSeperator = true;

                arrowEnd = aboveKeyNumber == 1 ? 0 : 0;
                //if (aboveKeyNumber == 1) arrowStart = -3;  
                if (IsArrowByLength)
                {
                    toAddSeperator = false;
                    if (!isSecondArrow)
                    {
                        colSpan = numberAbove;
                        if (aboveKeyNumber - numberAbove < 0)
                        {
                            colSpan = aboveKeyNumber; if (colSpan > FINGER_SEPERATOR) toAddSeperator = true;
                            int secondArrowColSpan = numberAbove-aboveKeyNumber;

                            secondArrowColSpan = (secondArrowColSpan > FINGER_SEPERATOR) ? secondArrowColSpan + 1 : secondArrowColSpan;
                            Console.WriteLine("Adding second arrow with colSpan {0}", secondArrowColSpan);
                            AddArrow(Direction.Left, NUMBER_OF_KEYS, -1 , 1, columnWidth, secondArrowColSpan, true);
                            arrowEnd -= 3 * arrow_reduction;
                        }
                        if (aboveKeyNumber > FINGER_SEPERATOR && aboveKeyNumber - numberAbove <= FINGER_SEPERATOR)
                        {
                            toAddSeperator = true;
                        }
                    }
                    else
                    {                       
                        colSpan = columnspan; 
                        if(colSpan> FINGER_SEPERATOR) toAddSeperator = true;
                        Console.WriteLine("Colspan second arrow: {0}", colSpan);
                        arrowStart += 2*STROKE_THICKNESS;
                        arrowEnd -= 2*STROKE_THICKNESS;                        
                    }
                }
                arrowStart += colSpan * (columnWidth+border_width) + (toAddSeperator ? seperator_width : 0) - border_width - STROKE_THICKNESS; 
                
                
                arrowEnd += arrow_reduction;
                arrowEdgeX = arrowEnd + arrow_reduction;
                //numberLabel.HorizontalOptions = LayoutOptions.End;
            }
            string pathData = KeyboardArrowPathBuilder.BuildPathData(
                Config.ArrowType,
                direction,
                aboveKeyNumber,
                columnWidth,
                arrowStart,
                arrowEnd,
                arrowEdgeX);

            if (Config.ArrowType != ArrowType.Rounded)
                Console.WriteLine("Arrow path data: " + pathData + " colspan {0}", colSpan);

            Console.WriteLine(pathData);
            Grid arrowVisual = KeyboardArrowVisualFactory.CreateArrowVisual(pathData, numberAbove, STROKE_THICKNESS, labelTextOverride);


            //if(aboveKeyNumber==0)aboveKeyNumber = 1;
            // Add the number and the arrow to the combined object grid
            Arrow.Add(arrowVisual, 0, 0);
            Grid.SetRowSpan(arrowVisual, 2);

            // Add the combined object to the main grid in the correct column
            int column = aboveKeyNumber - 1;

            if (toAddSeperator) { colSpan++; }
            
            //Console.WriteLine("Adding arrow at column {0} colSpan {2}", column, row, colSpan);
            if (direction == Direction.Left) column=column+1 - colSpan;
            //TODO: is the column==-1 check needed?
            if (column == -1 || (FINGER_SEPERATOR>0 && ((column>FINGER_SEPERATOR-1 && direction == Direction.Right) || (aboveKeyNumber > FINGER_SEPERATOR && direction == Direction.Left)))) column++;
            //Console.WriteLine("Adding arrow (2) at column {0} colSpan {2}", column, row, colSpan);


            //Arrow.BackgroundColor = Colors.Blue;
            Arrow.VerticalOptions = LayoutOptions.End;
            this.Add(Arrow, column, row);
            //this.RowDefinitions[1].Height = new GridLength(60);

            Grid.SetColumnSpan(Arrow, colSpan);
            Console.WriteLine("Arrow added!!! at column {0} row {1} colSpan {2}", column, row, colSpan);

            if (Arrow1 == null) Arrow1 = Arrow; else Arrow2 = Arrow;
        }

        public void RemoveArrows()
        {
            if (Arrow1 != null) this.Remove(Arrow1);
            if (Arrow2 != null) this.Remove(Arrow2);
            Arrow1 = null; Arrow2 = null;
        }

        public KeyboardConfig Config;

        public PianoKeyboardReadOnly() : base()
        {
            Config = new();
            Config.NormalizeWeightedLayout();
            int keysInRow = Config.KeysInRow;
            int rows = Config.Rows;
            NUMBER_OF_KEYS = keysInRow * rows;
            this.FlowDirection = FlowDirection.LeftToRight;  // << force LTR
            InitializeWithConfig(Config);
        }
        public PianoKeyboardReadOnly(KeyboardConfig config) : base()
        {
            config.NormalizeWeightedLayout();
            int keysInRow = config.KeysInRow;
            int rows = config.Rows;
            NUMBER_OF_KEYS = keysInRow * rows;
            this.FlowDirection = FlowDirection.LeftToRight;  // << force LTR
            InitializeWithConfig(config);
        }

        private void OnSizeChanged(object sender, EventArgs e)
        {
            int v = ++_layoutVersion;
            //await Task.Delay(200);
            if (v != _layoutVersion) return;

            if (btnKeys.Length > 0)
            {
                int keysInRow = Config.KeysInRow;

                double available = Width;
                if (available > 0)
                {
                    double spacing = (keysInRow - 1) * ColumnSpacing;
                    double sep = (keysInRow > 10 ? 0 : FINGER_SEPERATOR);

                    double desiredKeyWidth = (available - spacing - sep) / keysInRow;
                    desiredKeyWidth = Math.Min(desiredKeyWidth, MAX_KEY_WIDTH);
                    if (desiredKeyWidth < 0) desiredKeyWidth = 0;

                    double contentWidth = keysInRow * desiredKeyWidth + spacing + sep;
                    double extra = available - contentWidth;
                    if (extra < 0) extra = 0;

                    var newPadding = new Thickness(extra / 2, Padding.Top, extra / 2, Padding.Bottom);
                    if (Math.Abs(newPadding.Left - Padding.Left) > 0.5)
                        Padding = newPadding;

                    ActualKeyWidth = desiredKeyWidth;
                }
            }

            FixOverlaySpan();
            LayoutReady?.Invoke(this, EventArgs.Empty);
            //InvalidateOverlay();


            //LayoutReady?.Invoke(this, EventArgs.Empty);

            if (ArrowLength != null && AboveNumber != null)
            {
                int arrowLength = (int)ArrowLength;
                int aboveNumber = (int)AboveNumber;
                Direction dir = Direction;

                RemoveArrows();
                AddArrow(dir, aboveNumber, arrowLength, 1, ActualKeyWidth);
            }

        }


        private void InitializeWithConfig(KeyboardConfig config)
        {
            config.NormalizeWeightedLayout();
            Config = config;
            int keysInRow = config.KeysInRow;
            int rows = config.Rows;
            this.ColumnSpacing = 5;
            this.BackgroundColor = Colors.Black;
            //this.Padding = textBoxesQuantity==0? new Thickness(0, 30, 0, 0):0;
            int handSeperator = keysInRow/2; if (keysInRow > 10) handSeperator = keysInRow + 1;

            colors = new Color[NUMBER_OF_KEYS];

            if (config.IsArrow /*|| config.ImposeEdges*/)
            {
                IsArrowByLength = config.IsArrowLengthDynamic??false;
                heading_height = 34;
                Console.WriteLine("Heading height: " + heading_height);
            }
            this.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(heading_height) });
            if (config.IsArrow /*|| config.ImposeEdges*/)
            {
                this.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            }
            btnKeys = new MR.Gestures.Button[NUMBER_OF_KEYS];
            _traceOverlayViews = new Microsoft.Maui.Controls.BoxView[NUMBER_OF_KEYS];
            _traceOverlaySecondaryViews = new Microsoft.Maui.Controls.BoxView[NUMBER_OF_KEYS];
            for (int i = 0; i < keysInRow + (handSeperator < keysInRow ? 1 : 0); i++)
                this.ColumnDefinitions.Add((i == handSeperator) ? new ColumnDefinition { Width = new GridLength(5) } : new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            for (int r = 0; r < rows; r++)
                this.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            for (int r = 0; r < rows; r++)
            {
                for (int i = 0; i < keysInRow; i++)//TODO: enable 2 rows with 10 keys
                {
                    int keyIndex = i + keysInRow * r;
                    this.Add(
                    btnKeys[keyIndex] = new()
                    {
                        Text = GetKeyDisplayText(keyIndex),
                        ClassId = "PianoKeyButton",
                        TextColor = Colors.Black,
                        BackgroundColor = COLOR_FREE,  
                        CommandParameter = keyIndex + 1,
                        MaximumWidthRequest = MAX_KEY_WIDTH,
                        Margin = new Thickness(0, 5, 0, 0),
                        //DownCommand = new Command<MR.Gestures.DownUpEventArgs>(OnDown), 
                        //UpCommand =  new Command<MR.Gestures.DownUpEventArgs>(OnUp), 
                        FlowDirection = FlowDirection.LeftToRight  // << force LTR
                    }, (i < handSeperator) ? i : i + 1,
                        //r+1 
                        rows - r + (config.IsArrow ? 1 : 0)
                    );
                    HookPianoKeyVisualNormalization(btnKeys[keyIndex]);

                    VisualStateManager.SetVisualStateGroups(btnKeys[keyIndex], new VisualStateGroupList
                    {
                        new VisualStateGroup
                        {
                            Name = "CommonStates",
                            States =
                            {
                                new VisualState
                                {
                                    Name = "Normal",
                                    Setters =
                                    {
                                        new Setter { Property = MR.Gestures.Button.TextColorProperty, Value = Colors.Black },
                                        new Setter { Property = MR.Gestures.Button.OpacityProperty, Value = 1d }
                                    }
                                },
                                new VisualState
                                {
                                    Name = "Pressed",
                                    Setters =
                                    {
                                        new Setter { Property = MR.Gestures.Button.TextColorProperty, Value = Colors.Black },
                                        new Setter { Property = MR.Gestures.Button.OpacityProperty, Value = 1d }
                                    }
                                },
                                new VisualState
                                {
                                    Name = "Disabled",
                                    Setters =
                                    {
                                        new Setter { Property = MR.Gestures.Button.TextColorProperty, Value = Colors.Black },
                                        new Setter { Property = MR.Gestures.Button.OpacityProperty, Value = 1d }
                                    }
                                }
                            }
                        }
                    });

                    int column = (i < handSeperator) ? i : i + 1;
                    int row = rows - r + (config.IsArrow ? 1 : 0);
                    this.Add(
                        _traceOverlaySecondaryViews[keyIndex] = new Microsoft.Maui.Controls.BoxView
                        {
                            InputTransparent = true,
                            IsVisible = false,
                            BackgroundColor = Colors.Transparent,
                            Margin = new Thickness(12, 40, 12, 0),
                            HorizontalOptions = LayoutOptions.Fill,
                            VerticalOptions = LayoutOptions.Start,
                            HeightRequest = 14,
                            Opacity = 1,
                            ZIndex = 18
                        },
                        column,
                        row);

                    this.Add(
                        _traceOverlayViews[keyIndex] = new Microsoft.Maui.Controls.BoxView
                        {
                            InputTransparent = true,
                            IsVisible = false,
                            BackgroundColor = Colors.Transparent,
                            Margin = new Thickness(8, 18, 8, 0),
                            HorizontalOptions = LayoutOptions.Fill,
                            VerticalOptions = LayoutOptions.Start,
                            HeightRequest = 14,
                            Opacity = 1,
                            ZIndex = 20
                        },
                        column,
                        row);
                }
            }
            /*if (config.ImposeEdges)
            {
                this.AddArrow(Direction.Right, 1);
                this.AddArrow(Direction.Left, 10);
            }*/



            this.SizeChanged -= OnSizeChanged;
            this.SizeChanged += OnSizeChanged;

            FixOverlaySpan();
            KeysRebuilt?.Invoke(this, EventArgs.Empty);
            InvalidateOverlay();

        }

        private string GetKeyDisplayText(int keyIndex)
        {
            if (Config == null || !Config.ShowNumbersOnKeys)
                return string.Empty;

            if (Config.WeightsArray != null && keyIndex < Config.WeightsArray.Length)
                return Config.WeightsArray[keyIndex].ToString();

            return (keyIndex + 1).ToString();
        }

        public void RefreshKeyCaptions()
        {
            if (btnKeys == null)
                return;

            for (int i = 0; i < btnKeys.Length; i++)
            {
                btnKeys[i].Text = GetKeyDisplayText(i);
            }

            ScheduleNormalizeAllPianoKeyVisuals();
        }

        protected void NormalizeAllPianoKeyVisuals()
        {
            if (btnKeys == null)
                return;

            for (int i = 0; i < btnKeys.Length; i++)
            {
                NormalizePianoKeyVisual(btnKeys[i]);
            }
        }

        protected void ScheduleNormalizeAllPianoKeyVisuals()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                NormalizeAllPianoKeyVisuals();

                Dispatcher?.DispatchDelayed(TimeSpan.FromMilliseconds(40), () =>
                {
                    NormalizeAllPianoKeyVisuals();
                });
            });
        }

        protected void NormalizePianoKeyVisual(MR.Gestures.Button button)
        {
            button.TextColor = Colors.Black;
            button.Opacity = 1;

#if IOS
            if (button.Handler?.PlatformView is UIButton nativeButton)
            {
                nativeButton.Highlighted = false;
                nativeButton.Selected = false;
                nativeButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
                nativeButton.SetTitleColor(UIColor.Black, UIControlState.Highlighted);
                nativeButton.SetTitleColor(UIColor.Black, UIControlState.Disabled);
                nativeButton.SetTitleColor(UIColor.Black, UIControlState.Selected);
                nativeButton.SetTitleColor(UIColor.Black, UIControlState.Focused);
                nativeButton.TintColor = UIColor.Black;
                nativeButton.TintAdjustmentMode = UIViewTintAdjustmentMode.Normal;
                nativeButton.Alpha = 1;
                nativeButton.AdjustsImageWhenDisabled = false;
                nativeButton.ConfigurationUpdateHandler = null;
                nativeButton.TitleLabel.TextColor = UIColor.Black;
            }
#endif
        }

        private void HookPianoKeyVisualNormalization(MR.Gestures.Button button)
        {
            button.Loaded += (_, _) => NormalizePianoKeyVisual(button);
            button.HandlerChanged += (_, _) => NormalizePianoKeyVisual(button);
        }
        /// <summary>
        /// Creates pressed piano
        /// </summary>
        /// <param name="array">Must be the size of the piano buttons</param>
        public void PianoInit(Boolean[] array)
        {
            int limit = Math.Min(btnKeys.Length, array?.Length ?? 0);
            for (int i = 0; i < limit; i++)
            {
                btnKeys[i].BackgroundColor = (array[i]) ? COLOR_PRESSED : COLOR_FREE;
            }
            for (int i = limit; i < btnKeys.Length; i++)
            {
                btnKeys[i].BackgroundColor = COLOR_FREE;
            }
            SaveColors();
            ScheduleNormalizeAllPianoKeyVisuals();
        }
        public void PianoInit(Color[] array)
        {
            int limit = Math.Min(btnKeys.Length, array?.Length ?? 0);
            for (int i = 0; i < limit; i++)
            {
                btnKeys[i].BackgroundColor = array[i];
            }
            for (int i = limit; i < btnKeys.Length; i++)
            {
                btnKeys[i].BackgroundColor = COLOR_FREE;
            }
            SaveColors();
            ScheduleNormalizeAllPianoKeyVisuals();
        }
        public void PianoInit(bool[] primaryArray, bool[] secondaryArray, Color primaryColor, Color secondaryColor)
        {
            int limit = btnKeys.Length;
            for (int i = 0; i < limit; i++)
            {
                bool primary = primaryArray != null && i < primaryArray.Length && primaryArray[i];
                bool secondary = secondaryArray != null && i < secondaryArray.Length && secondaryArray[i];

                if (primary)
                    btnKeys[i].BackgroundColor = primaryColor;
                else if (secondary)
                    btnKeys[i].BackgroundColor = secondaryColor;
                else
                    btnKeys[i].BackgroundColor = COLOR_FREE;
            }

            SaveColors();
            ScheduleNormalizeAllPianoKeyVisuals();
        }
        public void Random()
        {
            Random r = new();

            for (int i = 0; i < btnKeys.Length; i++)
            {
                btnKeys[i].BackgroundColor = (r.Next(2) == 1) ? COLOR_PRESSED : COLOR_FREE;
            }
            SaveColors();
            ScheduleNormalizeAllPianoKeyVisuals();
        }

        public bool[] ToBitArray()
        {
            bool[] bitArray = new bool[btnKeys.Length];
            for (int i = 0; i < btnKeys.Length; i++)
                bitArray[i] = btnKeys[i].BackgroundColor != COLOR_FREE;
            return bitArray;
        }

        public int GetColorCount(Color color)
        {
            int count = 0;
            for (int i = 0; i < btnKeys.Length; i++)
            {
                if (btnKeys[i].BackgroundColor == color)
                    count++;
            }

            return count;
        }

        public bool[] GetBitsForColor(Color color)
        {
            bool[] bits = new bool[btnKeys.Length];
            for (int i = 0; i < btnKeys.Length; i++)
            {
                bits[i] = btnKeys[i].BackgroundColor == color;
            }

            return bits;
        }

        public int GetNonFreeColorCount()
        {
            int count = 0;
            for (int i = 0; i < btnKeys.Length; i++)
            {
                if (btnKeys[i].BackgroundColor != COLOR_FREE)
                    count++;
            }

            return count;
        }

        public Color[] GetCurrentColors()
        {
            Color[] snapshot = new Color[btnKeys.Length];
            for (int i = 0; i < btnKeys.Length; i++)
            {
                snapshot[i] = btnKeys[i].BackgroundColor;
            }

            return snapshot;
        }

        public virtual int Sum
        {
            get
            {
                int sum = 0;
                for (int i = 0; i < btnKeys.Length; i++)
                    sum += btnKeys[i].BackgroundColor == COLOR_PRESSED ? 1 : 0;
                return sum;
            }
        }

        protected void SaveColors()
        {

            for (int i = 0; i < NUMBER_OF_KEYS; i++) colors[i] = btnKeys[i].BackgroundColor;
        }

        public void SpecialColors()
        {             for (int i = 0; i < NUMBER_OF_KEYS; i++)
            {
                btnKeys[i].BackgroundColor = btnKeys[i].BackgroundColor == COLOR_PRESSED ? Colors.Blue : Colors.Red;
                
            }
            this.RowSpacing = 0;
        }
    }
}
