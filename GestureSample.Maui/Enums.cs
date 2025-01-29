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
        public static string Win
        {
            get
            {
                Application.Current.MainPage.DisplayAlert("Win", "🎉😊🏅", "OK");
                return "🎉😊🏅";
            }
        }
        public static string Win2(TimeSpan ts)
        {
            Application.Current.MainPage.DisplayAlert("Win", "🎉😊🏅\n" + ts.ToFormattedString("mm:ss"), "OK");
            return "🎉😊🏅";
        }

        public static string Lose
        {
            get
            {
                Application.Current.MainPage.DisplayAlert("Lose", "🤷", "OK");
                return "🤷";
            }
        }

    }
}
