using GestureSample.Maui.Data;
using SQLite;
using System.Collections.ObjectModel;

namespace GestureSample.Views
{
    public partial class ShowDataXaml
    {
        //private readonly RealmService _realmService;
        public ObservableCollection<string> GameIdentifiers { get; set; } = new ObservableCollection<string>();
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
            LoadStatesToPicker();
            if(gameId !=null)
            {
                LoadStatesToGrid(gameId);
            }
        }

        private async void LoadStatesToPicker()
        {
            //await _database.CreateTableAsync<Game>();
            var states = await StateConnection.Instance.GetStatesAsync();
            foreach (var state in states)
            {
                if(state != null && !GameIdentifiers.Contains( state.Id))
                    GameIdentifiers.Add(state.Id);
            }
        }

        private async void LoadStatesToGrid(string selectedIdentifier)
        {
            var gameStats = await StateConnection.Instance.GetStatesAsync();
            var gameStats2 = gameStats.Where(g => g.Id == selectedIdentifier);
            if (gameStats2 != null)
            {
                StateList.ItemsSource = gameStats2;
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
                LoadStatesToGrid(picker.SelectedItem.ToString());                
            }
            
        }
    }
}