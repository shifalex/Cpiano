using GestureSample.Maui.Data;
using GestureSample.Maui.Models;
using SQLite;
using System.Collections.ObjectModel;

namespace GestureSample.Views
{
    public partial class ShowDataXaml
    {

        //private readonly RealmService _realmService;
        private ObservableCollection<DateTime> GameDates { get; set; } = new();
        private List<Game> GameIdentifiers { get; set; } = new();
        private ObservableCollection<Game> GameIdentifiersFiltered { get; set; } = new();
        public ShowDataXaml(string gameId = null)
        {
            InitializeComponent();
            //StateList.ItemsSource = App.CurrentDB.GetStates();
            //_realmService = new RealmService();
            //StateList.ItemsSource = _realmService.GetItems();
            ShowData(gameId);
        }

        public async void ShowData(string gameId=null)
        {

            GameIdentifiers = await StateConnection.Instance.GetGamesAsync();
            GameIdentifiers.Reverse();

            await LoadDates();
             LoadGames();
            if(gameId !=null)
            {
                 await LoadStatesToGrid(gameId);
            }
        }

        private async Task LoadDates()
        {

            foreach (var game in GameIdentifiers)
                
            {
                    if(GameDates.Count>0 && 
                        GameDates[GameDates.Count-1].Date.Year == game.TimeStart.Year &&
                        GameDates[GameDates.Count - 1].Date.Month == game.TimeStart.Month &&
                        GameDates[GameDates.Count - 1].Date.Day == game.TimeStart.Day)
                    {

                    }
                    else
                    {
                        GameDates.Add(game.TimeStart.Date);
                    }

                }
            
            //GameDates = new ObservableCollection<DateTime>(GameDates.Reverse());
            DatePicker.ItemsSource = GameDates;
            if(GameDates.Count>0) {DatePicker.SelectedIndex = 0;
               
            }

            //GamePicker.ItemsSource = GameIdentifiers;
            
        }

        private async Task LoadStatesToGrid(string selectedIdentifier)
        {
            var gameStats = await StateConnection.Instance.GetStatesByQueryAsync(selectedIdentifier);
            List<ShowState> states = new ();
            ShowState s_prev = null;
            foreach (var state in gameStats)
            {
                ShowState s = new(state);
                Color color = Colors.LightGray;
                if (s.Sum == PPWGamePlay.NAN || s.Addend1 == PPWGamePlay.NAN || s.Addend2 == PPWGamePlay.NAN) // Assuming Sum is the property to be checked
                {

                    if (s_prev == null)//After a good answer
                        s_prev = s;
                    if(gameStats[gameStats.Count - 1] == state)//Last not answered
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
                    

                
            }

            if (states.Any())
            {
                for (int i = 0; i < states.Count; i++)
                {
                    states[i].RowBackgroundColor = states[i].QuestionNumber % 2 == 0 ? Colors.LightGray : Colors.White;
                    
                }

                StateList.ItemsSource = states;
            }
            else
            {
                StateList.ItemsSource = null;
            }

            
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
                    if (GameIdentifiers[i].TimeStart.Year == ((DateTime)DatePicker.SelectedItem).Year &&
                        GameIdentifiers[i].TimeStart.Month == ((DateTime)DatePicker.SelectedItem).Month &&
                        GameIdentifiers[i].TimeStart.Day == ((DateTime)DatePicker.SelectedItem).Day)
                        GameIdentifiersFiltered.Add(GameIdentifiers[i]);
                }
            }
            GamePicker.ItemsSource = GameIdentifiersFiltered;
            if (GamePicker.Items.Count > 0)
            {
                GamePicker.SelectedIndex = 0;
                //OnPickerSelectedIndexChanged(sender, e);
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
    }
}