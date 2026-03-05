using FitStreak.Core.Data;
using FitStreak.Core.Models.Workout;
using Microsoft.EntityFrameworkCore;

namespace FitStreak.Core.Services;

public class ScheduleService : IScheduleService
{
    private readonly AppDbContext _db;

    public ScheduleService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<WorkoutSchedule>> GetTodaySchedulesAsync()
    {
        var today = DateTime.Today;
        return await _db.WorkoutSchedules
            .Include(ws => ws.Workout)
            .Where(ws => ws.ScheduledDate.Date == today)
            .OrderBy(ws => ws.ScheduledDate)
            .ToListAsync();
    }

    public async Task<List<WorkoutSchedule>> GetSchedulesForDateAsync(DateTime date)
    {
        return await _db.WorkoutSchedules
            .Include(ws => ws.Workout)
            .Where(ws => ws.ScheduledDate.Date == date.Date)
            .OrderBy(ws => ws.ScheduledDate)
            .ToListAsync();
    }

    public async Task<List<WorkoutSchedule>> GetSchedulesInRangeAsync(DateTime from, DateTime to)
    {
        return await _db.WorkoutSchedules
            .Include(ws => ws.Workout)
            .Where(ws => ws.ScheduledDate.Date >= from.Date &&
                         ws.ScheduledDate.Date <= to.Date)
            .OrderBy(ws => ws.ScheduledDate)
            .ToListAsync();
    }

    public async Task<List<WorkoutSchedule>> GetMissedSchedulesAsync()
    {
        var today = DateTime.Today;
        return await _db.WorkoutSchedules
            .Include(ws => ws.Workout)
            .Where(ws => ws.Status == ScheduleStatus.Pending &&
                         ws.ScheduledDate.Date < today)
            .OrderByDescending(ws => ws.ScheduledDate)
            .ToListAsync();
    }

    public async Task<WorkoutSchedule> ScheduleWorkoutAsync(
        int workoutId, DateTime date, RecurrenceType recurrence)
    {
        var schedule = new WorkoutSchedule
        {
            WorkoutId = workoutId,
            ScheduledDate = date.Date,
            RecurrenceType = recurrence,
            Status = ScheduleStatus.Pending
        };

        _db.WorkoutSchedules.Add(schedule);
        await _db.SaveChangesAsync();
        return schedule;
    }

    public async Task CompleteScheduleAsync(int scheduleId)
    {
        var schedule = await _db.WorkoutSchedules.FindAsync(scheduleId);
        if (schedule is null) return;

        schedule.Status = ScheduleStatus.Completed;
        schedule.CompletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // Auto-generate next recurrence if this is a repeating schedule
        if (schedule.RecurrenceType != RecurrenceType.None)
            await GenerateNextRecurrenceAsync(schedule);
    }

    public async Task RescheduleAsync(int scheduleId, DateTime newDate)
    {
        var schedule = await _db.WorkoutSchedules.FindAsync(scheduleId);
        if (schedule is null) return;

        // Store original date for history tracking
        schedule.OriginalScheduledDate ??= schedule.ScheduledDate;
        schedule.ScheduledDate = newDate.Date;
        schedule.Status = ScheduleStatus.Rescheduled;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteScheduleAsync(int scheduleId)
    {
        var schedule = await _db.WorkoutSchedules.FindAsync(scheduleId);
        if (schedule is not null)
        {
            _db.WorkoutSchedules.Remove(schedule);
            await _db.SaveChangesAsync();
        }
    }

    public async Task GenerateNextRecurrenceAsync(WorkoutSchedule completed)
    {
        var nextDate = completed.RecurrenceType switch
        {
            RecurrenceType.Weekly => completed.ScheduledDate.AddDays(7),
            RecurrenceType.Monthly => completed.ScheduledDate.AddMonths(1),
            RecurrenceType.Yearly => completed.ScheduledDate.AddYears(1),
            _ => (DateTime?)null
        };

        if (nextDate is null) return;

        // Don't duplicate — check if a schedule already exists for that date
        var exists = await _db.WorkoutSchedules.AnyAsync(ws =>
            ws.WorkoutId == completed.WorkoutId &&
            ws.ScheduledDate.Date == nextDate.Value.Date &&
            ws.Status == ScheduleStatus.Pending);

        if (exists) return;

        var next = new WorkoutSchedule
        {
            WorkoutId = completed.WorkoutId,
            ScheduledDate = nextDate.Value,
            RecurrenceType = completed.RecurrenceType,
            Status = ScheduleStatus.Pending
        };

        _db.WorkoutSchedules.Add(next);
        await _db.SaveChangesAsync();
    }

    public async Task MarkMissedSchedulesAsync()
    {
        var today = DateTime.Today;

        var overdue = await _db.WorkoutSchedules
            .Where(ws => ws.Status == ScheduleStatus.Pending &&
                         ws.ScheduledDate.Date < today)
            .ToListAsync();

        if (overdue.Count == 0) return;

        foreach (var schedule in overdue)
            schedule.Status = ScheduleStatus.Missed;

        await _db.SaveChangesAsync();
    }
}