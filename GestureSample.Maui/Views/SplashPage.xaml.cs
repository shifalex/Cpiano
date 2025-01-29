using GestureSample.Maui.Data;
using GestureSample.Maui.Handlers;
using GestureSample.Views;

namespace GestureSample.Maui.Views;

public partial class SplashPage : ContentPage
{
    private readonly UserRepository _userRepo;

    public SplashPage()
    {
        _userRepo = ServiceHelper.GetService<UserRepository>();
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var users = await _userRepo.GetUsersAsync();



        if (users.Count == 0)
        {
            await Navigation.PushAsync(new CreateUserPage(firstUser: true));
            return;
        }
        var currentUser = ServiceHelper.GetService<CurrentUserSession>().ActiveUser;

        if (currentUser == null)
        {
            await ServiceHelper.GetService<CurrentUserSession>().LoadUserAsync(users[0].Id);
            currentUser = ServiceHelper.GetService<CurrentUserSession>().ActiveUser;
            currentUser.LastLoginTime = DateTime.Now;
            await _userRepo.UpdateAsync(currentUser);

        }


        // Go to the main page
        
        //await Navigation.PopToRootAsync(new MainPage("Control Categories", null));
        await Navigation.PushAsync(new MainPage("Control Categories", null));
        //Application.Current.MainPage = new NavigationPage();

    }
}