using FitStreak.Core.Data;
using FitStreak.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace FitStreak;

public partial class App : Application
{
    private readonly AppDbContext _dbContext;
    private readonly IScheduleService _scheduleService;

    public App(AppDbContext dbContext, IScheduleService scheduleService)
    {
        InitializeComponent();
        _dbContext = dbContext;
        _scheduleService = scheduleService;
    }

    protected override async void OnStart()
    {
        base.OnStart();

        // EnsureCreatedAsync — creates the DB and all tables on fresh install.
        // If DB already exists, does nothing.
        // NOTE: Replace with MigrateAsync() before first production release
        // once migrations are confirmed working.
        await _dbContext.Database.MigrateAsync();

        // Mark any past pending schedules as Missed on every app launch
        // Runs fast — only updates rows where Status=Pending and Date < today
        await _scheduleService.MarkMissedSchedulesAsync();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}