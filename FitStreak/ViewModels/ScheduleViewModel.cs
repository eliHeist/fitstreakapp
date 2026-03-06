using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FitStreak.Core.Models.Workout;
using FitStreak.Core.Services;
using FitStreak.ViewModels.Base;
using System.Collections.ObjectModel;

namespace FitStreak.ViewModels;

public partial class ScheduleViewModel : BaseViewModel
{
    private readonly IScheduleService _scheduleService;
    private readonly IWorkoutService _workoutService;
    private readonly INotificationService _notificationService;

    [ObservableProperty]
    private ObservableCollection<WorkoutSchedule> _schedulesForSelectedDate = [];

    [ObservableProperty]
    private ObservableCollection<Workout> _availableWorkouts = [];

    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Today;

    [ObservableProperty]
    private Workout? _selectedWorkout;

    [ObservableProperty]
    private RecurrenceType _selectedRecurrence = RecurrenceType.None;

    public List<RecurrenceType> RecurrenceOptions { get; } =
        Enum.GetValues<RecurrenceType>().ToList();

    public ScheduleViewModel(
        IScheduleService scheduleService,
        IWorkoutService workoutService,
        FitStreak.Core.Services.INotificationService notificationService)
    {
        _scheduleService = scheduleService;
        _workoutService = workoutService;
        _notificationService = notificationService;
        Title = "Schedule";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        await RunSafeAsync(async () =>
        {
            var workouts = await _workoutService.GetAllWorkoutsAsync();
            AvailableWorkouts = new ObservableCollection<Workout>(workouts);
            await LoadSchedulesForDateAsync();
        });
    }

    [RelayCommand]
    public async Task LoadSchedulesForDateAsync()
    {
        await RunSafeAsync(async () =>
        {
            var schedules = await _scheduleService.GetSchedulesForDateAsync(SelectedDate);
            SchedulesForSelectedDate = new ObservableCollection<WorkoutSchedule>(schedules);
        });
    }

    [RelayCommand]
    public async Task AddScheduleAsync()
    {
        if (SelectedWorkout is null) return;

        await RunSafeAsync(async () =>
        {
            var schedule = await _scheduleService.ScheduleWorkoutAsync(
                SelectedWorkout.Id, SelectedDate, SelectedRecurrence);

            // Schedule notifications for this workout
            await _notificationService.ScheduleWorkoutReminderAsync(
                schedule.Id, SelectedWorkout.Name, SelectedDate);
            await _notificationService.ScheduleEveningReminderAsync(
                schedule.Id, SelectedWorkout.Name, SelectedDate);

            await LoadSchedulesForDateAsync();
        });
    }

    [RelayCommand]
    public async Task DeleteScheduleAsync(int scheduleId)
    {
        await RunSafeAsync(async () =>
        {
            await _notificationService.CancelNotificationsAsync(scheduleId);
            await _scheduleService.DeleteScheduleAsync(scheduleId);
            await LoadSchedulesForDateAsync();
        });
    }

    [RelayCommand]
    public async Task RescheduleAsync((WorkoutSchedule Schedule, DateTime NewDate) args)
    {
        await RunSafeAsync(async () =>
        {
            await _notificationService.CancelNotificationsAsync(args.Schedule.Id);
            await _scheduleService.RescheduleAsync(args.Schedule.Id, args.NewDate);
            await _notificationService.ScheduleWorkoutReminderAsync(
                args.Schedule.Id, args.Schedule.Workout!.Name, args.NewDate);
            await _notificationService.ScheduleEveningReminderAsync(
                args.Schedule.Id, args.Schedule.Workout!.Name, args.NewDate);
            await LoadSchedulesForDateAsync();
        });
    }

    // Called when user changes the selected date on the calendar
    partial void OnSelectedDateChanged(DateTime value)
    {
        _ = LoadSchedulesForDateAsync();
    }
}