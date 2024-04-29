namespace GestureSample.Maui
{
    public enum GameType
    {
        GuessOne,
        CompletionOneInAddition,
        SimpleDecompositionGame,
        DecompositionGameFull,
        DecompositionGameFullWithKeyboardHelp,
        DecompositionGame,
        DecompositionGameWithKeyboardHelp,
        Multiplication,
        Logic
    }

    public enum VariableTypes
    {
        OneNoSum =1,
        TwoNoSum,
        OneCanBeSum,
        SumOnly,
        TwoAny


    }

    public static class Statement
    {
        public const string Neutral = "| |";
        public const string True = "Correct :D";
        public const string False = "Wrong :(";
        public const string WrongInput = "Wrong Imput";
        public const string New = "Find NEW combination";
        public static string Win { get
            {
                Application.Current.MainPage.DisplayAlert("Win", "You Won!!", "OK");
                return "YOU WON!!!"; } }
        public static string Lose
        {
            get
            {
                Application.Current.MainPage.DisplayAlert("Lose", "You Lost!!", "OK");
                return "You lost!";
            }
        }

    }
}
