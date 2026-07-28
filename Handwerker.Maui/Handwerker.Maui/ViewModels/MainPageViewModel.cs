using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Handwerker.Maui.ViewModels;

public partial class MainPageViewModel : LocalizedViewModelBase
{
    // Primary constructor style for DI-ready ViewModel
    public MainPageViewModel()
    {
        // Nutze lokalisierte Texte statt hardcodierte Strings
        UpdateLocalizedTexts();

        // Aktualisiere Texte bei Sprachwechsel
        Localization.PropertyChanged += (_, _) =>
        {
            UpdateLocalizedTexts();
        };
    }

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _subtitle = string.Empty;

    [ObservableProperty]
    private bool _isOnline = true;

    // Computed Property für Online-Status-Text
    public string OnlineStatusText => IsOnline ? GetText("Online") : GetText("Offline");

    partial void OnIsOnlineChanged(bool value)
    {
        OnPropertyChanged(nameof(OnlineStatusText));
    }

    private void UpdateLocalizedTexts()
    {
        Title = GetText("WelcomeMessage");
        Subtitle = GetText("HelloWorld");
        OnPropertyChanged(nameof(OnlineStatusText));
    }

    [RelayCommand]
    private Task StartJobAsync()
    {
        // Minimal: Navigations- oder Job-Service-Aufruf später injizieren
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task SyncAsync()
    {
        // Simulierter Sync: in real app, call SyncService with retry/backoff
        await Task.Delay(600);
    }
}
