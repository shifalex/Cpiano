using GestureSample.Maui.Data;
using GestureSample.Maui.Handlers;
using GestureSample.Views;

namespace GestureSample.Maui.Views;

public partial class SplashPage : ContentPage
{
    private readonly IUserRepository _userRepo;

    public SplashPage(IUserRepository userRepo)
    {
        InitializeComponent();
        _userRepo = userRepo;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var users = await _userRepo.GetUsersAsync();

        if (users.Count == 0)
        {
            // No users -> force to CreateUserPage
            await Navigation.PushAsync(new CreateUserPage(_userRepo, firstUser: true));
        }
        else
        {
            // We have users. Check if we have a "last active user" in Preferences
            var lastUserId = ActiveUserHelper.CurrentUserId;

            if (lastUserId == null || !users.Any(u => u.Id == lastUserId.Value))
            {
                // If there's no valid active user, pick the first user or let them choose
                // For simplicity, let's pick the first user
                var defaultUser = users[0];
                ActiveUserHelper.CurrentUserId = defaultUser.Id;
            }

            // Go to the main page
            await Navigation.PushAsync(new MainPage());
        }
    }
}