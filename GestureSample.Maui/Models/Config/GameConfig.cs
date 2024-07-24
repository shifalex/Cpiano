using GestureSample.Maui.Models;
using GestureSample.Maui;

//https://appstoreconnect.apple.com/access/integrations/api

//=======ADDING NEW CONFIGURATION
//Start with new configuration here
//Add the new configuration to MainPage.Xaml.cs
//Add to SimpleViewCellsPage InitializeUI the UI that goes with the constructor
//SimpleViewCellsPage: Change buttonNext and Generate exercise event handling if needed
//SimpleViewCellsPage: change UpdateView with an "ïf" accordingly
//PPWGamePlay: change the correct GamePlay
//Set the configuration page

namespace GestureSample.Maui
{
    #region enums
    public enum UIQuestionType
    {
        //OneText,//Can be also objects.. always exists
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

    public enum Operation
    {
        Sum,
        Multiplication,
        Copy,
        Quantity,
        Serialize, //TODO: Try to solve the conflict that they can be both together and separate entities
        //Reorder,
        Not,
        And,
        Or,
        Neutralize

    }

    public enum VariableTypes
    {
        ShowOnlySum=0,
        OneNoSum,
        TwoNoSum,
        OneCanBeSum,
        SumOnly,
        TwoAny,
        


    }
    #endregion

    public class GameConfig
    {

        public static List<Operation> LogicaOperations = new() { Operation.Or, Operation.And, Operation.Neutralize, Operation.Not };
        public static List<Operation> ArithmeticOperations = new() { Operation.Multiplication, Operation.Sum };
        public static List<Operation> BitArrayOperation = new() { Operation.Copy, Operation.Quantity, Operation.Serialize };
        public static List<Operation> LogicalDualOperations = new() { Operation.Or, Operation.And, Operation.Neutralize };
        // Properties with default values
        public bool IsHistory { get; set; } = false;

        public int MinAddend { get; set; } = 0;
        public int MaxAddend { get; set; } = 5;
        public int MinSum { get; set; } = 1;
        public int MaxSum { get; set; } = 10;

        public bool OnlyThrougTen = false;

        public List<int> addendsList = new();
        public List<int> addendsListSecond = null;


        public bool FromNumToNum { get; set; } = false;

        public int SecondsToShowExercise { get; set; } = -1;

        public UIQuestionType UIQuestionType = UIQuestionType.ThreeTexts;
        public List<Operation> OperationList = new (){ Operation.Sum };
        public VariableTypes VariableTypes { get; set; } = VariableTypes.TwoNoSum;
        // Nested configuration with defaults
        public KeyboardConfig KeyboardConfig { get; set; } = null;
    }

    
}