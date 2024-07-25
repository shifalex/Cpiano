namespace GestureSample.Maui
{
    //ODO: addends of history get lost
    //ODO: number shows keyboard when shouldnt
    //ODO: keyboard questions
    //ODO: Hand questions
    //ODO: check the hand image and check if it is dosen't flip the bits of left hand
    //ODO: Design KeyboardReadOnly
    //ODO: HS second number bug fix - Couldn't find any bug
    //ODO: Combine the Config of GamePlay to two: GameUI and Logic
    //ODO: Show Objects in different sizes on each canvas
    //ODO: Make all GamePlay parameters using Config
    //ODO: Logic questions
    //ODO: Mixed addition and multiplication with negatives addends to 10
    //ODO: Make Code more consistent in SimpleViewCellsPage and the PPWObjects
    //ODO: Solve Errors
    //ODO: Neuralize fix and width 100% for keyboards
    //ODO: Check that in decomposition game the level is changing
    //ODO: Equation questions
    //ODO: GUI equation and decomposition game(= Sum width)
    //ODO:InitializeLogicalKeyboardsUI Obselete
    //ODO: for ios: entry only numerical? keyboard down on button press?
    //ODO: Allow minus and divide in equation
    
    //TODO: Missing objects task - hide some finger behind a curtain an press only on the missing ones. Hide after several seconds (Maybe One Side Maybe two)
    //TODO: Dummies for keyboard
    //TODO: Only one hand for decompositions (up to 4)
    //TODO: Only scond hand for decompositions with dummies(up to 4)
    //TODO: Object counting - sometime more smaller then less bigger. So also size games.
    //TODO: Arrows for keyboard (First one arrow, then 2 arrows)
    
    //TODO: Fill textBoxes According To Keyboard
    //TODO: decide on button text on the keys...
    //TODO: Learn manage TODOS
    //TODO: Timers for checking the exercise and timers for moving to the next
    //TODO: A Button that animates equation into PPW scheme for help
    //TODO: equation with one more entry

    //TODO: add recognition of pattern as Copy of the question as opposed to reorganization of the question
    //TODO: generate a configuration page using chatGPT and the config classes. By categories

    //TODO: make piano keyboard as independent as possible by using GamePlay.Check(this) and moving all the functions of pattern analysis to the GamePlay classes.
    //Make it impose patterns on press by using the pattern list of patterns: RHfirst, LHfirst, RTL, LTR, EDGES, SEQUENCE, DIVIDEdSequenceBy1, DividedSequenceOnce,PseudoSymetrical, HSSequence, Other

    //TODO: Make only HalfSync and small pictures on small screens


    //TODO: Multiplication reordering of multipliers(start with 1 being 2 and the other anyone, next some by three and etc.
    //TODO: Interleaving && Block && Block-withException practices
    //TODO: TowerText and TowerKeyboard Interfaces
    //TODO: DB save
    //TODO: combination of questions on the same exercise in different modalities (first missing, second missin, third missing, addition, subtraction, addition subtraction with different addends)
    //TODO: Negative numbers interface(for Keyboard)
    //TODO: pattern recognizer - and ask for new pattern..
    //TODO:
    //TODO: a list of configuration you can go from one screen to another. These configurations can change dynamiclly.
    //TODO: chain questions with more than 2 addends
    //TODO: Save things on screen, not one-by-one but one after another


    //TODO: small classroom model (6 children - teacher can see how everyone of them works and progress of everyone)
    //TODO: Another app for Trinom, golden equations etc.
    //TODO: another app for 3D puzzels moving it with hands
    //TODO: rhythmic and "melody" patterns

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
