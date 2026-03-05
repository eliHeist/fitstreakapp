using FitStreak.Core.Services;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;

namespace FitStreak.Services;

public class NotificationService : FitStreak.Core.Services.INotificationService
{
    // Notification ID ranges — prevents collisions between notification types
    // Morning reminders:  scheduleId * 10 + 0
    // Evening reminders:  scheduleId * 10 + 1
    // Missed reminders:   scheduleId * 10 + 2

    public async Task<bool> RequestPermissionAsync()
    {
        return await LocalNotificationCenter.Current.RequestNotificationPermission();
    }

    public async Task ScheduleWorkoutReminderAsync(
        int scheduleId, string workoutName, DateTime scheduledDate)
    {
        // Fire at 8:00 AM on the scheduled day
        var notifyTime = scheduledDate.Date.AddHours(8);

        // Don't schedule if the time has already passed
        if (notifyTime <= DateTime.Now) return;

        var request = new NotificationRequest
        {
            NotificationId = scheduleId * 10 + 0,
            Title = "Workout Reminder",
            Description = $"You have {workoutName} scheduled today.",
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = notifyTime,
            },
            Android = new AndroidOptions
            {
                ChannelId = "workout_reminders",
                Priority = AndroidPriority.Default,
            }
        };

        await LocalNotificationCenter.Current.Show(request);
    }

    public async Task ScheduleEveningReminderAsync(
        int scheduleId, string workoutName, DateTime scheduledDate)
    {
        // Fire at 8:00 PM on the scheduled day as a follow-up if not done
        var notifyTime = scheduledDate.Date.AddHours(20);

        if (notifyTime <= DateTime.Now) return;

        var request = new NotificationRequest
        {
            NotificationId = scheduleId * 10 + 1,
            Title = "Don't forget!",
            Description = $"{workoutName} is still pending today.",
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = notifyTime,
            },
            Android = new AndroidOptions
            {
                ChannelId = "workout_reminders",
                Priority = AndroidPriority.Default,
            }
        };

        await LocalNotificationCenter.Current.Show(request);
    }

    public async Task ScheduleMissedNotificationAsync(int scheduleId, string workoutName)
    {
        // Fire next morning at 9:00 AM
        var notifyTime = DateTime.Today.AddDays(1).AddHours(9);

        var request = new NotificationRequest
        {
            NotificationId = scheduleId * 10 + 2,
            Title = "Missed Workout",
            Description = $"{workoutName} was missed. Tap to reschedule.",
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = notifyTime,
            },
            Android = new AndroidOptions
            {
                ChannelId = "workout_reminders",
                Priority = AndroidPriority.High,
            }
        };

        await LocalNotificationCenter.Current.Show(request);
    }

    public Task CancelNotificationsAsync(int scheduleId)
    {
        // Cancel all three notification types for this schedule
        LocalNotificationCenter.Current.Cancel(
            scheduleId * 10 + 0,
            scheduleId * 10 + 1,
            scheduleId * 10 + 2);

        return Task.CompletedTask;
    }

    public Task CancelAllNotificationsAsync()
    {
        LocalNotificationCenter.Current.CancelAll();
        return Task.CompletedTask;
    }
}