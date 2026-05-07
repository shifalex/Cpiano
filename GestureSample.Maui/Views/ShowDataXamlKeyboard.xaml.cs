using GestureSample.Maui.Data;
using GestureSample.Maui.Data.SQLite;
using GestureSample.Maui.Handlers;
using GestureSample.Maui.Models;
using GestureSample.Maui;
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
        private readonly TimerChangeEventRepository _timerChangeEventRepository;
        private readonly BackgroundSyncService _backgroundSyncService;
        private readonly SyncToolbarStatusController _syncToolbarStatusController;

        private ObservableCollection<DateWraper> GameDates { get; set; } = new();
        private List<Game> GameIdentifiers { get; set; } = new();
        private Game CurrentGame { get; set; } = null;
        private ObservableCollection<Game> GameIdentifiersFiltered { get; set; } = new();
        private readonly bool _showSelectors;
        private readonly ToolbarItem _dataToolbarItem;
        private readonly ToolbarItem _gamesToolbarItem;
        private readonly ToolbarItem _sortToolbarItem;
        private Guid? _currentSelectedGameId;
        private DateTime? _currentSelectedDate;
        private bool _sortNewestFirst = true;
        public ShowDataXamlKeyboard(Guid? gameId = null, bool showSelectors = true, bool sortNewestFirst = true)
        {
            InitializeComponent();
            Title = "Keyboard Data";
            _showSelectors = showSelectors;
            _sortNewestFirst = sortNewestFirst;
            _currentSelectedGameId = gameId;
            _gameRepository = ServiceHelper.GetService<GameRepository>();
            _keyboardQuestionRepository = ServiceHelper.GetService<KeyboardQuestionRepository>();
            _keyEventRepository = ServiceHelper.GetService<KeyEventRepository>();
            _timerChangeEventRepository = ServiceHelper.GetService<TimerChangeEventRepository>();
            _backgroundSyncService = ServiceHelper.GetService<BackgroundSyncService>();
            _syncToolbarStatusController = new SyncToolbarStatusController(this, _backgroundSyncService);
            _dataToolbarItem = new ToolbarItem
            {
                Text = "Data",
                Priority = 0,
                Order = ToolbarItemOrder.Primary,
                Command = new Command(async () => await NavigateToDataPageAsync())
            };
            ToolbarItems.Add(_dataToolbarItem);
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

        public async void ShowData(Guid? gameId = null)
        {
                gameId ??= _currentSelectedGameId;
                GameIdentifiers = await _gameRepository.GetAllByUserAsync(ServiceHelper.GetService<CurrentUserSession>().ActiveUser.Id);
                await _timerChangeEventRepository.EnsureInitialEventsAsync(GameIdentifiers);
                if (GameIdentifiers == null || GameIdentifiers.Count == 0)
                {
                    Console.WriteLine("no games played");
                    // If not first user, just go back to whoever called this page (e.g. SwitchUserPage)
                    await Navigation.PopAsync();
                }
            
            if (gameId == null && GameIdentifiers.Count > 0)
            {
                DateTime today = DateTime.Today;
                CurrentGame = GameIdentifiers.FirstOrDefault(game => game.TimeStart.Date == today)
                    ?? GameIdentifiers[0];
            }
            for (int i = 0; i < GameIdentifiers.Count; i++)
            {
                if (gameId != null && GameIdentifiers[i].Id.Equals(gameId)) CurrentGame = GameIdentifiers[i];
                GameIdentifiers[i].index = i + 1;
            }

            if (CurrentGame != null)
            {
                _currentSelectedGameId = CurrentGame.Id;
                _currentSelectedDate = CurrentGame.TimeStart.Date;
            }

            ApplySort();

            if (_showSelectors)
            {
                await LoadDates();
                LoadGames();
            }

            if (gameId != null)
                await LoadStatesToGrid((Guid)gameId);
        }

        private Task LoadDates()
        {
            GameDates.Clear();

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
                    GameDates.Add(new DateWraper(game.TimeStart.Date));
                }

            }

            DatePicker.ItemsSource = GameDates;
            if (GameDates.Count == 0)
                return Task.CompletedTask;

            DateWraper? preferredDate = _currentSelectedDate.HasValue
                ? GameDates.FirstOrDefault(item => item.Date.Date == _currentSelectedDate.Value.Date)
                : null;

            if (preferredDate != null)
            {
                DatePicker.SelectedItem = preferredDate;
                return Task.CompletedTask;
            }

            DatePicker.SelectedIndex = 0;
            return Task.CompletedTask;
        }

        private async Task LoadStatesToGrid(Guid? selectedIdentifier)
        {
            /*for (int i = 0; i < GameIdentifiers.Count; i++)
                if (selectedIdentifier != null && GameIdentifiers[i].Id.Equals(selectedIdentifier))
                    CurrentGame = GameIdentifiers[i];*/

            List<KeyboardQuestion> questionList =new();
            List<KeyEvent> gamePresses = new();
            List<TimerChangeEvent> timerEvents = new();


            if (selectedIdentifier != null)
            {
                questionList = await _keyboardQuestionRepository.GetKeyboardQuestionByQueryAsync(selectedIdentifier);
                gamePresses = await _keyEventRepository.GetKeyEventsByQueryAsync((Guid)selectedIdentifier);
                timerEvents = await _timerChangeEventRepository.GetByGameAsync((Guid)selectedIdentifier);
            }
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
                foreach (var questionGroup in questionList
                    .GroupBy(item => item.QuestionNumber)
                    .OrderBy(group => group.Key))
                {
                    List<KeyboardQuestion> orderedAttempts = questionGroup
                        .OrderBy(item => item.AttemptNumber == 0 ? int.MaxValue : item.AttemptNumber)
                        .ThenBy(item => item.QuestionID)
                        .ToList();

                    List<KeyEvent> combinedEvents = gamePresses
                        .Where(item => item.QuestionNumber == questionGroup.Key)
                        .OrderBy(item => item.EventTime)
                        .ThenBy(item => item.id)
                        .ToList();

                    foreach (KeyboardQuestion q in orderedAttempts)
                    {
                        List<KeyEvent> attemptEvents = ResolveAttemptEvents(q, orderedAttempts, combinedEvents);

                        mainItems.Add(new()
                        {
                            Question = q,
                            SubItems = attemptEvents,
                            CombinedSubItems = combinedEvents,
                            TimerEvents = ResolveAttemptTimerEvents(q, orderedAttempts, timerEvents),
                            CombinedTimerEvents = timerEvents
                                .Where(item => item.QuestionNumber == questionGroup.Key)
                                .OrderBy(item => item.EventTime)
                                .ThenBy(item => item.Id)
                                .ToList(),
                            CombinedFinalKeyboard = orderedAttempts
                                .LastOrDefault(item => item.HasSubmittedKeyboard)?
                                .SubmittedKeyboard?
                                .ToArray(),
                            TimerRegimeText = ResolveTimerRegimeText(q, attemptEvents, timerEvents)
                        });
                    }
                }

                mainItems = (_sortNewestFirst
                    ? mainItems
                        .OrderByDescending(item => item.Question?.QuestionNumber ?? 0)
                        .ThenByDescending(item => item.Question == null ? int.MinValue : item.Question.AttemptNumber == 0 ? int.MaxValue : item.Question.AttemptNumber)
                        .ThenByDescending(item => item.Question?.QuestionID ?? 0)
                    : mainItems
                        .OrderBy(item => item.Question?.QuestionNumber ?? 0)
                        .ThenBy(item => item.Question == null ? int.MaxValue : item.Question.AttemptNumber == 0 ? int.MaxValue : item.Question.AttemptNumber)
                        .ThenBy(item => item.Question?.QuestionID ?? 0))
                    .ToList();

                for (int i = 0; i < mainItems.Count; i++)
                    mainItems[i].Question.RowBackgroundColor = i % 2 == 0 ? Colors.White : Colors.LightGray;
            }
            Questions.ItemsSource = null;
            Questions.ItemsSource = new ObservableCollection<MainItem>(mainItems);
            if (mainItems.Count > 0)
                Questions.ScrollTo(0, position: ScrollToPosition.Start, animate: false);
            //StateList.ItemsSource = gamePresses;
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
        }

        private async void OnPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_showSelectors)
                return;

            var picker = sender as Picker;
            if (picker.SelectedIndex != -1)
            {
                CurrentGame = GameIdentifiersFiltered[picker.SelectedIndex];
                _currentSelectedGameId = CurrentGame.Id;
                _currentSelectedDate = CurrentGame.TimeStart.Date;
                if (ShowDataRoutingHelper.ShouldUseKeyboardData(CurrentGame.Config))
                {
                    await LoadStatesToGrid(CurrentGame.Id);
                    return;
                }

                await NavigateToDataPageAsync(CurrentGame.Id);
            }

        }
        private async void OnReplayClicked(object sender, EventArgs e)
        {
            if (sender is not Button button || button.BindingContext is not MainItem item)
                return;

            KeyboardConfig keyboardConfig = CurrentGame?.Config?.KeyboardConfig ?? new KeyboardConfig();
            await OpenReplayPageAsync(new KeyboardReplayPage(
                item.ReplayTitle,
                item.SubItems,
                item.Question,
                keyboardConfig,
                item.Question?.SubmittedKeyboard,
                item.TimerRegimeText,
                item.TimerEvents));
        }

        private async void OnReplayAllClicked(object sender, EventArgs e)
        {
            if (sender is not Button button || button.BindingContext is not MainItem item || !item.HasCombinedReplay)
                return;

            KeyboardConfig keyboardConfig = CurrentGame?.Config?.KeyboardConfig ?? new KeyboardConfig();
            await OpenReplayPageAsync(new KeyboardReplayPage(
                item.CombinedReplayTitle,
                item.CombinedSubItems,
                item.Question,
                keyboardConfig,
                item.CombinedFinalKeyboard,
                item.TimerRegimeText,
                item.CombinedTimerEvents));
        }

        private string ResolveTimerRegimeText(KeyboardQuestion question, List<KeyEvent> attemptEvents, List<TimerChangeEvent> timerEvents)
        {
            DateTime referenceTime = question.SubmittedTime
                ?? attemptEvents
                    .Where(item => item.EventType == 2)
                    .OrderBy(item => item.EventTime)
                    .Select(item => (DateTime?)item.EventTime)
                    .FirstOrDefault()
                ?? question.Time;

            TimerChangeEvent? matchingEvent = timerEvents
                .Where(item => item.EventTime <= referenceTime)
                .OrderBy(item => item.EventTime)
                .ThenBy(item => item.Id)
                .LastOrDefault();

            if (matchingEvent != null)
                return FormatTimerSetting(matchingEvent.NewSetting);

            int? configuredDefault = CurrentGame?.Config?.KeyboardConfig?.SecondsPressingToAnswer;
            if (configuredDefault.HasValue)
                return FormatTimerSetting(configuredDefault.Value);

            return "Unknown";
        }

        private List<TimerChangeEvent> ResolveAttemptTimerEvents(KeyboardQuestion question, List<KeyboardQuestion> orderedAttempts, List<TimerChangeEvent> timerEvents)
        {
            if (question == null || timerEvents == null || timerEvents.Count == 0)
                return new List<TimerChangeEvent>();

            int attemptIndex = -1;
            for (int i = 0; i < orderedAttempts.Count; i++)
            {
                if (ReferenceEquals(orderedAttempts[i], question) || orderedAttempts[i].QuestionID == question.QuestionID)
                {
                    attemptIndex = i;
                    break;
                }
            }

            if (attemptIndex < 0)
                attemptIndex = 0;

            DateTime lowerBound = attemptIndex > 0
                ? orderedAttempts[attemptIndex - 1].SubmittedTime ?? orderedAttempts[attemptIndex - 1].Time
                : question.Time.AddMilliseconds(-1);
            DateTime upperBound = question.SubmittedTime ?? question.Time;

            return timerEvents
                .Where(item => item.QuestionNumber == question.QuestionNumber &&
                               item.EventTime > lowerBound &&
                               item.EventTime <= upperBound.AddMilliseconds(10))
                .OrderBy(item => item.EventTime)
                .ThenBy(item => item.Id)
                .ToList();
        }

        private static string FormatTimerSetting(int setting)
        {
            if (setting == 0)
                return "Off";

            int seconds = Math.Abs(setting);
            string mode = setting < 0 ? "Whole Answer" : "After Last Key";
            return $"{seconds}s • {mode}";
        }

        private async Task OpenReplayPageAsync(Page replayPage)
        {
            Page? visiblePage = GetVisiblePage(
                Application.Current?.Windows.FirstOrDefault()?.Page ??
                Application.Current?.MainPage);

            INavigation? navigation = visiblePage?.Navigation ?? Navigation;
            if (navigation == null)
                return;

            AddModalCloseButton(replayPage);
            await navigation.PushModalAsync(new NavigationPage(replayPage));
        }

        private static Page? GetVisiblePage(Page? page)
        {
            if (page == null)
                return null;

            if (page.Navigation?.ModalStack?.Count > 0)
                return GetVisiblePage(page.Navigation.ModalStack[^1]);

            if (page is NavigationPage navigationPage)
                return GetVisiblePage(navigationPage.CurrentPage);

            if (page is TabbedPage tabbedPage)
                return GetVisiblePage(tabbedPage.CurrentPage);

            if (page is FlyoutPage flyoutPage)
                return GetVisiblePage(flyoutPage.Detail);

            return page;
        }

        public async Task<string> SyncCurrentGameAsync()
        {
            var activeUser = ServiceHelper.GetService<CurrentUserSession>().ActiveUser;
            if (activeUser == null)
                throw new InvalidOperationException("No active user for sync.");

            await GestureSample.Maui.Data.SupaBase.SupabaseService.SyncUserDataAsync(activeUser);
            await RefreshCurrentGameFromSupabaseAsync();

            if (CurrentGame != null)
                await LoadStatesToGrid(CurrentGame.Id);

            return "Keyboard tables synced with Supabase.";
        }

        private async Task RefreshCurrentGameFromSupabaseAsync()
        {
            if (CurrentGame == null)
                return;

            List<KeyboardQuestion> remoteQuestions = await GestureSample.Maui.Data.SupaBase.SupabaseService.GetKeyboardQuestionByQueryAsync(CurrentGame.Id);
            List<KeyEvent> remoteKeyEvents = await GestureSample.Maui.Data.SupaBase.SupabaseService.GetKeyEventsByQueryAsync(CurrentGame.Id);

            await _keyboardQuestionRepository.ReplaceForGameAsync(CurrentGame.Id.ToString(), remoteQuestions);
            await _keyEventRepository.ReplaceForGameAsync(CurrentGame.Id.ToString(), remoteKeyEvents);
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
                    await LoadDates();
                    LoadGames();
                }
                if (CurrentGame != null)
                    await LoadStatesToGrid(CurrentGame.Id);
            }
        }

        private string GetSortToolbarText() => _sortNewestFirst ? "Newest" : "Oldest";

        private static void AddModalCloseButton(Page replayPage)
        {
            if (replayPage.ToolbarItems.Any(item => item.Text == "Close"))
                return;

            replayPage.ToolbarItems.Add(new ToolbarItem
            {
                Text = "Close",
                Command = new Command(async () => await replayPage.Navigation.PopModalAsync())
            });
        }

        private async Task NavigateToDataPageAsync(Guid? gameId = null)
        {
            if (gameId == null &&
                Navigation?.NavigationStack?.Count > 1 &&
                Navigation.NavigationStack[Navigation.NavigationStack.Count - 2] is ShowDataXaml)
            {
                await Navigation.PopAsync();
                return;
            }

            ShowDataXaml dataPage = new(false, gameId, false);
            if (Navigation?.NavigationStack?.Count > 0)
            {
                Navigation.InsertPageBefore(dataPage, this);
                await Navigation.PopAsync();
                return;
            }

            Application.Current.MainPage = new NavigationPage(dataPage);
        }

        private async Task NavigateToChooserAsync(Guid? gameId)
        {
            Page chooserPage = ShowDataRoutingHelper.CreateChooserPage(gameId);
            if (Navigation?.NavigationStack?.Count > 0)
            {
                Navigation.InsertPageBefore(chooserPage, this);
                await Navigation.PopAsync();
                return;
            }

            Application.Current.MainPage = new NavigationPage(chooserPage);
        }

        private static List<KeyEvent> ResolveAttemptEvents(
            KeyboardQuestion question,
            IReadOnlyList<KeyboardQuestion> orderedAttempts,
            List<KeyEvent> combinedEvents)
        {
            if (combinedEvents.Count == 0)
                return new List<KeyEvent>();

            bool hasAttemptScopedEvents = combinedEvents.Any(item => item.AttemptNumber > 0);
            if (hasAttemptScopedEvents && question.AttemptNumber > 0)
            {
                List<KeyEvent> scopedEvents = combinedEvents
                    .Where(item => item.AttemptNumber == question.AttemptNumber)
                    .ToList();

                if (HasReplayStrokeData(scopedEvents))
                    return scopedEvents;
            }

            int attemptIndex = -1;
            for (int i = 0; i < orderedAttempts.Count; i++)
            {
                if (ReferenceEquals(orderedAttempts[i], question) || orderedAttempts[i].QuestionID == question.QuestionID)
                {
                    attemptIndex = i;
                    break;
                }
            }

            if (attemptIndex < 0)
                attemptIndex = 0;

            DateTime lowerBound = attemptIndex > 0
                ? orderedAttempts[attemptIndex - 1].SubmittedTime ?? orderedAttempts[attemptIndex - 1].Time
                : question.Time;
            DateTime upperBound = question.SubmittedTime ?? question.Time;

            List<KeyEvent> timeSlicedEvents = combinedEvents
                .Where(item => item.EventTime > lowerBound && item.EventTime <= upperBound.AddMilliseconds(10))
                .ToList();

            if (HasReplayStrokeData(timeSlicedEvents))
                return timeSlicedEvents;

            return hasAttemptScopedEvents && question.AttemptNumber > 0
                ? combinedEvents.Where(item => item.AttemptNumber == question.AttemptNumber).ToList()
                : combinedEvents;
        }

        private static bool HasReplayStrokeData(List<KeyEvent> events)
        {
            return events.Any(item => item.EventType == 0 || item.EventType == 1 || item.EventType == 3);
        }
    }

    public class MainItem : INotifyPropertyChanged
    {
        public KeyboardQuestion Question { get; set; }
        public List<KeyEvent> SubItems { get; set; }
        public List<KeyEvent> CombinedSubItems { get; set; }
        public List<TimerChangeEvent> TimerEvents { get; set; } = new();
        public List<TimerChangeEvent> CombinedTimerEvents { get; set; } = new();
        public bool[] CombinedFinalKeyboard { get; set; }
        public string TimerRegimeText { get; set; } = "Unknown";
        public bool HasReplay => SubItems != null && SubItems.Count > 0;
        public bool HasCombinedReplay => CombinedSubItems != null && CombinedSubItems.Count > (SubItems?.Count ?? 0);
        public bool HasTimingSummary => !string.IsNullOrWhiteSpace(TimingSummaryText);
        public string ReplayTitle => Question == null ? "Replay" : $"Question {Question.QuestionNumber} - {Question.AttemptText}";
        public string CombinedReplayTitle => Question == null ? "Replay" : $"Question {Question.QuestionNumber} - All Trials";

        public string TimingSummaryText
        {
            get
            {
                if (Question == null)
                    return string.Empty;

                List<string> parts = new();

                DateTime? replayStart = GetReplayStartTime();
                if (replayStart.HasValue)
                    parts.Add($"Start: {FormatDuration(replayStart.Value - Question.Time)}");

                DateTime? answerTime = GetAnswerTime();
                if (answerTime.HasValue)
                    parts.Add($"Answer: {FormatDuration(answerTime.Value - Question.Time)}");

                return string.Join("  |  ", parts);
            }
        }

        private DateTime? GetReplayStartTime()
        {
            if (SubItems == null || SubItems.Count == 0)
                return null;

            KeyEvent firstTouch = SubItems
                .Where(item => item.EventType == 1)
                .OrderBy(item => item.EventTime)
                .FirstOrDefault();

            firstTouch ??= SubItems
                .Where(item => item.EventType != 2 && item.EventType != 3)
                .OrderBy(item => item.EventTime)
                .FirstOrDefault();

            firstTouch ??= SubItems.OrderBy(item => item.EventTime).FirstOrDefault();
            return firstTouch?.EventTime;
        }

        private DateTime? GetAnswerTime()
        {
            if (Question?.SubmittedTime != null)
                return Question.SubmittedTime.Value;

            if (SubItems == null || SubItems.Count == 0)
                return null;

            KeyEvent checkEvent = SubItems
                .Where(item => item.EventType == 2)
                .OrderBy(item => item.EventTime)
                .FirstOrDefault();

            return checkEvent?.EventTime;
        }

        private static string FormatDuration(TimeSpan duration)
        {
            if (duration < TimeSpan.Zero)
                duration = TimeSpan.Zero;

            if (duration.TotalMinutes >= 1)
                return duration.ToString(@"m\:ss\.fff");

            return $"{duration.TotalSeconds:0.000}s";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
