using GestureSample.Views;
using GestureSample.Maui.Data;

namespace GestureSample.Maui;

public partial class App : Application
{
    public static NavigationPage MainNavigation;
    private readonly IUserRepository _userRepository;

    public App()
    {
        InitializeComponent();

        var mainPage = new MainPage("Control Categories", null);
        MainPage = MainNavigation = new NavigationPage(mainPage);
    }

    public App(IUserRepository userRepository)
    {
        InitializeComponent();

        _userRepository = userRepository;

        // Wrap our initial Page in a NavigationPage
        //MainPage = new NavigationPage(new SplashPage(_userRepository));
    }
}
