using FitStreak.Core.Models.Workout;
using System.Globalization;

namespace FitStreak.Converters;

/// <summary>
/// Converts a ScheduleStatus to a unicode icon character for display.
/// </summary>
public class StatusToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ScheduleStatus status)
        {
            return status switch
            {
                ScheduleStatus.Pending => "⏳",
                ScheduleStatus.Completed => "✅",
                ScheduleStatus.Missed => "❌",
                ScheduleStatus.Rescheduled => "🔄",
                _ => "❓",
            };
        }

        return "❓";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}