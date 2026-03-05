using FitStreak.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace FitStreak;

public partial class App : Application
{
    private readonly AppDbContext _dbContext;

    public App(AppDbContext dbContext)
    {
        InitializeComponent();
        _dbContext = dbContext;
    }

    protected override async void OnStart()
    {
        base.OnStart();

        // EnsureCreatedAsync — creates the DB and all tables on fresh install.
        // If DB already exists, does nothing.
        // NOTE: Replace with MigrateAsync() before first production release
        // once migrations are confirmed working.
        await _dbContext.Database.MigrateAsync();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}