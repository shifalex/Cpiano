using GestureSample.Maui.Data;
using GestureSample.Maui.Data.SQLite;
using GestureSample.Maui.Handlers;
using Microsoft.Maui.Controls.Shapes;
using System.Collections.ObjectModel;

namespace GestureSample.Views
{
    public class ShowDataChooserPage : ContentPage
    {
        private const string FilterFontFamily = "Georgia";
        private sealed class DateWraper
        {
            public DateTime Date { get; set; }
            public DateWraper(DateTime d) { Date = d; }
            public override string ToString() => Date.ToShortDateString();
        }

        private readonly bool _forTeacher;
        private readonly Guid? _selectedGameId;
        private readonly GameRepository _gameRepository;
        private readonly ObservableCollection<DateWraper> _gameDates = new();
        private readonly ObservableCollection<Game> _filteredGames = new();
        private readonly Picker _datePicker;
        private readonly Picker _gamePicker;
        private readonly ContentView _detailHost;
        private List<Game> _games = new();
        private bool _suppressSelectionNavigation;
        private Page? _activeDetailPage;
        private bool _hasLoadedGames;
        private Guid? _currentSelectedGameId;

        public ShowDataChooserPage(bool forTeacher = false, Guid? selectedGameId = null)
        {
            _forTeacher = forTeacher;
            _selectedGameId = selectedGameId;
            _currentSelectedGameId = selectedGameId;
            _gameRepository = ServiceHelper.GetService<GameRepository>();

            Title = "Games";
            BackgroundColor = Color.FromArgb("#FBF8FE");

            _datePicker = new Picker
            {
                Title = "Pick a day",
                FontFamily = FilterFontFamily,
                FontSize = 16,
                TextColor = Color.FromArgb("#342048"),
                TitleColor = Color.FromArgb("#8B7D9C"),
                BackgroundColor = Colors.Transparent
            };
            _datePicker.SelectedIndexChanged += OnDatePickerSelectedIndexChanged;

            _gamePicker = new Picker
            {
                Title = "Pick a session",
                FontFamily = FilterFontFamily,
                FontSize = 16,
                TextColor = Color.FromArgb("#342048"),
                TitleColor = Color.FromArgb("#8B7D9C"),
                BackgroundColor = Colors.Transparent
            };
            _gamePicker.SelectedIndexChanged += OnGamePickerSelectedIndexChanged;

            _detailHost = new ContentView
            {
                Margin = new Thickness(0, 8, 0, 0),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            ToolbarItems.Add(new ToolbarItem
            {
                Text = "Back",
                Command = new Command(async () => await NavigateBackAsync())
            });

            VerticalStackLayout filterStack = new()
            {
                Spacing = 8,
                Children =
                {
                    new Label
                    {
                        Text = "Choose a game",
                        FontSize = 22,
                        FontFamily = FilterFontFamily,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#322042"),
                        HorizontalTextAlignment = TextAlignment.Start
                    },
                    new Label
                    {
                        Text = "Pick the day above, then the exact session below.",
                        FontSize = 12,
                        TextColor = Color.FromArgb("#756781")
                    },
                    CreatePickerCard("DATE", _datePicker),
                    CreatePickerCard("GAME", _gamePicker)
                }
            };

            Grid rootGrid = new()
            {
                Padding = new Thickness(10, 10, 10, 0),
                RowSpacing = 0,
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Star }
                }
            };

            Border filterCard = new()
            {
                Stroke = Color.FromArgb("#E3D8F0"),
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(20) },
                Background = new SolidColorBrush(Colors.White),
                Padding = new Thickness(14, 14, 14, 12),
                Shadow = new Shadow
                {
                    Brush = new SolidColorBrush(Color.FromArgb("#15000000")),
                    Offset = new Point(0, 6),
                    Radius = 14,
                    Opacity = 0.5f
                },
                Content = filterStack
            };

