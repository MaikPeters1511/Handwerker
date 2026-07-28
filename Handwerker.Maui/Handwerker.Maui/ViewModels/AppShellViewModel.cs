using CommunityToolkit.Mvvm.ComponentModel;

namespace Handwerker.Maui.ViewModels;

/// <summary>
/// ViewModel für AppShell mit lokalisierten Tab-Titles.
/// </summary>
public partial class AppShellViewModel : LocalizedViewModelBase
{
    public AppShellViewModel()
    {
        UpdateLocalizedTexts();

        // Aktualisiere bei Sprachwechsel
        Localization.PropertyChanged += (_, _) =>
        {
            UpdateLocalizedTexts();
        };
    }

    [ObservableProperty]
    private string _appTitle = string.Empty;

    [ObservableProperty]
    private string _homeTitle = string.Empty;

    [ObservableProperty]
    private string _settingsTitle = string.Empty;

    private void UpdateLocalizedTexts()
    {
        AppTitle = GetText("AppName");
        HomeTitle = GetText("Home");
        SettingsTitle = GetText("Settings");
    }
}
