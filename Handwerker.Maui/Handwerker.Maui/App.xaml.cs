using Microsoft.Extensions.DependencyInjection;

namespace Handwerker.Maui;

public partial class App : Application
{
    public static IServiceProvider Services { get; internal set; } = default!;

    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}