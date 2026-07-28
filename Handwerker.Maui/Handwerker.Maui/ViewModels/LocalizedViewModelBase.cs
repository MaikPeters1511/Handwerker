using CommunityToolkit.Mvvm.ComponentModel;
using Handwerker.Maui.Services;

namespace Handwerker.Maui.ViewModels;

/// <summary>
/// Base ViewModel für alle Pages mit automatischer Localization-Update.
/// Erbt von diesem ViewModel, um automatische UI-Updates bei Sprachwechsel zu erhalten.
/// </summary>
public partial class LocalizedViewModelBase : ObservableObject
{
    [ObservableProperty]
    private LocalizationService _localization = LocalizationService.Instance;

    public LocalizedViewModelBase()
    {
        // Höre auf Sprachwechsel und trigger UI-Update
        LocalizationService.Instance.PropertyChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(Localization));
        };
    }

    /// <summary>
    /// Shortcut für Übersetzungen: GetText("Settings")
    /// </summary>
    protected string GetText(string key) => Localization[key];
}
