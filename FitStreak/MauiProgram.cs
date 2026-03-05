using CommunityToolkit.Maui;
using FitStreak.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FitStreak;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
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
            ServiceLifetime.Singleton);  // Singleton — one DB connection for the app lifetime

        // -------------------------------------------------------------------------
        // Services — registered in FitStreak.Core
        // Add each service here as we build them in Step 6
        // -------------------------------------------------------------------------
        // builder.Services.AddSingleton<IWorkoutService, WorkoutService>();
        // builder.Services.AddSingleton<IScheduleService, ScheduleService>();
        // builder.Services.AddSingleton<IStreakService, StreakService>();
        // builder.Services.AddSingleton<INotificationService, NotificationService>();

        // -------------------------------------------------------------------------
        // ViewModels — registered as Transient (new instance per page navigation)
        // Add each ViewModel here as we build them in Step 7
        // -------------------------------------------------------------------------
        // builder.Services.AddTransient<HomeViewModel>();
        // builder.Services.AddTransient<WorkoutsViewModel>();
        // builder.Services.AddTransient<CreateWorkoutViewModel>();
        // builder.Services.AddTransient<ScheduleViewModel>();
        // builder.Services.AddTransient<WorkoutRunnerViewModel>();
        // builder.Services.AddTransient<StreakViewModel>();

        // -------------------------------------------------------------------------
        // Views — registered as Transient (new instance per navigation)
        // Add each View here as we build them in Step 8
        // -------------------------------------------------------------------------
        // builder.Services.AddTransient<HomePage>();
        // builder.Services.AddTransient<WorkoutsPage>();
        // builder.Services.AddTransient<CreateWorkoutPage>();
        // builder.Services.AddTransient<SchedulePage>();
        // builder.Services.AddTransient<WorkoutRunnerPage>();
        // builder.Services.AddTransient<StreakPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}