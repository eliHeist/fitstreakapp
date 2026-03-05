using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FitStreak.Core.Models.Workout;
using FitStreak.Core.Services;
using FitStreak.ViewModels.Base;
using System.Collections.ObjectModel;

namespace FitStreak.ViewModels;

public partial class CreateWorkoutViewModel : BaseViewModel
{
    private readonly IWorkoutService _workoutService;

    // -------------------------------------------------------------------------
    // Workout fields
    // -------------------------------------------------------------------------
    [ObservableProperty]
    private string _workoutName = string.Empty;

    [ObservableProperty]
    private string _workoutDescription = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Exercise> _exercises = [];

    // -------------------------------------------------------------------------
    // Exercise being added/edited
    // -------------------------------------------------------------------------
    [ObservableProperty]
    private string _exerciseName = string.Empty;

    [ObservableProperty]
    private int _exerciseDuration = 30;

    [ObservableProperty]
    private int _exerciseRest = 20;

    [ObservableProperty]
    private string _exerciseNotes = string.Empty;

    [ObservableProperty]
    private bool _isEditing;

    // Holds the workout being edited — null if creating new
    private Workout? _existingWorkout;

    public CreateWorkoutViewModel(IWorkoutService workoutService)
    {
        _workoutService = workoutService;
        Title = "Create Workout";
    }

    /// <summary>Call this when editing an existing workout</summary>
    public async Task LoadForEditAsync(int workoutId)
    {
        await RunSafeAsync(async () =>
        {
            _existingWorkout = await _workoutService.GetWorkoutByIdAsync(workoutId);
            if (_existingWorkout is null) return;

            WorkoutName = _existingWorkout.Name;
            WorkoutDescription = _existingWorkout.Description;
            Exercises = new ObservableCollection<Exercise>(_existingWorkout.Exercises);
            IsEditing = true;
            Title = "Edit Workout";
        });
    }

    [RelayCommand]
    public async Task AddExerciseAsync()
    {
        if (string.IsNullOrWhiteSpace(ExerciseName)) return;

        var exercise = new Exercise
        {
            Name = ExerciseName,
            DurationSeconds = ExerciseDuration,
            RestAfterSeconds = ExerciseRest,
            Notes = ExerciseNotes,
            OrderIndex = Exercises.Count
        };

        if (_existingWorkout is not null)
        {
            // Editing existing workout — save to DB immediately
            exercise.WorkoutId = _existingWorkout.Id;
            await _workoutService.AddExerciseAsync(exercise);
        }

        Exercises.Add(exercise);
        ClearExerciseForm();
    }

    [RelayCommand]
    public async Task RemoveExerciseAsync(Exercise exercise)
    {
        await RunSafeAsync(async () =>
        {
            if (exercise.Id > 0)
                await _workoutService.DeleteExerciseAsync(exercise.Id);

            Exercises.Remove(exercise);
        });
    }

    [RelayCommand]
    public async Task SaveWorkoutAsync()
    {
        if (string.IsNullOrWhiteSpace(WorkoutName)) return;

        await RunSafeAsync(async () =>
        {
            if (_existingWorkout is null)
            {
                // Create new workout with all exercises
                var workout = new Workout
                {
                    Name = WorkoutName,
                    Description = WorkoutDescription,
                    Exercises = Exercises.ToList()
                };

                await _workoutService.CreateWorkoutAsync(workout);
            }
            else
            {
                // Update existing workout details
                _existingWorkout.Name = WorkoutName;
                _existingWorkout.Description = WorkoutDescription;
                await _workoutService.UpdateWorkoutAsync(_existingWorkout);
            }
        });
    }

    private void ClearExerciseForm()
    {
        ExerciseName = string.Empty;
        ExerciseDuration = 30;
        ExerciseRest = 20;
        ExerciseNotes = string.Empty;
    }
}