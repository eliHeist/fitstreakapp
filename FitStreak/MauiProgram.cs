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
        // EF Core — SQLite database
        // DbPath points to the app's local data folder on the device
        // -------------------------------------------------------------------------
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "fitstreak.db");

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}