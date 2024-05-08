using System.Collections.Generic;
using System.Linq;
using RssReader.MVVM.DataAccess.Interfaces;
using RssReader.MVVM.Models;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.Services;

public class ChannelService : IChannelService
{
    private readonly IChannelsGroups _channelsGroups;
    private readonly IChannels _channels;
    private readonly IChannelItems _channelItems;
    public ChannelService(IChannelsGroups channelsGroups, IChannels channels, IChannelItems channelItems)
    {
        _channelsGroups = channelsGroups;
        _channels = channels;
        _channelItems = channelItems;
    }
    public void AddChannel(ChannelModel channel)
    {
        throw new System.NotImplementedException();
    }

    public void DeleteChannel(ChannelModel channel)
    {
        throw new System.NotImplementedException();
    }

    public ChannelModel GetChannel(ChannelModelType channelModelType)
    {
        switch (channelModelType)
        {
            case ChannelModelType.All:
                return new ChannelModel(ChannelModelType.All, "All", _channels.GetAllUnreadCount());
            case ChannelModelType.Starred:
                return new ChannelModel(ChannelModelType.Starred, "Starred", _channels.GetStarredCount());
            case ChannelModelType.ReadLater:
                return new ChannelModel(ChannelModelType.ReadLater, "Read Later", _channels.GetReadLaterCount());
            default:
                return new ChannelModel(ChannelModelType.All, "All", _channels.GetAllUnreadCount());
        }
    }

    public ChannelItemModel GetChannelItem(long channelItemId)
    {
        return new ChannelItemModel(_channelItems.Get(channelItemId));
    }

    public IEnumerable<ChannelModel> GetChannels()
    {
        var retVal = _channelsGroups.GetAll().Select(group =>
            new ChannelModel(group.Id, group.Name, _channels.GetByGroupId(group.Id).Select(x =>
            new ChannelModel(x.Id, x.Title, x.Description, x.Url, x.ImageUrl, x.Link, _channels.GetChannelUnreadCount(x.Id), x.Rank)))).ToList();

        retVal.AddRange(_channels.GetByGroupId(null).Select(x =>
            new ChannelModel(x.Id, x.Title, x.Description, x.Url, x.ImageUrl, x.Link,
            _channels.GetChannelUnreadCount(x.Id), x.Rank)).ToList());

        return retVal;
    }

    public void UpdateChannel(ChannelModel channel)
    {
        throw new System.NotImplementedException();
    }
}
