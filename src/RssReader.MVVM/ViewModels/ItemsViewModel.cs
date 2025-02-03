using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using DynamicData.Binding;
using DynamicData;
using ReactiveUI;
using RssReader.MVVM.DataAccess.Interfaces;
using RssReader.MVVM.DataAccess.Models;
using RssReader.MVVM.Models;
using RssReader.MVVM.Services.Interfaces;



namespace RssReader.MVVM.ViewModels;

public class ItemsViewModel : ViewModelBase
{
    private readonly IChannelItems _channelItems;
    private readonly IChannelModelUpdater _channelModelUpdater;
    private readonly IIconConverter _iconConverter;
    private readonly IDispatcherWrapper _dispatcherWrapper;
    public ItemsViewModel(IChannelItems channelItems, IChannelModelUpdater channelModelUpdater, IIconConverter iconConverter, IDispatcherWrapper dispatcherWrapper)
    {
        _channelItems = channelItems;
        _channelModelUpdater = channelModelUpdater;
        _iconConverter = iconConverter;
        _dispatcherWrapper = dispatcherWrapper;
        MarkAsReadCommand = CreateMarkAsReadCommand();
        RefreshCommand = CreateRefreshCommand();

        SourceItems = new ObservableCollectionExtended<ChannelItemModel>();
        SourceItems.ToObservableChangeSet()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Filter(Filter)
            .Bind(out _items)
            .Subscribe();

        this.WhenAnyValue(x => x.ChannelModel)
            .WhereNotNull()
            .Subscribe(channelModel =>
            {
                LoadItems();
            });

        this.WhenAnyValue(x => x.SelectedChannelItem)
            .WhereNotNull()
            .Subscribe(channelItem =>
            {
                channelItem.WhenAnyValue(x => x.IsDeleted)
                    .Subscribe(isDeleted =>
                    {
                        if (isDeleted)
                        {
                            var index = Items.IndexOf(channelItem);
                            SourceItems.Remove(channelItem);
                            if (index > 0 && index < Items.Count)
                            {
                                SelectedChannelItem = Items.ElementAt(index);
                            }
                        }
                    });
            });
    }

    public void LoadItems()
    {
        if (ChannelModel is not null)
        {
            Title = ChannelModel.Title;
            ImageSource = ChannelModel.ImageSource;

            IEnumerable<ChannelItem> items;
            switch (ChannelModel.ModelType)
            {
                case ChannelModelType.All:
                    items = _channelItems.GetByRead(false);
                    break;
                case ChannelModelType.Starred:
                    items = _channelItems.GetByFavorite(true);
                    break;
                case ChannelModelType.ReadLater:
                    items = _channelItems.GetByReadLater(true);
                    break;
                default:
                    if (ChannelModel.IsChannelsGroup)
                    {
                        items = _channelItems.GetByGroupId(ChannelModel.Id);
                    }
                    else
                    {
                        items = _channelItems.GetByChannelId(ChannelModel.Id);
                    }
                    break;
            }

            SourceItems.Clear();
            SourceItems.Load(items.Select(x => new ChannelItemModel(x, _iconConverter)));
        }
    }

    public void LoadItems(Category category)
    {
        if (category is not null && category.Id > 0)
        {
            Title = category.Name;
            using (var defaultStream = AssetLoader.Open(new Uri($"{AppSettings.AvaResPath}/rss-button-orange.32.png")))
            {
                ImageSource = new Bitmap(defaultStream);
            }
            var items = _channelItems.GetByCategory(category.Id);
            SourceItems.Clear();
            SourceItems.Load(items.Select(x => new ChannelItemModel(x, _iconConverter)));
        }
    }

    private IObservable<Func<ChannelItemModel, bool>> Filter =>
        this.WhenAnyValue(x => x.SearchText)
            .Select((x) => MakeFilter(x));

