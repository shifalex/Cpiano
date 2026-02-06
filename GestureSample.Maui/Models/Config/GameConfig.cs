using GestureSample.Maui.Models;
using System.ComponentModel;
using System.Reflection;

//https://appstoreconnect.apple.com/access/integrations/api

//=======ADDING NEW CONFIGURATION:
//Start with new configuration here - enum if needed
//PPWGamePlay: change the correct GamePlay - both the constructor, check and generate exercise - add function if needed or gameplay if needed
//Add to SimpleViewCellsPage InitializeUI the UI that goes with the constructor
//SimpleViewCellsPage: change UpdateView with an "ïf" accordingly
//SimpleViewCellsPage: Change buttonNext and Generate exercise event handling if needed
//Add the new configuration to MainPage.Xaml.cs

// ✅ GameConfig is a record (immutable, value-based equality)
// 🧠 When adding a new property:
//   - Make it `init;` or readonly
//   - Use default (`= ...`) or nullable (e.g., int?)
//   - Avoid breaking old JSON: missing fields are OK
//   - Use `with { ... }` to clone+modify
//   - If needed, bump `Version` and migrate old configs
/* FOR CHATGPT:
 I’m about to evolve a config record in C#.

Please remind me:
- how to add a field safely without breaking old JSON
- when to use init;
- when to migrate versions
- how to test this safely

(And whether record or class is better.)*/

namespace GestureSample.Maui
{
    #region enums

    public static class EnumExtensions
    {
        /*public static GameConfig MigrateFromV1(GameConfig old)
        {
            return old with
            {
                MaxSecondsPerQuestion = old.MaxSecondsPerQuestion ?? 999
            };
        }*/
public static string ToDString<TEnum>(this TEnum enumValue) where TEnum : struct, IConvertible
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException("TEnum must be an enumerated type");

            FieldInfo fi = enumValue.GetType().GetField(enumValue.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes.Length > 0 ? attributes[0].Description : enumValue.ToString();
        }
    }


    public enum UIQuestionType
    {
        OnlyKeyboard,
        OneText,//Can be also objects.. always exists
        ThreeTexts, //Can be with following options: history, levelPicker, DirectionsText, Guess one
        SimpleEquation,//Can have +- or /* and sumonly or onemissing variable type, should have options,
        CanvasesHands,
        CanvasesObjects,
        DecompositionGame,
        //BitArrayQuestion, //Can be Hand or Keyboard
        Tower, //Can be text or keys tower
        ArrowOnKeyboard,//May also have twoArrows. Can have or not a number on it On every exercise it is like new., Without text or with for pattern recognition
        LogicalKeyboards //2kyboards, second optional, 1 operand
    }


    public enum Invisability
    {
        None,
        All,
        OneSideOnly,
        RightSideOnly,
        LeftSideOnly,
        PartialScreenCovering
    }

    public enum PlanStepKind
    {
        NewQuestion,     // generate a fresh question
        RepeatQuestion,  // reuse previous question as-is
        UsePrevAnswer    // take previous answer and make it the new question (BitArray Not-Not)
    }

    public enum PlanOpMode
    {
        Keep,                 // don't change CurrentOperation
        Fixed,                // set to Step.Operation
        RandomFromConfigList  // random from Config.OperationList
    }

    public enum PermutationPolicy
    {
        None,
        ConstantForChain,
        RandomEachStep,
        RandomAfterFirst
    }

    public sealed class ExercisePlanStep
    {
        public PlanStepKind Kind { get; init; } = PlanStepKind.NewQuestion;

        // how many times to do this step before moving on
        public int Repeat { get; init; } = 1;

        // operation selection for this step
        public PlanOpMode OpMode { get; init; } = PlanOpMode.RandomFromConfigList;
        public Operation Operation { get; init; } = Operation.Sum;

        // BitArray-specific extras (safe to keep for PPW too)
        public PermutationPolicy PermutationPolicy { get; init; } = PermutationPolicy.None;

        // if true, BitArray builds Question2 as a permuted version of Question1 (Or/And/Xor style)
        public bool UseSecondOperandFromPermutation { get; init; } = false;
    }

    public sealed class ExercisePlan
    {
        // if null -> old behavior (no plan)
        public List<ExercisePlanStep> Steps { get; init; } = new();

        // if true, after plan ends: loop back to beginning
        public bool Loop { get; init; } = true;

        // deterministic chains (optional)
        public int? Seed { get; init; } = null;
    }

