using GestureSample.Maui.Data.SQLite;
using GestureSample.Maui.Data;
using GestureSample.Maui.Handlers;
using GestureSample.Maui.Models;
using GestureSample.Maui;
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
        private ObservableCollection<DateWraper> GameDates { get; set; } = new();
        private List<Game> GameIdentifiers { get; set; } = new();
        private Game CurrentGame { get; set; } = null;
        private ObservableCollection<Game> GameIdentifiersFiltered { get; set; } = new();

        private DataTable _usersDataTable = new DataTable();

        private readonly UserRepository _userRepo;
        private readonly GameRepository _gameRepository;
        private readonly QuestionAnswerRepository _questionAnswerRepository;
        private readonly QuestionAnswerPartRepository _questionAnswerPartRepository;
        private readonly BackgroundSyncService _backgroundSyncService;
        private readonly SyncToolbarStatusController _syncToolbarStatusController;
        private Maui.Data.SQLite.User _currentUser;
        private bool _isTeacher = false;
        private readonly bool _showSelectors;
        private readonly ToolbarItem _backToolbarItem;
        private readonly ToolbarItem _gamesToolbarItem;
        private readonly ToolbarItem _sortToolbarItem;
        private Guid? _currentSelectedGameId;
        private DateTime? _currentSelectedDate;
        private bool _sortNewestFirst = true;
        private readonly Maui.Data.SQLite.User? _dataUser;

        public ShowDataXaml(bool forTeacher = false, Guid? gameId = null, bool showSelectors = true, bool sortNewestFirst = true, Maui.Data.SQLite.User? dataUser = null)
        {
           InitializeComponent();
            Title = "Data";
            _showSelectors = showSelectors;
            _sortNewestFirst = sortNewestFirst;
            _dataUser = dataUser;
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
            _questionAnswerPartRepository = ServiceHelper.GetService<QuestionAnswerPartRepository>();
            _backgroundSyncService = ServiceHelper.GetService<BackgroundSyncService>();
            _syncToolbarStatusController = new SyncToolbarStatusController(this, _backgroundSyncService);
            _currentUser = _dataUser ?? ServiceHelper.GetService<CurrentUserSession>().ActiveUser;
            _backToolbarItem = new ToolbarItem
            {
                Text = "Back",
                Priority = 0,
                Order = ToolbarItemOrder.Primary,
                Command = new Command(async () => await NavigateBackAsync())
            };
            ToolbarItems.Add(_backToolbarItem);
            _gamesToolbarItem = new ToolbarItem
            {
                Text = "Games",
                Priority = 1,
                Order = ToolbarItemOrder.Primary,
                Command = new Command(async () => await NavigateToChooserAsync(CurrentGame?.Id))
            };
            if (!_showSelectors)
                ToolbarItems.Add(_gamesToolbarItem);
            _sortToolbarItem = new ToolbarItem
            {
                Text = GetSortToolbarText(),
                Priority = 2,
                Order = ToolbarItemOrder.Primary,
                Command = new Command(async () => await ToggleSortAsync())
            };
            if (_showSelectors)
                ToolbarItems.Add(_sortToolbarItem);

            PickerPanel.IsVisible = _showSelectors;
            HeaderGrid.IsVisible = _showSelectors;
            UserPicker.IsVisible = false;
            if (gameId == null && (forTeacher || ServiceHelper.GetService<CurrentUserSession>().ActiveUser.IsTeacher))
            {
                _isTeacher = true;
                LoadClassroomUsers(); 
            }
            else
                ShowData(gameId);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _syncToolbarStatusController.Attach();
        }

        protected override void OnDisappearing()
        {
            _syncToolbarStatusController.Detach();
            base.OnDisappearing();
        }


        private async void LoadClassroomUsers()
        {
            try
            {
                if (!_currentUser.IsTeacher)
                {
                    _isTeacher = false;
                    UserPicker.IsVisible = false;
                    ShowData(null);
                    return;
                }

                UserPicker.IsVisible = true;
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
                { 
                    if(u.Id == _currentUser.Id) 
                        UserPicker.Items.Insert(0, u.Name);
                    else
                        UserPicker.Items.Add(u.Name);
                }
                UserPicker.IsVisible = users.Count > 0;
                if (UserPicker.Items.Count > 0)
                {
                    UserPicker.SelectedIndex = 0;
                    //OnPickerSelectedIndexChanged(sender, e);
                }
                else
                {
                    _isTeacher = false;
                    UserPicker.IsVisible = false;
                }
                ShowData(null);

            }
            catch (Exception ex)
            {
                _isTeacher = false;
                _currentUser = _dataUser ?? ServiceHelper.GetService<CurrentUserSession>().ActiveUser;
                await DisplayAlert("Error", $"Could not load users from Supabase. Showing local data instead.\n{ex.Message}", "OK");
                UserPicker.IsVisible = false;
                ShowData(null);
            }
        }

        public async void ShowData(Guid? gameId=null)
        {
            Console.WriteLine(_currentUser.Name);
            gameId ??= _currentSelectedGameId;
            try
            {
                GameIdentifiers = await (_isTeacher
                    ? Maui.Data.SupaBase.SupabaseService.GetAllByUserAsync(_currentUser.Id)
                    : _gameRepository.GetAllByUserAsync(_currentUser.Id));
            }
            catch (Exception ex)
            {
                _isTeacher = false;
                _currentUser = _dataUser ?? ServiceHelper.GetService<CurrentUserSession>().ActiveUser;
                UserPicker.IsVisible = false;
                await DisplayAlert("Supabase", $"Could not load remote data. Showing local data instead.\n{ex.Message}", "OK");
                GameIdentifiers = await _gameRepository.GetAllByUserAsync(_currentUser.Id);
            }
            Console.WriteLine("Loading Identifiers finished");
            if (GameIdentifiers == null || GameIdentifiers.Count==0)
            {
                Console.WriteLine("no games played");
                ClearDisplayedData();
                return;
            }
            if (gameId == null && GameIdentifiers.Count > 0)
            {
                DateTime today = DateTime.Today;
                CurrentGame = GameIdentifiers.FirstOrDefault(game => game.TimeStart.Date == today)
                    ?? GameIdentifiers[0];
            }
            for (int i = 0; i < GameIdentifiers.Count; i++) {
                if(gameId!=null &&  GameIdentifiers[i].Id.Equals(gameId)) CurrentGame = GameIdentifiers[i];
                GameIdentifiers[i].index = i+1;
            }

            if (CurrentGame != null)
            {
                _currentSelectedGameId = CurrentGame.Id;
                _currentSelectedDate = CurrentGame.TimeStart.Date;
            }

            ApplySort();
            //await StateConnection.Instance.Execute(string.Format("UPDATE Game SET seq = {1} WHERE id = '{0}'", GameIdentifiers[0].Id, GameIdentifiers[0].index));

            if (gameId != null && CurrentGame != null && ShowDataRoutingHelper.ShouldUseKeyboardData(CurrentGame.Config))
            {
                await OpenKeyboardDataPageAsync(CurrentGame.Id);
                return;
            }

            if (_showSelectors)
            {
                LoadDates();
                LoadGames();
            }

            if (gameId != null)
                await LoadStatesToGrid(gameId);

            Console.WriteLine(_currentUser.Name);
        }

        private void LoadDates()
        {
            GameDates.Clear();

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
            if (GameDates.Count == 0)
                return;

            DateWraper? preferredDate = _currentSelectedDate.HasValue
                ? GameDates.FirstOrDefault(item => item.Date.Date == _currentSelectedDate.Value.Date)
                : null;

            if (preferredDate != null)
            {
                DatePicker.SelectedItem = preferredDate;
                return;
            }

            DatePicker.SelectedIndex = 0;

            //GamePicker.ItemsSource = GameIdentifiers;
            
        }

        private async Task LoadStatesToGrid(Guid? selectedIdentifier)
        {
            for (int i = 0; i < GameIdentifiers.Count; i++)
               if (selectedIdentifier != null && GameIdentifiers[i].Id.Equals(selectedIdentifier)) 
                    CurrentGame = GameIdentifiers[i];

            List<QuestionAnswer> gameStats = new();
            Dictionary<int, List<QuestionAnswerPart>> helperPartsByQuestion = new();
            if (selectedIdentifier != null)
            {
                Console.WriteLine(selectedIdentifier.ToString()+" {0}", (Guid)selectedIdentifier);
                try
                {
                    gameStats = await (_isTeacher
                        ? Maui.Data.SupaBase.SupabaseService.GetAnswersByQueryAsync((Guid)selectedIdentifier)
                        : _questionAnswerRepository.GetAnswersByQueryAsync((Guid)selectedIdentifier));
                }
                catch (Exception ex)
                {
                    _isTeacher = false;
                    _currentUser = _dataUser ?? ServiceHelper.GetService<CurrentUserSession>().ActiveUser;
                    UserPicker.IsVisible = false;
                    await DisplayAlert("Supabase", $"Could not load remote answers. Showing local data instead.\n{ex.Message}", "OK");
                    gameStats = await _questionAnswerRepository.GetAnswersByQueryAsync((Guid)selectedIdentifier);
                }
                Console.WriteLine("Rows: {0}",gameStats.Count);

                if (!_isTeacher)
                {
                    List<QuestionAnswerPart> helperParts = await _questionAnswerPartRepository.GetByGameAsync((Guid)selectedIdentifier);
                    helperPartsByQuestion = helperParts
                        .GroupBy(item => item.QuestionNumber)
                        .ToDictionary(group => group.Key, group => group.ToList());
                }


            }
            List<ShowState> states = new ();
            ShowState s_prev = null;
            foreach (var state in gameStats)
            {
                ShowState s = new(state);
                if (helperPartsByQuestion.TryGetValue(s.QuestionNumber, out List<QuestionAnswerPart>? questionParts))
                    s.SetHelperParts(questionParts);
                s.TimeWarningSeconds = GetTimeWarningSeconds();
                s.ComplexArrowPathText = BuildComplexArrowPathText(s);
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
                states = (_sortNewestFirst
                    ? states.OrderByDescending(item => item.Time).ThenByDescending(item => item.QuestionNumber)
                    : states.OrderBy(item => item.Time).ThenBy(item => item.QuestionNumber))
                    .ToList();

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

        private string BuildComplexArrowPathText(ShowState state)
        {
            if (CurrentGame?.Config?.KeyboardConfig?.ArrowLabelExerciseMode is not
                (ArrowLabelExerciseMode.ComplexBridgeToNextTen or ArrowLabelExerciseMode.ComplexBridgeToAnyNextTen or ArrowLabelExerciseMode.ComplexLongDistance))
            {
                return string.Empty;
            }

            int start = state.Addend1;
            int totalDistance = state.Addend2;
            int end = state.Sum;
            if (start == PPWGamePlay.NAN || totalDistance == PPWGamePlay.NAN || end == PPWGamePlay.NAN)
                return string.Empty;

            if (totalDistance <= 0 || end <= start)
                totalDistance = end - start;

            if (totalDistance <= 0)
                return string.Empty;

            int middle = ((start / 10) + 1) * 10;
            if (middle <= start || middle >= end)
                return string.Empty;

            int distance1 = middle - start;
            int distance2 = end - middle;
            if (distance1 <= 0 || distance2 <= 0 || distance1 + distance2 != totalDistance)
                return string.Empty;

            return $"{start} + {totalDistance} = {start} + {distance1} + {distance2} = {middle} + {distance2} = {end}";
        }

        private double GetTimeWarningSeconds()
        {
            return IsComplexArrowLabelExercise(CurrentGame?.Config?.KeyboardConfig?.ArrowLabelExerciseMode)
                ? 20
                : 6;
        }

        private static bool IsComplexArrowLabelExercise(ArrowLabelExerciseMode? mode)
        {
            return mode.HasValue &&
                   mode.Value is ArrowLabelExerciseMode.ComplexBridgeToNextTen
                       or ArrowLabelExerciseMode.ComplexBridgeToAnyNextTen
                       or ArrowLabelExerciseMode.ComplexLongDistance;
        }


        private void OnDatePickerSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_showSelectors)
                return;

            if (DatePicker.SelectedItem is DateWraper selectedDate)
                _currentSelectedDate = selectedDate.Date;

            LoadGames();
        }

        private void LoadGames()
        {
            GameIdentifiersFiltered.Clear();
            for (int i = 0; i < GameIdentifiers.Count; i++)
                GameIdentifiersFiltered.Add(GameIdentifiers[i]);

            GamePicker.ItemsSource = GameIdentifiersFiltered;
            if (GamePicker.Items.Count == 0)
                return;

            Game? preferredGame = _currentSelectedGameId.HasValue
                ? GameIdentifiersFiltered.FirstOrDefault(game => game.Id == _currentSelectedGameId.Value)
                : null;

            if (preferredGame == null && DatePicker.SelectedItem is DateWraper selectedDate)
            {
                preferredGame = GameIdentifiersFiltered.FirstOrDefault(game =>
                    game.TimeStart.Year == selectedDate.Date.Year &&
                    game.TimeStart.Month == selectedDate.Date.Month &&
                    game.TimeStart.Day == selectedDate.Date.Day);
            }

            if (preferredGame != null)
            {
                GamePicker.SelectedItem = preferredGame;
                return;
            }

            GamePicker.SelectedIndex = 0;
            //OnPickerSelectedIndexChanged(sender, e);
        }

        private async void OnPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_showSelectors)
                return;

            var picker = sender as Picker;
            if (picker.SelectedIndex != -1)
            {
                Game selectedGame = GameIdentifiersFiltered[picker.SelectedIndex];
                CurrentGame = selectedGame;
                _currentSelectedGameId = selectedGame.Id;
                _currentSelectedDate = selectedGame.TimeStart.Date;
                if (ShowDataRoutingHelper.ShouldUseKeyboardData(selectedGame.Config))
                {
                    await OpenKeyboardDataPageAsync(selectedGame.Id);
                    return;
                }

                await LoadStatesToGrid(selectedGame.Id);                
            }
            
        }

        private async Task OpenKeyboardDataPageAsync(Guid gameId)
        {
            ShowDataXamlKeyboard keyboardPage = new(gameId, _showSelectors, _sortNewestFirst, _isTeacher, _isTeacher ? _currentUser : _dataUser)
            {
                BindingContext = BindingContext
            };

            if (Navigation?.NavigationStack?.Count > 0)
            {
                await Navigation.PushAsync(keyboardPage);
                return;
            }

            Application.Current.MainPage = new NavigationPage(keyboardPage);
        }

        private void ApplySort()
        {
            GameIdentifiers = (_sortNewestFirst
                ? GameIdentifiers.OrderByDescending(game => game.TimeStart)
                : GameIdentifiers.OrderBy(game => game.TimeStart))
                .ToList();
        }

        private async Task ToggleSortAsync()
        {
            _sortNewestFirst = !_sortNewestFirst;
            _sortToolbarItem.Text = GetSortToolbarText();

            if (CurrentGame != null)
            {
                _currentSelectedGameId = CurrentGame.Id;
                _currentSelectedDate = CurrentGame.TimeStart.Date;
            }

            if (GameIdentifiers.Count > 0)
            {
                ApplySort();
                if (_showSelectors)
                {
                    LoadDates();
                    LoadGames();
                }
                if (CurrentGame != null)
                    await LoadStatesToGrid(CurrentGame.Id);
            }
        }

        private string GetSortToolbarText() => _sortNewestFirst ? "Newest" : "Oldest";

        private  void OnUserPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            if(UserPicker.SelectedIndex != -1)
            {
               //GamePicker.Items.Clear();
                GameDates.Clear(); 
                //DatePicker.Items.Clear();
                GameIdentifiers.Clear();
                GameIdentifiersFiltered.Clear();

                if (UserPicker.SelectedIndex == 0)
                {
                    _currentUser = ServiceHelper.GetService<CurrentUserSession>().ActiveUser;
                    ShowData(null);
                    return;
                }
                var selectedUser = _usersDataTable.Rows[UserPicker.SelectedIndex-1];
                Guid userId = Guid.Parse(selectedUser["Id"].ToString());
                _currentUser = new Maui.Data.SQLite.User { Id = userId, Name = selectedUser["Name"].ToString() };
                ShowData(null);
            }
        }

        private void ClearDisplayedData()
        {
            CurrentGame = null;
            _currentSelectedGameId = null;
            _currentSelectedDate = null;
            GameIdentifiers.Clear();
            GameIdentifiersFiltered.Clear();
            GameDates.Clear();
            DatePicker.ItemsSource = null;
            GamePicker.ItemsSource = null;
            StateList.ItemsSource = null;
        }

        public async Task<string> SyncCurrentGameAsync()
        {
            if (_isTeacher)
                throw new InvalidOperationException("Teacher view cannot sync local data.");

            var activeUser = ServiceHelper.GetService<CurrentUserSession>().ActiveUser;
            if (activeUser == null)
                throw new InvalidOperationException("No active user for sync.");

            await GestureSample.Maui.Data.SupaBase.SupabaseService.SyncUserDataAsync(activeUser);

            if (CurrentGame != null)
            {
                CurrentGame.WasSynced = true;
                await _gameRepository.UpdateAsync(CurrentGame);
            }

            return "Data synced with Supabase.";
        }

        private async Task NavigateBackAsync()
        {
            if (!_showSelectors)
            {
                await NavigateToChooserAsync(CurrentGame?.Id);
                return;
            }

            if (Navigation?.NavigationStack?.Count > 1)
            {
                await Navigation.PopAsync();
                return;
            }

            Application.Current.MainPage = new NavigationPage(new MainPage("Control Categories", null));
        }

        private async Task NavigateToChooserAsync(Guid? gameId)
        {
            Page chooserPage = ShowDataRoutingHelper.CreateChooserPage(gameId, _isTeacher);
            if (Navigation?.NavigationStack?.Count > 0)
            {
                Navigation.InsertPageBefore(chooserPage, this);
                await Navigation.PopAsync();
                return;
            }

            Application.Current.MainPage = new NavigationPage(chooserPage);
        }
    }
}
