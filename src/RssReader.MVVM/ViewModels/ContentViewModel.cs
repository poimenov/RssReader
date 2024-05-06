using System;
using System.Reactive;
using ReactiveUI;
using RssReader.MVVM.DataAccess.Interfaces;
using RssReader.MVVM.Extensions;
using RssReader.MVVM.Models;

namespace RssReader.MVVM.ViewModels;

public class ContentViewModel : ViewModelBase
{
    private readonly IChannelItems _channelItems;
    public ContentViewModel(IChannelItems channelItems)
    {
        _channelItems = channelItems;
        OpenLinkCommand = CreateOpenLinkCommand();
    }

    private ChannelItemModel? _selectedChannelItem;
    public ChannelItemModel? SelectedChannelItem
    {
        get => _selectedChannelItem;
        set => this.RaiseAndSetIfChanged(ref _selectedChannelItem, value);
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