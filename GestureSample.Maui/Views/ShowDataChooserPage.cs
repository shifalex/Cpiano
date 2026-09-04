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
        private readonly BackgroundSyncService _backgroundSyncService;
        private readonly SyncToolbarStatusController _syncToolbarStatusController;
        private readonly ObservableCollection<DateWraper> _gameDates = new();
        private readonly ObservableCollection<Game> _filteredGames = new();
        private readonly Picker _datePicker;
        private readonly Picker _gamePicker;
        private readonly ContentView _detailHost;
        private readonly Button _sortButton;
        private readonly Label _headerLabel;
        private readonly Grid _pickerCardsHost;
        private readonly Border _datePickerCard;
        private readonly Border _gamePickerCard;
        private readonly Grid _loadingOverlay;
        private List<Game> _games = new();
        private bool _suppressSelectionNavigation;
        private Page? _activeDetailPage;
        private bool _hasLoadedGames;
        private Guid? _currentSelectedGameId;
        private bool _sortNewestFirst = false;
        private bool _skipNextPostSyncRefresh;

        public ShowDataChooserPage(bool forTeacher = false, Guid? selectedGameId = null)
        {
            _forTeacher = forTeacher;
            _selectedGameId = selectedGameId;
            _currentSelectedGameId = selectedGameId;
            _skipNextPostSyncRefresh = selectedGameId.HasValue;
            _gameRepository = ServiceHelper.GetService<GameRepository>();
            _backgroundSyncService = ServiceHelper.GetService<BackgroundSyncService>();
            _syncToolbarStatusController = new SyncToolbarStatusController(this, _backgroundSyncService);

            Title = "Games";
            BackgroundColor = Color.FromArgb("#FBF8FE");

            _datePicker = new Picker
            {
                Title = "Pick a day",
                FontFamily = FilterFontFamily,
                FontSize = 14,
                TextColor = Color.FromArgb("#342048"),
                TitleColor = Color.FromArgb("#8B7D9C"),
                BackgroundColor = Colors.Transparent
            };
            _datePicker.SelectedIndexChanged += OnDatePickerSelectedIndexChanged;

            _gamePicker = new Picker
            {
                Title = "Pick a session",
                FontFamily = FilterFontFamily,
                FontSize = 14,
                TextColor = Color.FromArgb("#342048"),
                TitleColor = Color.FromArgb("#8B7D9C"),
                BackgroundColor = Colors.Transparent
            };
            _gamePicker.SelectedIndexChanged += OnGamePickerSelectedIndexChanged;

            _detailHost = new ContentView
            {
                Margin = new Thickness(0, 8, 0, 0),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                IsClippedToBounds = true
            };

            ImageButton backButton = new()
            {
                Source = new FontImageSource
                {
                    Glyph = "←",
                    FontFamily = "Arial",
                    Size = 24,
                    Color = Color.FromArgb("#342048")
                },
                BackgroundColor = Colors.Transparent,
                WidthRequest = 44,
                HeightRequest = 44,
                MinimumWidthRequest = 44,
                MinimumHeightRequest = 44,
                Padding = 8,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                Command = new Command(async () => await NavigateBackAsync())
            };
            SemanticProperties.SetDescription(backButton, "Back");
            NavigationPage.SetTitleView(this, new HorizontalStackLayout
            {
                HorizontalOptions = LayoutOptions.Start,
                MinimumWidthRequest = 150,
                Spacing = 4,
                Children =
                {
                    backButton,
                    new Label
                    {
                        Text = "Games",
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#342048"),
                        VerticalTextAlignment = TextAlignment.Center
                    }
                }
            });
            _sortButton = new Button
            {
                Text = GetSortButtonText(),
                HorizontalOptions = LayoutOptions.End,
                BackgroundColor = Color.FromArgb("#F1EAFB"),
                TextColor = Color.FromArgb("#5A3C84"),
                FontFamily = FilterFontFamily,
                FontSize = 11,
                CornerRadius = 12,
                MinimumHeightRequest = 30,
                Padding = new Thickness(9, 2)
            };
            _sortButton.Clicked += async (_, _) => await ToggleSortAsync();
            _headerLabel = new Label
            {
                Text = "Games",
                FontSize = 16,
                FontFamily = FilterFontFamily,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#322042"),
                VerticalTextAlignment = TextAlignment.Center
            };

            _datePickerCard = CreatePickerCard("DATE", _datePicker);
            _datePickerCard.MaximumWidthRequest = 170;
            _datePickerCard.HorizontalOptions = LayoutOptions.Start;
            _gamePickerCard = CreatePickerCard("GAME", _gamePicker);
            _gamePickerCard.HorizontalOptions = LayoutOptions.Fill;
            _pickerCardsHost = new Grid
            {
                ColumnSpacing = 8,
                RowSpacing = 6
            };

            VerticalStackLayout filterStack = new()
            {
                Spacing = 4,
                Children =
                {
                    new Grid
                    {
                        ColumnDefinitions =
                        {
                            new ColumnDefinition { Width = GridLength.Star },
                            new ColumnDefinition { Width = GridLength.Auto }
                        },
                        Children =
                        {
                            _headerLabel,
                            _sortButton
                        }
                    },
                    _pickerCardsHost
                }
            };
            UpdatePickerCardsLayout(Width);
            Grid.SetColumn(_sortButton, 1);

            Grid rootGrid = new()
            {
                Padding = new Thickness(10, 6, 10, 0),
                RowSpacing = 0,
                IsClippedToBounds = true,
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
                Padding = new Thickness(10, 8, 10, 8),
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
            _loadingOverlay = CreateLoadingOverlay();
            rootGrid.Add(_loadingOverlay);
            Grid.SetRowSpan(_loadingOverlay, 2);
            Content = rootGrid;
            SizeChanged += OnSizeChanged;
        }

        private static Border CreatePickerCard(string title, Picker picker)
        {
            return new Border
            {
                Stroke = Color.FromArgb("#D9CBEA"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
                Background = new SolidColorBrush(Color.FromArgb("#FFFDFE")),
                Padding = new Thickness(10, 4, 10, 3),
                Content = new VerticalStackLayout
                {
                    Spacing = 0,
                    Children =
                    {
                        new Label
                        {
                            Text = title,
                            FontSize = 9,
                            FontFamily = FilterFontFamily,
                            FontAttributes = FontAttributes.Bold,
                            CharacterSpacing = 1.2,
                            TextColor = Color.FromArgb("#8A789B")
                        },
                        picker
                    }
                }
            };
        }

        private void OnSizeChanged(object? sender, EventArgs e) => UpdatePickerCardsLayout(Width);

        private void UpdatePickerCardsLayout(double availableWidth)
        {
            bool useWideLayout = availableWidth >= 900;
            _pickerCardsHost.Children.Clear();
            _pickerCardsHost.RowDefinitions.Clear();
            _pickerCardsHost.ColumnDefinitions.Clear();

            if (useWideLayout)
            {
                _pickerCardsHost.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                _pickerCardsHost.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                _pickerCardsHost.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                _pickerCardsHost.Add(_datePickerCard);
                Grid.SetColumn(_datePickerCard, 0);
                _pickerCardsHost.Add(_gamePickerCard);
                Grid.SetColumn(_gamePickerCard, 1);
                return;
            }

            _pickerCardsHost.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            _pickerCardsHost.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            _pickerCardsHost.Add(_datePickerCard);
            Grid.SetRow(_datePickerCard, 0);
            _pickerCardsHost.Add(_gamePickerCard);
            Grid.SetRow(_gamePickerCard, 1);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _backgroundSyncService.StateChanged += OnBackgroundSyncStateChanged;
            _syncToolbarStatusController.Attach();
            RefreshBackgroundSyncUi();
            if (_hasLoadedGames)
                return;

            await LoadGamesAsync();
            _hasLoadedGames = true;
        }

        protected override void OnDisappearing()
        {
            _backgroundSyncService.StateChanged -= OnBackgroundSyncStateChanged;
            _syncToolbarStatusController.Detach();
            base.OnDisappearing();
        }

        private async Task LoadGamesAsync()
        {
            _loadingOverlay.IsVisible = true;
            await Task.Yield();
            try
            {
                Guid currentUserId = ServiceHelper.GetService<CurrentUserSession>().ActiveUser.Id;
                _games = await _gameRepository.GetAllByUserAsync(currentUserId);

                if (_games == null || _games.Count == 0)
                {
                    await NavigateBackAsync();
                    return;
                }

                SortGamesForPickers();
                RebuildDates();
                SelectInitialGame();
            }
            finally
            {
                _loadingOverlay.IsVisible = false;
            }
        }

        private static Grid CreateLoadingOverlay()
        {
            Grid overlay = new()
            {
                BackgroundColor = Colors.White.WithAlpha(0.78f),
                ZIndex = 20,
                IsVisible = true
            };
            overlay.Add(new VerticalStackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Spacing = 10,
                Children =
                {
                    new ActivityIndicator { IsRunning = true, Color = Color.FromArgb("#6F4B82") },
                    new Label { Text = "Loading data…", TextColor = Color.FromArgb("#342048"), FontFamily = FilterFontFamily }
                }
            });
            return overlay;
        }

        private void SortGamesForPickers()
        {
            _games = _games
                .OrderByDescending(game => game.TimeStart)
                .ToList();
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
                : null;

            if (selectedGame == null)
            {
                DateTime today = DateTime.Today;
                selectedGame = _games.FirstOrDefault(game => game.TimeStart.Date == today)
                    ?? _games.FirstOrDefault();
            }

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
            if (_suppressSelectionNavigation)
                return;

            if (_datePicker.SelectedItem is not DateWraper selectedDate)
                return;

            LoadGamesForDate(selectedDate.Date);
        }

        private void LoadGamesForDate(DateTime date)
        {
            _filteredGames.Clear();
            foreach (Game game in _games)
            {
                _filteredGames.Add(game);
            }

            _gamePicker.ItemsSource = _filteredGames;
            if (_filteredGames.Count == 0)
                return;

            Game? preferredGame = _currentSelectedGameId.HasValue
                ? _filteredGames.FirstOrDefault(game => game.Id == _currentSelectedGameId.Value)
                : null;

            if (preferredGame == null)
                preferredGame = _filteredGames.FirstOrDefault(game => game.TimeStart.Date == date.Date);

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

            Page detailPage = ShowDataRoutingHelper.CreatePageForGame(selectedGame, _forTeacher, false, _sortNewestFirst);
            if (detailPage is ContentPage contentPage && contentPage.Content is View detailView)
            {
                detailView.HorizontalOptions = LayoutOptions.Fill;
                detailView.VerticalOptions = LayoutOptions.Fill;

                if (detailView is Layout detailLayout)
                    detailLayout.IsClippedToBounds = true;

                contentPage.Content = null;
                _activeDetailPage = detailPage;
                _detailHost.Content = new Grid
                {
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill,
                    IsClippedToBounds = true,
                    Children = { detailView }
                };
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

        private async Task ToggleSortAsync()
        {
            if (_games == null || _games.Count == 0)
                return;

            _sortNewestFirst = !_sortNewestFirst;
            _sortButton.Text = GetSortButtonText();
            await ShowSelectedGameAsync();
        }

        private string GetSortButtonText() => _sortNewestFirst ? "Newest" : "Oldest";

        private void OnBackgroundSyncStateChanged(object? sender, EventArgs e)
        {
            RefreshBackgroundSyncUi();
            if (!_backgroundSyncService.IsSyncing && _hasLoadedGames)
            {
                if (_skipNextPostSyncRefresh)
                {
                    _skipNextPostSyncRefresh = false;
                    return;
                }

                _ = RefreshGamesAfterSyncAsync();
            }
        }

        private void RefreshBackgroundSyncUi()
        {
            _sortButton.IsEnabled = true;
            _datePicker.IsEnabled = true;
            _gamePicker.IsEnabled = true;
        }

        private async Task RefreshGamesAfterSyncAsync()
        {
            await LoadGamesAsync();
        }
    }
}
