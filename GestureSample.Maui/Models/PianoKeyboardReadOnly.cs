using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureSample.Maui.Models
{
    internal class PianoKeyboardReadOnly : PianoKeyboard
    {

        protected new readonly int heading_height = 20;
        public PianoKeyboardReadOnly(PPWGamePlay gamePlay, Label lblTimer, int textBoxesQuantity = 0, int rows = 1, int keysInRow = 10, bool imposeEdges = false, bool fromNumToNum = false) : base(gamePlay, lblTimer, textBoxesQuantity, rows, keysInRow, imposeEdges, fromNumToNum)
        {
        }
    }
}
