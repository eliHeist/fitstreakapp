using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FitStreak.Core.Data;

/// <summary>
/// Used ONLY by dotnet-ef tooling at design time (migrations, scaffolding).
/// Never called at runtime — MAUI uses MauiProgram.cs instead.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Use a local dev path for tooling — this file is never created on device
        var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "fitstreak_dev.db");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");

        return new AppDbContext(optionsBuilder.Options);
    }
}