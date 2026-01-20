using System;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
//using Avalonia.ReactiveUI;
using ReactiveUI.Avalonia;
using log4net.Config;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using RssReader.MVVM.DataAccess.Interfaces;
using RssReader.MVVM.Extensions;
using RssReader.MVVM.ViewModels;
using RssReader.MVVM.Views;

namespace RssReader.MVVM;

public partial class App : Application
{
    private const string DATA_DIRECTORY = "DATA_DIRECTORY";
    private IHost? _host;
    private CancellationTokenSource? _cancellationTokenSource;
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            var builder = new HostBuilder()
                .ConfigureAppConfiguration(configBuilder =>
                {
                    configBuilder
                        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                        .AddJsonFile(AppSettings.JSON_FILE_NAME, optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables();
                })
                .ConfigureLogging(configureLogging =>
                {
                    configureLogging.ClearProviders();
                    configureLogging.AddLog4Net();
                })
                .ConfigureServices((context, services) =>
                {
                    services.UseMicrosoftDependencyResolver();
                    var resolver = Locator.CurrentMutable;
                    resolver.InitializeSplat();
                    resolver.RegisterConstant(new AvaloniaActivationForViewFetcher(), typeof(IActivationForViewFetcher));
                    resolver.RegisterConstant(new AutoDataTemplateBindingHook(), typeof(IPropertyBindingHook));
                    RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;

                    services
                        .Configure<AppSettings>(context.Configuration)
                        .AddDataAccess()
                        .AddServices()
                        .AddViewModels()
                        .AddSingleton<MainWindow>(service => new MainWindow
                        {
                            DataContext = service.GetRequiredService<MainViewModel>()
                        });
                });

            _host = builder.Build();
            _cancellationTokenSource = new();

            try
            {
                AppDomain.CurrentDomain.SetData("DataDirectory", Settings.AppDataPath);
                var dataDirectory = Environment.GetEnvironmentVariable(DATA_DIRECTORY);
                if (dataDirectory == null)
                {
                    Environment.SetEnvironmentVariable(DATA_DIRECTORY, Settings.AppDataPath);
                }

                XmlConfigurator.Configure(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config")));
                GetRequiredService<IDatabaseMigrator>().MigrateDatabase();
                RequestedThemeVariant = Settings.GetTheme();
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                desktop.MainWindow = this.TopWindow;
                desktop.MainWindow.Closing += (s, e) =>
                {
                    var vm = desktop.MainWindow.DataContext as MainViewModel;
                    if (vm != null)
                    {
                        vm.DeleteChannelItems();
                    }
                };
                desktop.ShutdownRequested += OnShutdownRequested;
                _ = _host.StartAsync(_cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // skip
            }
            catch (Exception ex)
            {
                Logger!.LogError(ex, ex.Message);
                ShowMessageBox("Unhandled Error", ex.Message);
                return;
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    #region Logger
    private ILogger<App>? _logger;
    private ILogger<App>? Logger
    {
        get
        {
            if (_logger == null)
            {
                _logger = GetRequiredService<ILogger<App>>();
            }

            return _logger;
        }
    }
    #endregion

    private MainWindow? _window;
    public MainWindow TopWindow
    {
        get
        {
            if (_window == null)
            {
                _window = GetRequiredService<MainWindow>();
            }
            return _window;
        }
    }

    private AppSettings Settings
    {
        get
        {
            var options = GetRequiredService<IOptions<AppSettings>>();
            return options.Value;
        }
    }

    public T GetRequiredService<T>() where T : notnull => _host!.Services.GetRequiredService<T>();

    private void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
        => _ = _host!.StopAsync(_cancellationTokenSource!.Token);

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = (Exception)e.ExceptionObject;
        Logger!.LogError(ex, ex.Message);
        ShowMessageBox("Unhandled Error", ex.Message);
    }

    private void ShowMessageBox(string title, string message)
    {
        var messageBoxStandardWindow = MessageBoxExtension.GetMessageBox(
                                            title, message, ButtonEnum.Ok, Icon.Stop);
        messageBoxStandardWindow.ShowAsync();
    }
}