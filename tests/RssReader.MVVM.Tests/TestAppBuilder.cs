using Avalonia;
using Avalonia.Headless;
using RssReader.MVVM.Tests;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace RssReader.MVVM.Tests;
public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

