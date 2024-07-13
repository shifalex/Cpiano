namespace GestureSample.Maui
{
        public enum SyncType
        {
            None = 0,
            Sync,
            HalfSync
        }
    public class KeyboardConfig
    {


        public SyncType SyncType { get; set; } = 0;
        public int TextBoxesQuantity { get; set; } = 0;
        public int Rows { get; set; } = 1;

        public int KeysInRow { get; set; } = 10;
        public bool ImposeEdges { get; set; } = false;
        public bool FromNumToNum { get; set; } = false;
        public bool WithoutZero { get; set; } = true;
        public bool AllowRemoval { get; set; } = false;

        public int AddendsNum { get; set; } = 2;
    }
}
