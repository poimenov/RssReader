using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using ReactiveUI;
using RssReader.MVVM.DataAccess;
using RssReader.MVVM.Models;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IChannelService _channelService;
    private readonly IExportImport _exportImport;
    private readonly IChannelReader _channelReader;

    public MainViewModel(IChannelService channelService, IExportImport exportImport, IChannelReader channelReader)
    {
        _channelService = channelService;
        _exportImport = exportImport;
        _channelReader = channelReader;
        _isPaneOpen = true;
        TriggerPaneCommand = CreateTriggerPaneCommand();
        OpenItemLinkCommand = CreateOpenItemLinkCommand();
        OpenChannelLinkCommand = CreateOpenChannelLinkCommand();
        ChannelsTreeViewModel = new ChannelsTreeViewModel(_channelService);
        ChannelsTreeViewModel.WhenAnyValue(x => x.SelectedChannelModel)
            .Where(x => x != null)
            .Subscribe(x =>
            {
                SelectedChannelItemsViewModel = new ChannelItemsViewModel(new ChannelItems())
                {
                    ChannelModel = x
                };

                SelectedChannelItemsViewModel.WhenAnyValue(x => x.SelectedChannelItem)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    SelectedChannelItem = _channelService.GetChannelItem(x!.Id);
                });
            });

        // _exportImport.Import("/home/poimenov/Desktop/feedly.opml");
        // _channelReader.ReadAllChannelsAsync();        
    }

    private bool _isPaneOpen;
    public bool IsPaneOpen
    {
        get => _isPaneOpen;
        set => this.RaiseAndSetIfChanged(ref _isPaneOpen, value);
    }


    private ChannelItemModel? _selectedChannelItem;
    public ChannelItemModel? SelectedChannelItem
    {
        get => _selectedChannelItem;
        set => this.RaiseAndSetIfChanged(ref _selectedChannelItem, value);
    }

    public ChannelsTreeViewModel ChannelsTreeViewModel { get; private set; }

    private ChannelItemsViewModel? _selectedChannelItemsViewModel;
    public ChannelItemsViewModel? SelectedChannelItemsViewModel
    {
        get => _selectedChannelItemsViewModel;
        set => this.RaiseAndSetIfChanged(ref _selectedChannelItemsViewModel, value);
    }

    #region Commands
    public IReactiveCommand TriggerPaneCommand { get; }

    private IReactiveCommand CreateTriggerPaneCommand()
    {
        return ReactiveCommand.Create(
        () =>
            {
                IsPaneOpen = !IsPaneOpen;
            }
        );
    }

    public IReactiveCommand OpenItemLinkCommand { get; }

    private IReactiveCommand CreateOpenItemLinkCommand()
    {
        return ReactiveCommand.Create(
        () =>
            {
                Open(SelectedChannelItemsViewModel?.SelectedChannelItem?.Link);
            }
        );
    }

    public IReactiveCommand OpenChannelLinkCommand { get; }
    private IReactiveCommand CreateOpenChannelLinkCommand()
    {
        return ReactiveCommand.Create(
        () =>
            {
                Open(SelectedChannelItemsViewModel?.ChannelModel?.Link);
            }
        );
    }
    #endregion

    private static void Open(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        if (Uri.IsWellFormedUriString(path, UriKind.Absolute))
        {
            path = $"\"{path}\"";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = path });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", path);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", path);
            }
        }
    }
}
