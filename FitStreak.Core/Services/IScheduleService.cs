using FitStreak.Core.Models.Workout;

namespace FitStreak.Core.Services;

public interface IScheduleService
{
    /// <summary>Get all schedules for today</summary>
    Task<List<WorkoutSchedule>> GetTodaySchedulesAsync();

    /// <summary>Get all schedules for a specific date</summary>
    Task<List<WorkoutSchedule>> GetSchedulesForDateAsync(DateTime date);

    /// <summary>Get all schedules in a date range — used by streak map and calendar</summary>
    Task<List<WorkoutSchedule>> GetSchedulesInRangeAsync(DateTime from, DateTime to);

    /// <summary>Get all missed schedules — pending and date has passed</summary>
    Task<List<WorkoutSchedule>> GetMissedSchedulesAsync();

    /// <summary>Schedule a workout on a date with optional recurrence</summary>
    Task<WorkoutSchedule> ScheduleWorkoutAsync(int workoutId, DateTime date, RecurrenceType recurrence);

    /// <summary>Mark a scheduled workout as complete</summary>
    Task CompleteScheduleAsync(int scheduleId);

    /// <summary>Reschedule a missed workout to a new date</summary>
    Task RescheduleAsync(int scheduleId, DateTime newDate);

    /// <summary>Delete a schedule entry</summary>
    Task DeleteScheduleAsync(int scheduleId);

    /// <summary>
    /// Generate the next recurrence entry for a completed schedule.
    /// Called automatically after CompleteScheduleAsync.
    /// </summary>
    Task GenerateNextRecurrenceAsync(WorkoutSchedule completed);

    /// <summary>Mark all past pending schedules as Missed — call on app launch</summary>
    Task MarkMissedSchedulesAsync();
}