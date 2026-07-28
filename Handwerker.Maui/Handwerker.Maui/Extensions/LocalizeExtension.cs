using Handwerker.Maui.Services;
using System.ComponentModel;

namespace Handwerker.Maui.Extensions;

/// <summary>
/// XAML MarkupExtension für Localization mit automatischer Update-Logik.
/// Usage: Text="{local:Localize WelcomeMessage}"
/// </summary>
[ContentProperty(nameof(Key))]
public class LocalizeExtension : IMarkupExtension<BindingBase>
{
    public string Key { get; set; } = string.Empty;

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        return new Binding
        {
            Mode = BindingMode.OneWay,
            Path = $"[{Key}]",
            Source = LocalizationService.Instance
        };
    }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
    {
        return ProvideValue(serviceProvider);
    }
}
