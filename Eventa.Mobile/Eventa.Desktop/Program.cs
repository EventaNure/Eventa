using System;
using Avalonia;

namespace Eventa.Desktop;

/// <summary>
/// Desktop application is only used for the Visual Studio 2022 designer, desktop version will not be published.
/// </summary>
sealed class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().UsePlatformDetect().WithInterFont().LogToTrace();
}