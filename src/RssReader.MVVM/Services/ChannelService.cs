using System;
using System.Collections.Generic;
using System.Linq;
using RssReader.MVVM.DataAccess.Interfaces;
using RssReader.MVVM.DataAccess.Models;
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
        if (channel == null || channel.ModelType != ChannelModelType.Default || string.IsNullOrWhiteSpace(channel.Title))
        {
            throw new ArgumentNullException(nameof(channel));
        }

        if (channel.IsChannelsGroup)
        {
            var id = _channelsGroups.Create(new ChannelsGroup() { Name = channel.Title });
            channel.Id = id;
        }
        else
        {
            var newChannel = new DataAccess.Models.Channel()
            {
                Url = channel.Url,
                Title = channel.Title
            };
            if (channel.Parent is not null)
            {
                newChannel.ChannelsGroupId = channel.Parent.Id;
            }

            var id = _channels.Create(newChannel);
            channel.Id = id;
        }
    }

    public void DeleteChannel(ChannelModel channel)
    {
        if (channel == null || channel.ModelType != ChannelModelType.Default)
        {
            throw new ArgumentNullException(nameof(channel));
        }

        if (channel.IsChannelsGroup)
        {
            _channelsGroups.Delete(channel.Id);
        }
        else
        {
            _channels.Delete(channel.Id);
        }
    }

    public ChannelModel GetChannel(ChannelModelType channelModelType)
    {
        switch (channelModelType)
        {
            case ChannelModelType.All:
                return new ChannelModel(ChannelModelType.All, ChannelModel.CHANNELMODELTYPE_ALL, _channels.GetAllUnreadCount());
            case ChannelModelType.Starred:
                return new ChannelModel(ChannelModelType.Starred, ChannelModel.CHANNELMODELTYPE_STARRED, _channels.GetStarredCount());
            case ChannelModelType.ReadLater:
                return new ChannelModel(ChannelModelType.ReadLater, ChannelModel.CHANNELMODELTYPE_READLATER, _channels.GetReadLaterCount());
            default:
                return new ChannelModel(ChannelModelType.All, ChannelModel.CHANNELMODELTYPE_ALL, _channels.GetAllUnreadCount());
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
