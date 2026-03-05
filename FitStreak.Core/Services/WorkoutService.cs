using FitStreak.Core.Data;
using FitStreak.Core.Models.Workout;
using Microsoft.EntityFrameworkCore;

namespace FitStreak.Core.Services;

public class WorkoutService : IWorkoutService
{
    private readonly AppDbContext _db;

    public WorkoutService(AppDbContext db)
    {
        _db = db;
    }

    // -------------------------------------------------------------------------
    // Workouts
    // -------------------------------------------------------------------------

    public async Task<List<Workout>> GetAllWorkoutsAsync()
    {
        return await _db.Workouts
            .OrderBy(w => w.Name)
            .ToListAsync();
    }

    public async Task<Workout?> GetWorkoutByIdAsync(int id)
    {
        return await _db.Workouts
            .Include(w => w.Exercises.OrderBy(e => e.OrderIndex))
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<Workout> CreateWorkoutAsync(Workout workout)
    {
        workout.CreatedAt = DateTime.UtcNow;
        _db.Workouts.Add(workout);
        await _db.SaveChangesAsync();
        return workout;
    }

    public async Task UpdateWorkoutAsync(Workout workout)
    {
        _db.Workouts.Update(workout);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteWorkoutAsync(int id)
    {
        var workout = await _db.Workouts.FindAsync(id);
        if (workout is not null)
        {
            _db.Workouts.Remove(workout);
            await _db.SaveChangesAsync();
            // EF cascade delete handles Exercises, Schedules and Sessions automatically
        }
    }

    // -------------------------------------------------------------------------
    // Exercises
    // -------------------------------------------------------------------------

    public async Task<List<Exercise>> GetExercisesForWorkoutAsync(int workoutId)
    {
        return await _db.Exercises
            .Where(e => e.WorkoutId == workoutId)
            .OrderBy(e => e.OrderIndex)
            .ToListAsync();
    }

    public async Task<Exercise> AddExerciseAsync(Exercise exercise)
    {
        // Auto-assign OrderIndex as last in the list
        var maxOrder = await _db.Exercises
            .Where(e => e.WorkoutId == exercise.WorkoutId)
            .MaxAsync(e => (int?)e.OrderIndex) ?? -1;

        exercise.OrderIndex = maxOrder + 1;

        _db.Exercises.Add(exercise);
        await _db.SaveChangesAsync();
        return exercise;
    }

    public async Task UpdateExerciseAsync(Exercise exercise)
    {
        _db.Exercises.Update(exercise);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteExerciseAsync(int id)
    {
        var exercise = await _db.Exercises.FindAsync(id);
        if (exercise is not null)
        {
            _db.Exercises.Remove(exercise);
            await _db.SaveChangesAsync();

            // Re-index remaining exercises so OrderIndex stays sequential
            var remaining = await _db.Exercises
                .Where(e => e.WorkoutId == exercise.WorkoutId)
                .OrderBy(e => e.OrderIndex)
                .ToListAsync();

            for (int i = 0; i < remaining.Count; i++)
                remaining[i].OrderIndex = i;

            await _db.SaveChangesAsync();
        }
    }

    public async Task ReorderExercisesAsync(List<Exercise> exercises)
    {
        // Update OrderIndex for each exercise based on its position in the list
        for (int i = 0; i < exercises.Count; i++)
        {
            exercises[i].OrderIndex = i;
            _db.Exercises.Update(exercises[i]);
        }

        await _db.SaveChangesAsync();
    }
}