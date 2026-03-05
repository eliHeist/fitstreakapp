using FitStreak.Core.Models.Workout;

namespace FitStreak.Core.Services;

public interface IWorkoutService
{
    // -------------------------------------------------------------------------
    // Workouts
    // -------------------------------------------------------------------------

    /// <summary>Get all workouts in the library</summary>
    Task<List<Workout>> GetAllWorkoutsAsync();

    /// <summary>Get a single workout by id, including its exercises in order</summary>
    Task<Workout?> GetWorkoutByIdAsync(int id);

    /// <summary>Create a new workout — returns the saved workout with Id populated</summary>
    Task<Workout> CreateWorkoutAsync(Workout workout);

    /// <summary>Update an existing workout's name and description</summary>
    Task UpdateWorkoutAsync(Workout workout);

    /// <summary>Delete a workout and all its exercises, schedules and sessions</summary>
    Task DeleteWorkoutAsync(int id);

    // -------------------------------------------------------------------------
    // Exercises
    // -------------------------------------------------------------------------

    /// <summary>Get all exercises for a workout, ordered by OrderIndex</summary>
    Task<List<Exercise>> GetExercisesForWorkoutAsync(int workoutId);

    /// <summary>Add an exercise to a workout</summary>
    Task<Exercise> AddExerciseAsync(Exercise exercise);

    /// <summary>Update an exercise</summary>
    Task UpdateExerciseAsync(Exercise exercise);

    /// <summary>Delete an exercise</summary>
    Task DeleteExerciseAsync(int id);

    /// <summary>Reorder exercises — pass the full reordered list</summary>
    Task ReorderExercisesAsync(List<Exercise> exercises);
}