using GestureSample.Maui.Data;
using GestureSample.Maui.Handlers;
using GestureSample.Maui.Views;
using GestureSample.Views;
using GestureSample.Maui.Models;

namespace GestureSample.Maui.Views
{

    public partial class CreateUserPage : ContentPage
    {
        private readonly UserRepository _userRepo;
        private readonly bool _firstUser;

        public CreateUserPage(UserRepository userRepo, bool firstUser = false)
        {
            InitializeComponent();
            _userRepo = userRepo;
            _firstUser = firstUser;
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

            await _userRepo.AddUserAsync(newUser);
            ActiveUserHelper.CurrentUserId = newUser.Id;

            if (_firstUser)
            {
                // Navigate to the MainPage and clear older pages (like SplashPage)
                await Navigation.PopToRootAsync();
            }
            else
            {
                // If not first user, just go back to whoever called this page (e.g. SwitchUserPage)
                await Navigation.PopAsync();
            }
        }
    }
}