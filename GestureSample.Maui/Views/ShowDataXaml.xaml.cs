using GestureSample.Maui.Data;
using GestureSample.Maui.Data.SQLite;
using GestureSample.Maui.Handlers;
using GestureSample.Maui.Models;
using System.Data;
using System.Collections.ObjectModel;

namespace GestureSample.Views
{
    public partial class ShowDataXaml
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
        private bool isSaveVisible = false;
        private ObservableCollection<DateWraper> GameDates { get; set; } = new();
        private List<Game> GameIdentifiers { get; set; } = new();
        private Game CurrentGame { get; set; } = null;
        private ObservableCollection<Game> GameIdentifiersFiltered { get; set; } = new();

        private DataTable _usersDataTable = new DataTable();

        private readonly UserRepository _userRepo;
        private readonly GameRepository _gameRepository;
        private readonly QuestionAnswerRepository _questionAnswerRepository;
        private bool IsUsersPickerVisible { get; set; } = false;
        private Maui.Data.SQLite.User _currentUser;

        public ShowDataXaml(bool forTeacher=false, Guid? gameId = null)
        {
           InitializeComponent();
            //StateList.ItemsSource = App.CurrentDB.GetStates();
            //_realmService = new RealmService();
            //StateList.ItemsSource = _realmService.GetItems();
            //if(gameId!=null)
            //{
                /*MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var navigation = Application.Current.MainPage.Navigation;
                    Console.WriteLine(navigation.NavigationStack.Count);
                    var currentPage = navigation.NavigationStack[navigation.NavigationStack.Count - 1];
                    if (navigation.NavigationStack.Count > 2)
                    {
                        while (navigation.NavigationStack.Count > 0)
                        {

                            Console.WriteLine(navigation.NavigationStack[navigation.NavigationStack.Count - 1].Title);
                            var previousPage = navigation.NavigationStack[navigation.NavigationStack.Count - 1];
                            navigation.RemovePage(previousPage);
                        }
                        await navigation.PushAsync(currentPage);
                    }
                    Console.WriteLine(navigation.NavigationStack.Count);
                });*/
            //}
            _userRepo = ServiceHelper.GetService<UserRepository>();
            _gameRepository = ServiceHelper.GetService<GameRepository>();
            _questionAnswerRepository = ServiceHelper.GetService<QuestionAnswerRepository>();
            _currentUser = ServiceHelper.GetService<CurrentUserSession>().ActiveUser;
            if (gameId == null && forTeacher && _currentUser.Name == "Alex")
            {

                IsUsersPickerVisible = true;
                LoadClassroomUsers();
            }
            else
                ShowData(gameId);
        }


        private async void LoadClassroomUsers()
        {
            try
            {

                // This will call your edge function via the Supabase client
                List<Maui.Data.SupaBase.User> users = await Maui.Data.SupaBase.SupabaseService.GetUsersOfUser(_currentUser);

                // Optionally convert to DataTable for other uses
                _usersDataTable = new DataTable();
                _usersDataTable.Columns.Add("Id", typeof(string));
                _usersDataTable.Columns.Add("Name", typeof(string));
                foreach (var u in users)
                {
                    var row = _usersDataTable.NewRow();
                    row["Id"] = u.Id.ToString();
                    row["Name"] = u.Name;
                    _usersDataTable.Rows.Add(row);
                }

                // Populate the Picker
                UserPicker.Items.Clear();
                foreach (var u in users)
                    UserPicker.Items.Add(u.Name);

                UserPicker.IsVisible = users.Count > 0;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not load users: {ex.Message}", "OK");
                UserPicker.IsVisible = false;
            }
        }

        private DataTable ConvertUsersToDataTable(List<Maui.Data.SupaBase.User> users)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Id", typeof(string));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("AvatarUri", typeof(string));

