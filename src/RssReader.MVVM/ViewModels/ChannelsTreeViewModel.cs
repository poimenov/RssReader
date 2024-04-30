using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using RssReader.MVVM.Models;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.ViewModels;

public class ChannelsTreeViewModel : ViewModelBase
{
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
                        new TextColumn<ChannelModel, string>(
                            string.Empty,
                            x => x.Title,
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

        var options = new ParallelOptions()
        {
            MaxDegreeOfParallelism = 10
        };

        Parallel.ForEach(SourceItems.SelectMany(x => x.Children, (group, channel) => new
        {
            GroupId = group.Id,
            Channel = channel
        }).Where(x => x != null), options, x => _channelReader.ReadChannelAsync(x.Channel!));
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
}
