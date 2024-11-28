using GestureSample.Maui.Data;
using GestureSample.Maui.Models;
using SQLite;
using System.Collections.ObjectModel;

namespace GestureSample.Views
{
    public partial class ShowDataXaml
    {
        private class GameItem
        {
            public GameItem(string id, string text, int index = 0)
            {
                Id = id;
                Text = text;
                Index = index;
            }

            public string Id { get; set; }
            public int Index { get; set; }
            public string Text { get; set; }

            public override string ToString()
            {
                return $"{Index} {Text}";
            }

            public override bool Equals(object obj)
            {
                if (obj is GameItem other)
                {
                    return Id == other.Id;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return Id != null ? Id.GetHashCode() : 0;
            }
        }

        //private readonly RealmService _realmService;
        private ObservableCollection<GameItem> GameIdentifiers { get; set; } = new();
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
            await LoadStatesToPicker();
            if(gameId !=null)
            {
                await LoadStatesToGrid(gameId);
            }
        }

        private async Task LoadStatesToPicker()
        {
            //await _database.CreateTableAsync<Game>();
            var states = await StateConnection.Instance.GetStatesAsync();
            int i = 1;
            foreach (var state in states)
                
            {
                GameItem gameItem = new GameItem(state.Id, state.TimeStamp.ToString("dd/MM/yy HH:mm"), i);
                if (state != null && !GameIdentifiers.Contains(gameItem))
                {
                    GameIdentifiers.Add(gameItem); i++;
                }
            }
            GameIdentifiers = new ObservableCollection<GameItem>(GameIdentifiers.Reverse());
            GamePicker.ItemsSource = GameIdentifiers;
            
        }

        private async Task LoadStatesToGrid(string selectedIdentifier)
        {
            var gameStats = await StateConnection.Instance.GetStatesAsync();
            var gameStats2 = gameStats.Where(g => g.Id == selectedIdentifier).ToList();
            List<ShowState> states = new List<ShowState>();
            ShowState s_prev = null;
            foreach (var state in gameStats2)
            {

                ShowState s = new ShowState(state);
                if (s_prev == null)
                {
                    s_prev = s;
                    continue;
                }
                if (s_prev.Sum == PPWGamePlay.NAN || s_prev.Addend1 == PPWGamePlay.NAN || s_prev.Addend2 == PPWGamePlay.NAN) // Assuming Sum is the property to be checked
                {
                    s.StartTime = s_prev.TimeStamp;
                    if (s_prev.Sum == PPWGamePlay.NAN) s.SumColor = Colors.LightGreen;
                    if (s_prev.Addend1 == PPWGamePlay.NAN) s.Addend1Color = Colors.LightGreen;
                    if (s_prev.Addend2 == PPWGamePlay.NAN) s.Addend2Color = Colors.LightGreen;
                }
                if (s.Sum == PPWGamePlay.NAN || s.Addend1 == PPWGamePlay.NAN || s.Addend2 == PPWGamePlay.NAN) // Assuming Sum is the property to be checked
                {
                    continue;
                }
                if ((s.Op == "+" && s.Addend1 + s.Addend2 != s.Sum) || (s.Op == "X" && s.Addend1 + s.Addend2 != s.Sum))
                {
                    s.Addend1Color = (s.Addend1Color==Colors.LightGreen) ?Colors.PaleVioletRed : Colors.White;
                    s.Addend2Color = (s.Addend2Color == Colors.LightGreen) ? Colors.PaleVioletRed : Colors.White;
                    s.SumColor = (s.SumColor == Colors.LightGreen) ? Colors.PaleVioletRed : Colors.White;
                }
                if ((s.Op == "+" && s.Addend1 + s.Addend2 == s.Sum) || (s.Op == "X" && s.Addend1 + s.Addend2 == s.Sum))
                {
                    s_prev = null;
                }
                states.Add(s);
                    

                
            }

            if (states.Any())
            {
                StateList.ItemsSource = states;
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
                await LoadStatesToGrid(GameIdentifiers[picker.SelectedIndex].Id);                
            }
            
        }
    }
}