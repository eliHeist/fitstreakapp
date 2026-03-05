using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FitStreak.Core.Models.Workout;
using FitStreak.Core.Services;
using FitStreak.ViewModels.Base;
using System.Collections.ObjectModel;

namespace FitStreak.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    private readonly IScheduleService _scheduleService;
    private readonly IStreakService _streakService;

    [ObservableProperty]
    private ObservableCollection<WorkoutSchedule> _todaySchedules = [];

    [ObservableProperty]
    private ObservableCollection<WorkoutSchedule> _missedSchedules = [];

    [ObservableProperty]
    private int _currentStreak;

    [ObservableProperty]
    private int _totalCompletions;

    [ObservableProperty]
    private bool _hasMissed;

    public HomeViewModel(IScheduleService scheduleService, IStreakService streakService)
    {
        _scheduleService = scheduleService;
        _streakService = streakService;
        Title = "Today";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        await RunSafeAsync(async () =>
        {
            var today = await _scheduleService.GetTodaySchedulesAsync();
            TodaySchedules = new ObservableCollection<WorkoutSchedule>(today);

            var missed = await _scheduleService.GetMissedSchedulesAsync();
            MissedSchedules = new ObservableCollection<WorkoutSchedule>(missed);
            HasMissed = missed.Count > 0;

            CurrentStreak = await _streakService.GetCurrentStreakAsync();
            TotalCompletions = await _streakService.GetTotalCompletionsAsync();
        });
    }

    [RelayCommand]
    public async Task CompleteWorkoutAsync(int scheduleId)
    {
        await RunSafeAsync(async () =>
        {
            await _scheduleService.CompleteScheduleAsync(scheduleId);
            await LoadAsync();
        });
    }

    [RelayCommand]
    public async Task RescheduleAsync(WorkoutSchedule schedule)
    {
        // Navigation to reschedule page is handled in the View
        // This command is a hook for the ViewModel to react after rescheduling
        await LoadAsync();
    }
}