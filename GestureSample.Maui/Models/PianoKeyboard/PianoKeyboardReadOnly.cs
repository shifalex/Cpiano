using Microsoft.Maui.Controls.Shapes;
using MongoDB.Bson;

namespace GestureSample.Maui.Models
{


    public class PianoKeyboardReadOnly : MR.Gestures.Grid
    {


        public Grid Arrow1; // The combined object containing the number and the arrow
        public Grid Arrow2;


        protected readonly int NUMBER_OF_KEYS;
        protected readonly int FINGER_SEPERATOR = 5;
        protected MR.Gestures.Button[] btnKeys;
        public int Length => btnKeys.Length;
        protected virtual int heading_height { get; set; } = 5;


        protected readonly Color COLOR_PRESSED = Colors.Yellow;
        protected readonly Color COLOR_FREE = Colors.White;
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
                    control.AddArrow(control.Direction, (int)control.AboveNumber, (int)control.ArrowLength,1,8);

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
                    control.AddArrow(control.Direction, (int)control.AboveNumber, (int)control.ArrowLength, 1, 8);
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
                    control.AddArrow(control.Direction, (int)control.AboveNumber, (int)control.ArrowLength, 1, 8);
            }
        }


        public void AddArrow(Direction direction, int aboveKeyNumber, int numberAbove = -1, int row = 1, double columnWidth=106, int columnspan=2)
        {
            Console.WriteLine("Adding arrow: {0} {1} {2} {3}", direction, aboveKeyNumber, numberAbove, row);
            double seperator_width = 16;
            double arrow_reduction = 13;
             double border_width = 5;
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
                }
            };

            var request = btnKeys[0].Measure(double.PositiveInfinity, double.PositiveInfinity);
            //double columnWidth = request.Request.Width;

            //double columnWidth = btnKeys[0].Width;


            if (columnWidth == -1)
                columnWidth = 13;
            //TODO: ARROW DRAWING - First draw buttons
            //TODO: ARROW DRAWING - solve orientation switch arrow bug
            // Create the number label
            Label numberLabel = new()
            {
                Text = numberAbove.ToString(),
                TextColor = Colors.White,
                FontSize = 25,
                //WidthRequest = columnWidth,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                FontAttributes = Microsoft.Maui.Controls.FontAttributes.Bold,
                FontFamily = DeviceInfo.Platform == DevicePlatform.iOS ? "HelveticaNeue-Bold" : null, // Use iOS system bold font

                IsVisible = numberAbove > -1

            };

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
                if (aboveKeyNumber == 5) { toAddSeperator = true; } // Because of ColSpan 3
                if (IsArrowByLength)
                {
                    Console.WriteLine("",aboveKeyNumber, numberAbove);
                    toAddSeperator = false;
                    if (numberAbove > 0)
                    {
                        colSpan = numberAbove;
                        arrowEnd = 0;
                        if (aboveKeyNumber + numberAbove > 10+1)
                        {
                            colSpan = 10 - aboveKeyNumber + 1;
                            int secondArrowColSpan = aboveKeyNumber + numberAbove - 10-1;
                            secondArrowColSpan = (secondArrowColSpan>FINGER_SEPERATOR)? secondArrowColSpan+1 : secondArrowColSpan;
                            AddArrow(Direction.Right, 1,-1,1, columnWidth, secondArrowColSpan);
                        }
                        if (aboveKeyNumber < 6 && aboveKeyNumber + numberAbove > 6)
                        {
                            toAddSeperator = true;
                            
                        }
                        
                    }
                    else { 
                    }
                 }
                arrowEnd += arrowStart + columnWidth * colSpan+(toAddSeperator? seperator_width : 0) -border_width- arrow_reduction ;

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
                if (aboveKeyNumber == 6) toAddSeperator = true;
                //if (aboveKeyNumber == 1) arrowStart = -3;  
                if (IsArrowByLength)
                {
                    toAddSeperator = false;
                    if (numberAbove > 0)
                    {
                        colSpan = numberAbove;
                        if (aboveKeyNumber - numberAbove < 0)
                        {
                            colSpan = aboveKeyNumber + 1;
                            int secondArrowColSpan = numberAbove-aboveKeyNumber;
                            secondArrowColSpan = (secondArrowColSpan > 5) ? secondArrowColSpan + 1 : secondArrowColSpan;
                            AddArrow(Direction.Left, 10, -1, 1, columnWidth, secondArrowColSpan);
                        }
                        if (aboveKeyNumber > 6 && aboveKeyNumber - numberAbove < 5)
                        {
                            toAddSeperator = true;
                        }
                    }
                    else
                    {
                    }
                }
                arrowStart += colSpan * columnWidth + (toAddSeperator ? seperator_width : 0)- border_width; 
                
                
                arrowEnd = aboveKeyNumber == 1 ? 0 : 0;
                arrowEnd += arrow_reduction;
                arrowEdgeX = arrowEnd + arrow_reduction;
                //numberLabel.HorizontalOptions = LayoutOptions.End;
            }
            // Create the arrow
            string pathData;
            if (Config.ArrowType == ArrowType.Rounded)
            {
                arrowStart = columnWidth / 2 + ((aboveKeyNumber == 1 || direction == Direction.Right) ? 0 : columnWidth);
                double arcEnd = arrowStart + (direction == Direction.Right ? 20 : -20);
                arrowEnd = arcEnd + (direction == Direction.Right ? 20 : -20);
                arrowEdgeX = arrowEnd + (direction == Direction.Right ? -10 : 10);
                int clockwise = direction == Direction.Right ? 1 : 0;
                pathData = String.Format("M {0},30 A 20,20 0 0 {4} {3},10 L {1},10 L {2},0 M {1},10 L {2},20", arrowStart, arrowEnd, arrowEdgeX, arcEnd, clockwise);
            }
            else
            {
                pathData = String.Format("M {0},50 L {0},15 L {1},15 L {2},2 M {1},15 L {2},28", arrowStart, arrowEnd, arrowEdgeX);
                Console.WriteLine("Arrow path data: " + pathData + "colspan {0}", colSpan);
            }
            Console.WriteLine(pathData);
            Microsoft.Maui.Controls.Shapes.Path arrow = CreateArrowPath(pathData, Colors.White);


            //if(aboveKeyNumber==0)aboveKeyNumber = 1;
            // Add the number and the arrow to the combined object grid
            Arrow.Add(numberLabel, 0, 0);
            Arrow.Add(arrow, 0, 1);

            // Add the combined object to the main grid in the correct column
            int column = aboveKeyNumber - 1;

            if (toAddSeperator) { colSpan++; }
            
            //Console.WriteLine("Adding arrow at column {0} colSpan {2}", column, row, colSpan);
            if (direction == Direction.Left) column=column+1 - colSpan;
            //TODO: is the column==-1 check needed?
            if (column == -1 || (FINGER_SEPERATOR>0 && ((column>FINGER_SEPERATOR-1 && direction == Direction.Right) || (aboveKeyNumber > FINGER_SEPERATOR && direction == Direction.Left)))) column++;
            //Console.WriteLine("Adding arrow (2) at column {0} colSpan {2}", column, row, colSpan);
            this.Add(Arrow, column, row);

            Grid.SetColumnSpan(Arrow, colSpan);

            if (Arrow1 == null) Arrow1 = Arrow; else Arrow2 = Arrow;
        }

        public void RemoveArrows()
        {
            if (Arrow1 != null) this.Remove(Arrow1);
            if (Arrow2 != null) this.Remove(Arrow2);
            Arrow1 = null; Arrow2 = null;
        }


        private Microsoft.Maui.Controls.Shapes.Path CreateArrowPath(string data, Color color)
        {
            return new Microsoft.Maui.Controls.Shapes.Path
            {
                Data = (Geometry)new PathGeometryConverter().ConvertFromInvariantString(data),
                Fill = Colors.Transparent,
                Stroke = color,
                StrokeThickness = 6,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center
            };
        }
        public KeyboardConfig Config;

        public PianoKeyboardReadOnly() : base()
        {
            Config = new();
            int keysInRow = Config.KeysInRow;
            int rows = Config.Rows;
            NUMBER_OF_KEYS = keysInRow * rows;
            InitializeWithConfig(Config);
        }
        public PianoKeyboardReadOnly(KeyboardConfig config) : base()
        {

            int keysInRow = config.KeysInRow;
            int rows = config.Rows;
            NUMBER_OF_KEYS = keysInRow * rows;
            InitializeWithConfig(config);
        }

        private void InitializeWithConfig(KeyboardConfig config)
        {
            Config = config;
            int keysInRow = config.KeysInRow;
            int rows = config.Rows;
            this.ColumnSpacing = 5;
            this.BackgroundColor = Colors.Black;
            //this.Padding = textBoxesQuantity==0? new Thickness(0, 30, 0, 0):0;
            int handSeperator = 5; if (keysInRow > 10) handSeperator = keysInRow + 1;

            colors = new Color[NUMBER_OF_KEYS];

            if (config.IsArrow /*|| config.ImposeEdges*/)
            {
                IsArrowByLength = config.IsArrowLengthDynamic??false;
                heading_height = 20;
                Console.WriteLine("Heading height: " + heading_height);
            }
            this.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(heading_height) });
            if (config.IsArrow /*|| config.ImposeEdges*/)
            {
                this.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            }
            btnKeys = new MR.Gestures.Button[NUMBER_OF_KEYS];
            for (int i = 0; i < keysInRow + (handSeperator < keysInRow ? 1 : 0); i++)
                this.ColumnDefinitions.Add((i == handSeperator) ? new ColumnDefinition { Width = new GridLength(5) } : new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            for (int r = 0; r < rows; r++)
                this.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            for (int r = 0; r < rows; r++)
            {
                for (int i = 0; i < keysInRow; i++)//TODO: enable 2 rows with 10 keys
                {
                    this.Add(
                    btnKeys[i + keysInRow * r] = new()
                    {
                        Text = (config.ShowNumbersOnKeys) ? (i + 1 + keysInRow * r).ToString() : "",
                        TextColor = Colors.Black,
                        BackgroundColor = COLOR_FREE,  
                        CommandParameter = i + 1 + keysInRow * r,
                        Margin = new Thickness(0, 5, 0, 0),
                        //DownCommand = new Command<MR.Gestures.DownUpEventArgs>(OnDown), 
                        //UpCommand =  new Command<MR.Gestures.DownUpEventArgs>(OnUp), 
                    }, (i < handSeperator) ? i : i + 1,
                        //r+1 
                        rows - r + (config.IsArrow ? 1 : 0)
                    );
                }
            }
            /*if (config.ImposeEdges)
            {
                this.AddArrow(Direction.Right, 1);
                this.AddArrow(Direction.Left, 10);
            }*/
        }
        /// <summary>
        /// Creates pressed piano
        /// </summary>
        /// <param name="array">Must be the size of the piano buttons</param>
        public void PianoInit(Boolean[] array)
        {
            for (int i = 0; i < btnKeys.Length; i++)
            {
                btnKeys[i].BackgroundColor = (array[i]) ? COLOR_PRESSED : COLOR_FREE;
            }
            SaveColors();
        }
        public void PianoInit(Color[] array)
        {
            for (int i = 0; i < btnKeys.Length; i++)
            {
                btnKeys[i].BackgroundColor = array[i];
            }
            SaveColors();
        }
        public void Random()
        {
            Random r = new();

            for (int i = 0; i < btnKeys.Length; i++)
            {
                btnKeys[i].BackgroundColor = (r.Next(2) == 1) ? COLOR_PRESSED : COLOR_FREE;
            }
            SaveColors();
        }

        public bool[] ToBitArray()
        {
            bool[] bitArray = new bool[btnKeys.Length];
            for (int i = 0; i < btnKeys.Length; i++)
                bitArray[i] = btnKeys[i].BackgroundColor != COLOR_FREE;
            return bitArray;
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
    }
}
