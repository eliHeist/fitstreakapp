using FitStreak.Core.Models.Workout;
using System.Globalization;

namespace FitStreak.Converters;

/// <summary>
/// Converts a ScheduleStatus enum value to a Color for UI display.
/// Pending = Orange, Completed = Green, Missed = Red, Rescheduled = Blue
/// </summary>
public class StatusToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ScheduleStatus status)
        {
            return status switch
            {
                ScheduleStatus.Pending => Color.FromArgb("#FFA500"),  // Orange
                ScheduleStatus.Completed => Color.FromArgb("#4CAF50"),  // Green
                ScheduleStatus.Missed => Color.FromArgb("#F44336"),  // Red
                ScheduleStatus.Rescheduled => Color.FromArgb("#2196F3"),  // Blue
                _ => Color.FromArgb("#9E9E9E"),  // Grey fallback
            };
        }

        return Color.FromArgb("#9E9E9E");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}