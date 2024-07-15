using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureSample.Maui.Models
{
    internal class PianoKeyboardReadOnly : MR.Gestures.Grid
    {

        
        protected readonly int NUMBER_OF_KEYS;
        protected readonly int FINGER_SEPERATOR = 5;
        protected readonly MR.Gestures.Button[] btnKeys;
        
        protected readonly Color COLOR_PRESSED = Colors.Yellow;
        protected readonly Color COLOR_FREE = Colors.White;
        public Color[] colors;
        public int  Length => btnKeys.Length;
        protected virtual int heading_height { get; } = 20;
        public PianoKeyboardReadOnly(int rows = 1, int keysInRow = 10) : base()
        {


            NUMBER_OF_KEYS = keysInRow * rows;
            this.ColumnSpacing = 5;
            this.BackgroundColor = Colors.Black;
            //this.Padding = textBoxesQuantity==0? new Thickness(0, 30, 0, 0):0;
            int handSeperator = 5; if (keysInRow > 10) handSeperator = keysInRow + 1;

            colors = new Color[NUMBER_OF_KEYS];

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
                        Text = "Button " + (i + 1 + keysInRow * r).ToString(),
                        BackgroundColor = COLOR_FREE,
                        CommandParameter = i + 1 + keysInRow * r,
                        Margin = new Thickness(0, 5, 0, 0),
                        //DownCommand = new Command<MR.Gestures.DownUpEventArgs>(OnDown), 
                        //UpCommand =  new Command<MR.Gestures.DownUpEventArgs>(OnUp), 
                    }, (i < handSeperator) ? i : i + 1,/*r+1*/ rows - r
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
            for(int i = 0;  i < btnKeys.Length;i++)
            {
                btnKeys[i].BackgroundColor = (array[i])?COLOR_PRESSED: COLOR_FREE;
            }
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
                btnKeys[i].BackgroundColor = (r.Next(2)==1) ? COLOR_PRESSED : COLOR_FREE;
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

        public int Sum {  get {
                int sum = 0;
                for (int i = 0; i < btnKeys.Length; i++)
                    sum += btnKeys[i].BackgroundColor == COLOR_PRESSED ? 1 : 0;
                return sum; } }

        protected void SaveColors()
        {

            for (int i = 0; i < NUMBER_OF_KEYS; i++) colors[i] = btnKeys[i].BackgroundColor;
        }
    }
}
