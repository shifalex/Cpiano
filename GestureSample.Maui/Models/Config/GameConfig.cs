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
        ThreeAddends,
        TwoLinesTwoAddends, 
        Tower, //Can be text or keys tower
        SimpleEquation,//Can have +- or /* and sumonly or onemissing variable type, should have options,
        CanvasesHands,
        CanvasesObjects,
        DecompositionGame,
        //BitArrayQuestion, //Can be Hand or Keyboard
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
        [Description("GROUP to the RIGHT -->")]
        SequenceLTR,
        [Description("<-- GROUP to the LEFT")]
        SequenceRTL,
        [Description("<- SPLIT ->")]
        Split,
        [Description("GROUP -><-to CENTER")]
        Centrelize,
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
        [Description("XOR")]
        ExclusiveOr,
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

    public enum PlayUiState
    {
        Question,
        ReadyForInput,
        Tutorial,
        FeedbackCorrect,
        FeedbackWrong,
        Disabled
    }

    public enum NumericInputMode
    {
        Auto,
        AppKeypad,
        SystemKeyboard
    }
    #endregion

    public class GameConfig
    {
        public static class Operations
        {
            public static List<Operation> Logical { get; } = new() { Operation.Or, Operation.And, Operation.ExclusiveOr, Operation.Not };
            public static List<Operation> Arithmetic { get; } = new() { Operation.Sum, Operation.Multiplication, Operation.Divide, Operation.Minus };
            public static List<Operation> BitArray { get; } = new() { Operation.Copy, Operation.Quantity, Operation.SequenceRTL, Operation.SequenceLTR, Operation.Split, Operation.MoveBy, Operation.Mirror, Operation.Not };
            public static List<Operation> LogicalDual { get; } = new() { Operation.Or, Operation.And, Operation.ExclusiveOr, Operation.SUMM };
        }

        private int minAddend = 0;
        private int maxAddend = 5;
        private int minAddend2 = 0;
        private int maxAddend2 = 5;
        private PPWObject? _defaultTriad;

        public GameConfig()
        {
            // DefaultTriad is computed lazily when first used.
        }

        // Plan / identity
        public ExercisePlan? Plan { get; set; } = null;
        public string GameName { get; set; } = "";

        // Core rules
        public bool IsHistory { get; set; } = false;
        public bool IsHistorySymetrical { get; set; } = false;
        public int MinAddend { get => minAddend; set { minAddend = value; minAddend2 = value; } }
        public int MaxAddend { get => maxAddend; set { maxAddend = value; maxAddend2 = value; } }
        public int MinAddend2 { get => minAddend2; set => minAddend2 = value; }
        public int MaxAddend2 { get => maxAddend2; set => maxAddend2 = value; }
        public int MinSum { get; set; } = 1;
        public int MaxSum { get; set; } = 10;
        public VariableTypes VariableTypes { get; set; } = VariableTypes.TwoNoSum;
        public List<Operation> OperationList { get; set; } = new() { Operation.Sum };
        public QuestionOrder QuestionOrder { get; set; } = QuestionOrder.Random;

        // Presentation / question style
        public UIQuestionType UIQuestionType { get; set; } = UIQuestionType.ThreeTexts;
        public bool EnforceOperationLabel { get; set; } = false;
        public bool FromNumToNum { get; set; } = false;
        public bool ShowPrev { get; set; } = false;
        public bool IncludeTutorials { get; set; } = false;
        public NumericInputMode NumericInputMode { get; set; } = NumericInputMode.AppKeypad;

        // Exercise generation
        public bool isLargerAddend1 { get; set; } = false;
        public bool OnlyThrougTen { get; set; } = false;
        public bool OnlyToTen { get; set; } = false;
        public bool isHelpEntries { get; set; } = false;
        public bool isHelpThroughTen { get; set; } = false;
        public bool isOnlySequence { get; set; } = true;
        public bool isOnlyKeyboard { get; set; } = false;
        public bool OnlyCloseTriad { get; set; } = false;
        public int RepeatingTimesOfTriad { get; set; } = 1;
        public int RepeatingTimesOfSum { get; set; } = 1;

        // Readability aliases that preserve the existing config surface.
        public bool PreferLargerAddend1 { get => isLargerAddend1; set => isLargerAddend1 = value; }
        public bool OnlyThroughTen { get => OnlyThrougTen; set => OnlyThrougTen = value; }
        public bool HelpEntries { get => isHelpEntries; set => isHelpEntries = value; }
        public bool HelpThroughTen { get => isHelpThroughTen; set => isHelpThroughTen = value; }
        public bool OnlySequence { get => isOnlySequence; set => isOnlySequence = value; }
        public bool KeyboardOnly { get => isOnlyKeyboard; set => isOnlyKeyboard = value; }

        // Input / pacing
        public int SecondsTillHideExercise { get; set; } = -1;
        public int SecondsTillAllowInput { get; set; } = -1;
        public int SecondsTillNextExercise { get; set; } = 2;

        // Targets / constraints
        public int NumberOfTasksToWin { get; set; } = -1;
        public int NumberOfMistakesToLose { get; set; } = -1;
        public bool DenyStrangeOrSameGroups { get; set; } = false;
        public bool TwoKeybordsOnOne { get; set; } = false;
        public Direction? WhichHand { get; set; } = null;
        public bool IsOnlyOneHand { get; set; } = false;
        public bool IsSpecialColor { get; set; } = false;

        // Optional value pools
        public List<int> addendsList { get; set; } = new();
        public List<int>? addendsListSecond { get; set; } = null;

        // Nested configuration
        public KeyboardConfig? KeyboardConfig { get; set; } = null;

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
            if (OnlyThrougTen && MaxSum > 10)
                return new PPWObject(8, 7, 15);
            if (maxAddend2 < 2)
                return new PPWObject(2, 1, 3);
            return new PPWObject(3, 2, 5);
        }

        // Readability helpers for gameplay and UI code
        public bool HasExercisePlan => Plan?.Steps?.Count > 0;
        public bool HasKeyboard => KeyboardConfig != null;
        public bool HasTaskGoal => NumberOfTasksToWin > -1;
        public bool HasMistakeLimit => NumberOfMistakesToLose > -1;
        public bool UsesQuestionPreview => SecondsTillHideExercise > 0;
        public bool DelaysInput => SecondsTillAllowInput > 0;
        public int EffectiveMinAddend2 => MinAddend2 == PPWGamePlay.NAN ? MinAddend : MinAddend2;
        public int EffectiveMaxAddend2 => MaxAddend2 == PPWGamePlay.NAN ? MaxAddend : MaxAddend2;
    }


}
