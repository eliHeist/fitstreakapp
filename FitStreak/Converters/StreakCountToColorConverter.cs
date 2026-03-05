using System.Globalization;

namespace FitStreak.Converters;

/// <summary>
/// Converts a workout completion count for a day into a heatmap color.
/// Mirrors the GitHub contribution graph color scale.
/// 0 = no activity (grey), 1 = light green, 2 = medium green, 3+ = dark green
/// </summary>
public class StreakCountToColorConverter : IValueConverter
{
    // Colors match GitHub-style heatmap
    private static readonly Color NoActivity = Color.FromArgb("#1E1E1E"); // dark grey
    private static readonly Color LowActivity = Color.FromArgb("#0E4429"); // darkest github green
    private static readonly Color MidActivity = Color.FromArgb("#006D32"); // mid green
    private static readonly Color HighActivity = Color.FromArgb("#26A641"); // bright green
    private static readonly Color MaxActivity = Color.FromArgb("#39D353"); // brightest green

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            return count switch
            {
                0 => NoActivity,
                1 => LowActivity,
                2 => MidActivity,
                3 => HighActivity,
                >= 4 => MaxActivity,
                _ => NoActivity
            };
        }

        return NoActivity;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}