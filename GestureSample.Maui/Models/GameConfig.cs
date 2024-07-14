using GestureSample.Maui.Models;
using GestureSample.Maui;

namespace GestureSample.Maui
{
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


        public VariableTypes VariableTypes { get; set; } = VariableTypes.TwoNoSum;
        // Nested configuration with defaults
        public KeyboardConfig KeyboardConfig { get; set; } = null;
    }

    
}