    public enum Operation
    {
        [Description("+")]
        Sum,
        [Description("X")]
        Multiplication,
        [Description(":")]
        Divide,
        [Description("-")]
        Minus,
        [Description("COPY")]
        Copy,
       [Description("EQUAL")]
        Quantity,
        [Description("GROUP to the RIGHT ->")]
        SequenceLTR,
        [Description("<- GROUP to the LEFT")]
        SequenceRTL,
        [Description("<- SPLIT ->")]
        Split,
        [Description("SHIFT")]
        MoveBy,
        [Description("MIRROR")]
        Mirror,
        //Serialize, //TODO: Try to solve the conflict that they can be both together and separate entities
        //Reorder,
        [Description("NOT(!)")]
        Not,
        [Description("AND(&&)")]
        And,
        [Description("OR(||)")]
        Or,
        [Description("Neutralise")]
        Neutralise,
        [Description("SUM(+)")]
        SUMM

    }

    public enum VariableTypes
    {
        OneCanBeSum,
        OneNoSum,
        SumOnly,
        //TwoAny,
        TwoNoSum,
        Three
    }

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public enum QuestionOrder
    {
        Random,
        CyclicalLeft,
        CyclicalRight,
        CyclicalMixed,
        FromLeft,
        BackAndForth,
        ToLeft
    }

    #endregion

    public class GameConfig
    {
        public class Operations
        {
            public static List<Operation> Logical = new() { Operation.Or, Operation.And, Operation.Neutralise, Operation.Not };
            public static List<Operation> Arithmetic = new() { Operation.Sum, Operation.Multiplication, Operation.Divide, Operation.Minus };
            public static List<Operation> BitArray = new() { Operation.Copy, Operation.Quantity, Operation.SequenceRTL, Operation.SequenceLTR, Operation.Split, Operation.MoveBy, Operation.Mirror, Operation.Not };
            public static List<Operation> LogicalDual = new() { Operation.Or, Operation.And, Operation.Neutralise, Operation.SUMM };
        }

        public ExercisePlan? Plan { get; set; } = null;


        public string GameName = "";
        // Properties with default values
        public bool IsHistory { get; set; } = false;
        public bool IsHistorySymetrical { get; set; } = false;
        private int minAddend = 0, maxAddend = 5, minAddend2 = 0, maxAddend2 = 5;
        public int MinAddend { get { return minAddend; } set { minAddend = value; minAddend2 = value; } }
        public int MaxAddend { get { return maxAddend; } set { maxAddend = value; maxAddend2 = value; } }
        public int MinAddend2 { get { return minAddend2; } set { minAddend2 = value; } }
        public int MaxAddend2 { get { return maxAddend2; } set { maxAddend2 = value; } }
        public int MinSum { get; set; } = 1;
        public int MaxSum { get; set; } = 10;
        public bool OnlyThrougTen = false;
        public bool OnlyToTen = false;
        public bool isHelpEntries = false;
        public bool isHelpThroughTen = false;
        public bool isOnlySequence = true;
        public bool isOnlyKeyboard = false;
        public bool ShowPrev = false;

        public List<int> addendsList = new();
        public List<int> addendsListSecond = null;

        public bool EnforceOperationLabel { get; set; } = false;
        public bool FromNumToNum { get; set; } = false;

        public int SecondsTillHideExercise { get; set; } = -1;
        public int SecondsTillAllowInput { get; set; } = -1;
        public int SecondsTillNextExercise { get; set; } = 2;
        public int RepeatingTimesOfTriad { get; set; } = 1;
        public bool OnlyCloseTriad { get; set; } = false;

        public bool IncludeTutorials { get; set; } = false;

        public bool DenyStrangeOrSameGroups { get; set; } = false;

        public bool TwoKeybordsOnOne { get; set; } = false;

        public Direction? WhichHand { get; set; } = null;
        public bool IsOnlyOneHand { get; set; } = false;

        // DefaultTriad is computed lazily from the current configuration when first requested.
        private PPWObject? _defaultTriad;
        public PPWObject DefaultTriad
        {
            get
            {
                if (_defaultTriad is null)
                {
                    _defaultTriad = ComputeDefaultTriad();
                }
                return _defaultTriad;
            }
            set => _defaultTriad = value;
        }

        private PPWObject ComputeDefaultTriad()
        {
            // compute based on the (already-initialized) properties
            // adjust conditions to match desired logic
            if (OnlyThrougTen && MaxSum > 10)
                return new PPWObject(8, 7, 15);
            return new PPWObject(3, 2, 5);
        }

        public int NumberOfTasksToWin { get; set; } = -1;
        public int NumberOfMistakesToLose { get; set; } = -1;

        public UIQuestionType UIQuestionType = UIQuestionType.ThreeTexts;
        
        public QuestionOrder QuestionOrder { get; set; } = QuestionOrder.Random;
        public List<Operation> OperationList = new() { Operation.Sum };
        public VariableTypes VariableTypes { get; set; } = VariableTypes.TwoNoSum;
        // Nested configuration with defaults
        public KeyboardConfig KeyboardConfig { get; set; } = null;
        public GameConfig()
        {
            // No eager DefaultTriad initialization here; DefaultTriad is computed lazily when first used.
        }


    }


}