            foreach (Maui.Data.SupaBase.User user in users)
            {
                DataRow row = dt.NewRow();
                row["Id"] = user.Id.ToString();
                row["Name"] = user.Name;
                row["AvatarUri"] = user.AvatarUri;
                dt.Rows.Add(row);
            }
            return dt;
        }

        private void PopulatePicker(DataTable dataTable)
        {
            UserPicker.Items.Clear();
            foreach (DataRow row in dataTable.Rows)
            {
                // Assuming your edge function returns a "Name" column.
                UserPicker.Items.Add(row["Name"].ToString());
            }
        }
    
        public async void ShowData(Guid? gameId=null)
        {
            Console.WriteLine(_currentUser.Name);
            GameIdentifiers = await _gameRepository.GetAllByUserAsync(_currentUser.Id);
            Console.WriteLine("Loading Identifiers finished");
            if (GameIdentifiers == null || GameIdentifiers.Count==0)
            {
                Console.WriteLine("no games played");
                // If not first user, just go back to whoever called this page (e.g. SwitchUserPage)
                await Navigation.PopAsync();
            }
            if (gameId == null && GameIdentifiers.Count > 0) CurrentGame = GameIdentifiers[0];
            for (int i = 0; i < GameIdentifiers.Count; i++) {
                if(gameId!=null &&  GameIdentifiers[i].Id.Equals(gameId)) CurrentGame = GameIdentifiers[i];
                GameIdentifiers[i].index = i+1;
                await _gameRepository.UpdateAsync(GameIdentifiers[i]);

            }

            GameIdentifiers.Reverse();
            //await StateConnection.Instance.Execute(string.Format("UPDATE Game SET seq = {1} WHERE id = '{0}'", GameIdentifiers[0].Id, GameIdentifiers[0].index));

            LoadDates();
             LoadGames();
            if(gameId !=null)
            {
                 await LoadStatesToGrid(gameId);
            }
            Console.WriteLine(_currentUser.Name);
        }

        private void LoadDates()
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
                        GameDates.Add(new DateWraper(game.TimeStart.Date));
                    }

                }
            
            //GameDates = new ObservableCollection<DateTime>(GameDates.Reverse());
            DatePicker.ItemsSource = GameDates;
            if(GameDates.Count>0) {DatePicker.SelectedIndex = 0;
               
            }

            //GamePicker.ItemsSource = GameIdentifiers;
            
        }

        private async Task LoadStatesToGrid(Guid? selectedIdentifier)
        {
            for (int i = 0; i < GameIdentifiers.Count; i++)
               if (selectedIdentifier != null && GameIdentifiers[i].Id.Equals(selectedIdentifier)) 
                    CurrentGame = GameIdentifiers[i];
            isSaveVisible = (CurrentGame!=null && !CurrentGame.WasSynced); 
            btnSave.IsVisible = isSaveVisible;

            List<QuestionAnswer> gameStats = new();
            if (selectedIdentifier != null)
            {
                Console.WriteLine(selectedIdentifier.ToString());
                gameStats = await _questionAnswerRepository.GetAnswersByQueryAsync((Guid)selectedIdentifier);
                Console.WriteLine("Rows: {0}",gameStats.Count);


            }
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
                if (s_prev?.Sum == PPWGamePlay.NAN || s_prev?.Addend1 == PPWGamePlay.NAN || s_prev?.Addend2 == PPWGamePlay.NAN) // Assuming Sum is the property to be checked
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
                    if(states[i].Op == Maui.Operation.Divide || states[i].Op == Maui.Operation.Minus)
                    {
                        int oldSum = states[i].Sum; Color oldSumColor = states[i].SumColor;
                        states[i].Sum = states[i].Addend1; states[i].SumColor = states[i].Addend1Color;
                        states[i].Addend1 = oldSum; states[i].Addend1Color = oldSumColor;
                    }
                }

                StateList.ItemsSource = states;
            }
            else
            {
                StateList.ItemsSource = null;
            }

            
        }


        private void OnDatePickerSelectedIndexChanged(object sender, EventArgs e)
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
                        GameIdentifiers[i].TimeStart.Day == ((DateWraper)DatePicker.SelectedItem).Date.Day)
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

        private  void OnUserPickerSelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private async void OnDampButtonClicked(object sender, EventArgs e)
        {
            //var users = await _userRepo.GetUsersAsync();
            //foreach (var user in users)
            {
                //await _userRepo.UpdateUserAsync(user);
                await GestureSample.Maui.Data.SupaBase.SupabaseService.SyncUserDataAsync(ServiceHelper.GetService<CurrentUserSession>().ActiveUser); // Sync with Supabase
            }
            isSaveVisible = false;
            btnSave.IsVisible = isSaveVisible;

            await DisplayAlert("Sync Complete", "Users synced with Supabase", "OK");
        }
    }
}