    private Func<ChannelItemModel, bool> MakeFilter(string? searchText)
    {
        return item =>
        {
            var retVal = true;
            if (!string.IsNullOrEmpty(searchText) && searchText?.Length > 3)
            {
                var inTitle = item.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase);
                var inDescription = item.Description?.Contains(searchText, StringComparison.OrdinalIgnoreCase);
                var inContent = item.Content?.Contains(searchText, StringComparison.OrdinalIgnoreCase);
                retVal = inTitle || (inDescription ?? false) || (inContent ?? false);
            }

            return retVal;
        };
    }

    private ChannelModel? _channelModel;
    public ChannelModel? ChannelModel
    {
        get => _channelModel;
        set => this.RaiseAndSetIfChanged(ref _channelModel, value);
    }

    private List<ChannelModel>? _allChannels;
    public List<ChannelModel>? AllChannels
    {
        get => _allChannels;
        set => this.RaiseAndSetIfChanged(ref _allChannels, value);
    }

    #region Items
    public ObservableCollectionExtended<ChannelItemModel> SourceItems;
    private readonly ReadOnlyObservableCollection<ChannelItemModel> _items;
    public ReadOnlyObservableCollection<ChannelItemModel> Items => _items;
    #endregion

    private ChannelItemModel? _selectedChannelItem;
    public ChannelItemModel? SelectedChannelItem
    {
        get => _selectedChannelItem;
        set => this.RaiseAndSetIfChanged(ref _selectedChannelItem, value);
    }

    private string? _searchText;
    public string? SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    private string? _title;
    public string? Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    private Bitmap? _imageSource;
    public Bitmap? ImageSource
    {
        get => _imageSource;
        set => this.RaiseAndSetIfChanged(ref _imageSource, value);
    }


    public IReactiveCommand? PaneCommand { get; set; }

    public IReactiveCommand RefreshCommand { get; }
    private IReactiveCommand CreateRefreshCommand()
    {
        return ReactiveCommand.Create(
            async () =>
            {
                if (ChannelModel is not null &&
                        ChannelModel.ModelType != ChannelModelType.Starred &&
                        ChannelModel.ModelType != ChannelModelType.ReadLater)
                {
                    IEnumerable<ChannelItem> items;
                    if (ChannelModel.ModelType == ChannelModelType.Default)
                    {
                        if (ChannelModel.IsChannelsGroup && ChannelModel.Children!.Any())
                        {
                            await Task.WhenAll(ChannelModel.Children!.Select(x => _channelModelUpdater.ReadChannelAsync(x, default, _dispatcherWrapper)));
                            items = _channelItems.GetByGroupId(ChannelModel.Id);
                        }
                        else
                        {
                            await _channelModelUpdater.ReadChannelAsync(ChannelModel, default, _dispatcherWrapper);
                            items = _channelItems.GetByChannelId(ChannelModel.Id);
                        }
                    }
                    else
                    {
                        await Task.WhenAll(AllChannels!.Select(x => _channelModelUpdater.ReadChannelAsync(x, default, _dispatcherWrapper)));
                        items = _channelItems.GetByRead(false);
                    }

                    SourceItems.Load(items.Select(x => new ChannelItemModel(x, _iconConverter)));
                }
            }, this.WhenAnyValue(x => x.ChannelModel, (m) => m is not null &&
                        m.ModelType != ChannelModelType.Starred && m.ModelType != ChannelModelType.ReadLater)
        );
    }

    public IReactiveCommand MarkAsReadCommand { get; }
    private IReactiveCommand CreateMarkAsReadCommand()
    {
        return ReactiveCommand.Create(
            () =>
            {
                if (ChannelModel is not null)
                {
                    if (ChannelModel.ModelType == ChannelModelType.Default)
                    {
                        if (ChannelModel.IsChannelsGroup)
                        {
                            _channelItems.SetReadByGroupId(ChannelModel.Id, true);
                            if (ChannelModel.Children is not null)
                            {
                                foreach (var child in ChannelModel.Children)
                                {
                                    child.UnreadItemsCount = 0;
                                }
                            }
                        }
                        else
                        {
                            _channelItems.SetReadByChannelId(ChannelModel.Id, true);
                            ChannelModel.UnreadItemsCount = 0;
                        }
                    }
                    else if (ChannelModel.ModelType == ChannelModelType.All)
                    {
                        _channelItems.SetReadAll(true);
                        ChannelModel.UnreadItemsCount = 0;
                    }

                    foreach (var item in SourceItems)
                    {
                        item.IsRead = true;
                    }
                }
            }, this.WhenAnyValue(x => x.ChannelModel, (m) => m is not null && m.UnreadItemsCount > 0 &&
                        (m.ModelType == ChannelModelType.Default || m.ModelType == ChannelModelType.All))
        );
    }
}
