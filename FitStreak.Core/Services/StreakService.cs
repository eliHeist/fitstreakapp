using FitStreak.Core.Data;
using FitStreak.Core.Models.Workout;
using Microsoft.EntityFrameworkCore;

namespace FitStreak.Core.Services;

public class StreakService : IStreakService
{
    private readonly AppDbContext _db;

    public StreakService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Dictionary<DateOnly, int>> GetYearlyActivityAsync()
    {
        var from = DateTime.Today.AddYears(-1);
        var to = DateTime.Today;

        // Get all completed schedules in the past year
        var completions = await _db.WorkoutSchedules
            .Where(ws => ws.Status == ScheduleStatus.Completed &&
                         ws.CompletedAt.HasValue &&
                         ws.CompletedAt.Value.Date >= from &&
                         ws.CompletedAt.Value.Date <= to)
            .Select(ws => ws.CompletedAt!.Value.Date)
            .ToListAsync();

        // Group by date and count completions per day
        return completions
            .GroupBy(date => DateOnly.FromDateTime(date))
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<int> GetCurrentStreakAsync()
    {
        var activity = await GetYearlyActivityAsync();

        int streak = 0;
        var day = DateOnly.FromDateTime(DateTime.Today);

        // Walk backwards from today counting consecutive active days
        while (activity.ContainsKey(day))
        {
            streak++;
            day = day.AddDays(-1);
        }

        return streak;
    }

    public async Task<int> GetLongestStreakAsync()
    {
        // Get all completed dates ever (not just past year)
        var completions = await _db.WorkoutSchedules
            .Where(ws => ws.Status == ScheduleStatus.Completed &&
                         ws.CompletedAt.HasValue)
            .Select(ws => DateOnly.FromDateTime(ws.CompletedAt!.Value.Date))
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();

        if (completions.Count == 0) return 0;

        int longest = 1;
        int current = 1;

        for (int i = 1; i < completions.Count; i++)
        {
            // Check if this date is exactly one day after the previous
            if (completions[i] == completions[i - 1].AddDays(1))
            {
                current++;
                if (current > longest)
                    longest = current;
            }
            else
            {
                current = 1;
            }
        }

        return longest;
    }

    public async Task<int> GetTotalCompletionsAsync()
    {
        return await _db.WorkoutSchedules
            .CountAsync(ws => ws.Status == ScheduleStatus.Completed);
    }
}