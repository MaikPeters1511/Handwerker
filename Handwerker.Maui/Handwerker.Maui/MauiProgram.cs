using System;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Handwerker.Maui.ViewModels;
using Handwerker.Maui.Services;
using Microsoft.Maui.Devices;
using System.Net.Http;

namespace Handwerker.Maui;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>().ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        }).UseMauiCommunityToolkit();

        // Localization Service (Singleton für App-weite Sprachverwaltung)
        builder.Services.AddSingleton<LocalizationService>(_ => LocalizationService.Instance);

        // HttpClient für API-Service konfigurieren
        // Für Android Emulator: API ist unter http://10.0.2.2:PORT erreichbar
        // Für iOS Simulator: API ist unter http://localhost:PORT erreichbar
        // Für physische Geräte: Verwende die IP-Adresse des Entwicklungsrechners
        var apiBaseUrl = GetApiBaseUrl();
        
        // Einige MAUI-Projekt-Setups erkennen AddHttpClient-Erweiterungsmethoden nicht zuverlässig.
        // Registriere stattdessen einen HttpClient direkt.
        builder.Services.AddSingleton<HttpClient>(sp => new HttpClient
        {
            BaseAddress = new Uri(apiBaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        });

        // Register services and viewmodels
        builder.Services.AddSingleton<AppShellViewModel>();
        builder.Services.AddSingleton<MainPageViewModel>();
        builder.Services.AddSingleton<SettingsViewModel>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<SettingsPage>();

        // Register App so DI can construct it with IServiceProvider
        builder.Services.AddSingleton<App>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // expose service provider for AppShell/Pages to resolve DI-registered pages
        App.Services = app.Services;

        return app;
    }

    private static string GetApiBaseUrl()
    {
        // Für Android Emulator: 10.0.2.2 ist die spezielle IP für den Host-Rechner
        // Für iOS Simulator: localhost funktioniert
        // Für physische Geräte: Ersetze mit der tatsächlichen IP deines Entwicklungsrechners
#if ANDROID
        return DeviceInfo.DeviceType == DeviceType.Virtual
            ? "http://10.0.2.2:5000" // Android Emulator - PORT anpassen!
            : "http://192.168.1.100:5000"; // Physisches Android-Gerät - IP und PORT anpassen!
#elif IOS
        return DeviceInfo.DeviceType == DeviceType.Virtual
            ? "http://localhost:5000" // iOS Simulator - PORT anpassen!
            : "http://192.168.1.100:5000"; // Physisches iOS-Gerät - IP und PORT anpassen!
#else
        return "http://localhost:5000"; // Windows/Mac Desktop - PORT anpassen!
#endif
    }
}