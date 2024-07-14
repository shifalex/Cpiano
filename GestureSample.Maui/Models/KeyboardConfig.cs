namespace GestureSample.Maui
{
    //TODO: addends of history get lost
    //TODO: number shows keyboard when shouldnt
    //TODO: keyboard questions
    //TODO: Hand questions
    //TODO: Logic questions
    //TODO: Arrows for kwyboard (First one arrow, then 2 arrows)
    //TODO: Dummies for keyboard
    //TODO: Multiplication reordering of multipliers(start with 1 being 2 and the other anyone, next some by three and etc.
    //TODO: Interleaving && Block && Block-withException practices
    //TODO: DB save
    //TODO: combination of questions on the same exercise in different modalities (first missing, second missin, third missing, addition, subtraction, addition subtraction with different addends)
    //TODO: Negative numbers interface
    //TODO: pattern recognizer - and ask for new pattern..


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
