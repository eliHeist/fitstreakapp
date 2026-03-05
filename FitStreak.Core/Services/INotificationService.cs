namespace FitStreak.Core.Services;

public interface INotificationService
{
    /// <summary>Schedule a reminder notification for a workout on a given date</summary>
    Task ScheduleWorkoutReminderAsync(int scheduleId, string workoutName, DateTime scheduledDate);

    /// <summary>Schedule an end-of-day reminder if workout not yet completed</summary>
    Task ScheduleEveningReminderAsync(int scheduleId, string workoutName, DateTime scheduledDate);

    /// <summary>Schedule a missed workout notification for the next morning</summary>
    Task ScheduleMissedNotificationAsync(int scheduleId, string workoutName);

    /// <summary>Cancel all notifications for a specific schedule</summary>
    Task CancelNotificationsAsync(int scheduleId);

    /// <summary>Cancel all pending notifications — call when schedule is deleted</summary>
    Task CancelAllNotificationsAsync();

    /// <summary>Request notification permission from the user — call on first launch</summary>
    Task<bool> RequestPermissionAsync();
}