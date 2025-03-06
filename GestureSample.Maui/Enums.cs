using Microsoft.Maui.Platform;

namespace GestureSample.Maui
{

    public static class Statement
    {
        public const string Neutral = "   ";
        public const string True = "💪";
        public const string False = "🤔";
        public const string WrongInput = "Wrong Input";
        public const string New = "Find NEW combination";
        public const string Selecting = "SELECTING...";
        public static async Task<string> Win(TimeSpan? ts = null)
        {
            
                await Application.Current.MainPage.DisplayAlert("Win", "🎉😊🏅"+(ts==null?"":("\n" + ((TimeSpan)ts).ToFormattedString("mm: ss"))), "OK");
                return "🎉😊🏅";
        }
        public static async Task<string> Lose()
        {
                await Application.Current.MainPage.DisplayAlert("Lose", "🤷", "OK");
                return "🤷";
            
        }

    }
}
