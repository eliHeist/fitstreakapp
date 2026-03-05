using CommunityToolkit.Maui;
using FitStreak.Core.Data;
using FitStreak.Core.Services;
using FitStreak.Services;
using FitStreak.ViewModels;
using FitStreak.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;

namespace FitStreak;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseLocalNotification()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // -------------------------------------------------------------------------
        // Database — SQLite via EF Core
        // FileSystem.AppDataDirectory resolves to the correct app folder on Android
        // -------------------------------------------------------------------------
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "fitstreak.db");

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"),
            ServiceLifetime.Singleton);

        // -------------------------------------------------------------------------
        // Services — all in FitStreak.Core except NotificationService
        // -------------------------------------------------------------------------
        builder.Services.AddSingleton<IWorkoutService, WorkoutService>();
        builder.Services.AddSingleton<IScheduleService, ScheduleService>();
        builder.Services.AddSingleton<IStreakService, StreakService>();
        builder.Services.AddSingleton<FitStreak.Core.Services.INotificationService, NotificationService>();

        // -------------------------------------------------------------------------
        // ViewModels — Transient so each page navigation gets a fresh instance
        // -------------------------------------------------------------------------
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<WorkoutsViewModel>();
        builder.Services.AddTransient<CreateWorkoutViewModel>();
        builder.Services.AddTransient<ScheduleViewModel>();
        builder.Services.AddTransient<WorkoutRunnerViewModel>();
        builder.Services.AddTransient<StreakViewModel>();

        // -------------------------------------------------------------------------
        // Views — Transient, one per navigation
        // -------------------------------------------------------------------------
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<WorkoutsPage>();
        builder.Services.AddTransient<CreateWorkoutPage>();
        builder.Services.AddTransient<SchedulePage>();
        builder.Services.AddTransient<WorkoutRunnerPage>();
        builder.Services.AddTransient<StreakPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}