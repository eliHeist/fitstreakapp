using Android.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FitStreak.Core.Models.Workout;
using FitStreak.Core.Services;
using FitStreak.ViewModels.Base;

namespace FitStreak.ViewModels;

public partial class WorkoutRunnerViewModel : BaseViewModel
{
    private readonly IWorkoutService _workoutService;
    private readonly IScheduleService _scheduleService;

    private List<Exercise> _exercises = [];
    private IDispatcherTimer? _timer;
    private int _scheduleId;

    [ObservableProperty] private Exercise? _currentExercise;
    [ObservableProperty] private Exercise? _nextExercise;
    [ObservableProperty] private int _secondsRemaining;
    [ObservableProperty] private bool _isResting;
    [ObservableProperty] private bool _isCompleted;
    [ObservableProperty] private int _currentIndex;
    [ObservableProperty] private int _totalExercises;
    [ObservableProperty] private double _progress;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PauseButtonText))]
    private bool _isPaused;

    public string PauseButtonText => _isPaused ? "▶ Resume" : "⏸ Pause";

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
            IsPaused = false;

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
        IsPaused = false;
        SecondsRemaining = CurrentExercise.DurationSeconds;
        Progress = (double)index / TotalExercises;

        StartTimer();
    }

    private void StartRest(int afterIndex)
    {
        // Guard: if no next exercise, finish instead
        if (afterIndex + 1 >= _exercises.Count)
        {
            FinishWorkout();
            return;
        }

        var exercise = _exercises[afterIndex];
        if (exercise.RestAfterSeconds <= 0)
        {
            StartExercise(afterIndex + 1);
            return;
        }

        IsResting = true;
        IsPaused = false;
        SecondsRemaining = exercise.RestAfterSeconds;
        // Keep CurrentIndex pointing at the exercise we just finished
        // so OnTimerTick knows which exercise comes next
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
        if (IsPaused) return;

        SecondsRemaining--;
        if (SecondsRemaining > 0) return;

        _timer?.Stop();

        if (IsResting)
            StartExercise(CurrentIndex + 1);
        else
            StartRest(CurrentIndex);
    }

    [RelayCommand]
    public void TogglePause()
    {
        IsPaused = !IsPaused;
    }

    [RelayCommand]
    public void SkipCurrent()
    {
        _timer?.Stop();
        IsPaused = false;

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
    }

    private void FinishWorkout()
    {
        _timer?.Stop();
        IsCompleted = true;
        Progress = 1.0;
        CurrentExercise = null;
        NextExercise = null;
    }
}