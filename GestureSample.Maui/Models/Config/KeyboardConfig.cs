namespace GestureSample.Maui
{
    //ODO: addends of history get lost
    //ODO: number shows keyboard when shouldnt
    //TODO: HS second number bug fix
    //TODO: Design KeyboardReadOnly
    //TODO: Combine the Config of GamePlay to two: GameUI and Logic
    //TODO: make piano keyboard as independent as possible by using GamePlay.Check(this) and moving all the functions of pattern analysis to the GamePlay classes.
                //Make it impose patterns on press by using the pattern list of patterns: RHfirst, LHfirst, RTL, LTR, EDGES, SEQUENCE, DIVIDEdSequenceBy1, DividedSequenceOnce,PseudoSymetrical, HSSequence, Other
    //TODO: keyboard questions
    //TODO: Check decompositiongame
    //TODO: Hand questions
    //TODO: Logic questions
    //TODO: Arrows for kwyboard (First one arrow, then 2 arrows)
    //TODO: Dummies for keyboard
    //TODO: Show Objects in different sizes
    //TODO: add recognition of pattern as Copy of the question as opposed to reorganization of the question
    //TODO: generate a configuration page using chatGPT and the config classes. By categories
    //TODO: Multiplication reordering of multipliers(start with 1 being 2 and the other anyone, next some by three and etc.
    //TODO: Interleaving && Block && Block-withException practices
    //TODO: DB save
    //TODO: combination of questions on the same exercise in different modalities (first missing, second missin, third missing, addition, subtraction, addition subtraction with different addends)
    //TODO: Negative numbers interface
    //TODO: pattern recognizer - and ask for new pattern..
    //TODO: 
    //TODO: a list of configuration you can go from one screen to another. These configurations can change dynamiclly.
    // /TODO: chain questions with more than 2 addends


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
        public bool ImposeSerealization { get; set; } = false;
        public bool WithoutZero { get; set; } = true;
        public bool AllowRemoval { get; set; } = false;

        public int AddendsNum { get; set; } = 2;

        public bool KeyboardOnlyForHelp = false;
        public bool KeyboardAsAQuestion = false;
    }
}
