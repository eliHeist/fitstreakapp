using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FitStreak.Core.Models.Workout;
using FitStreak.Core.Services;
using FitStreak.ViewModels.Base;
using System;

namespace FitStreak.ViewModels;

public partial class WorkoutRunnerViewModel : BaseViewModel
{
    private readonly IWorkoutService _workoutService;
    private readonly IScheduleService _scheduleService;

    private List<Exercise> _exercises = [];
    private IDispatcherTimer? _timer;
    private int _scheduleId;

    // -------------------------------------------------------------------------
    // Current state
    // -------------------------------------------------------------------------
    [ObservableProperty]
    private Exercise? _currentExercise;

    [ObservableProperty]
    private Exercise? _nextExercise;

    [ObservableProperty]
    private int _secondsRemaining;

    [ObservableProperty]
    private bool _isResting;

    [ObservableProperty]
    private bool _isCompleted;

    [ObservableProperty]
    private int _currentIndex;

    [ObservableProperty]
    private int _totalExercises;

    [ObservableProperty]
    private double _progress; // 0.0 to 1.0 for progress bar

    public WorkoutRunnerViewModel(
        IWorkoutService workoutService,
        IScheduleService scheduleService)
    {
        _workoutService = workoutService;
        _scheduleService = scheduleService;
        Title = "Workout";
    }

    public async Task StartAsync(int workoutId, int scheduleId)
    {
        await RunSafeAsync(async () =>
        {
            _scheduleId = scheduleId;
            _exercises = await _workoutService.GetExercisesForWorkoutAsync(workoutId);
            TotalExercises = _exercises.Count;
            CurrentIndex = 0;
            IsCompleted = false;

            if (_exercises.Count > 0)
                StartExercise(0);
        });
    }

    private void StartExercise(int index)
    {
        if (index >= _exercises.Count)
        {
            FinishWorkout();
            return;
        }

        CurrentIndex = index;
        CurrentExercise = _exercises[index];
        NextExercise = index + 1 < _exercises.Count ? _exercises[index + 1] : null;
        IsResting = false;
        SecondsRemaining = CurrentExercise.DurationSeconds;
        Progress = (double)index / TotalExercises;

        StartTimer();
    }

    private void StartRest(int afterIndex)
    {
        var exercise = _exercises[afterIndex];
        if (exercise.RestAfterSeconds <= 0)
        {
            // No rest — go straight to next exercise
            StartExercise(afterIndex + 1);
            return;
        }

        IsResting = true;
        SecondsRemaining = exercise.RestAfterSeconds;
        StartTimer();
    }

    private void StartTimer()
    {
        _timer?.Stop();
        _timer = Application.Current!.Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        SecondsRemaining--;

        if (SecondsRemaining > 0) return;

        _timer?.Stop();

        if (IsResting)
            StartExercise(CurrentIndex + 1);
        else
            StartRest(CurrentIndex);
    }

    [RelayCommand]
    public void SkipCurrent()
    {
        _timer?.Stop();

        if (IsResting)
            StartExercise(CurrentIndex + 1);
        else
            StartRest(CurrentIndex);
    }

    [RelayCommand]
    public async Task CompleteWorkoutAsync()
    {
        _timer?.Stop();
        IsCompleted = true;
        Progress = 1.0;

        await _scheduleService.CompleteScheduleAsync(_scheduleId);
    }

    [RelayCommand]
    public void AbandonWorkout()
    {
        _timer?.Stop();
        // Navigation back is handled in the View
        // No DB update — session is just discarded
    }

    private void FinishWorkout()
    {
        IsCompleted = true;
        Progress = 1.0;
        CurrentExercise = null;
        NextExercise = null;
    }

    // Formatted time string for display e.g. "0:45"
    public string TimeDisplay => $"{SecondsRemaining / 60}:{SecondsRemaining % 60:D2}";
}