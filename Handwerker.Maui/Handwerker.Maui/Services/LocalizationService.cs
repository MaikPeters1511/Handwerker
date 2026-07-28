using System.ComponentModel;
using System.Globalization;

namespace Handwerker.Maui.Services;

/// <summary>
/// Service für Internationalisierung mit Runtime-Sprachwechsel.
/// Nutzt INotifyPropertyChanged für automatische XAML-Updates via LocalizeExtension.
/// </summary>
public sealed class LocalizationService : INotifyPropertyChanged
{
    private const string LanguagePreferenceKey = "app_language";
    private static readonly Lazy<LocalizationService> _instance = new(() => new LocalizationService());

    public static LocalizationService Instance => _instance.Value;

    private CultureInfo _currentCulture;

    private LocalizationService()
    {
        // Lade gespeicherte Sprache oder nutze System-Sprache
        var savedLanguage = Preferences.Default.Get(LanguagePreferenceKey, string.Empty);

        if (!string.IsNullOrEmpty(savedLanguage))
        {
            _currentCulture = new CultureInfo(savedLanguage);
        }
        else
        {
            _currentCulture = CultureInfo.CurrentUICulture;

            // Fallback auf Deutsch wenn Systemsprache nicht unterstützt
            if (!IsLanguageSupported(_currentCulture.TwoLetterISOLanguageName))
            {
                _currentCulture = new CultureInfo("de");
            }
        }

        SetCulture(_currentCulture);
    }

    public CultureInfo CurrentCulture
    {
        get => _currentCulture;
        private set
        {
            if (_currentCulture.Name != value.Name)
            {
                _currentCulture = value;
                SetCulture(value);
                Preferences.Default.Set(LanguagePreferenceKey, value.Name);

                // Benachrichtige alle Bindings dass sich die Sprache geändert hat
                OnPropertyChanged(nameof(CurrentCulture));
                OnPropertyChanged("Item[]"); // Indexer-Property
                OnPropertyChanged(string.Empty); // Alle Properties
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Indexer für XAML-Binding. Gibt lokalisierten String aus AppResources zurück.
    /// </summary>
    public string this[string key] => Resources.Localization.AppResources.ResourceManager.GetString(key, CurrentCulture) ?? key;

    /// <summary>
    /// Ändert die App-Sprache zur Laufzeit.
    /// </summary>
    public void ChangeLanguage(string languageCode)
    {
        System.Diagnostics.Debug.WriteLine($"[LocalizationService] ChangeLanguage called with: {languageCode}");
        System.Diagnostics.Debug.WriteLine($"[LocalizationService] Current culture before: {_currentCulture.Name}");

        var culture = new CultureInfo(languageCode);
        CurrentCulture = culture;

        System.Diagnostics.Debug.WriteLine($"[LocalizationService] Current culture after: {_currentCulture.Name}");
        System.Diagnostics.Debug.WriteLine($"[LocalizationService] Test translation 'Settings': {this["Settings"]}");
    }

    private void SetCulture(CultureInfo culture)
    {
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        // Stelle sicher, dass AppResources die neue Culture nutzt
        Resources.Localization.AppResources.Culture = culture;
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Prüft ob Sprache unterstützt wird (de, en).
    /// </summary>
    public static bool IsLanguageSupported(string languageCode)
    {
        return languageCode is "de" or "en";
    }

    /// <summary>
    /// Gibt alle unterstützten Sprachen zurück.
    /// </summary>
    public static IReadOnlyList<LanguageOption> GetSupportedLanguages()
    {
        return new List<LanguageOption>
        {
            new("de", "Deutsch", "🇩🇪"),
            new("en", "English", "🇬🇧")
        };
    }
}

/// <summary>
/// Repräsentiert eine Sprachoption für die UI.
/// </summary>
public record LanguageOption(string Code, string DisplayName, string Flag);
