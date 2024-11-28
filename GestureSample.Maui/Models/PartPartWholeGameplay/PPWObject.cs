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

        public override bool Equals(object obj)
        {
            if (obj is PPWObject other)
            {
                return Addend1==other.Addend1 && Addend2==other.Addend2 && Sum ==other.Sum;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Addend1, Addend2);
        }
    }
}
