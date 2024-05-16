using System;
using System.Reactive;
using Avalonia.Media.Imaging;
using ReactiveUI;
using RssReader.MVVM.Extensions;
using RssReader.MVVM.Models;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.ViewModels;

public class ContentViewModel : ViewModelBase
{
    private readonly IChannelService _channelService;
    public ContentViewModel(IChannelService channelService)
    {
        _channelService = channelService;
        OpenLinkCommand = CreateOpenLinkCommand();
        this.WhenAnyValue(x => x.SelectedChannelItem)
            .WhereNotNull()
            .Subscribe(channelItem =>
            {
                ChannelImageSource = _channelService.GetChannelModel(channelItem.ChannelId)!.ImageSource;
            });
    }

    private ChannelItemModel? _selectedChannelItem;
    public ChannelItemModel? SelectedChannelItem
    {
        get => _selectedChannelItem;
        set => this.RaiseAndSetIfChanged(ref _selectedChannelItem, value);
    }

    private Bitmap? _channelImageSource;
    public Bitmap? ChannelImageSource
    {
        get => _channelImageSource;
        set => this.RaiseAndSetIfChanged(ref _channelImageSource, value);
    }

    public IReactiveCommand OpenLinkCommand { get; }
    private IReactiveCommand CreateOpenLinkCommand()
    {
        return ReactiveCommand.Create<string, Unit>(
            (link) =>
            {
                link.OpenUrl();
                return Unit.Default;
            }
        );
    }

}