using Handwerker.Maui.Services;
using System.Globalization;
using Xunit;

namespace Handwerker.Maui.Tests;

/// <summary>
/// Unit-Tests für das Internationalisierungssystem.
/// </summary>
public class LocalizationServiceTests
{
    [Fact]
    public void LocalizationService_IsSingleton()
    {
        // Arrange & Act
        var instance1 = LocalizationService.Instance;
        var instance2 = LocalizationService.Instance;

        // Assert
        Assert.Same(instance1, instance2);
    }

    [Theory]
    [InlineData("de")]
    [InlineData("en")]
    public void LocalizationService_SupportsLanguage(string languageCode)
    {
        // Act
        var isSupported = LocalizationService.IsLanguageSupported(languageCode);

        // Assert
        Assert.True(isSupported);
    }

    [Fact]
    public void LocalizationService_ChangesLanguage()
    {
        // Arrange
        var service = LocalizationService.Instance;
        var originalCulture = service.CurrentCulture;

        // Act
        service.ChangeLanguage("en");

        // Assert
        Assert.Equal("en", service.CurrentCulture.TwoLetterISOLanguageName);
        Assert.NotEqual(originalCulture.Name, service.CurrentCulture.Name);

        // Cleanup - zurück zur ursprünglichen Sprache
        service.ChangeLanguage(originalCulture.TwoLetterISOLanguageName);
    }

    [Fact]
    public void LocalizationService_ReturnsCorrectTranslation_German()
    {
        // Arrange
        var service = LocalizationService.Instance;
        service.ChangeLanguage("de");

        // Act
        var translation = service["SettingsTitle"];

        // Assert
        Assert.Equal("Einstellungen", translation);
    }

    [Fact]
    public void LocalizationService_ReturnsCorrectTranslation_English()
    {
        // Arrange
        var service = LocalizationService.Instance;
        service.ChangeLanguage("en");

        // Act
        var translation = service["SettingsTitle"];

        // Assert
        Assert.Equal("Settings", translation);
    }

    [Fact]
    public void LocalizationService_ReturnsKeyForMissingTranslation()
    {
        // Arrange
        var service = LocalizationService.Instance;
        var nonExistentKey = "ThisKeyDoesNotExist";

        // Act
        var result = service[nonExistentKey];

        // Assert
        Assert.Equal(nonExistentKey, result);
    }

    [Fact]
    public void LocalizationService_GetSupportedLanguages_ReturnsCorrectCount()
    {
        // Act
        var languages = LocalizationService.GetSupportedLanguages();

        // Assert
        Assert.Equal(2, languages.Count); // Deutsch + Englisch
        Assert.Contains(languages, l => l.Code == "de");
        Assert.Contains(languages, l => l.Code == "en");
    }

    [Fact]
    public void LanguageOption_HasCorrectProperties()
    {
        // Arrange
        var languages = LocalizationService.GetSupportedLanguages();
        var german = languages.First(l => l.Code == "de");

        // Assert
        Assert.Equal("de", german.Code);
        Assert.Equal("Deutsch", german.DisplayName);
        Assert.Equal("🇩🇪", german.Flag);
    }

    [Fact]
    public void LocalizationService_UpdatesCultureInfo()
    {
        // Arrange
        var service = LocalizationService.Instance;

        // Act
        service.ChangeLanguage("en");

        // Assert
        Assert.Equal("en", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
        Assert.Equal("en", CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
    }
}
