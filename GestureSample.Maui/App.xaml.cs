using GestureSample.Views;
using GestureSample.Maui.Data;
using GestureSample.Maui.Views;
using System;
using System.Threading.Tasks;

namespace GestureSample.Maui
{

    public partial class App : Application
    {
        public static NavigationPage MainNavigation;
        private static bool _exceptionHooksInitialized;


        private static void InitializeExceptionHooks()
        {
            if (_exceptionHooksInitialized)
                return;

            _exceptionHooksInitialized = true;

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Console.WriteLine($"[UnhandledException] {e.ExceptionObject}");
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                Console.WriteLine($"[UnobservedTaskException] {e.Exception}");
                e.SetObserved();
            };
        }

        public App()
        {
            InitializeComponent();
            InitializeExceptionHooks();

            // Start from SplashPage so user/database initialization is completed
            // before MainPage is created.
            MainPage = MainNavigation = new NavigationPage(new SplashPage());
        }

    }
}