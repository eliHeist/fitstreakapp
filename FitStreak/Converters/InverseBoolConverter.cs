using System.Globalization;

namespace FitStreak.Converters;

/// <summary>
/// Inverts a boolean value.
/// Used in XAML to show elements when a condition is false
/// e.g. show empty state when IsEmpty=true, hide list when IsEmpty=true
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b) return !b;
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b) return !b;
        return value;
    }
}