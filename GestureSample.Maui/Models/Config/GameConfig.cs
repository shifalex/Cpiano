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
       [Description("EQUAL\n=")]
        Quantity,
        [Description("GROUP to the RIGHT\n-->")]
        SequenceLTR,
        [Description("GROUP to the LEFT\n<--")]
        SequenceRTL,
        [Description("SPLIT\n<- ->")]
        Split,
        [Description("GROUP -><-to CENTER")]
        Centrelize,
        [Description("GROUP BY COLOR")]
        GroupByColor,
        [Description("SHIFT")]
        MoveBy,
        [Description("MIRROR\n<|>")]
        Mirror,
        //Serialize, //TODO: Try to solve the conflict that they can be both together and separate entities
        //Reorder,
        [Description("NOT\n!")]
        Not,
        [Description("AND\n&&")]
        And,
        [Description("OR\n||")]
        Or,
        [Description("XOR")]
        ExclusiveOr,
        [Description("SUM\n+")]
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
        ChoiceKeyboard,
        SystemKeyboard
    }

    [Flags]
    public enum PresentationFeatureFlags
    {
        None = 0,
        EnforceOperationLabel = 1,
        FromNumToNum = 2,
        ShowPrevious = 4,
        Tutorials = 8,
        HelpEntries = 16,
        HelpThroughTen = 32
    }

    [Flags]
    public enum TenBoundaryMode
    {
        None = 0,
        ToTen = 1,
        ThroughTen = 2
    }

    [Flags]
    public enum BitArrayGenerationFlags
    {
        None = 0,
        SequenceOnly = 1,
        KeyboardOnly = 2
    }

    [Flags]
    public enum GroupCombinationMode
    {
        None = 0,
        Overlapping = 1,
        Strange = 2,
        OneInsideAnother = 4,
        Same = 8,
        Empty = 16
    }

    [Flags]
    public enum LogicalKeyboardLayoutFlags
    {
        None = 0,
        CombinedOnSingleKeyboard = 1,
        OneHandOnly = 2,
        SpecialColor = 4
    }

    [Flags]
    public enum MissingValueTargetFlags
    {
        None = 0,
        Addend1 = 1,
        Addend2 = 2,
        Sum = 4,
        Addends = Addend1 | Addend2,
        All = Addends | Sum
    }

    [Flags]
    public enum MissingValueConstraintFlags
    {
        None = 0,
        KeepSumVisible = 1,
        KeepAtLeastOneAddendVisible = 2
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
            ApplyVariableType(VariableTypes.TwoNoSum);
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
        public MissingValueTargetFlags AllowedMissingValueTargets { get; set; } = MissingValueTargetFlags.Addends;
        public MissingValueConstraintFlags MissingValueConstraints { get; set; } = MissingValueConstraintFlags.None;
        public int HiddenValueCount { get; set; } = 2;
        public VariableTypes VariableTypes
        {
            get => InferVariableType();
            set => ApplyVariableType(value);
        }
        public List<Operation> OperationList { get; set; } = new() { Operation.Sum };
        public QuestionOrder QuestionOrder { get; set; } = QuestionOrder.Random;

        // Presentation / question style
        public UIQuestionType UIQuestionType { get; set; } = UIQuestionType.ThreeTexts;
        public PresentationFeatureFlags PresentationFeatures { get; set; } = PresentationFeatureFlags.None;
        public bool EnforceOperationLabel
        {
            get => PresentationFeatures.HasFlag(PresentationFeatureFlags.EnforceOperationLabel);
            set => PresentationFeatures = value
                ? PresentationFeatures | PresentationFeatureFlags.EnforceOperationLabel
                : PresentationFeatures & ~PresentationFeatureFlags.EnforceOperationLabel;
        }
        public bool FromNumToNum
        {
            get => PresentationFeatures.HasFlag(PresentationFeatureFlags.FromNumToNum);
            set => PresentationFeatures = value
                ? PresentationFeatures | PresentationFeatureFlags.FromNumToNum
                : PresentationFeatures & ~PresentationFeatureFlags.FromNumToNum;
        }
        public bool ShowPrev
        {
            get => PresentationFeatures.HasFlag(PresentationFeatureFlags.ShowPrevious);
            set => PresentationFeatures = value
                ? PresentationFeatures | PresentationFeatureFlags.ShowPrevious
                : PresentationFeatures & ~PresentationFeatureFlags.ShowPrevious;
        }
        public bool IncludeTutorials
        {
            get => PresentationFeatures.HasFlag(PresentationFeatureFlags.Tutorials);
            set => PresentationFeatures = value
                ? PresentationFeatures | PresentationFeatureFlags.Tutorials
                : PresentationFeatures & ~PresentationFeatureFlags.Tutorials;
        }
        public NumericInputMode NumericInputMode { get; set; } = NumericInputMode.AppKeypad;

        // Exercise generation
        public bool isLargerAddend1 { get; set; } = false;
        public TenBoundaryMode TenBoundaryModes { get; set; } = TenBoundaryMode.None;
        public BitArrayGenerationFlags BitArrayGeneration { get; set; } = BitArrayGenerationFlags.SequenceOnly;
        public bool isHelpEntries
        {
            get => PresentationFeatures.HasFlag(PresentationFeatureFlags.HelpEntries);
            set => PresentationFeatures = value
                ? PresentationFeatures | PresentationFeatureFlags.HelpEntries
                : PresentationFeatures & ~PresentationFeatureFlags.HelpEntries;
        }
        public bool isHelpThroughTen
        {
            get => PresentationFeatures.HasFlag(PresentationFeatureFlags.HelpThroughTen);
            set => PresentationFeatures = value
                ? PresentationFeatures | PresentationFeatureFlags.HelpThroughTen
                : PresentationFeatures & ~PresentationFeatureFlags.HelpThroughTen;
        }
        public bool isOnlySequence
        {
            get => BitArrayGeneration.HasFlag(BitArrayGenerationFlags.SequenceOnly);
            set => BitArrayGeneration = value
                ? BitArrayGeneration | BitArrayGenerationFlags.SequenceOnly
                : BitArrayGeneration & ~BitArrayGenerationFlags.SequenceOnly;
        }
        public bool isOnlyKeyboard
        {
            get => BitArrayGeneration.HasFlag(BitArrayGenerationFlags.KeyboardOnly);
            set => BitArrayGeneration = value
                ? BitArrayGeneration | BitArrayGenerationFlags.KeyboardOnly
                : BitArrayGeneration & ~BitArrayGenerationFlags.KeyboardOnly;
        }
        public bool OnlyCloseTriad { get; set; } = false;
        public bool AllowCloseTriadSumChange { get; set; } = false;
        public bool UsePairedCloseTriadBenchmark { get; set; } = false;
        public int RepeatingTimesOfTriad { get; set; } = 1;
        public int RepeatingTimesOfSum { get; set; } = 1;
        public bool UseDistortedVariantInRepeatSequence { get; set; } = false;

        // Readability aliases that preserve the existing config surface.
        public bool PreferLargerAddend1 { get => isLargerAddend1; set => isLargerAddend1 = value; }
        public bool OnlyThrougTen
        {
            get => TenBoundaryModes.HasFlag(TenBoundaryMode.ThroughTen);
            set => TenBoundaryModes = value
                ? TenBoundaryModes | TenBoundaryMode.ThroughTen
                : TenBoundaryModes & ~TenBoundaryMode.ThroughTen;
        }
        public bool OnlyToTen
        {
            get => TenBoundaryModes.HasFlag(TenBoundaryMode.ToTen);
            set => TenBoundaryModes = value
                ? TenBoundaryModes | TenBoundaryMode.ToTen
                : TenBoundaryModes & ~TenBoundaryMode.ToTen;
        }
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
        public GroupCombinationMode AllowedGroupCombinations { get; set; } = GroupCombinationMode.None;
        public LogicalKeyboardLayoutFlags LogicalKeyboardLayout { get; set; } = LogicalKeyboardLayoutFlags.None;
        public bool DenyStrangeOrSameGroups
        {
            get => AllowedGroupCombinations != GroupCombinationMode.None &&
                   !AllowedGroupCombinations.HasFlag(GroupCombinationMode.Strange) &&
                   !AllowedGroupCombinations.HasFlag(GroupCombinationMode.Same);
            set
            {
                if (value)
                    AllowedGroupCombinations = GroupCombinationMode.Overlapping | GroupCombinationMode.OneInsideAnother;
                else if (AllowedGroupCombinations == (GroupCombinationMode.Overlapping | GroupCombinationMode.OneInsideAnother))
                    AllowedGroupCombinations = GroupCombinationMode.None;
            }
        }
        public bool TwoKeybordsOnOne
        {
            get => LogicalKeyboardLayout.HasFlag(LogicalKeyboardLayoutFlags.CombinedOnSingleKeyboard);
            set => LogicalKeyboardLayout = value
                ? LogicalKeyboardLayout | LogicalKeyboardLayoutFlags.CombinedOnSingleKeyboard
                : LogicalKeyboardLayout & ~LogicalKeyboardLayoutFlags.CombinedOnSingleKeyboard;
        }
        public Direction? WhichHand { get; set; } = null;
        public bool IsOnlyOneHand
        {
            get => LogicalKeyboardLayout.HasFlag(LogicalKeyboardLayoutFlags.OneHandOnly);
            set => LogicalKeyboardLayout = value
                ? LogicalKeyboardLayout | LogicalKeyboardLayoutFlags.OneHandOnly
                : LogicalKeyboardLayout & ~LogicalKeyboardLayoutFlags.OneHandOnly;
        }
        public bool IsSpecialColor
        {
            get => LogicalKeyboardLayout.HasFlag(LogicalKeyboardLayoutFlags.SpecialColor);
            set => LogicalKeyboardLayout = value
                ? LogicalKeyboardLayout | LogicalKeyboardLayoutFlags.SpecialColor
                : LogicalKeyboardLayout & ~LogicalKeyboardLayoutFlags.SpecialColor;
        }

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

        public bool KeepsSumVisible => MissingValueConstraints.HasFlag(MissingValueConstraintFlags.KeepSumVisible);
        public bool KeepsAtLeastOneAddendVisible => MissingValueConstraints.HasFlag(MissingValueConstraintFlags.KeepAtLeastOneAddendVisible);
        public bool UsesCombinedLogicalKeyboard => LogicalKeyboardLayout.HasFlag(LogicalKeyboardLayoutFlags.CombinedOnSingleKeyboard);
        public bool RestrictsLogicalKeyboardToOneHand => LogicalKeyboardLayout.HasFlag(LogicalKeyboardLayoutFlags.OneHandOnly);
        public bool UsesSpecialLogicalKeyboardColors => LogicalKeyboardLayout.HasFlag(LogicalKeyboardLayoutFlags.SpecialColor);
        public bool RequiresBothAddendsInput => HiddenValueCount == 2 && AllowedMissingValueTargets == MissingValueTargetFlags.Addends;

        public bool TryGetLegacyVariableType(out VariableTypes variableType)
        {
            if (HiddenValueCount == 1 && AllowedMissingValueTargets == MissingValueTargetFlags.All)
            {
                variableType = VariableTypes.OneCanBeSum;
                return true;
            }

            if (HiddenValueCount == 1 && AllowedMissingValueTargets == MissingValueTargetFlags.Addends)
            {
                variableType = VariableTypes.OneNoSum;
                return true;
            }

            if (HiddenValueCount == 1 && AllowedMissingValueTargets == MissingValueTargetFlags.Sum)
            {
                variableType = VariableTypes.SumOnly;
                return true;
            }

            if (HiddenValueCount == 2 && AllowedMissingValueTargets == MissingValueTargetFlags.Addends)
            {
                variableType = VariableTypes.TwoNoSum;
                return true;
            }

            if (HiddenValueCount == 2 &&
                AllowedMissingValueTargets == MissingValueTargetFlags.All &&
                MissingValueConstraints.HasFlag(MissingValueConstraintFlags.KeepAtLeastOneAddendVisible))
            {
                variableType = VariableTypes.Three;
                return true;
            }

            variableType = default;
            return false;
        }

        private VariableTypes InferVariableType()
        {
            return TryGetLegacyVariableType(out VariableTypes variableType)
                ? variableType
                : VariableTypes.OneCanBeSum;
        }

        private void ApplyVariableType(VariableTypes value)
        {
            switch (value)
            {
                case VariableTypes.OneCanBeSum:
                    AllowedMissingValueTargets = MissingValueTargetFlags.All;
                    HiddenValueCount = 1;
                    MissingValueConstraints = MissingValueConstraintFlags.None;
                    break;
                case VariableTypes.OneNoSum:
                    AllowedMissingValueTargets = MissingValueTargetFlags.Addends;
                    HiddenValueCount = 1;
                    MissingValueConstraints = MissingValueConstraintFlags.None;
                    break;
                case VariableTypes.SumOnly:
                    AllowedMissingValueTargets = MissingValueTargetFlags.Sum;
                    HiddenValueCount = 1;
                    MissingValueConstraints = MissingValueConstraintFlags.None;
                    break;
                case VariableTypes.TwoNoSum:
                    AllowedMissingValueTargets = MissingValueTargetFlags.Addends;
                    HiddenValueCount = 2;
                    MissingValueConstraints = MissingValueConstraintFlags.None;
                    break;
                default:
                    AllowedMissingValueTargets = MissingValueTargetFlags.All;
                    HiddenValueCount = 2;
                    MissingValueConstraints = MissingValueConstraintFlags.KeepAtLeastOneAddendVisible;
                    break;
            }
        }
    }


}
