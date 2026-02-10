using Foundation;
using UIKit;
using System;

namespace GestureSample.Maui
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            CrashLog.Write("iOS FinishedLaunching start");
            try
            {
                var result = base.FinishedLaunching(application, launchOptions);
                CrashLog.Write("iOS FinishedLaunching completed");
                return result;
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("iOS FinishedLaunching failed", ex);
                throw;
            }
        }

        protected override MauiApp CreateMauiApp()
        {
            CrashLog.Write("CreateMauiApp invoked");
            return MauiProgram.CreateMauiApp();
        }
    }
}
