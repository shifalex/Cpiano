using GestureSample.Maui.Data;
using GestureSample.Maui.Handlers;
using GestureSample.Maui.Views;
using GestureSample.Maui.Models;
using GestureSample.Views;
using GestureSample.Maui.Data.SQLite;

namespace GestureSample.Maui.Views
{

    public partial class SwitchUserPage : ContentPage
    {
        private sealed class NumericInputOption
        {
            public string Label { get; init; } = string.Empty;
            public NumericInputMode Value { get; init; }
            public override string ToString() => Label;
        }

        private readonly UserRepository _userRepo;
        private readonly UserPreferenceService _userPreferenceService;
        private readonly BackgroundSyncService _backgroundSyncService;
        private readonly SyncToolbarStatusController _syncToolbarStatusController;
        private readonly ToolbarItem _syncToolbarItem;

        public SwitchUserPage()
        {
            InitializeComponent();
            // Retrieve the repository if not injected
            _userRepo = ServiceHelper.GetService<UserRepository>();
            _userPreferenceService = ServiceHelper.GetService<UserPreferenceService>();
            _backgroundSyncService = ServiceHelper.GetService<BackgroundSyncService>();
            _syncToolbarStatusController = new SyncToolbarStatusController(this, _backgroundSyncService);
            ConfigureNumericInputPicker();
            _syncToolbarItem = new ToolbarItem
            {
                Text = "Sync",
                Command = new Command(OnSyncToolbarClicked)
            };
            ToolbarItems.Add(_syncToolbarItem);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            _backgroundSyncService.StateChanged += OnBackgroundSyncStateChanged;
            _syncToolbarStatusController.Attach();
            var users = await _userRepo.GetUsersAsync();
            UsersCollectionView.ItemsSource = users;
            LoadActiveUserPreference();
            RefreshSyncUi();
        }

        protected override void OnDisappearing()
        {
            _backgroundSyncService.StateChanged -= OnBackgroundSyncStateChanged;
            _syncToolbarStatusController.Detach();
            base.OnDisappearing();
        }
              private async void OnUserSelected(object sender, SelectionChangedEventArgs e)
        {
            var selectedUser = e.CurrentSelection?.FirstOrDefault() as User;
            if (selectedUser != null)
            {
                // Set this as the active user

                await ServiceHelper.GetService<CurrentUserSession>().LoadUserAsync(selectedUser.Id);

                selectedUser.LastLoginTime = DateTime.Now;
                await _userRepo.UpdateAsync(selectedUser);
                // Go back to MainPage
                Application.Current.MainPage = new NavigationPage(new MainPage("Control Categories", null));
            }
        }

        private async void OnAddUserClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CreateUserPage(firstUser: false));
        }

        private void ConfigureNumericInputPicker()
        {
            NumericInputPicker.ItemsSource = new[]
            {
                new NumericInputOption { Label = "Stage default", Value = NumericInputMode.Auto },
                new NumericInputOption { Label = "App keypad", Value = NumericInputMode.AppKeypad },
                new NumericInputOption { Label = "System keyboard", Value = NumericInputMode.SystemKeyboard }
            };
        }

        private void LoadActiveUserPreference()
        {
            var activeUser = ServiceHelper.GetService<CurrentUserSession>().ActiveUser;
            ActiveUserPreferenceLabel.Text = activeUser == null
                ? "Numeric keyboard"
                : $"Numeric keyboard for {activeUser.Name}";

            NumericInputMode preferredMode = _userPreferenceService.GetPreferredNumericInputMode(activeUser?.Id);
            NumericInputOption? selectedOption = (NumericInputPicker.ItemsSource as IEnumerable<NumericInputOption>)
                ?.FirstOrDefault(item => item.Value == preferredMode);
            NumericInputPicker.SelectedItem = selectedOption ?? (NumericInputPicker.ItemsSource as IEnumerable<NumericInputOption>)?.FirstOrDefault();
            NumericInputPicker.IsEnabled = activeUser != null;
        }

        private void OnNumericInputPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            var activeUser = ServiceHelper.GetService<CurrentUserSession>().ActiveUser;
            if (activeUser == null)
                return;

            if (NumericInputPicker.SelectedItem is NumericInputOption option)
                _userPreferenceService.SetPreferredNumericInputMode(activeUser.Id, option.Value);
        }


        private async void OnDampButtonClicked(object sender, EventArgs e)
        {
            User? activeUser = ServiceHelper.GetService<CurrentUserSession>().ActiveUser;
            if (!_backgroundSyncService.TryStartSync(activeUser, forceFullResync: true))
            {
                await DisplayAlert("Sync", _backgroundSyncService.IsSyncing ? "Sync is already running." : "No active user to sync.", "OK");
                return;
            }

            RefreshSyncUi();
        }

        private async void OnSyncToolbarClicked()
        {
            User? activeUser = ServiceHelper.GetService<CurrentUserSession>().ActiveUser;
            if (!_backgroundSyncService.TryStartSync(activeUser))
            {
                await DisplayAlert("Sync", _backgroundSyncService.IsSyncing ? "Sync is already running." : "No active user to sync.", "OK");
                return;
            }

            RefreshSyncUi();
        }


        private async void OnDeleteAllUsersClicked(object sender, EventArgs e)
        {
            var users = await _userRepo.GetUsersAsync();
            foreach (var user in users)
            {
                await _userRepo.DeleteAsync(user);
            }

            await ServiceHelper.GetService<CurrentUserSession>().LoadUserAsync(null);

            await DisplayAlert("Delete Complete", "All users deleted", "OK");
            await Navigation.PopToRootAsync();
        }

        private void OnBackgroundSyncStateChanged(object? sender, EventArgs e)
        {
            RefreshSyncUi();
        }

        private void RefreshSyncUi()
        {
            SyncActivityIndicator.IsRunning = _backgroundSyncService.IsSyncing;
            SyncActivityIndicator.IsVisible = _backgroundSyncService.IsSyncing;
            SyncStatusLabel.Text = _backgroundSyncService.StatusText;
            SyncStatusLabel.IsVisible = !string.IsNullOrWhiteSpace(_backgroundSyncService.StatusText);
            SyncStatusLabel.TextColor = string.IsNullOrWhiteSpace(_backgroundSyncService.LastErrorMessage)
                ? Colors.Black
                : Colors.IndianRed;
            _syncToolbarItem.IsEnabled = !_backgroundSyncService.IsSyncing;
            btnSave.IsEnabled = !_backgroundSyncService.IsSyncing;
        }
    }
}
