using Microsoft.Extensions.DependencyInjection;

namespace AutoShut;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        UserAppTheme = AppTheme.Dark;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = _serviceProvider.GetRequiredService<AppShell>();
        var window = new Window(shell);
        
#if WINDOWS
        window.Width = 1000;
        window.Height = 700;
        window.MinimumWidth = 800;
        window.MinimumHeight = 600;
#endif
        
        return window;
    }
}
