namespace GestureSample.Maui.Models
{
    internal class PPWObject
    {
        public PPWObject(int addend1, int addend2, int sum)
        {
            Addend1 = addend1;
            Addend2 = addend2;
            Sum = sum;
        }

        public int Addend1 { get; set; }
        public int Addend2 { get; set; }
        public int Sum { get; set; }

    }
}
