using GestureSample.Maui.Data.SQLite;
using GestureSample.Maui.Data;
using GestureSample.Maui.Handlers;
using GestureSample.Maui.Views;
using GestureSample.Views;
using GestureSample.Maui.Models;

namespace GestureSample.Maui.Views
{

    public partial class CreateUserPage : ContentPage
    {
        private sealed class NumericInputOption
        {
            public string Label { get; init; } = string.Empty;
            public NumericInputMode Value { get; init; }
            public override string ToString() => Label;
        }

        private readonly UserRepository _userRepo;
        private readonly UserPreferenceService _userPreferenceService;
        private readonly bool _firstUser;

        public CreateUserPage(bool firstUser = false)
        {
            InitializeComponent();
            _userRepo = ServiceHelper.GetService<UserRepository>();
            _userPreferenceService = ServiceHelper.GetService<UserPreferenceService>();
            _firstUser = firstUser;
            ConfigureNumericInputPicker();
        }

        private async void OnCreateButtonClicked(object sender, EventArgs e)
        {
            var name = UserNameEntry.Text?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                await DisplayAlert("Error", "Name cannot be empty.", "OK");
                return;
            }

            // This is just a placeholder if you had an actual avatar selection
            var avatarUri = string.Empty;

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Name = name,
                AvatarUri = avatarUri
            };

            await _userRepo.SaveAsync(newUser);
            NumericInputMode preferredMode = GetSelectedNumericInputMode();
            _userPreferenceService.SetPreferredNumericInputMode(newUser.Id, preferredMode);
            var currentUserSession = ServiceHelper.GetService<CurrentUserSession>();

            if (_firstUser)
            {               

            // Load the active user asynchronously (this should only happen once per login).
            await currentUserSession.LoadUserAsync(newUser.Id);
                // Navigate to the MainPage and clear older pages (like SplashPage)
                await Navigation.PopToRootAsync();
            }
            else
            {

                currentUserSession.ActiveUser.LastLoginTime = DateTime.Now;
                await _userRepo.UpdateAsync(currentUserSession.ActiveUser);
                // If not first user, just go back to whoever called this page (e.g. SwitchUserPage)
                await Navigation.PopAsync();
            }
        }

        private void ConfigureNumericInputPicker()
        {
            NumericInputPicker.ItemsSource = new[]
            {
                new NumericInputOption { Label = "Stage default", Value = NumericInputMode.Auto },
                new NumericInputOption { Label = "App keypad", Value = NumericInputMode.AppKeypad },
                new NumericInputOption { Label = "System keyboard", Value = NumericInputMode.SystemKeyboard }
            };
            NumericInputPicker.SelectedIndex = 0;
        }

        private NumericInputMode GetSelectedNumericInputMode()
        {
            return NumericInputPicker.SelectedItem is NumericInputOption option
                ? option.Value
                : NumericInputMode.Auto;
        }
    }
}
