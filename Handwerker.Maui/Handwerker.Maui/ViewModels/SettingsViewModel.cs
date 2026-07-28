using CommunityToolkit.Mvvm.ComponentModel;
using Handwerker.Maui.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Handwerker.Maui.ViewModels;

public partial class SettingsViewModel(LocalizationService localizationService) : LocalizedViewModelBase
{
    const string PrefKey = "app_theme_dark";

    private readonly LocalizationService _localizationService = localizationService;

    public SettingsViewModel() : this(LocalizationService.Instance)
    {
    }

    // Theme
    [ObservableProperty]
    private bool _isDark = Preferences.Default.Get(PrefKey, false);

    // Sprachen
    public ObservableCollection<LanguageOption> AvailableLanguages { get; } =
        new(LocalizationService.GetSupportedLanguages());

    [ObservableProperty]
    private LanguageOption? _selectedLanguage;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(IsDark))
        {
            Preferences.Default.Set(PrefKey, IsDark);
            ApplyTheme(IsDark);
        }
    }

    // Initialisierung nach Konstruktor
    public void Initialize()
    {
        ApplyTheme(IsDark);

        // Aktuell ausgewählte Sprache setzen
        var currentCode = _localizationService.CurrentCulture.TwoLetterISOLanguageName;
        SelectedLanguage = AvailableLanguages.FirstOrDefault(l => l.Code == currentCode);

        System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] Initialize - Current language: {currentCode}");
    }

    partial void OnSelectedLanguageChanged(LanguageOption? value)
    {
        if (value is not null && value.Code != _localizationService.CurrentCulture.TwoLetterISOLanguageName)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] Language changed to: {value.Code}");
            _localizationService.ChangeLanguage(value.Code);

            // Trigger UI-Update für alle lokalisierten Texte
            OnPropertyChanged(nameof(Localization));
        }
    }

    private static void ApplyTheme(bool dark)
    {
        var theme = dark ? AppTheme.Dark : AppTheme.Light;
        Application.Current?.Dispatcher.Dispatch(() => Application.Current.UserAppTheme = theme);
    }
}
