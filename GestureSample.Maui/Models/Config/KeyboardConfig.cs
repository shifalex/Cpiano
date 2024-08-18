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
    //ODO: why when 11 its by pattern and when not its by hand
    //ODO: Only one hand for decompositions (up to 4)
    //ODO: Make arrow a BitArray Gameplay instead of usual!!
    //ODO: Make arrow numbers look nice
    //ODO: Move Arrow logic to GamePlay
    //ODO: Arrow Go Back To left
    //ODO: Arrow from left without minuses - not allow bigger left the prev right
    //ODO: splitter for bigger number decomposition


    //TODO: FIX Xamarin.PreBuilt.iOS[1405:810074] Warning: observer object was not disposed manually with Dispose()
    //TODO: onlyToTen, OnlyThroughTen in bitArray
    //TODO: less time on arrow mission
    //TODO: Mediation till questions of several steps
    //TODO: Arrow till 10 back and forth
    //TODO: Make 2 lines only arrows interface - left arrow to top line, right arrow to bottom line
    //TODO: ARRow levels
    //TODO: faster time restriction on choosing the numbers!
    //TODO: MinMax= [MinAddend MaxAddend MinSum MaxSum]
    //TODO: Dummies for keyboard
    //TODO: Dummies bug - History - it may show 6 six times lathough dummies have 5 checked - show 6 only 2 times( 6 &0)Half History bug, Especially with Dummies
    //TODO: Dummies bug - COLORS opacity changes backgroungcolor and so you choose pressed or not pressed by color only maybe use colors[i]
    //TODO: Dummies bug - on hand raise it still counts the dummies

    //TODO: when it is spatial recognition and when it is by hand - it should be set and not according the number of keys
    //TODO: generate the Pool list of available answers sometimes according to dummies, sometimes according to multiplication and not in the
    //TODO: Missing objects task - hide some finger behind a curtain an press only on the missing ones. Hide after several seconds (Maybe One Side Maybe two)

    //TODO: Only scond hand for decompositions with dummies(up to 4)
    //TODO: Object counting - sometime more smaller then less bigger. So also size games.
    //TODO: Arrows for keyboard (First one arrow, then 2 arrows)
    //TODO: Timers for checking the exercise and timers for moving to the next

    //TODO: an abacus finger game - showing numbers, adding substracting - with mediation - pushing 1st finger right will make all left turn on
    //TODO: Fill textBoxes According To Keyboard
    //TODO: decide on button text on the keys...
    //TODO: Learn manage TODOS
    //TODO: A Button that animates equation into PPW scheme for help
    //TODO: equation with one more entry

    //TODO: add recognition of pattern as Copy of the question as opposed to reorganization of the question
    //TODO: generate a configuration page using chatGPT and the config classes. By categories

    //TODO: make piano keyboard as independent as possible by using GamePlay.Check(this) and moving all the functions of pattern analysis to the GamePlay classes.
    //Make it impose patterns on press by using the pattern list of patterns: RHfirst, LHfirst, RTL, LTR, EDGES, SEQUENCE, DIVIDEdSequenceBy1, DividedSequenceOnce,PseudoSymetrical, HSSequence, Other

    //TODO: Make only HalfSync and small pictures on small screens
    //TODO: Multiplication HistoryGame

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
    //TODO: Timers level and game
    //TODO:Equation levels with goind down and up. Only Sum in +- will mean the =, only +X, only positive numbers etc.
    //TODO: heptic feedback
    //TODO: Timers for next after correct answer instead of next.imer for exercise, timer for total
    //TODO: Win screen with stats.
    //TODO: solveTillCorrect?
    //TODO: 9,5,2 multiplication helper. 7, 6 jumps multiplication helper. Make helper view functions
    //TODO: MOve automatically from one win to another
    //TODO: Levels configurations with HATNAYOT
    //TODO:Change how we understand recursion. When we remove one or when we add one.
    //TODO: small classroom model (6 children - teacher can see how everyone of them works and progress of everyone)
    //TODO: Another app for Trinom, golden equations etc.
    //TODO: Bar Models game- grid with and bars you put over there
    //TODO: another app for 3D puzzels moving it with hands
    //TODO: rhythmic and "melody" patterns
    //TODO: Cheng's game of number decomposition with multitouch
    //TODO: Arrow and +- direction or length change on what is shown in the equation (a melody of arrow change)
    //TODO: +- app of adding and removing pluses and minuses and changing their symbols
    //TODO: Voice Qestion and answer interface in equations
    //TODO: finger camera answer interface
    //TODO:Graph Equation wirting and checking interface. g(x)=-l*|f(-k(x+m))|+n
    //QUESTION: Why Ido is uncapble to get into the new epestemic form by himself? I need to try and make himm discover and make the discovery simple enough

    //IDO got it with not pressed dummies - first with both hands. Then I told him play with one hand. He got it with all pressed dummies for 7=5+2, 7=5+0. But he couldn't make 7=6+1.
    //Next time, maybe I should use simgle key dummies instead. It is a weird problem of artifact-finger connection that may infor us on the problems for genralization he has.
    public enum SyncType
    {
        None = 0,
        Sync = 1,
        Spatial = 2,
        HalfSync = 3
    }
    public class KeyboardConfig
    {


        public SyncType SyncType { get; set; } = 0;
        public int TextBoxesQuantity { get; set; } = 0;
        public int Rows { get; set; } = 1;
        public bool ShowNumbersOnKeys { get; set; } = false;

        public int KeysInRow { get; set; } = 10;
        public bool ImposeEdges { get; set; } = false;
        public bool ImposeSerealization { get; set; } = false;
        public bool WithoutZero { get; set; } = true;
        public bool AllowRemoval { get; set; } = false;

        public int AddendsNum { get; set; } = 2;

        public bool KeyboardOnlyForHelp { get; set; } = false;
        public bool KeyboardAsAQuestion { get; set; } = false;

        public int SecondsPressingToAnswer { get; set; } = 2;

        public int[] DummiesArray = null;
        public int LeftAddendIndex { get; set; } = 0;

        public bool IsArrow { get; set; } = false;


    }
}
