using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FitStreak.Core.Services;
using FitStreak.ViewModels.Base;

namespace FitStreak.ViewModels;

public partial class StreakViewModel : BaseViewModel
{
    private readonly IStreakService _streakService;
    private readonly IScheduleService _scheduleService;

    [ObservableProperty]
    private Dictionary<DateOnly, int> _yearlyActivity = [];

    [ObservableProperty]
    private int _currentStreak;

    [ObservableProperty]
    private int _longestStreak;

    [ObservableProperty]
    private int _totalCompletions;

    // Selected day detail
    [ObservableProperty]
    private DateOnly _selectedDay;

    [ObservableProperty]
    private int _selectedDayCount;

    [ObservableProperty]
    private bool _hasSelectedDay;

    public StreakViewModel(IStreakService streakService, IScheduleService scheduleService)
    {
        _streakService = streakService;
        _scheduleService = scheduleService;
        Title = "Streaks";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        await RunSafeAsync(async () =>
        {
            YearlyActivity = await _streakService.GetYearlyActivityAsync();
            CurrentStreak = await _streakService.GetCurrentStreakAsync();
            LongestStreak = await _streakService.GetLongestStreakAsync();
            TotalCompletions = await _streakService.GetTotalCompletionsAsync();
        });
    }

    [RelayCommand]
    public void SelectDay(DateOnly day)
    {
        SelectedDay = day;
        SelectedDayCount = YearlyActivity.TryGetValue(day, out var count) ? count : 0;
        HasSelectedDay = true;
    }
}