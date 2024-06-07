using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Media.Imaging;
using ReactiveUI;
using RssReader.MVVM.DataAccess.Interfaces;
using RssReader.MVVM.DataAccess.Models;
using RssReader.MVVM.Models;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.ViewModels;

public class ContentViewModel : ViewModelBase
{
    private readonly IChannelService _channelService;
    private readonly ICategories _categories;
    private readonly ILinkOpeningService _linkOpeningService;
    private readonly IClipboardService _clipboardService;
    private readonly IThemeService _themeService;
    public ContentViewModel(IChannelService channelService, ICategories categories, ILinkOpeningService linkOpeningService, IClipboardService clipboardService, IThemeService themeService)
    {
        _channelService = channelService;
        _categories = categories;
        _linkOpeningService = linkOpeningService;
        _clipboardService = clipboardService;
        _themeService = themeService;
        OpenLinkCommand = CreateOpenLinkCommand();
        CopyLinkCommand = CreateCopyLinkCommand();
        ViewPostsCommand = CreateViewPostsCommand();

        _themeService.WhenAnyValue(x => x.RequestedThemeVariant)
        .Subscribe(x =>
        {
            var pallete = _themeService.GetColorPaletteResource(_themeService.ActualThemeVariant);
            if (pallete is not null)
            {
                Css = @$"body {{ color: {pallete.BaseHigh};}}
                div {{ color: {pallete.BaseHigh};}}
                span {{ color: {pallete.BaseHigh};}}
                table {{ color: {pallete.BaseHigh};}}
                b {{ color: {pallete.BaseHigh};}}
                h1, h2, h3 {{ color: {pallete.BaseHigh}; }}
                p {{ color: {pallete.BaseHigh};}}";
            }
        });

        _unreadItemsCountChanged = false;
        _starredCount = _channelService.GetStarredCount();
        _readLaterCount = _channelService.GetReadLaterCount();
        this.WhenAnyValue(x => x.SearchName)
        .WhereNotNull()
        .Subscribe(x =>
        {
            SearchCategories = _categories.GetByName(x);
        });

        this.WhenAnyValue(x => x.SelectedChannelItem)
            .WhereNotNull()
            .Subscribe(channelItem =>
            {
                ItemCategories = _categories.GetByChannelItem(channelItem.Id);
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

    private string? _css;
    public string? Css
    {
        get => _css;
        set => this.RaiseAndSetIfChanged(ref _css, value);
    }

    private IEnumerable<Category>? _itemCategories;
    public IEnumerable<Category>? ItemCategories
    {
        get => _itemCategories;
        set => this.RaiseAndSetIfChanged(ref _itemCategories, value);
    }

    private Category? _selectedCategory;
    public Category? SelectedCategory
    {
        get => _selectedCategory;
        set => this.RaiseAndSetIfChanged(ref _selectedCategory, value);
    }

    private string? _searchName;
    public string? SearchName
    {
        get => _searchName;
        set => this.RaiseAndSetIfChanged(ref _searchName, value);
    }

    private IEnumerable<Category>? _searchCategories;
    public IEnumerable<Category>? SearchCategories
    {
        get => _searchCategories;
        set => this.RaiseAndSetIfChanged(ref _searchCategories, value);
    }

    public IReactiveCommand OpenLinkCommand { get; }
    private IReactiveCommand CreateOpenLinkCommand()
    {
        return ReactiveCommand.Create<string, Unit>(
            (link) =>
            {
                _linkOpeningService.OpenUrl(link);
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
                if (!string.IsNullOrWhiteSpace(link))
                {
                    await _clipboardService.SetTextAsync(link);
                }

                return Unit.Default;
            }
        );
    }

    public IReactiveCommand ViewPostsCommand { get; }
    private IReactiveCommand CreateViewPostsCommand()
    {
        return ReactiveCommand.Create<string, Unit>(
            (categoryName) =>
            {
                if (SearchCategories is not null && SearchCategories.Any(x => x.Name == categoryName))
                {
                    SelectedCategory = SearchCategories.First(x => x.Name == categoryName);
                }
                return Unit.Default;
            }, this.WhenAnyValue(x => x.SearchCategories, x => x is not null &&
                !string.IsNullOrEmpty(SearchName) && x.Any(x => x.Name.ToLower() == SearchName.ToLower()))
        );
    }
}