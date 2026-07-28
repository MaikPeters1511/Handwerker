using System.Globalization;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;

namespace Handwerker.Maui.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        if (value is bool b && b)
            return Colors.Green;
        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotSupportedException();
    }
}
