using GestureSample.Maui.Data;
using GestureSample.Maui.Handlers;
using GestureSample.Maui.Views;
using GestureSample.Views;
using GestureSample.Maui.Models;

namespace GestureSample.Maui.Views{

    public partial class SplashPagecs : ContentPage
    {
        private readonly IUserRepository _userRepo;

        public SplashPagecs(IUserRepository userRepo)
        {
            InitializeComponent();
            _userRepo = userRepo;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var users = await _userRepo.GetUsersAsync();

            // If no users at all, force new user creation
            if (users.Count == 0)
            {
                await Navigation.PushAsync(new CreateUserPage(_userRepo, firstUser: true));
            }
            else
            {
                // Check if we have a stored "last active user"
                var lastUserId = ActiveUserHelper.CurrentUserId;
                if (lastUserId == null || !users.Any(u => u.Id == lastUserId.Value))
                {
                    // If no valid stored user, pick the first or let them choose
                    var defaultUser = users.First();
                    ActiveUserHelper.CurrentUserId = defaultUser.Id;
                }

                // Proceed to MainPage
                await Navigation.PushAsync(new MainPage());
            }
        }
    }
}