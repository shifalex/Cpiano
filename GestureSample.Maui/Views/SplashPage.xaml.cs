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
        Console.WriteLine("Splash constructing..");
        InitializeComponent();

        Console.WriteLine("Initialize succeeded..");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isInitialized)
            return;

        _isInitialized = true;

        try
        {
            Console.WriteLine("Splash appearing");

            _userRepo ??= ServiceHelper.GetService<UserRepository>();
            Console.WriteLine("User repo resolved..");

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
                Console.WriteLine("Connecting user");
                await session.LoadUserAsync(users[0].Id);
                currentUser = session.ActiveUser;

                if (currentUser != null)
                {
                    currentUser.LastLoginTime = DateTime.Now;
                    await _userRepo.UpdateAsync(currentUser);
                    Console.WriteLine("User updated");
                }
            }

            Console.WriteLine("Going to Main page now");
            Application.Current.MainPage = new NavigationPage(new MainPage("Control Categories", null));
        }
        catch (Exception ex)
        {
            _isInitialized = false;
            Console.WriteLine($"Splash startup failed: {ex}");
            await DisplayAlert("Startup Error", "Could not initialize user data. Please restart the app.", "OK");
        }
    }
}