using System.Globalization;
using Handwerker.Maui.Services;

namespace Handwerker.Maui.Extensions;

/// <summary>
/// Converter für die Anzeige von LanguageOption im Picker.
/// Kombiniert Flag und DisplayName: "🇩🇪 Deutsch"
/// </summary>
public class LanguageDisplayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is LanguageOption language)
        {
            return $"{language.Flag} {language.DisplayName}";
        }
        return value?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
