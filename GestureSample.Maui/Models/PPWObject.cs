using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureSample.Maui.Models
{
    internal class PPWObject
    {
        public PPWObject(int addend1, int addend2, int sum)
        {
            addend1 = addend1;
            addend2 = addend2;
            Sum = sum;
        }

        public int addend1 { get; set; }
        public int addend2 { get; set; }
        public int Sum { get; set; }

    }
}
