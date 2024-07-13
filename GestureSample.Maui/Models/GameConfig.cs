using GestureSample.Maui.Models;
using GestureSample.Maui;

namespace GestureSample.Maui
{
    public class GameConfig
    {
        // Properties with default values
        public GameType GameType { get; set; } = GameType.SimpleDecompositionGame;
        public bool IsHistory { get; set; } = false;


        // Nested configuration with defaults
        public KeyboardConfig KeyboardConfig { get; set; } = new KeyboardConfig();
    }

    
}