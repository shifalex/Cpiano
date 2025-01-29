using GestureSample.Maui.Data;
using GestureSample.Maui.Handlers;
using GestureSample.Maui.Models;
using SQLite;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GestureSample.Views
{
    public partial class ShowDataXamlKeyboard
    {
        private class DateWraper
        {
            public DateTime Date { get; set; }
            public DateWraper(DateTime d) { Date = d; }

            public override string ToString()
            {
                return Date.ToShortDateString();
            }
        }
        private readonly GameRepository _gameRepository;
        private readonly KeyboardQuestionRepository _keyboardQuestionRepository;
        private readonly KeyEventRepository _keyEventRepository;

        private ObservableCollection<DateWraper> GameDates { get; set; } = new();
        private List<Game> GameIdentifiers { get; set; } = new();
        private Game CurrentGame { get; set; } = null;
        private ObservableCollection<Game> GameIdentifiersFiltered { get; set; } = new();
        public ShowDataXamlKeyboard(string gameId = null)
        {
            InitializeComponent();
            _gameRepository = ServiceHelper.GetService<GameRepository>();
            _keyboardQuestionRepository = ServiceHelper.GetService<KeyboardQuestionRepository>();
            _keyEventRepository = ServiceHelper.GetService<KeyEventRepository>();
            ShowData(gameId);
        }

        public async void ShowData(string gameId = null)
        {
                GameIdentifiers = await _gameRepository.GetAllByUserAsync(ServiceHelper.GetService<CurrentUserSession>().ActiveUser.Id);
                if (GameIdentifiers == null || GameIdentifiers.Count == 0)
                {
                    Console.WriteLine("no games played");
                    // If not first user, just go back to whoever called this page (e.g. SwitchUserPage)
                    await Navigation.PopAsync();
                }
            
            if (gameId == null && GameIdentifiers.Count > 0) CurrentGame = GameIdentifiers[0];
            for (int i = 0; i < GameIdentifiers.Count; i++)
            {
                if (gameId != null && GameIdentifiers[i].Id.Equals(gameId)) CurrentGame = GameIdentifiers[i];
                GameIdentifiers[i].index = i + 1;
                await _gameRepository.UpdateAsync(GameIdentifiers[i]);

            }

            GameIdentifiers.Reverse();

            await LoadDates();
            LoadGames();
            if (gameId != null)
            {
                await LoadStatesToGrid(gameId);
            }
        }

        private async Task LoadDates()
        {

            foreach (var game in GameIdentifiers)

            {
                if (GameDates.Count > 0 &&
                    GameDates[GameDates.Count - 1].Date.Year == game.TimeStart.Year &&
                    GameDates[GameDates.Count - 1].Date.Month == game.TimeStart.Month &&
                    GameDates[GameDates.Count - 1].Date.Day == game.TimeStart.Day
                    )
                {

                }
                else
                {
                    if (game.Config?.KeyboardConfig != null)
                        GameDates.Add(new DateWraper(game.TimeStart.Date));
                }

            }

            DatePicker.ItemsSource = GameDates;
            if (GameDates.Count > 0)
            {
                DatePicker.SelectedIndex = 0;

            }

        }

        private async Task LoadStatesToGrid(string selectedIdentifier)
        {
            /*for (int i = 0; i < GameIdentifiers.Count; i++)
                if (selectedIdentifier != null && GameIdentifiers[i].Id.Equals(selectedIdentifier))
                    CurrentGame = GameIdentifiers[i];*/

            List<KeyboardQuestion> questionList = await _keyboardQuestionRepository.GetKeyboardQuestionByQueryAsync(selectedIdentifier);
            List<KeyEvent> gamePresses = await _keyEventRepository.GetKeyEventsByQueryAsync(selectedIdentifier);
            
            /*foreach (var state in gamePresses)
            {
                ShowState s = new(state);
                Color color = Colors.LightGray;
                if (s.Sum == PPWGamePlay.NAN || s.Addend1 == PPWGamePlay.NAN || s.Addend2 == PPWGamePlay.NAN) // Assuming Sum is the property to be checked
                {

                    if (s_prev == null)//After a good answer
                        s_prev = s;
                    if(gamePresses[gamePresses.Count - 1] == state)//Last not answered
                    {
                        if (s.Sum == PPWGamePlay.NAN) { s.SumColor = color; s.Sum = 0; }
                        if (s.Addend1 == PPWGamePlay.NAN) { s.Addend1Color = color; s.Addend1 = 0; }
                        if (s.Addend2 == PPWGamePlay.NAN) { s.Addend2Color = color; s.Addend2 = 0; }
                        states.Add(s);
                    }
                    continue;
        }
        color = s.ResultStatus switch
                {
                    0 => Colors.PaleVioletRed,
                    1 => Colors.LightGreen,
                    2 => Colors.LightYellow,
                    _ => Colors.White
                };
                if (s_prev.Sum == PPWGamePlay.NAN || s_prev.Addend1 == PPWGamePlay.NAN || s_prev.Addend2 == PPWGamePlay.NAN) // Assuming Sum is the property to be checked
                {
                    s.StartTime = s_prev.Time;
                    if (s_prev.Sum == PPWGamePlay.NAN) s.SumColor = color;
                    if (s_prev.Addend1 == PPWGamePlay.NAN) s.Addend1Color = color;
                    if (s_prev.Addend2 == PPWGamePlay.NAN) s.Addend2Color = color;
                }
                
                if ((s.ResultStatus==1))
                {
                    s_prev = null;
                }
                states.Add(s);
                    

                
            }*/
            
            List<MainItem> mainItems = new();
            if (questionList.Any())
            {
                foreach (KeyboardQuestion q in questionList)
                {
                    q.RowBackgroundColor = q.QuestionNumber % 2 == 0 ? Colors.LightGray : Colors.White;
                    mainItems.Add(new() { 
                        Question= q,
                        SubItems = gamePresses.Where(item => item.QuestionNumber == q.QuestionNumber).ToList()
                    });
                    /*if(states[i].Op == Maui.Operation.Divide || states[i].Op == Maui.Operation.Minus)
                    {
                        int oldSum = states[i].Sum; Color oldSumColor = states[i].SumColor;
                        states[i].Sum = states[i].Addend1; states[i].SumColor = states[i].Addend1Color;
                        states[i].Addend1 = oldSum; states[i].Addend1Color = oldSumColor;
                    }*/
                }
               
            }
            Questions.ItemsSource = mainItems;
            //StateList.ItemsSource = gamePresses;
        }


        private async void OnDatePickerSelectedIndexChanged(object sender, EventArgs e)
        {
            LoadGames();
        }

        private void LoadGames()
        {
            if (DatePicker.SelectedIndex != -1)
            {
                GameIdentifiersFiltered.Clear();
                for (int i = 0; i < GameIdentifiers.Count; i++)
                {
                    if (GameIdentifiers[i].TimeStart.Year == ((DateWraper)DatePicker.SelectedItem).Date.Year &&
                        GameIdentifiers[i].TimeStart.Month == ((DateWraper)DatePicker.SelectedItem).Date.Month &&
                        GameIdentifiers[i].TimeStart.Day == ((DateWraper)DatePicker.SelectedItem).Date.Day
                            )
                        if (GameIdentifiers[i].Config?.KeyboardConfig != null)
                            GameIdentifiersFiltered.Add(GameIdentifiers[i]);
                }
            }
            GamePicker.ItemsSource = GameIdentifiersFiltered;
            if (GamePicker.Items.Count > 0)
            {
                GamePicker.SelectedIndex = 0;
            }
        }

        private async void OnPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = sender as Picker;
            if (picker.SelectedIndex != -1)
            {
                await LoadStatesToGrid(GameIdentifiers[picker.SelectedIndex].Id);
            }

        }
        private void OnToggleSubgridClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is MainItem item)
            {
                item.IsSubCollectionVisible = !item.IsSubCollectionVisible;

                // Set HeightRequest manually for debugging
                var parentLayout = (Grid)button.Parent.Parent;
                var subCollection = parentLayout.FindByName<CollectionView>("SubCollection");

                subCollection.HeightRequest = item.IsSubCollectionVisible ? 200 : 10;

            }/*

            if (sender is Button button && button.CommandParameter is MainItem item)
            {
                // Find the parent VerticalStackLayout and the SubCollection
                var parentLayout = (VerticalStackLayout)button.Parent;
                var subCollection = parentLayout.FindByName<CollectionView>("StateList");

                // Toggle visibility
                subCollection.IsVisible = !subCollection.IsVisible;

                // Update button text
                button.Text = subCollection.IsVisible ? "Collapse Items" : "Expand Items";

                // Set the sub-collection items
                if (subCollection.ItemsSource == null)
                {
                    subCollection.ItemsSource = item.SubItems;
                }
            }*/
        }
    }

    public class MainItem : INotifyPropertyChanged
    {
        public KeyboardQuestion Question { get; set; }
        public List<KeyEvent> SubItems { get; set; }

        private bool _isSubCollectionVisible;
        public bool IsSubCollectionVisible
        {
            get => _isSubCollectionVisible;
            set
            {
                _isSubCollectionVisible = value;
                SubGridHeight = value ? 200 : 10; // Adjust height based on visibility
                OnPropertyChanged();
                OnPropertyChanged(nameof(ButtonText)); // Notify UI to update ButtonText
            }
        }

        private double _subGridHeight = 10; // Default collapsed height
        public double SubGridHeight
        {
            get => _subGridHeight;
            set
            {
                _subGridHeight = value;
                OnPropertyChanged();
            }
        }

        public string ButtonText => IsSubCollectionVisible ? "Collapse Items" : "Expand Items";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}