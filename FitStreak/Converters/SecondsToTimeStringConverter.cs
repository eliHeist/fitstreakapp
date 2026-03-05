using System.Globalization;

namespace FitStreak.Converters;

/// <summary>
/// Converts an integer seconds value to a MM:SS display string.
/// e.g. 90 → "1:30", 45 → "0:45"
/// Used in the workout runner timer display.
/// </summary>
public class SecondsToTimeStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int seconds)
        {
            var minutes = seconds / 60;
            var secs = seconds % 60;
            return $"{minutes}:{secs:D2}";
        }

        return "0:00";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}