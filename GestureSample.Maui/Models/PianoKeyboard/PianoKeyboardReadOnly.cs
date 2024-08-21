using Microsoft.Maui.Controls.Shapes;

namespace GestureSample.Maui.Models
{


    internal class PianoKeyboardReadOnly : MR.Gestures.Grid
    {


        public Grid Arrow1; // The combined object containing the number and the arrow
        public Grid Arrow2;


        protected readonly int NUMBER_OF_KEYS;
        protected readonly int FINGER_SEPERATOR = 5;
        protected readonly MR.Gestures.Button[] btnKeys;


        protected readonly Color COLOR_PRESSED = Colors.Yellow;
        protected readonly Color COLOR_FREE = Colors.White;
        public Color[] colors;
        public int Length => btnKeys.Length;
        protected virtual int heading_height { get; } = 5;

        public void AddArrow(Direction direction, int aboveKeyNumber, int numberAbove = -1, int row = 1)
        {
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

            double columnWidth = btnKeys[0].Width;
            if (columnWidth == -1) columnWidth = 100;

            // Create the number label
            Label numberLabel = new()
            {
                Text = numberAbove.ToString(),
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                IsVisible = numberAbove > -1

            };

            int colSpan;
            double arrowStart, arrowEnd, arrowEdgeX;
            if (direction == Direction.Right)
            {
                arrowStart = columnWidth / 2;
                arrowEnd = columnWidth / 2 + columnWidth - (aboveKeyNumber == 10 ? columnWidth / 2 + 10 : 0) + 10;
                arrowEdgeX = arrowEnd - 10;
                colSpan = aboveKeyNumber switch
                {
                    10 => 1,
                    5 => 3,
                    _ => 2
                };
            }
            else
            {
                arrowStart = aboveKeyNumber == 1 ? columnWidth / 2 : columnWidth / 2 + columnWidth;
                arrowEnd = aboveKeyNumber == 1 ? 0 : columnWidth / 2 + 10;
                arrowEdgeX = arrowEnd + 10;
                colSpan = aboveKeyNumber switch
                {
                    1 => 1,
                    6 => 3,
                    _ => 2
                };
            }
            // Create the arrow
            Microsoft.Maui.Controls.Shapes.Path arrow = CreateArrowPath(String.Format("M {0},30 L {0},10 L {1},10 L {2},0 M {1},10 L {2},20", arrowStart, arrowEnd, arrowEdgeX), Colors.White);



            //if(aboveKeyNumber==0)aboveKeyNumber = 1;
            // Add the number and the arrow to the combined object grid
            Arrow.Add(numberLabel, 0, 0);
            Arrow.Add(arrow, 0, 1);

            // Add the combined object to the main grid in the correct column
            int column = aboveKeyNumber - 1;
            if (direction == Direction.Left) column--;
            if (FINGER_SEPERATOR >= 0 && aboveKeyNumber > FINGER_SEPERATOR) column++;
            if (direction == Direction.Left && aboveKeyNumber==6) column--;//Because of ColSpan 3
            if (column == -1) column = 0;
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
                StrokeThickness = 2,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center
            };
        }
        public PianoKeyboardReadOnly(KeyboardConfig _config) : base()
        {

            int keysInRow = _config.KeysInRow;
            int rows = _config.Rows;
            NUMBER_OF_KEYS = keysInRow * rows;
            this.ColumnSpacing = 5;
            this.BackgroundColor = Colors.Black;
            //this.Padding = textBoxesQuantity==0? new Thickness(0, 30, 0, 0):0;
            int handSeperator = 5; if (keysInRow > 10) handSeperator = keysInRow + 1;

            colors = new Color[NUMBER_OF_KEYS];

            if (_config.IsArrow)
            {
                this.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });


            }
            this.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(heading_height) });
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
                        Text = (_config.ShowNumbersOnKeys) ? (i + 1 + keysInRow * r).ToString() : "",
                        TextColor = Colors.Black,
                        BackgroundColor = COLOR_FREE,
                        CommandParameter = i + 1 + keysInRow * r,
                        Margin = new Thickness(0, 5, 0, 0),
                        //DownCommand = new Command<MR.Gestures.DownUpEventArgs>(OnDown), 
                        //UpCommand =  new Command<MR.Gestures.DownUpEventArgs>(OnUp), 
                    }, (i < handSeperator) ? i : i + 1,
                        //r+1 
                        rows - r + (_config.IsArrow ? 1 : 0)
                    );
                }
            }
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
