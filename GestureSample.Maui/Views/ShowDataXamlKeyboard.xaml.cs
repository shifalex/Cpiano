using GestureSample.Maui.Data;
using GestureSample.Maui.Models;
using SQLite;
using System.Collections.ObjectModel;

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
        //private readonly RealmService _realmService;
        private ObservableCollection<DateWraper> GameDates { get; set; } = new();
        private List<Game> GameIdentifiers { get; set; } = new();
        private Game CurrentGame { get; set; } = null;
        private ObservableCollection<Game> GameIdentifiersFiltered { get; set; } = new();
        public ShowDataXamlKeyboard(string gameId = null)
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
            if (gameId == null && GameIdentifiers.Count > 0) CurrentGame = GameIdentifiers[0];
            for (int i = 0; i < GameIdentifiers.Count; i++) {
                if(gameId!=null &&  GameIdentifiers[i].Id.Equals(gameId)) CurrentGame = GameIdentifiers[i];
                GameIdentifiers[i].index = i+1;
                await StateConnection.Instance.UpdateGameAsync(GameIdentifiers[i]);

            }

            GameIdentifiers.Reverse();
            //await StateConnection.Instance.Execute(string.Format("UPDATE Game SET seq = {1} WHERE id = '{0}'", GameIdentifiers[0].Id, GameIdentifiers[0].index));

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
                        GameDates[GameDates.Count - 1].Date.Day == game.TimeStart.Day
                        )
                    {

                    }
                    else
                    {
                    if(game.Config?.KeyboardConfig != null)
                        GameDates.Add(new DateWraper(game.TimeStart.Date));
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
            for (int i = 0; i < GameIdentifiers.Count; i++)
               if (selectedIdentifier != null && GameIdentifiers[i].Id.Equals(selectedIdentifier)) 
                    CurrentGame = GameIdentifiers[i];


                var gamePresses = await StateConnection.Instance.GetKeyEventsByQueryAsync(selectedIdentifier);
           // List<KeyEvent> Events = new ();


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
            if (gamePresses.Any())
            {
                for (int i = 0; i < gamePresses.Count; i++)
                {
                    /*gamePresses[i].RowBackgroundColor = gamePresses[i].QuestionNumber % 2 == 0 ? Colors.LightGray : Colors.White;
                    if(states[i].Op == Maui.Operation.Divide || states[i].Op == Maui.Operation.Minus)
                    {
                        int oldSum = states[i].Sum; Color oldSumColor = states[i].SumColor;
                        states[i].Sum = states[i].Addend1; states[i].SumColor = states[i].Addend1Color;
                        states[i].Addend1 = oldSum; states[i].Addend1Color = oldSumColor;
                    }*/
                }

                StateList.ItemsSource = gamePresses;
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
                    if (GameIdentifiers[i].TimeStart.Year == ((DateWraper)DatePicker.SelectedItem).Date.Year &&
                        GameIdentifiers[i].TimeStart.Month == ((DateWraper)DatePicker.SelectedItem).Date.Month &&
                        GameIdentifiers[i].TimeStart.Day == ((DateWraper)DatePicker.SelectedItem).Date.Day 
                            )
                        if(GameIdentifiers[i].Config?.KeyboardConfig != null)
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