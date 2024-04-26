using System;
using ReactiveUI;
using RssReader.MVVM.DataAccess.Interfaces;
using RssReader.MVVM.Models;
using System.Reactive.Linq;
using Avalonia.Controls;
using RssReader.MVVM.DataAccess.Models;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using RssReader.MVVM.Views;
using System.Linq;


namespace RssReader.MVVM.ViewModels;

public class ChannelItemsViewModel : ViewModelBase
{
    private readonly IChannelItems _channelItems;
    public ChannelItemsViewModel(IChannelItems channelItems)
    {
        _channelItems = channelItems;
        this.WhenAnyValue(x => x.ChannelModel)
            .WhereNotNull()
            .Subscribe(channelModel =>
            {
                FlatTreeDataGridSource<ChannelItem> source;
                if (channelModel.IsChannelsGroup)
                {
                    source = new FlatTreeDataGridSource<ChannelItem>(_channelItems.GetByGroupId(channelModel.Id))
                    {
                        Columns =
                        {
                            new TemplateColumn<ChannelItem>(string.Empty,
                                new FuncDataTemplate<ChannelItem>((a,e) => new ChannelItemView
                                {
                                    DataContext = new ChannelItemViewModel(a)
                                }))
                        }
                    };
                }
                else
                {
                    source = new FlatTreeDataGridSource<ChannelItem>(_channelItems.GetByChannelId(channelModel.Id))
                    {
                        Columns =
                        {
                            new TemplateColumn<ChannelItem>(string.Empty,
                                new FuncDataTemplate<ChannelItem>((a,e) => new ChannelItemView
                                {
                                    DataContext = new ChannelItemViewModel(a)
                                }))
                        }
                    };
                }

                source.RowSelection!.SelectionChanged += (sender, args) =>
                {
                    SelectedChannelItem = args.SelectedItems.FirstOrDefault();
                };

                Source = source;

            });
    }

    private ChannelModel? _channelModel;
    public ChannelModel? ChannelModel
    {
        get => _channelModel;
        set => this.RaiseAndSetIfChanged(ref _channelModel, value);
    }

    private ITreeDataGridSource<ChannelItem>? _source;
    public ITreeDataGridSource<ChannelItem>? Source
    {
        get => _source;
        set => this.RaiseAndSetIfChanged(ref _source, value);
    }

    private ChannelItem? _selectedChannelItem;
    public ChannelItem? SelectedChannelItem
    {
        get => _selectedChannelItem;
        set => this.RaiseAndSetIfChanged(ref _selectedChannelItem, value);
    }
}
