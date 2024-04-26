using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using ReactiveUI;
using RssReader.MVVM.Models;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.ViewModels;

public class ChannelsTreeViewModel : ViewModelBase
{
    private readonly IChannelService _channelsService;
    public ChannelsTreeViewModel(IChannelService channelsService)
    {
        _channelsService = channelsService;
        var source = new HierarchicalTreeDataGridSource<ChannelModel>(_channelsService.GetChannels())
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

    }

    public ITreeDataGridSource<ChannelModel> Source { get; private set; }

    private ChannelModel? _selectedChannelModel;
    public ChannelModel? SelectedChannelModel
    {
        get => _selectedChannelModel;
        set => this.RaiseAndSetIfChanged(ref _selectedChannelModel, value);
    }
}