            rootGrid.Add(filterCard);
            Grid.SetRow(filterCard, 0);
            rootGrid.Add(_detailHost);
            Grid.SetRow(_detailHost, 1);
            Content = rootGrid;
        }

        private static View CreatePickerCard(string title, Picker picker)
        {
            return new Border
            {
                Stroke = Color.FromArgb("#D9CBEA"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(16) },
                Background = new SolidColorBrush(Color.FromArgb("#FFFDFE")),
                Padding = new Thickness(12, 7, 12, 5),
                Content = new VerticalStackLayout
                {
                    Spacing = 1,
                    Children =
                    {
                        new Label
                        {
                            Text = title,
                            FontSize = 10,
                            FontFamily = FilterFontFamily,
                            FontAttributes = FontAttributes.Bold,
                            CharacterSpacing = 1.5,
                            TextColor = Color.FromArgb("#8A789B")
                        },
                        picker
                    }
                }
            };
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_hasLoadedGames)
                return;

            await LoadGamesAsync();
            _hasLoadedGames = true;
        }

        private async Task LoadGamesAsync()
        {
            Guid currentUserId = ServiceHelper.GetService<CurrentUserSession>().ActiveUser.Id;
            _games = await _gameRepository.GetAllByUserAsync(currentUserId);

            if (_games == null || _games.Count == 0)
            {
                await NavigateBackAsync();
                return;
            }

            _games.Reverse();
            RebuildDates();
            SelectInitialGame();
        }

        private void RebuildDates()
        {
            _gameDates.Clear();
            DateTime? lastDate = null;

            foreach (Game game in _games)
            {
                DateTime currentDate = game.TimeStart.Date;
                if (lastDate == currentDate)
                    continue;

                _gameDates.Add(new DateWraper(currentDate));
                lastDate = currentDate;
            }

            _datePicker.ItemsSource = _gameDates;
        }

        private void SelectInitialGame()
        {
            Guid? preferredGameId = _currentSelectedGameId ?? _selectedGameId;
            Game? selectedGame = preferredGameId.HasValue
                ? _games.FirstOrDefault(game => game.Id == preferredGameId.Value)
                : _games.FirstOrDefault();

            if (selectedGame == null)
                return;

            DateWraper? selectedDate = _gameDates.FirstOrDefault(item => item.Date.Date == selectedGame.TimeStart.Date);
            if (selectedDate == null)
                return;

            _suppressSelectionNavigation = true;
            _datePicker.SelectedItem = selectedDate;
            LoadGamesForDate(selectedDate.Date);
            _gamePicker.SelectedItem = _filteredGames.FirstOrDefault(game => game.Id == selectedGame.Id);
            _suppressSelectionNavigation = false;
            _ = ShowSelectedGameAsync();
        }

        private void OnDatePickerSelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_datePicker.SelectedItem is not DateWraper selectedDate)
                return;

            LoadGamesForDate(selectedDate.Date);
        }

        private void LoadGamesForDate(DateTime date)
        {
            _filteredGames.Clear();
            foreach (Game game in _games)
            {
                if (game.TimeStart.Date == date.Date)
                    _filteredGames.Add(game);
            }

            _gamePicker.ItemsSource = _filteredGames;
            if (_filteredGames.Count == 0)
                return;

            Game? preferredGame = _currentSelectedGameId.HasValue
                ? _filteredGames.FirstOrDefault(game => game.Id == _currentSelectedGameId.Value)
                : null;

            if (preferredGame != null)
            {
                _gamePicker.SelectedItem = preferredGame;
                return;
            }

            _gamePicker.SelectedIndex = 0;
        }

        private async void OnGamePickerSelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_suppressSelectionNavigation)
                return;

            if (_gamePicker.SelectedItem is Game selectedGame)
                _currentSelectedGameId = selectedGame.Id;

            await ShowSelectedGameAsync();
        }

        private async Task ShowSelectedGameAsync()
        {
            if (_gamePicker.SelectedItem is not Game selectedGame)
            {
                _activeDetailPage = null;
                _detailHost.Content = null;
                return;
            }

            _currentSelectedGameId = selectedGame.Id;

            Page detailPage = ShowDataRoutingHelper.CreatePageForGame(selectedGame, _forTeacher, false);
            if (detailPage is ContentPage contentPage && contentPage.Content is View detailView)
            {
                contentPage.Content = null;
                _activeDetailPage = detailPage;
                _detailHost.Content = detailView;
                return;
            }

            _activeDetailPage = null;
            _detailHost.Content = new Label
            {
                Text = "This data view cannot be shown inline.",
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 20)
            };

            await Task.CompletedTask;
        }

        private async Task NavigateBackAsync()
        {
            if (Navigation?.NavigationStack?.Count > 1)
            {
                await Navigation.PopAsync();
                return;
            }

            Application.Current.MainPage = new NavigationPage(new MainPage("Control Categories", null));
        }
    }
}
