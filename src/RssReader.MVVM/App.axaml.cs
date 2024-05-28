using System;
using System.Reflection;
using System.Threading;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using log4net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MsBox.Avalonia.Enums;
using RssReader.MVVM.Converters;
using RssReader.MVVM.DataAccess;
using RssReader.MVVM.DataAccess.Interfaces;
using RssReader.MVVM.Extensions;
using RssReader.MVVM.Services;
using RssReader.MVVM.Services.Interfaces;
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
            AppDomain.CurrentDomain.SetData("DataDirectory", AppSettings.AppDataPath);

            var dataDirectory = Environment.GetEnvironmentVariable(DATA_DIRECTORY);
            if (dataDirectory == null)
            {
                Environment.SetEnvironmentVariable(DATA_DIRECTORY, AppSettings.AppDataPath);
            }

            var builder = Host.CreateApplicationBuilder();

            builder.Configuration
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile(AppSettings.JSON_FILE_NAME, optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            builder.Logging
                .ClearProviders()
                .AddLog4Net();

            IServiceCollection services = builder.Services;

            services
                .Configure<AppSettings>(builder.Configuration)
                .AddSingleton<ILog>(LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType))
                .AddSingleton<MainWindow>(service => new MainWindow
                {
                    DataContext = new MainViewModel(
                        GetRequiredService<IChannelService>(),
                         GetRequiredService<IExportImport>(),
                         GetRequiredService<IChannelReader>(),
                         GetRequiredService<IChannelItems>(),
                         GetRequiredService<ICategories>(),
                         GetRequiredService<ILog>()
                     )
                })
                //data access 
                .AddTransient<IDatabaseMigrator, DatabaseMigrator>()
                .AddTransient<IChannelsGroups, ChannelsGroups>()
                .AddTransient<IChannels, Channels>()
                .AddTransient<IChannelItems, ChannelItems>()
                .AddTransient<ICategories, Categories>()
                .AddTransient<IIconConverter, IconConverter>()
                //services                
                .AddTransient<IExportImport, ExportImport>()
                .AddTransient<IChannelReader, ChannelReader>()
                .AddTransient<IChannelService, ChannelService>();


            _host = builder.Build();
            _cancellationTokenSource = new();

            try
            {
                GetRequiredService<IDatabaseMigrator>().MigrateDatabase();
                RequestedThemeVariant = Settings.GetTheme();
                desktop.MainWindow = this.TopWindow;
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

    public AppSettings Settings
    {
        get
        {
            var options = GetRequiredService<IOptions<AppSettings>>();
            return options.Value;
        }
    }

    public T GetRequiredService<T>() => _host!.Services.GetRequiredService<T>();

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