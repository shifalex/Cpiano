using GestureSample.Maui.Data;
using GestureSample.Maui.Handlers;
using GestureSample.Views;

namespace GestureSample.Maui.Views;

public partial class SplashPage : ContentPage
{
    private readonly UserRepository _userRepo;

    public SplashPage()
    {
        Console.WriteLine("Splash constructing..");
        _userRepo = ServiceHelper.GetService<UserRepository>();

        Console.WriteLine("User repo succeeded..");
        InitializeComponent();

        Console.WriteLine("Initialize succeeded..");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        Console.WriteLine("Splash appearing");

        var users = await _userRepo.GetUsersAsync();



        if (users.Count == 0)
        {
            await Navigation.PushAsync(new CreateUserPage(firstUser: true));
            return;
        }
        var currentUser = ServiceHelper.GetService<CurrentUserSession>().ActiveUser;

        if (currentUser == null)
        {
            Console.WriteLine("Connecting user");
            await ServiceHelper.GetService<CurrentUserSession>().LoadUserAsync(users[0].Id);
            currentUser = ServiceHelper.GetService<CurrentUserSession>().ActiveUser;
            currentUser.LastLoginTime = DateTime.Now;
            Console.WriteLine("User Connected");
            await _userRepo.UpdateAsync(currentUser);

            Console.WriteLine("User Updated");

        }


        // Go to the main page

        Console.WriteLine("Going to Main page now");
        //await Navigation.PopToRootAsync(new MainPage("Control Categories", null));
        //await Navigation.PushAsync(new MainPage("Control Categories", null));
        Application.Current.MainPage = new NavigationPage(new MainPage("Control Categories", null));

    }
}