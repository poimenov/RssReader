using System;
using ReactiveUI;
using RssReader.MVVM.DataAccess.Interfaces;
using RssReader.MVVM.Models;
using System.Reactive.Linq;
using RssReader.MVVM.DataAccess.Models;
using System.Linq;
using DynamicData.Binding;
using System.Collections.ObjectModel;
using DynamicData;
using System.Collections.Generic;
using RssReader.MVVM.Services.Interfaces;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace RssReader.MVVM.ViewModels;

public class ItemsViewModel : ViewModelBase
{
    private readonly IChannelItems _channelItems;
    private readonly IChannelReader _channelReader;
    private readonly IIconConverter _iconConverter;
    public ItemsViewModel(IChannelItems channelItems, IChannelReader channelReader, IIconConverter iconConverter)
    {
        _channelItems = channelItems;
        _channelReader = channelReader;
        _iconConverter = iconConverter;
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
                            await Task.WhenAll(ChannelModel.Children!.Select(x => _channelReader.ReadChannelAsync(x, default)));
                            items = _channelItems.GetByGroupId(ChannelModel.Id);
                        }
                        else
                        {
                            await _channelReader.ReadChannelAsync(ChannelModel, default);
                            items = _channelItems.GetByChannelId(ChannelModel.Id);
                        }
                    }
                    else
                    {
                        await Task.WhenAll(AllChannels!.Select(x => _channelReader.ReadChannelAsync(x, default)));
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
