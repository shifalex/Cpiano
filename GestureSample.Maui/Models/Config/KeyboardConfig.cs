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
    //ODO: Dummies for keyboard
    //ODO: Dummies bug - History - it may show 6 six times lathough dummies have 5 checked - show 6 only 2 times( 6 &0)Half History bug, Especially with Dummies
    //ODO: Dummies bug - COLORS opacity changes backgroungcolor and so you choose pressed or not pressed by color only maybe use colors[i]
    //ODO: Dummies bug - on hand raise it still counts the dummies
    //ODO: when it is spatial recognition and when it is by hand - it should be set and not according the number of keys
    //ODO: Fix equation +X sometimes there is a disonance
    //ODO: Remove underline in android entry
    //ODO: Solve why when I wrong with keyboard, numeric keyboard comes up
    //ODO: play with possibilities of not removing the yellow, as long the sender is tapped 
    //ODO: Dates picker to choose games from
    //ODO: Seriazable op
    //ODO: Game Save
    //ODO: edit the table with question number
    //ODO: Understand Why I always lose in show data
    //ODO: Question correct from the start/from
    //ODO: nicer dates
    //ODO: Game Index
    //ODO: Game Name
    //ODO:Equations save(replace addend1 and sum)
    //ODO: decomposition game save check
    //ODO: Grid Height & Android?
    //ODO: Local Users
    //ODO: Fix keyboard disappearing bug
    //ODO:Back to main in Data
    //ODO: Menu Page that is comfortable - both for small screen and for big
    //ODO: Code Review about Users and CurrentUserId. Straight from mainPage to CreatePage(withou Splash). Change GetCurrentUser to property
    //ODO: Back button from Grid to "New Number"
    //ODO: split pianoKeyboard and number games

    //TODO: DUMP data button
    //TODO: Save on outside db and upload to local if needed
    //TODO: Show user name and icon in the up
    //TODO: add feedback and timer together
    //TODO: GMAil/device sync save
    //TODO: RECORD BY GAME DATA
    //TODO: Decide whats parralar(db, showing the message) and what serial and make it this way
    //TODO: progress bar and save button unvalaible while saving data
    //TODO: PainoKeyboard TilltheEnd Arrow
    //TODO: save piano on db
    //TODO: Save key event only if they make a difference
    //TODO: PianoKeyboardData That Works and paints well
    //TODO: Admin page to search by user to search a data
    //TODO: add all the x and y of the touches on the keyboard to different db table using e.Touches[0] - will be needed for the touching patterns. Make it a seperate event of touch the grid which doesn't interfere
    //TODO: Understand what I do with await
    //TODO: design - menu and pick avatar
    //TODO: PUBLISH!
    //TODO: Graphs page
    //TODO: ACT-R simulator
    //TODO: click point places simulator
    //TODO: create a different "thinking" sign
    //TODO: Avatars
    //TODO: make better helping text boxes through 10 design(more algined)
    //TODO: part part whole addition/subtraction mission with the piano - with async piano mode 
    //TODO: start from 50-50-100 in the big numbers??
    //TODO: numbers/keyboard menu
    //TODO: switch in mainpage when switching user
    //TODO: options - time till yellow :/ both in multilication and addition
    //TODO: feedback on the game to the AI regarding how was it to me? hard/easy?
    //TODO: Random change in up and down of the text boxes
    //TODO: User cclassroom=1, a teacher bit, creation time, local order of he user,last login time.

    //TODO: GameName length fixed
    //TODO: COLOR of grid heading as upper color
    //TODO: Allow Part-Part-Whole in show game results???
    //TODO: add levels with save
    //TODO: check Last User is really saved. Usually it is the first out of table and not the last login time. So add login time and order by login time
    //TODO: show statistics wrong/right per sum or through ten/not through ten/ or on the 10/10 table
    //TODO: Feedback on Wrong Input(Empty/NAN/Too big/small+blinking textBox), feedback on Lose(correct answer to last wrong question)
    //TODO: play with check visability??
    //TODO: consider max length for textboxes according to the max sum and min sum
    //TODO: the first Focus on a textbox doesn't work

    //TODO: Class teacher interface with links
    //TODO: Suscription Used. Available to one year... Renew somehow

    //TODO: Multilevel game. with conditions to apply to level and move to and from
    //TODO: Addition with rule on higher number
    //TODO: Multiplication With Rule
    //TODO: exception rules(both +1 or -1, 2 items of the previous triads in different order with third different item, sometimes right and sometimes false) 
    //TODO: Bring back game serial number and date with wrapper and .ToString(). Add Game's Name
    //TODO: Bring back save - this time with piano keyboard.
    /// <summary>
    /// /TODO: database save game data and show all after game is over - game over screen
    /// </summary>
    /// //TODO: Make enteries closer to each other 
    /// TODO: save game in the middle
    /// TODO: save game's name...
    /// //TODO: back to menu or a new game and no picker on aftergame showdataxaml
    // TODO: Triads repeating from 2 to 4 times randomly
    // TODO: Triads repeating interlevingly
    //TODO: simple to 5 games with images of objects and lengths
    //TODO: Proximate triads n+1/n-1/n+2/n-2 on one or n+1/n-1 both addends
    //TODO: Impose edges - 2 ordinal arrows
    //TODO: Fix ImposeEdge arrow bug
    //TODO: Rounded ordinal arrow from the center
    //TODO: Move Cardinal arrows to the edges
    //TODO: Ordinal with a loophole - show once to the left, once to the right, from and to the same places
    //TODO: Oridinal Plus starts from the 10(0)
    //TODO: add NOT to copy/quantity in the fingers game
    //TODO: Pictorial and width adaptive triads( show objects that change colors after 5  and have completion
    //TODO: add record and migration in Configs
    //TODO: splti config to different cofigs UI, GamePlay, GameParameters, Arrow

    //TODO: Pattern for numbers
    //TODO: One Button - lights several number: 1-2-4, 5-1-1-1-1
    //TODO: One button - smbolizes 1-2-4, 5-1-1-1-1, 10-10-10-10-50, 10-10-10-10-50--5-1-1-1-1, 1-2-4-8-16-32--64-128-256-512, 1-2-2-4-10-20-20-40-100-200
    //TODO: Addition and subtraction Abbacus??
    //TODO: Division from triad using the keyboard as an answer
    //TODO: Blinking keyboard for logic
    //TODO: appearing help of several steps to solve the adding through ten 
    //TODO: From Keyboard To triad - through ten in 2 different ways

    //TODO: FIX Xamarin.PreBuilt.iOS[1405:810074] Warning: observer object was not disposed manually with Dispose()
    //TODO: ARROW DRAWING - First draw buttons
    //TODO: ARROW DRAWING - solve orientation switch arrow bug
    //TODO: onlyToTen, OnlyThroughTen in bitArray
    //TODO: less time on arrow mission
    //TODO: Mediation till questions of several steps
    //TODO: Arrow till 10 back and forth
    //TODO: Make 2 lines only arrows interface - left arrow to top line, right arrow to bottom line
    //TODO: ARRow levels
    //TODO: faster time restriction on choosing the numbers!
    //TODO: MinMax= [MinAddend MaxAddend MinSum MaxSum]
    //TODO: Fill textBoxes According To Keyboard - HalfSync and Spatial
    //TODO: Make game of filling the symmetry - with the source
    //TODO: Generate a configuration page for choosing the game properties

    //TODO: generate the Pool list of available answers sometimes according to dummies, sometimes according to multiplication and not in the
    //TODO: Missing objects task - hide some finger behind a curtain an press only on the missing ones. Hide after several seconds (Maybe One Side Maybe two)

    //TODO: Only scond hand for decompositions with dummies(up to 4)
    //TODO: Object counting - sometime more smaller then less bigger. So also size games.
    //TODO: Timers for checking the exercise and timers for moving to the next

    //TODO: an abacus finger game - showing numbers, adding substracting - with mediation - pushing 1st finger right will make all left turn on
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
    //TODO: Prediction models for using the app using AI
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
    public enum ArrowType
    {
        Straight,
        Rounded
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
        public ArrowType ArrowType { get; set; } = ArrowType.Straight;
        public bool? IsArrowLengthDynamic { get; set; } = false;

        public int[] WeightsArray = null;
        public List<List<int>> DependancyArray = null;

    }
}
