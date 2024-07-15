using GestureSample.Maui.Models;
using GestureSample.Maui;

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
        SimpleEquation,//Can have +- or /* and sumonly or onemissing variable type, should have options
        BitArrayQuestion, //Can be Hand or Keyboard
        Tower, //Can be text or keys tower
        ArrowQuestion,//May also have twoArrows. Can have or not a number on it On every exercise it is like new., Without text or with for pattern recognition
        LogicalQuestion //2kyboards, first optional, 1 operand
    }

    public enum ArrayQuestionTypes
    {
        TextNumber,
        Hand,
        Keyboard,
        Objects
    }


    public enum GameType//TODO: Difuse to "variable types" and "three texts"
    {
        GuessOne,
        CompletionOneInAddition,
        SimpleDecomposition,
        DecompositionGame,
        BitArrayGame,
        Multiplication,
        Logic
    }

    public enum BitArrayGameType
    {
        Copy,
        Quantity,
        Reorder,
        SerializeWithArrow //TODO: Try to solve the conflict that they can be both together and separate entities

    }

    public enum VariableTypes
    {
        OneNoSum = 1,
        TwoNoSum,
        OneCanBeSum,
        SumOnly,
        TwoAny


    }
    #endregion

    public class GameConfig
    {

        // Properties with default values
        public GameType GameType { get; set; } = GameType.SimpleDecomposition;
        public bool IsHistory { get; set; } = false;

        public int MinAddend { get; set; } = 0;
        public int MaxAddend { get; set; } = 5;
        public int MinSum {  get; set; } = 1;
        public int MaxSum { get; set; } = 10;

        public bool OnlyThrougTen = false;

        public List<int> addendsList = new();
        public List<int> addendsListSecond =null;


        public bool FromNumToNum { get; set; } = false;

        public UIQuestionType UIQuestionType = UIQuestionType.ThreeTexts;

        public ArrayQuestionTypes ArrayQuestionTypes = ArrayQuestionTypes.Keyboard;
        public BitArrayGameType BitArrayGameType = BitArrayGameType.Quantity;


        public VariableTypes VariableTypes { get; set; } = VariableTypes.TwoNoSum;
        // Nested configuration with defaults
        public KeyboardConfig KeyboardConfig { get; set; } = null;
    }

    
}