using Microsoft.Maui.Controls;
using Handwerker.Maui.ViewModels;

namespace Handwerker.Maui;

public partial class MainPage : ContentPage
{
    public MainPage(MainPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}