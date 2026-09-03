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


    //TODO: Hand tutorials
    //TODO: Through ten instead text boxes
    //TODO: Prev button disable when needed
    //TODO: Audion - sound BEKA/CountOn/CountUsual
    //TODO: More user friendly beginer boolean algebra
    //TODO: BUG - equations???
    //TODO: BUG - + in some binay algebra still left
    //TODO: shift cyclical, shift only part. Equivalences
    //TODO: From Keyboard to PPW and back
    //TODO: Kyboard going up & down - create my own buttons on screen?? Ot use someone else's keyboard
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

    public enum ArrowMovementMode
    {
        Legacy = 0,
        AllTogether = 1,
        Arpeggio = 2,
        Splited = 3,
        MiddleSplited = 4,
        JumpToEnd = 5,
        OneByOne = 6,
        JumpThroughMiddle = 7
    }

    [Flags]
    public enum KeyboardFeatureFlags
    {
        None = 0,
        ShowNumbersOnKeys = 1,
        ImposeEdges = 2,
        ImposeSerialization = 4,
        WithoutZero = 8,
        AllowRemoval = 16,
        KeyboardOnlyForHelp = 32,
        KeyboardAsQuestion = 64,
        ArrowQuestion = 128,
        Multicolor = 256,
        HelpAvailable = 512,
        PermutationTraceColors = 1024,
        HideMainKeyboard = 2048
    }

    [Flags]
    public enum ArrowFeatureFlags
    {
        None = 0,
        DynamicLength = 1,
        Rounded = 2
    }

    [Flags]
    public enum ArrowMovementModeFlags
    {
        None = 0,
        AllTogether = 1,
        Arpeggio = 2,
        Splited = 4,
        MiddleSplited = 8,
        JumpToEnd = 16,
        OneByOne = 32,
        JumpThroughMiddle = 64,
        CountOn = OneByOne,
        All = AllTogether | Arpeggio | Splited | MiddleSplited | JumpToEnd | OneByOne | JumpThroughMiddle
    }

    [Flags]
    public enum KeyboardAudioFeatureFlags
    {
        None = 0,
        NumberVoice = 1,
        SingleVoice = 2,
        MultipleVoices = 4
    }

    public enum PpwKeyboardSeedMode
    {
        None = 0,
        VisiblePartPressed = 1,
        WholePressed = 2,
        VisiblePartsColored = 3
    }

    public enum KeyboardColorInteractionMode
    {
        Default = 0,
        AddSecondColor = 1,
        RemoveWithRed = 2
    }

    public enum GroupByColorLayoutMode
    {
        Free = 0,
        CommutativityEdges = 1,
        AssociativityEdges = 2
    }

    public enum ArrowLabelExerciseMode
    {
        None = 0,
        StartAndLength = 1,
        StartAndEndWithMissingLength = 2,
        EndAndLengthWithMissingStart = 3,
        OrdinalStartAndLength = 4,
        ComplexBridgeToNextTen = 5,
        ComplexLongDistance = 6,
        ComplexBridgeToAnyNextTen = 7
    }

    [Flags]
    public enum ArrowPromptKindFlags
    {
        None = 0,
        OnKeyboard = 1,
        SpecialPrompt = 2
    }

    [Flags]
    public enum ArrowRouteKindFlags
    {
        None = 0,
        Cardinal = 1,
        Ordinal = 2
    }

    public enum ArrowFeedbackMode
    {
        Icon = 0,
        CorrectResponse = 1
    }

    public enum ArrowDirectionMode
    {
        Auto = 0,
        LeftToRight = 1,
        RightToLeft = 2,
        Alternating = 3,
        Random = 4
    }

    public enum ArrowLabelRetryMode
    {
        None = 0,
        ShowKeyboardHelp = 1,
        RevealComplexThroughTen = 2
    }

    public enum KeyLabelVerticalPosition
    {
        Middle = 0,
        Top = 1
    }

    public enum PrecisionShiftAxis
    {
        Horizontal = 0,
        Vertical = 1
    }

    [Flags]
    public enum PrecisionPinchMoveOptions
    {
        None = 0,
        ShiftWhole = 1,
        MoveLower = 2,
        MoveUpper = 4,
        All = ShiftWhole | MoveLower | MoveUpper
    }

    [Flags]
    public enum TwoHandCombinationOptions
    {
        None = 0,
        Commutativity = 1 << 0,
        Associativity = 1 << 1,
        ResizeUpper = 1 << 2,
        ResizeLowerAttached = 1 << 3,
        FlipAdditionSubtraction = 1 << 4,
        Difference = 1 << 5,
        Split = 1 << 6,
        SplitJump = 1 << 7,
        NearHalf = 1 << 8,
        HalfOfHalf = 1 << 9,
        LittleSmaller = 1 << 10,
        MuchSmaller = 1 << 11,
        LittleBigger = 1 << 12,
        MuchBigger = 1 << 13,
        Half = 1 << 14,
        SubtrahendOneStepBigger = 1 << 15,
        IncreaseLowerByOne = 1 << 16,
        IncreaseUpperByOne = 1 << 17,
        DecreaseLowerByOne = 1 << 18,
        DecreaseUpperByOne = 1 << 19,
        SubtrahendOneStepSmaller = 1 << 20,
        MoreThanHalf = 1 << 21,
        LessThanHalf = 1 << 22,
        Default = Associativity | FlipAdditionSubtraction | Difference | Split | Half |
                  MoreThanHalf | LessThanHalf |
                  SubtrahendOneStepBigger | SubtrahendOneStepSmaller |
                  IncreaseLowerByOne | DecreaseLowerByOne |
                  IncreaseUpperByOne | DecreaseUpperByOne,
        All = Commutativity | Associativity | ResizeUpper | ResizeLowerAttached |
              FlipAdditionSubtraction | Difference | Split | SplitJump |
              HalfOfHalf | LittleSmaller | MuchSmaller | LittleBigger | MuchBigger | Half |
              SubtrahendOneStepBigger | SubtrahendOneStepSmaller |
              IncreaseLowerByOne | IncreaseUpperByOne | DecreaseLowerByOne | DecreaseUpperByOne |
              MoreThanHalf | LessThanHalf
    }

    public class KeyboardConfig
    {
        public void NormalizeWeightedLayout()
        {
            if (WeightsArray == null || WeightsArray.Length == 0)
                return;

            Rows = Rows <= 0 ? 1 : Rows;
            KeysInRow = KeysInRow <= 0 ? 1 : KeysInRow;

            int configuredKeyCount = Rows * KeysInRow;
            if (configuredKeyCount == WeightsArray.Length)
                return;

            if (Rows == 1 || WeightsArray.Length % Rows != 0)
            {
                Rows = 1;
                KeysInRow = WeightsArray.Length;
                return;
            }

            KeysInRow = Math.Max(1, WeightsArray.Length / Rows);
        }


        public SyncType SyncType { get; set; } = 0;
        public int TextBoxesQuantity { get; set; } = 0;
        public int Rows { get; set; } = 1;
        public KeyboardFeatureFlags KeyboardFeatures { get; set; } = KeyboardFeatureFlags.WithoutZero;
        public ArrowFeatureFlags ArrowFeatures { get; set; } = ArrowFeatureFlags.None;
        public KeyboardAudioFeatureFlags AudioFeatures { get; set; } = KeyboardAudioFeatureFlags.None;
        public bool ShowNumbersOnKeys
        {
            get => KeyboardFeatures.HasFlag(KeyboardFeatureFlags.ShowNumbersOnKeys);
            set => KeyboardFeatures = value
                ? KeyboardFeatures | KeyboardFeatureFlags.ShowNumbersOnKeys
                : KeyboardFeatures & ~KeyboardFeatureFlags.ShowNumbersOnKeys;
        }

        public int KeysInRow { get; set; } = 10;
        public bool AllowKeyWidthAdjustment { get; set; } = false;
        public bool IsPrecisionPinchExercise { get; set; } = false;
        public int PrecisionPinchMemorizeDelaySeconds { get; set; } = 0;
        public bool IsPrecisionPinchSequenceMemorize { get; set; } = false;
        public bool IsTwoHandCombinationMemorize { get; set; } = false;
        public TwoHandCombinationOptions TwoHandCombinationOptions { get; set; } = TwoHandCombinationOptions.Default;
        public bool AnimateTwoHandCombinations { get; set; } = true;
        public bool RandomizeTwoHandCombinationSizes { get; set; } = true;
        public bool AnchorTwoHandCombinationsToBottom { get; set; } = true;
        public bool ReadTwoHandCombinationInstructionAloud { get; set; } = false;
        public bool AskOnlyTwoHandCombinationTarget { get; set; } = false;
        public bool AllowImmediateCorrectPrecisionAnswer { get; set; } = false;
        public int PrecisionSequenceRecognitionWindowSeconds { get; set; } = 8;
        public int PrecisionPinchSequenceSecondMaxDistance { get; set; } = 1;
        public bool ShowPrecisionPinchGuideLine { get; set; } = true;
        public bool SeparatePrecisionPinchColumnsOnTablet { get; set; } = false;
        public double PrecisionPinchTabletColumnGap { get; set; } = 96;
        public bool IsTransformativePrecisionCopyExercise { get; set; } = false;
        public bool CopyPrecisionPinchToOtherHand { get; set; } = false;
        public bool IsVerticalPrecisionPinchExercise { get; set; } = false;
        public bool IsPrecisionShiftExercise { get; set; } = false;
        public bool IsPrecisionSignLearningExercise { get; set; } = false;
        public bool IsPrecisionSynchronousProcessExercise { get; set; } = false;
        public bool PrecisionShiftBothHands { get; set; } = false;
        public PrecisionShiftAxis PrecisionShiftAxis { get; set; } = PrecisionShiftAxis.Horizontal;
        public PrecisionPinchMoveOptions PrecisionPinchMoveOptions { get; set; } = PrecisionPinchMoveOptions.All;
        public int PrecisionMoveLowerPercent { get; set; } = -1;
        public bool PrecisionShiftSynchronizeHands { get; set; } = false;
        public bool PrecisionShiftStaggerHandsInitially { get; set; } = false;
        public bool IsPrecisionGrammarExercise { get; set; } = false;
        public int PrecisionShiftNewPinchPercent { get; set; } = 0;
        public int PrecisionShiftMinDistance { get; set; } = 1;
        public int PrecisionShiftMaxDistance { get; set; } = 1;
        public int PrecisionPinchMaxInterval { get; set; } = int.MaxValue;
        public bool IsPrecisionArrowDesignLab { get; set; } = false;
        public bool ImposeEdges
        {
            get => KeyboardFeatures.HasFlag(KeyboardFeatureFlags.ImposeEdges);
            set => KeyboardFeatures = value
                ? KeyboardFeatures | KeyboardFeatureFlags.ImposeEdges
                : KeyboardFeatures & ~KeyboardFeatureFlags.ImposeEdges;
        }
        public bool ImposeSerealization
        {
            get => KeyboardFeatures.HasFlag(KeyboardFeatureFlags.ImposeSerialization);
            set => KeyboardFeatures = value
                ? KeyboardFeatures | KeyboardFeatureFlags.ImposeSerialization
                : KeyboardFeatures & ~KeyboardFeatureFlags.ImposeSerialization;
        }
        public bool WithoutZero
        {
            get => KeyboardFeatures.HasFlag(KeyboardFeatureFlags.WithoutZero);
            set => KeyboardFeatures = value
                ? KeyboardFeatures | KeyboardFeatureFlags.WithoutZero
                : KeyboardFeatures & ~KeyboardFeatureFlags.WithoutZero;
        }
        public bool AllowRemoval
        {
            get => KeyboardFeatures.HasFlag(KeyboardFeatureFlags.AllowRemoval);
            set => KeyboardFeatures = value
                ? KeyboardFeatures | KeyboardFeatureFlags.AllowRemoval
                : KeyboardFeatures & ~KeyboardFeatureFlags.AllowRemoval;
        }

        public int AddendsNum { get; set; } = 2;

        public bool KeyboardOnlyForHelp
        {
            get => KeyboardFeatures.HasFlag(KeyboardFeatureFlags.KeyboardOnlyForHelp);
            set => KeyboardFeatures = value
                ? KeyboardFeatures | KeyboardFeatureFlags.KeyboardOnlyForHelp
                : KeyboardFeatures & ~KeyboardFeatureFlags.KeyboardOnlyForHelp;
        }
        public bool KeyboardAsAQuestion
        {
            get => KeyboardFeatures.HasFlag(KeyboardFeatureFlags.KeyboardAsQuestion);
            set => KeyboardFeatures = value
                ? KeyboardFeatures | KeyboardFeatureFlags.KeyboardAsQuestion
                : KeyboardFeatures & ~KeyboardFeatureFlags.KeyboardAsQuestion;
        }
        public bool HideMainKeyboard
        {
            get => KeyboardFeatures.HasFlag(KeyboardFeatureFlags.HideMainKeyboard);
            set => KeyboardFeatures = value
                ? KeyboardFeatures | KeyboardFeatureFlags.HideMainKeyboard
                : KeyboardFeatures & ~KeyboardFeatureFlags.HideMainKeyboard;
        }

        public int SecondsPressingToAnswer { get; set; } = 2;

        public int[] DummiesArray = null;
        public int LeftAddendIndex { get; set; } = 0;

        public bool IsArrow
        {
            get => KeyboardFeatures.HasFlag(KeyboardFeatureFlags.ArrowQuestion);
            set => KeyboardFeatures = value
                ? KeyboardFeatures | KeyboardFeatureFlags.ArrowQuestion
                : KeyboardFeatures & ~KeyboardFeatureFlags.ArrowQuestion;
        }

        public bool IsMulticolor
        {
            get => KeyboardFeatures.HasFlag(KeyboardFeatureFlags.Multicolor);
            set => KeyboardFeatures = value
                ? KeyboardFeatures | KeyboardFeatureFlags.Multicolor
                : KeyboardFeatures & ~KeyboardFeatureFlags.Multicolor;
        }
        public ArrowType ArrowType
        {
            get => ArrowFeatures.HasFlag(ArrowFeatureFlags.Rounded) ? ArrowType.Rounded : ArrowType.Straight;
            set => ArrowFeatures = value == ArrowType.Rounded
                ? ArrowFeatures | ArrowFeatureFlags.Rounded
                : ArrowFeatures & ~ArrowFeatureFlags.Rounded;
        }
        public bool? IsArrowLengthDynamic
        {
            get => ArrowFeatures.HasFlag(ArrowFeatureFlags.DynamicLength);
            set => ArrowFeatures = value == true
                ? ArrowFeatures | ArrowFeatureFlags.DynamicLength
                : ArrowFeatures & ~ArrowFeatureFlags.DynamicLength;
        }

        public int[] WeightsArray = null;
        public List<List<int>> DependancyArray = null;
        public bool UseDynamicMultiplicationWeights { get; set; } = false;
        public bool UseWeightedCustomStageTargets { get; set; } = false;
        public bool AllowImpossibleWeightedAnswer { get; set; } = false;
        public int MaskThirdArrowAfterCycleCount { get; set; } = 0;
        public bool UseFullHandTutorial { get; set; } = false;
        public bool AllowAnswerTimePanelToggleFromKeyboardHeader { get; set; } = true;
        public bool AllowSumHeaderVisibilityToggle { get; set; } = false;
        public GroupByColorLayoutMode GroupByColorLayoutMode { get; set; } = GroupByColorLayoutMode.Free;
        public ArrowLabelExerciseMode ArrowLabelExerciseMode { get; set; } = ArrowLabelExerciseMode.None;
        public int MaxArrowLabelDistance { get; set; } = 0;
        public bool EnableArrowLabelRetry { get; set; } = false;
        public ArrowLabelRetryMode ArrowLabelRetryMode { get; set; } = ArrowLabelRetryMode.None;
        public bool UseKeyboardQuestionAfterArrowLabelHelp { get; set; } = false;
        public ArrowLabelExerciseMode ArrowLabelRetryAlternateMode { get; set; } = ArrowLabelExerciseMode.None;
        public MissingValueTargetFlags SpecialArrowRetryAlternateTargets { get; set; } = MissingValueTargetFlags.None;
        public bool UseFixedComplexMiddle { get; set; } = false;
        public bool AllowRtlComplexPrompts { get; set; } = false;
        public bool AllowLearnerChosenComplexMiddle { get; set; } = false;
        public bool StartArrowLabelRetryWithEquation { get; set; } = false;
        public ArrowPromptKindFlags AllowedArrowPromptKinds { get; set; } = ArrowPromptKindFlags.None;
        public ArrowRouteKindFlags AllowedArrowRouteKinds { get; set; } = ArrowRouteKindFlags.None;
        public MissingValueTargetFlags SpecialArrowMissingTargets { get; set; } = MissingValueTargetFlags.None;
        public ArrowFeedbackMode ArrowFeedbackMode { get; set; } = ArrowFeedbackMode.Icon;
        public ArrowDirectionMode ArrowDirectionMode { get; set; } = ArrowDirectionMode.Auto;
        public ArrowMovementMode ArrowMovementMode { get; set; } = ArrowMovementMode.Legacy;
        public ArrowMovementModeFlags AllowedArrowMovementModes { get; set; } = ArrowMovementModeFlags.None;
        public bool EnableSecondArrowLeftTrace { get; set; } = false;
        public KeyLabelVerticalPosition KeyLabelVerticalPosition { get; set; } = KeyLabelVerticalPosition.Middle;
        public int GroupByColorColorCount { get; set; } = 2;
        public int[]? GroupByColorCounts { get; set; }
        public bool GroupByColorAllowSameSideTargets { get; set; } = false;
        public bool GroupByColorKeepOuterColorsOnSides { get; set; } = false;
        public bool GroupByColorKeepBlueInMiddle { get; set; } = false;
        public PpwKeyboardSeedMode PpwKeyboardSeedMode { get; set; } = PpwKeyboardSeedMode.None;
        public KeyboardColorInteractionMode ColorInteractionMode { get; set; } = KeyboardColorInteractionMode.Default;
        public bool EnableColorDrag { get; set; } = false;

        public bool IsNumberVoice
        {
            get => AudioFeatures.HasFlag(KeyboardAudioFeatureFlags.NumberVoice);
            set => AudioFeatures = value
                ? AudioFeatures | KeyboardAudioFeatureFlags.NumberVoice
                : AudioFeatures & ~KeyboardAudioFeatureFlags.NumberVoice;
        }
        public bool IsVoice
        {
            get => AudioFeatures.HasFlag(KeyboardAudioFeatureFlags.SingleVoice);
            set => AudioFeatures = value
                ? AudioFeatures | KeyboardAudioFeatureFlags.SingleVoice
                : AudioFeatures & ~KeyboardAudioFeatureFlags.SingleVoice;
        }
        public bool IsVoices
        {
            get => AudioFeatures.HasFlag(KeyboardAudioFeatureFlags.MultipleVoices);
            set => AudioFeatures = value
                ? AudioFeatures | KeyboardAudioFeatureFlags.MultipleVoices
                : AudioFeatures & ~KeyboardAudioFeatureFlags.MultipleVoices;
        }

        public bool IsHelpNeeded
        {
            get => KeyboardFeatures.HasFlag(KeyboardFeatureFlags.HelpAvailable);
            set => KeyboardFeatures = value
                ? KeyboardFeatures | KeyboardFeatureFlags.HelpAvailable
                : KeyboardFeatures & ~KeyboardFeatureFlags.HelpAvailable;
        }

        public bool UsePermutationTraceColors
        {
            get => KeyboardFeatures.HasFlag(KeyboardFeatureFlags.PermutationTraceColors);
            set => KeyboardFeatures = value
                ? KeyboardFeatures | KeyboardFeatureFlags.PermutationTraceColors
                : KeyboardFeatures & ~KeyboardFeatureFlags.PermutationTraceColors;
        }

    }
}
