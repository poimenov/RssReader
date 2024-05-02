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

        SourceItems = new ObservableCollectionExtended<ChannelModel>(_channelsService.GetChannels());
        SourceItems.ToObservableChangeSet()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _items)
            .Subscribe();

        var source = new HierarchicalTreeDataGridSource<ChannelModel>(Items)
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

        var channelsForUpdate = SourceItems.Where(x => x.Children != null)
            .SelectMany(x => x.Children!, (group, channel) => new
            {
                GroupId = group.Id,
                Channel = channel
            }).Where(x => x != null);

        var options = new ParallelOptions()
        {
            MaxDegreeOfParallelism = 10
        };

        Parallel.ForEachAsync(channelsForUpdate, options, async (x, ct) => { await _channelReader.ReadChannelAsync(x.Channel!, ct); });
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
                Bitmap defaultIcon;
                using (var folderOpenStream = AssetLoader.Open(new Uri("avares://RssReader.MVVM/Assets/rss-button-orange.32.png")))
                {
                    defaultIcon = new Bitmap(folderOpenStream);
                }

                Dictionary<string, Bitmap> icons = new Dictionary<string, Bitmap>();
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

                _iconConverter = new IconConverter(icons, defaultIcon);
            }

            return _iconConverter;
        }
    }

    private class IconConverter : IValueConverter
    {
        private readonly Dictionary<string, Bitmap> _icons;
        private readonly Bitmap _defaultIcon;

        public IconConverter(Dictionary<string, Bitmap> icons, Bitmap defaultIcon)
        {
            _icons = icons;
            _defaultIcon = defaultIcon;
        }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ChannelModel channel)
            {
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
            }

            return _defaultIcon;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
