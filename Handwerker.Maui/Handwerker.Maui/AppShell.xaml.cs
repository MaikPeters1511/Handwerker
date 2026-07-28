using Handwerker.Maui.ViewModels;

namespace Handwerker.Maui;

public partial class AppShell : Shell
{
    private readonly AppShellViewModel _viewModel;
    private ShellContent? _mainItem;
    private ShellContent? _settingsItem;

    public AppShell()
    {
        InitializeComponent();

        // Hole ViewModel aus DI
        _viewModel = App.Services.GetRequiredService<AppShellViewModel>();
        BindingContext = _viewModel;

        var main = App.Services.GetRequiredService<MainPage>();
        var settings = App.Services.GetRequiredService<SettingsPage>();

        var tabBar = new TabBar();

        _mainItem = new ShellContent { Icon = "dotnet_bot.png", Content = main };
        _settingsItem = new ShellContent { Icon = "dotnet_bot.png", Content = settings };

        // Setze initiale Titles
        UpdateTabTitles();

        // Höre auf ViewModel-Änderungen
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.HomeTitle) ||
                e.PropertyName == nameof(_viewModel.SettingsTitle))
            {
                UpdateTabTitles();
            }
        };

        tabBar.Items.Add(_mainItem);
        tabBar.Items.Add(_settingsItem);

        Items.Add(tabBar);
    }

    private void UpdateTabTitles()
    {
        if (_mainItem != null)
            _mainItem.Title = _viewModel.HomeTitle;

        if (_settingsItem != null)
            _settingsItem.Title = _viewModel.SettingsTitle;
    }
}