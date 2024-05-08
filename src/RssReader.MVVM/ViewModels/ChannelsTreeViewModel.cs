using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
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
using ReactiveUI;
using RssReader.MVVM.Models;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.ViewModels;

public class ChannelsTreeViewModel : ViewModelBase
{
    private static IconConverter? _iconConverter;
    private readonly IChannelService _channelsService;
    private readonly IChannelReader _channelReader;
    public ChannelsTreeViewModel(IChannelService channelsService, IChannelReader channelReader)
    {
        _channelsService = channelsService;
        _channelReader = channelReader;

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
                        null,
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
        var items = new ObservableCollectionExtended<ChannelModel>
        {
            _channelsService.GetChannel(ChannelModelType.All),
            _channelsService.GetChannel(ChannelModelType.Starred),
            _channelsService.GetChannel(ChannelModelType.ReadLater)
        };
        items.AddRange(_channelsService.GetChannels());
        SourceItems.AddRange(items);
        ((HierarchicalTreeDataGridSource<ChannelModel>)Source).Items = Items;

        var channelsForUpdate = SourceItems.Where(x => x.IsChannelsGroup == true && x.Children != null && x.Children.Count > 0)
            .SelectMany(x => x.Children!, (group, channel) => new
            {
                GroupId = group.Id,
                Channel = channel
            }).Where(x => x != null).Select(x => x.Channel).ToList();

        channelsForUpdate.AddRange(SourceItems.Where(x => x.IsChannelsGroup == false).ToList());

        Task.WhenAll(channelsForUpdate.Select(x => _channelReader.ReadChannelAsync(x, default)));
    }

    #region Items
    public ObservableCollectionExtended<ChannelModel> SourceItems;
    private readonly ReadOnlyObservableCollection<ChannelModel> _items;
    public ReadOnlyObservableCollection<ChannelModel> Items => _items;
    #endregion    

    public ITreeDataGridSource<ChannelModel> Source { get; private set; }

    private ChannelModel? _selectedChannelModel;
    public ChannelModel? SelectedChannelModel
    {
        get => _selectedChannelModel;
        set => this.RaiseAndSetIfChanged(ref _selectedChannelModel, value);
    }

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
}
