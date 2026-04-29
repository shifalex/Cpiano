using MR.Gestures;
using GestureSample.Maui.Data;
using Supabase;
using GestureSample.Maui.Handlers;
using Plugin.Maui.Audio;
using GestureSample.Maui.Models;

namespace GestureSample.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            }).ConfigureMauiHandlers(handlers =>
            {
#if IOS
                // Register the custom handler for iOS so that every Entry uses it.
                handlers.AddHandler<Microsoft.Maui.Controls.Entry, CustomEntryHandler>();
#endif
#if ANDROID
                    handlers.AddHandler<Microsoft.Maui.Controls.Entry, CustomEntryHandler>();
#endif
            })
            //.ConfigureMRGestures("ALZ9-BPVU-XQ35-CEBG-5ZRR-URJQ-ED5U-TSY8-6THP-3GVU-JW8Z-RZGE-CQW6");        // GestureSample
            //.ConfigureMRGestures("NDTK-G7T7-QBLH-B48D-CKGP-F2NP-CV2N-B4M3-BXUR-WGQA-PLNK-BZVD-ZVCY");       // GestureSample.Maui

            .ConfigureMRGestures();

        // builder.Services.AddSingleton(_ => StateConnection.Instance.Database);
        Console.WriteLine("a");
        builder.Services.AddSingleton<UserRepository>();
        builder.Services.AddSingleton<CurrentUserSession>();

        builder.Services.AddSingleton(AudioManager.Current);
        builder.Services.AddSingleton<SoundService>();


        Console.WriteLine("b");
        //builder.Services.AddSingleton<IUserRepository, SupabaseUserRepository>();

        builder.Services.AddTransient<QuestionAnswerRepository>();
        builder.Services.AddTransient<QuestionAnswerPartRepository>();
        builder.Services.AddTransient<KeyboardQuestionRepository>();
        builder.Services.AddTransient<GameRepository>();
        builder.Services.AddTransient<KeyEventRepository>();
        builder.Services.AddTransient<TimerChangeEventRepository>();
        builder.Services.AddTransient<VisibilityChangeEventRepository>();
        builder.Services.AddTransient<CustomStageDefinitionRepository>();
        builder.Services.AddTransient<CustomStageFlowDefinitionRepository>();
        Console.WriteLine("c");

        var mauiApp = builder.Build();
        // Store the DI container (ServiceProvider)
        ServiceHelper.Services = mauiApp.Services;
        Console.WriteLine("d");
        return mauiApp;
    }
}
