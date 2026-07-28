using System.Globalization;
using Microsoft.Maui.Controls;
using Handwerker.Maui.Services;

namespace Handwerker.Maui.Converters;

public class OnlineTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        if (value is bool b && b)
            return LocalizationService.Instance["Online"];
        return LocalizationService.Instance["Offline"];
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotSupportedException();
    }
}
