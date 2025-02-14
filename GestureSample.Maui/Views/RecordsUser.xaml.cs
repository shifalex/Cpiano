using GestureSample.Maui.Data;
using GestureSample.Maui.Data.SQLite;
using GestureSample.Maui.Handlers;
using GestureSample.Maui.Models;
using SQLite;
using System.Collections.ObjectModel;

namespace GestureSample.Views
{
    public partial class RecordsUser
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
        private ObservableCollection<DateWraper> GameDates { get; set; } = new();
        private List<string> GameIdentifiers { get; set; } = new();
        private string CurrentGame { get; set; } = null;
        private ObservableCollection<Game> GameIdentifiersFiltered { get; set; } = new();

        private readonly GameRepository _gameRepository;
        public RecordsUser()
        {
            InitializeComponent();
            _gameRepository = ServiceHelper.GetService<GameRepository>();
            ShowData();
        }

        public async void ShowData()
        {
            GameIdentifiers = await _gameRepository.GetDistinctGameNamesAsync(ServiceHelper.GetService<CurrentUserSession>().ActiveUser.Id);
            Console.WriteLine("Loading Identifiers finished");
            if (GameIdentifiers == null || GameIdentifiers.Count == 0)
            {
                Console.WriteLine("no games played");
                // If not first user, just go back to whoever called this page (e.g. SwitchUserPage)
                await Navigation.PopAsync();
            }
            else
            {
                CurrentGame = GameIdentifiers[0];
                GamePicker.ItemsSource = GameIdentifiers;
            }
            // await LoadStatesToGrid(CurrentGame);
           
        }

        private async Task LoadStatesToGrid(string game)
        {
            CurrentGame = game;
            var   gameStats = await _gameRepository.GetRecordsByGameNamesAsync(ServiceHelper.GetService<CurrentUserSession>().ActiveUser.Id,game);
                Console.WriteLine("Rows: {0}",gameStats.Count);


            
            if (gameStats.Any())
            {
                
                StateList.ItemsSource = gameStats;
            }
            else
            {
                StateList.ItemsSource = null;
            }

            
        }


        private async void OnPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = sender as Picker;
            if (picker.SelectedIndex != -1)
            {
                 await LoadStatesToGrid(picker.SelectedItem.ToString());                
            }
            
        }

    }
}