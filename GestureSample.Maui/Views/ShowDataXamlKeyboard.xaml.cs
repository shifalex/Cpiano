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

        private ObservableCollection<DateWraper> GameDates { get; set; } = new();
        private List<Game> GameIdentifiers { get; set; } = new();
        private Game CurrentGame { get; set; } = null;
        private ObservableCollection<Game> GameIdentifiersFiltered { get; set; } = new();
        public ShowDataXamlKeyboard(Guid? gameId = null)
        {
            InitializeComponent();
            _gameRepository = ServiceHelper.GetService<GameRepository>();
            _keyboardQuestionRepository = ServiceHelper.GetService<KeyboardQuestionRepository>();
            _keyEventRepository = ServiceHelper.GetService<KeyEventRepository>();
            ShowData(gameId);
        }

        public async void ShowData(Guid? gameId = null)
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
                await LoadStatesToGrid((Guid)gameId);
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

        private async Task LoadStatesToGrid(Guid? selectedIdentifier)
        {
            /*for (int i = 0; i < GameIdentifiers.Count; i++)
                if (selectedIdentifier != null && GameIdentifiers[i].Id.Equals(selectedIdentifier))
                    CurrentGame = GameIdentifiers[i];*/

            List<KeyboardQuestion> questionList =new();
            List<KeyEvent> gamePresses = new();


            if (selectedIdentifier != null)
            {
                questionList = await _keyboardQuestionRepository.GetKeyboardQuestionByQueryAsync(selectedIdentifier);
                gamePresses = await _keyEventRepository.GetKeyEventsByQueryAsync((Guid)selectedIdentifier);
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
                int displayIndex = 0;
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

                        q.RowBackgroundColor = displayIndex % 2 == 0 ? Colors.White : Colors.LightGray;
                        displayIndex++;

                        mainItems.Add(new()
                        {
                            Question = q,
                            SubItems = attemptEvents,
                            CombinedSubItems = combinedEvents
                        });
                    }
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
                CurrentGame = GameIdentifiersFiltered[picker.SelectedIndex];
                await LoadStatesToGrid(CurrentGame.Id);
            }

        }
        private void OnToggleSubgridClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is MainItem item)
            {
                item.IsSubCollectionVisible = !item.IsSubCollectionVisible;
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
                keyboardConfig));
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
                keyboardConfig));
        }

        private async Task OpenReplayPageAsync(Page replayPage)
        {
            if (Navigation?.NavigationStack?.Count > 0)
            {
                await Navigation.PushAsync(replayPage);
                return;
            }

            Application.Current.MainPage = new NavigationPage(replayPage);
        }

        private async void OnSyncClicked(object sender, EventArgs e)
        {
            try
            {
                var activeUser = ServiceHelper.GetService<CurrentUserSession>().ActiveUser;
                if (activeUser == null)
                {
                    await DisplayAlert("Sync", "No active user for sync.", "OK");
                    return;
                }

                await GestureSample.Maui.Data.SupaBase.SupabaseService.SyncUserDataAsync(activeUser);
                await DisplayAlert("Sync", "Keyboard tables synced with Supabase.", "OK");

                if (CurrentGame != null)
                    await LoadStatesToGrid(CurrentGame.Id);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Sync Error", ex.Message, "OK");
            }
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

        private bool _isSubCollectionVisible;
        public bool IsSubCollectionVisible
        {
            get => _isSubCollectionVisible;
            set
            {
                _isSubCollectionVisible = value;
                SubGridHeight = value ? 260 : 0;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ButtonText)); // Notify UI to update ButtonText
            }
        }

        private double _subGridHeight = 0;
        public double SubGridHeight
        {
            get => _subGridHeight;
            set
            {
                _subGridHeight = value;
                OnPropertyChanged();
            }
        }

        public string ButtonText => IsSubCollectionVisible ? "Hide Events" : "Show Events";

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
