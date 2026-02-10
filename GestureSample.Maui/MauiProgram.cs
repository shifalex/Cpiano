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
            });

        //.ConfigureMRGestures("ALZ9-BPVU-XQ35-CEBG-5ZRR-URJQ-ED5U-TSY8-6THP-3GVU-JW8Z-RZGE-CQW6");        // GestureSample
        //.ConfigureMRGestures("NDTK-G7T7-QBLH-B48D-CKGP-F2NP-CV2N-B4M3-BXUR-WGQA-PLNK-BZVD-ZVCY");       // GestureSample.Maui

        try
        {
            builder.ConfigureMRGestures();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MR.Gestures MAUI init failed: {ex}");
        }

        /*builder.Services.AddSingleton((_) => new Supabase.Client(
            "https://njsspracfpbyozvandph.supabase.co", // Replace with your Supabase URL
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im5qc3NwcmFjZnBieW96dmFuZHBoIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzYwMTg5MzcsImV4cCI6MjA1MTU5NDkzN30.yrk-QUINVC1rR4km1dO0X5OaMEdZbmGUGtgExTcxOiA" // Replace with your Supabase API Key
        ));//Password: c!L2TkQ@8wLPt2e
        */
        // builder.Services.AddSingleton(_ => StateConnection.Instance.Database);
        Console.WriteLine("a");
        builder.Services.AddSingleton<UserRepository>();
        builder.Services.AddSingleton<CurrentUserSession>();

        builder.Services.AddSingleton(AudioManager.Current);
        builder.Services.AddSingleton<SoundService>();


        Console.WriteLine("b");
        //builder.Services.AddSingleton<IUserRepository, SupabaseUserRepository>();

        builder.Services.AddTransient<QuestionAnswerRepository>();
        builder.Services.AddTransient<KeyboardQuestionRepository>();
        builder.Services.AddTransient<GameRepository>();
        builder.Services.AddTransient<KeyEventRepository>();
        Console.WriteLine("c");

        var mauiApp = builder.Build();
        // Store the DI container (ServiceProvider)
        ServiceHelper.Services = mauiApp.Services;
        Console.WriteLine("d");
        return mauiApp;
    }
}
