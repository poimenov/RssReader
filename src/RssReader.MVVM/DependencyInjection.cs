using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using RssReader.MVVM.DataAccess;
using RssReader.MVVM.DataAccess.Interfaces;
using RssReader.MVVM.Services;
using RssReader.MVVM.Services.Interfaces;
using RssReader.MVVM.ViewModels;
using Splat;

namespace RssReader.MVVM;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services)
    {
        services.AddTransient<IDatabaseMigrator, DatabaseMigrator>()
                .AddTransient<IChannelsGroups, ChannelsGroups>()
                .AddTransient<IChannels, Channels>()
                .AddTransient<IChannelItems, ChannelItems>()
                .AddTransient<ICategories, Categories>();

        return services;
    }
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddTransient<IIconConverter, IconConverter>()
                .AddTransient<IThemeService, ThemeService>()
                .AddTransient<IFilePickerService, FilePickerService>()
                .AddTransient<IClipboardService, ClipboardService>()
                .AddTransient<IPlatformService, PlatformService>()
                .AddTransient<IProcessService, ProcessService>()
                .AddTransient<ILinkOpeningService, LinkOpeningService>()
                .AddTransient<IDispatcherWrapper, DispatcherWrapper>()
                .AddTransient<IHttpHandler, HttpHandler>()
                .AddTransient<IExportImport, ExportImport>()
                .AddTransient<IChannelReader, ChannelReader>()
                .AddTransient<IChannelModelUpdater, ChannelModelUpdater>()
                .AddTransient<IChannelService, ChannelService>();

        return services;
    }

    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddSingleton<MainViewModel>();
        services.AddTransient<ItemsViewModel>();
        services.AddTransient<ContentViewModel>();
        services.AddTransient<HeaderViewModel>();
        services.AddTransient<TreeViewModel>();
        Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());

        return services;
    }
}
