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


namespace RssReader.MVVM.ViewModels;

public class ChannelItemsViewModel : ViewModelBase
{
    private readonly IChannelItems _channelItems;
    public ChannelItemsViewModel(IChannelItems channelItems)
    {
        _channelItems = channelItems;
        SourceItems = new ObservableCollectionExtended<ChannelItemModel>();
        SourceItems.ToObservableChangeSet()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _items)
            .Subscribe();

        this.WhenAnyValue(x => x.ChannelModel)
            .WhereNotNull()
            .Subscribe(channelModel =>
            {
                IEnumerable<ChannelItem> items;
                switch (channelModel.ModelType)
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
                        if (channelModel.IsChannelsGroup)
                        {
                            items = _channelItems.GetByGroupId(channelModel.Id);
                        }
                        else
                        {
                            items = _channelItems.GetByChannelId(channelModel.Id);
                        }
                        break;
                }

                SourceItems.Load(items.Select(x => new ChannelItemModel(x)));
            });
    }

    private ChannelModel? _channelModel;
    public ChannelModel? ChannelModel
    {
        get => _channelModel;
        set => this.RaiseAndSetIfChanged(ref _channelModel, value);
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
}
