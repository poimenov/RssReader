using System;
using System.Collections;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
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
        CopyLinkCommand = CreateCopyLinkCommand();
        _unreadItemsCountChanged = false;
        _starredCount = _channelService.GetStarredCount();
        _readLaterCount = _channelService.GetReadLaterCount();
        this.WhenAnyValue(x => x.SelectedChannelItem)
            .WhereNotNull()
            .Subscribe(channelItem =>
            {
                SelectedChannelModel = _channelService.GetChannelModel(channelItem.ChannelId);
                ChannelImageSource = SelectedChannelModel!.ImageSource;
                ItemButtonsViewModel = new ItemButtonsViewModel(channelItem);
                ItemButtonsViewModel.SelectedChannelItem.WhenAnyValue(x => x.IsRead)
                .Subscribe(x =>
                {
                    _channelService.UpdateChannelItem(channelItem);
                    var count = _channelService.GetChannelUnreadCount(SelectedChannelModel.Id);
                    if (SelectedChannelModel.UnreadItemsCount != count)
                    {
                        SelectedChannelModel.UnreadItemsCount = count;
                        UnreadItemsCountChanged = !UnreadItemsCountChanged;
                    }
                });
                channelItem.IsRead = true;
                ItemButtonsViewModel.SelectedChannelItem.WhenAnyValue(x => x.IsFavorite)
                .Subscribe(x =>
                {
                    _channelService.UpdateChannelItem(channelItem);
                    StarredCount = _channelService.GetStarredCount();
                });
                ItemButtonsViewModel.SelectedChannelItem.WhenAnyValue(x => x.IsReadLater)
                .Subscribe(x =>
                {
                    _channelService.UpdateChannelItem(channelItem);
                    ReadLaterCount = _channelService.GetReadLaterCount();
                });
                ItemButtonsViewModel.SelectedChannelItem.WhenAnyValue(x => x.IsDeleted)
                .Subscribe(x =>
                {
                    if (x)
                    {
                        channelItem.IsRead = x;
                        channelItem.IsFavorite = !x;
                        channelItem.IsReadLater = !x;
                    }

                    _channelService.UpdateChannelItem(channelItem);
                    ReadLaterCount = _channelService.GetReadLaterCount();
                });
            });
    }

    private bool _unreadItemsCountChanged;
    public bool UnreadItemsCountChanged
    {
        get => _unreadItemsCountChanged;
        set => this.RaiseAndSetIfChanged(ref _unreadItemsCountChanged, value);
    }

    private int _starredCount;
    public int StarredCount
    {
        get => _starredCount;
        set => this.RaiseAndSetIfChanged(ref _starredCount, value);
    }

    private int _readLaterCount;
    public int ReadLaterCount
    {
        get => _readLaterCount;
        set => this.RaiseAndSetIfChanged(ref _readLaterCount, value);
    }

    private ChannelItemModel? _selectedChannelItem;
    public ChannelItemModel? SelectedChannelItem
    {
        get => _selectedChannelItem;
        set => this.RaiseAndSetIfChanged(ref _selectedChannelItem, value);
    }

    private ChannelModel? _selectedChannelModel;
    public ChannelModel? SelectedChannelModel
    {
        get => _selectedChannelModel;
        set => this.RaiseAndSetIfChanged(ref _selectedChannelModel, value);
    }

    private IEnumerable? _itemsSource;
    public IEnumerable? ItemsSource
    {
        get => _itemsSource;
        set => this.RaiseAndSetIfChanged(ref _itemsSource, value);
    }

    private Bitmap? _channelImageSource;
    public Bitmap? ChannelImageSource
    {
        get => _channelImageSource;
        set => this.RaiseAndSetIfChanged(ref _channelImageSource, value);
    }

    private ItemButtonsViewModel? _itemButtonsViewModel;
    public ItemButtonsViewModel? ItemButtonsViewModel
    {
        get => _itemButtonsViewModel;
        set => this.RaiseAndSetIfChanged(ref _itemButtonsViewModel, value);
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

    public IReactiveCommand CopyLinkCommand { get; }
    private IReactiveCommand CreateCopyLinkCommand()
    {
        return ReactiveCommand.CreateFromTask<string, Unit>(
            async (link) =>
            {
                if (Application.Current is App app && app.TopWindow is not null && !string.IsNullOrWhiteSpace(link))
                {
                    var clipboard = TopLevel.GetTopLevel(app.TopWindow)?.Clipboard;
                    if (clipboard is not null)
                    {
                        await clipboard.SetTextAsync(link);
                    }
                }

                return Unit.Default;
            }
        );
    }

}