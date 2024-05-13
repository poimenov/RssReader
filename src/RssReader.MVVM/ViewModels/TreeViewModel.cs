using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using DynamicData;
using DynamicData.Binding;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using RssReader.MVVM.Extensions;
using RssReader.MVVM.Models;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.ViewModels;

public class TreeViewModel : ViewModelBase
{
    private static IconConverter? _iconConverter;
    private readonly IChannelService _channelsService;
    private readonly IChannelReader _channelReader;
    public TreeViewModel(IChannelService channelsService, IChannelReader channelReader)
    {
        _channelsService = channelsService;
        _channelReader = channelReader;
        AddFolderCommand = CreateAddFolderCommand();
        AddFeedCommand = CreateAddFeedCommand();
        DeleteCommand = CreateDeleteCommand();

        SourceItems = new ObservableCollectionExtended<ChannelModel>();

        SourceItems.ToObservableChangeSet()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _items)
            .Subscribe();

        var source = new HierarchicalTreeDataGridSource<ChannelModel>(Array.Empty<ChannelModel>())
        {
            Columns =
            {
                new HierarchicalExpanderColumn<ChannelModel>(
                    new TemplateColumn<ChannelModel>(
                        string.Empty,
                        "ChannelNameCell",
                        "ChannelNameEditCell",
                        GridLength.Star),
                    x => x.Children),
                new TextColumn<ChannelModel, int>(
                    string.Empty,
                    x => x.UnreadItemsCount,
                    GridLength.Auto),
            }
        };

        source.RowSelection!.SelectionChanged += (sender, args) =>
        {
            SelectedChannelModel = args.SelectedItems.FirstOrDefault();
        };

        Source = source;

