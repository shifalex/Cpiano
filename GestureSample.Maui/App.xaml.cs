using GestureSample.Views;
using GestureSample.Maui.Data;
using GestureSample.Maui.Views;

namespace GestureSample.Maui
{

    public partial class App : Application
    {
        public static NavigationPage MainNavigation;

        public App()
        {
            InitializeComponent();

            // Start from SplashPage so user/database initialization is completed
            // before MainPage is created.
            MainPage = MainNavigation = new NavigationPage(new SplashPage());
        }

    }
}