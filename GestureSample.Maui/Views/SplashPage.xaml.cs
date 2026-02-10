using GestureSample.Maui.Data;
using GestureSample.Maui.Handlers;
using GestureSample.Views;

namespace GestureSample.Maui.Views;

public partial class SplashPage : ContentPage
{
    private UserRepository? _userRepo;
    private bool _isInitialized;

    public SplashPage()
    {
        CrashLog.Write("Splash constructing");
        InitializeComponent();

        CrashLog.Write("Splash InitializeComponent succeeded");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isInitialized)
            return;

        _isInitialized = true;

        try
        {
            CrashLog.Write("Splash appearing");

            _userRepo ??= ServiceHelper.GetService<UserRepository>();
            CrashLog.Write("UserRepository resolved");

            var users = await _userRepo.GetUsersAsync();

            if (users.Count == 0)
            {
                await Navigation.PushAsync(new CreateUserPage(firstUser: true));
                return;
            }

            var session = ServiceHelper.GetService<CurrentUserSession>();
            var currentUser = session.ActiveUser;

            if (currentUser == null)
            {
                CrashLog.Write("Connecting user");
                await session.LoadUserAsync(users[0].Id);
                currentUser = session.ActiveUser;

                if (currentUser != null)
                {
                    currentUser.LastLoginTime = DateTime.Now;
                    await _userRepo.UpdateAsync(currentUser);
                    CrashLog.Write("User updated");
                }
            }

            CrashLog.Write("Going to MainPage");
            Application.Current.MainPage = new NavigationPage(new MainPage("Control Categories", null));
        }
        catch (Exception ex)
        {
            _isInitialized = false;
            CrashLog.WriteException("Splash startup failed", ex);
            await DisplayAlert("Startup Error", $"Could not initialize user data. Log: {CrashLog.GetPath()}", "OK");
        }
    }
}