        LoadChannels();
    }

    public void LoadChannels()
    {
        SourceItems.Clear();
        var channelAll = _channelsService.GetChannel(ChannelModelType.All);
        var items = new ObservableCollectionExtended<ChannelModel>
        {
            channelAll,
            _channelsService.GetChannel(ChannelModelType.Starred),
            _channelsService.GetChannel(ChannelModelType.ReadLater)
        };
        items.AddRange(_channelsService.GetChannels());
        SourceItems.AddRange(items);
        ((HierarchicalTreeDataGridSource<ChannelModel>)Source).Items = Items;

        Folders = GetFolders();

        var channelsForUpdate = GetChannelsForUpdate();

        channelsForUpdate.ForEach(x => x.WhenAnyValue(m => m.UnreadItemsCount).Subscribe(c => { channelAll.UnreadItemsCount = GetAllUnreadCount(); }));

        Parallel.ForEachAsync(channelsForUpdate, cancellationToken: default, async (x, ct) => { await _channelReader.ReadChannelAsync(x, ct); });
    }

    public List<ChannelModel> GetChannelsForUpdate()
    {
        var retVal = SourceItems.Where(x => x.IsChannelsGroup == true && x.Children != null && x.Children.Count > 0)
            .SelectMany(x => x.Children!, (group, channel) => new
            {
                GroupId = group.Id,
                Channel = channel
            }).Where(x => x != null).Select(x => x.Channel).ToList();
        retVal.AddRange(SourceItems.Where(x => x.IsChannelsGroup == false && x.ModelType == ChannelModelType.Default).ToList());
        return retVal;
    }

    private List<string> GetFolders()
    {
        var retVal = new List<string>
        {
            string.Empty
        };
        retVal.AddRange(SourceItems.Where(x => x.IsChannelsGroup == true).Select(x => x.Title));
        return retVal;
    }

    private int GetAllUnreadCount()
    {
        var channelsForUpdate = GetChannelsForUpdate();
        return channelsForUpdate.Sum(x => x.UnreadItemsCount);
    }

    private bool IsValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        return Uri.IsWellFormedUriString(url, UriKind.Absolute) &&
            !GetChannelsForUpdate().Any(x => x.Url.Equals(FeedUrl, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsValidFolderName(string? folderName)
    {
        var reservedNames = new string[] { ChannelModel.CHANNELMODELTYPE_ALL, ChannelModel.CHANNELMODELTYPE_STARRED, ChannelModel.CHANNELMODELTYPE_READLATER };

        return !string.IsNullOrWhiteSpace(folderName) && !reservedNames.Contains(folderName, StringComparer.OrdinalIgnoreCase) &&
                    Folders!.All(x => !x.Equals(folderName, StringComparison.OrdinalIgnoreCase));
    }

    #region FolderName
    private string? _folderName;
    public string? FolderName
    {
        get => _folderName;
        set => this.RaiseAndSetIfChanged(ref _folderName, value);
    }
    #endregion

    #region FeedUrl
    private string? _feedUrl;
    public string? FeedUrl
    {
        get => _feedUrl;
        set => this.RaiseAndSetIfChanged(ref _feedUrl, value);
    }
    #endregion

    #region Folders
    private IEnumerable<string>? _folders;
    public IEnumerable<string>? Folders
    {
        get => _folders;
        set => this.RaiseAndSetIfChanged(ref _folders, value);
    }
    #endregion

    #region SelectedFolder
    private string? _selectedFolder;
    public string? SelectedFolder
    {
        get => _selectedFolder;
        set => this.RaiseAndSetIfChanged(ref _selectedFolder, value);
    }
    #endregion

    #region Items
    public ObservableCollectionExtended<ChannelModel> SourceItems;
    private readonly ReadOnlyObservableCollection<ChannelModel> _items;
    public ReadOnlyObservableCollection<ChannelModel> Items => _items;
    #endregion    

    public ITreeDataGridSource<ChannelModel> Source { get; private set; }

    #region SelectedChannelModel
    private ChannelModel? _selectedChannelModel;
    public ChannelModel? SelectedChannelModel
    {
        get => _selectedChannelModel;
        set => this.RaiseAndSetIfChanged(ref _selectedChannelModel, value);
    }
    #endregion

    #region AddFolderCommand
    public IReactiveCommand AddFolderCommand { get; }
    private IReactiveCommand CreateAddFolderCommand()
    {
        return ReactiveCommand.Create<string, Unit>(
            (folderName) =>
            {
                if (IsValidFolderName(folderName))
                {
                    var folder = new ChannelModel(ChannelModelType.Default, folderName, 0)
                    {
                        IsChannelsGroup = true
                    };
                    _channelsService.AddChannel(folder);
                    if (SourceItems.Any(x => x.IsChannelsGroup == true))
                    {
                        var maxIndex = SourceItems.Where(x => x.IsChannelsGroup == true).Select(x => SourceItems.IndexOf(x)).Max();
                        SourceItems.Insert(maxIndex + 1, folder);
                    }
                    else
                    {
                        var index = SourceItems.IndexOf(SourceItems.First(x => x.ModelType == ChannelModelType.ReadLater));
                        SourceItems.Insert(index + 1, folder);
                    }

                    Folders = GetFolders();
                    FolderName = null;
                }

                return Unit.Default;
            }, this.WhenAnyValue(x => x.FolderName, (folderName) => IsValidFolderName(folderName)));
    }
    #endregion

    #region AddFeedCommand
    public IReactiveCommand AddFeedCommand { get; }
    private IReactiveCommand CreateAddFeedCommand()
    {
        return ReactiveCommand.CreateFromTask<string, Unit>(
            async (feedUrl) =>
            {
                if (IsValidUrl(feedUrl))
                {
                    var feed = new ChannelModel(ChannelModelType.Default, new Uri(feedUrl).Host, 0)
                    {
                        IsChannelsGroup = false,
                        Url = feedUrl!
                    };

                    if (!string.IsNullOrEmpty(SelectedFolder))
                    {
                        var folder = SourceItems.FirstOrDefault(x => x.Title.Equals(SelectedFolder, StringComparison.OrdinalIgnoreCase));
                        if (folder != null)
                        {
                            feed.Parent = folder;
                            folder.Children!.Add(feed);
                            feed.WhenAnyValue(x => x.UnreadItemsCount)
                                .Subscribe(count =>
                                {
                                    folder.RaisePropertyChanged(nameof(ChannelModel.UnreadItemsCount));
                                });
                        }
                        SelectedFolder = string.Empty;
                    }
                    else
                    {
                        SourceItems.Add(feed);
                    }

                    _channelsService.AddChannel(feed);
                    FeedUrl = null;
                    var channelAll = SourceItems.First(x => x.ModelType == ChannelModelType.All);
                    feed.WhenAnyValue(m => m.UnreadItemsCount).Subscribe(c => { channelAll.UnreadItemsCount = GetAllUnreadCount(); });
                    await _channelReader.ReadChannelAsync(feed, default);
                    var source = (HierarchicalTreeDataGridSource<ChannelModel>)Source;
                    source.RowSelection?.Select(source.Items.IndexOf(feed));
                }

                return Unit.Default;
            }, this.WhenAnyValue(x => x.FeedUrl, (feedUrl) => IsValidUrl(feedUrl)));
    }
    #endregion

    #region DeleteCommand
    public IReactiveCommand DeleteCommand { get; }
    private IReactiveCommand CreateDeleteCommand()
    {
        return ReactiveCommand.Create(
            async () =>
            {
                if (SelectedChannelModel != null &&
                    SelectedChannelModel.ModelType == ChannelModelType.Default)
                {
                    var message = $"Delete {SelectedChannelModel.Title} {(SelectedChannelModel.IsChannelsGroup ? "folder" : "feed")}?";
                    var dialog = this.GetMessageBox("Delete", message, ButtonEnum.YesNo, Icon.Question);
                    var result = await dialog.ShowAsync();
                    if (result == ButtonResult.No)
                    {
                        return;
                    }

                    _channelsService.DeleteChannel(SelectedChannelModel);
                    if (SelectedChannelModel.IsChannelsGroup)
                    {
                        SourceItems.Remove(SelectedChannelModel);
                        Folders = GetFolders();
                    }
                    else
                    {
                        var parent = SelectedChannelModel.Parent;
                        if (parent != null)
                        {
                            parent.Children!.Remove(SelectedChannelModel);
                            parent.RaisePropertyChanged(nameof(ChannelModel.Children));
                        }
                        else
                        {
                            SourceItems.Remove(SelectedChannelModel);
                        }
                    }

                    SelectedFolder = string.Empty;
                    var source = (HierarchicalTreeDataGridSource<ChannelModel>)Source;
                    source.RowSelection?.Select(0);
                    var channelAll = SourceItems.First(x => x.ModelType == ChannelModelType.All);
                    channelAll.UnreadItemsCount = GetAllUnreadCount();
                }
            }, this.WhenAnyValue(x => x.SelectedChannelModel, x => x != null && x.ModelType == ChannelModelType.Default));
    }
    #endregion

    #region ChannelIconConverter
    public static IValueConverter ChannelIconConverter
    {
        get
        {
            if (_iconConverter is null)
            {
                Dictionary<string, Bitmap> icons = new Dictionary<string, Bitmap>();

                using (var defaultStream = AssetLoader.Open(new Uri("avares://RssReader.MVVM/Assets/rss-button-orange.32.png")))
                using (var allStream = AssetLoader.Open(new Uri("avares://RssReader.MVVM/Assets/document-documents-file-page-svgrepo-com.png")))
                using (var starredStream = AssetLoader.Open(new Uri("avares://RssReader.MVVM/Assets/bookmark-favorite-rating-star-svgrepo-com.png")))
                using (var readLaterStream = AssetLoader.Open(new Uri("avares://RssReader.MVVM/Assets/flag-location-map-marker-pin-pointer-svgrepo-com.png")))
                {
                    icons.Add("default", new Bitmap(defaultStream));
                    icons.Add(ChannelModelType.All.ToString(), new Bitmap(allStream));
                    icons.Add(ChannelModelType.Starred.ToString(), new Bitmap(starredStream));
                    icons.Add(ChannelModelType.ReadLater.ToString(), new Bitmap(readLaterStream));
                }

                var directioryPath = Path.Combine(AppSettings.AppDataPath, "Icons");
                var iconsExtensions = new[] { ".ico", ".png" };
                if (Directory.Exists(directioryPath))
                {
                    foreach (var fileIcon in Directory.GetFiles(directioryPath))
                    {
                        if (iconsExtensions.Contains(Path.GetExtension(fileIcon)))
                        {
                            using (var stream = File.OpenRead(fileIcon))
                            {
                                icons.Add(Path.GetFileNameWithoutExtension(fileIcon), new Bitmap(stream));
                            }
                        }
                    }
                }

                _iconConverter = new IconConverter(icons);
            }

            return _iconConverter;
        }
    }

    private class IconConverter : IValueConverter
    {
        private readonly Dictionary<string, Bitmap> _icons;
        private readonly Bitmap _defaultIcon;
        private readonly Bitmap _allIcon;
        private readonly Bitmap _starredIcon;
        private readonly Bitmap _readLaterIcon;

        public IconConverter(Dictionary<string, Bitmap> icons)
        {
            _icons = icons;
            _defaultIcon = icons["default"];
            _allIcon = icons[ChannelModelType.All.ToString()];
            _starredIcon = icons[ChannelModelType.Starred.ToString()];
            _readLaterIcon = icons[ChannelModelType.ReadLater.ToString()];
        }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ChannelModel channel)
            {
                switch (channel.ModelType)
                {
                    case ChannelModelType.All:
                        return _allIcon;
                    case ChannelModelType.Starred:
                        return _starredIcon;
                    case ChannelModelType.ReadLater:
                        return _readLaterIcon;
                    default:
                        var url = string.IsNullOrEmpty(channel.Link) ? channel.Url : channel.Link;
                        if (!string.IsNullOrEmpty(url))
                        {
                            var key = new Uri(url).Host;
                            if (_icons.ContainsKey(key))
                            {
                                return _icons[key];
                            }
                        }
                        else
                        {
                            return null;
                        }
                        break;
                }
            }

            return _defaultIcon;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
