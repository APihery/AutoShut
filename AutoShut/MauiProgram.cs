using AutoShut.Services;
using AutoShut.ViewModels;
using Microsoft.Extensions.Logging;

namespace AutoShut;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
#if WINDOWS
                fonts.AddFont("Segoe UI", "SegoeUI");
                fonts.AddFont("Segoe UI Semibold", "SegoeUISemibold");
#elif MACCATALYST || IOS
                fonts.AddFont("SF Pro Text", "SFProText");
                fonts.AddFont("SF Pro Display", "SFProDisplay");
#elif ANDROID
                fonts.AddFont("Roboto", "Roboto");
                fonts.AddFont("Roboto Medium", "RobotoMedium");
#endif
            });

        builder.Services.AddSingleton<IBlenderSettingsExtractor, BlenderSettingsExtractor>();
        builder.Services.AddSingleton<IBlenderRenderer, BlenderRenderer>();
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
