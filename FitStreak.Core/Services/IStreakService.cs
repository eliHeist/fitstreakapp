namespace FitStreak.Core.Services;

public interface IStreakService
{
    /// <summary>
    /// Get workout completion counts per day for the past year.
    /// Key = date, Value = number of workouts completed that day.
    /// Used to render the GitHub-style heatmap.
    /// </summary>
    Task<Dictionary<DateOnly, int>> GetYearlyActivityAsync();

    /// <summary>Current streak — consecutive days with at least one completion up to today</summary>
    Task<int> GetCurrentStreakAsync();

    /// <summary>Longest streak ever recorded</summary>
    Task<int> GetLongestStreakAsync();

    /// <summary>Total number of workouts completed all time</summary>
    Task<int> GetTotalCompletionsAsync();
}