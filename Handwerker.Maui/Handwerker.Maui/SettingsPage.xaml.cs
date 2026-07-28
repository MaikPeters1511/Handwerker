using Handwerker.Maui.ViewModels;

namespace Handwerker.Maui;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        vm.Initialize();
    }
}
