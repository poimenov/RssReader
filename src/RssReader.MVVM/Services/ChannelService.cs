using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;
using RssReader.MVVM.Converters;
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
    private readonly IIconConverter _iconConverter;

    public ChannelService(IChannelsGroups channelsGroups, IChannels channels, IChannelItems channelItems, IIconConverter iconConverter)
    {
        _channelsGroups = channelsGroups;
        _channels = channels;
        _channelItems = channelItems;
        _iconConverter = iconConverter;
    }
    public void AddChannel(ChannelModel channel)
    {
        if (channel == null || channel.ModelType != ChannelModelType.Default || string.IsNullOrWhiteSpace(channel.Title))
        {
            throw new ArgumentNullException(nameof(channel));
        }

        if (channel.IsChannelsGroup)
        {
            channel.Id = _channelsGroups.Create(new ChannelsGroup() { Name = channel.Title });
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

            channel.Id = _channels.Create(newChannel);
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
                return new ChannelModel(ChannelModelType.All, ChannelModel.CHANNELMODELTYPE_ALL, _channels.GetAllUnreadCount(), _iconConverter);
            case ChannelModelType.Starred:
                return new ChannelModel(ChannelModelType.Starred, ChannelModel.CHANNELMODELTYPE_STARRED, _channels.GetStarredCount(), _iconConverter);
            case ChannelModelType.ReadLater:
                return new ChannelModel(ChannelModelType.ReadLater, ChannelModel.CHANNELMODELTYPE_READLATER, _channels.GetReadLaterCount(), _iconConverter);
            default:
                return new ChannelModel(ChannelModelType.All, ChannelModel.CHANNELMODELTYPE_ALL, _channels.GetAllUnreadCount(), _iconConverter);
        }
    }

    public ChannelModel? GetChannelModel(int channelId)
    {
        var channel = _channels.Get(channelId);
        if (channel == null)
        {
            return null;
        }

        return new ChannelModel(channel.Id, channel.Title,
                                channel.Description, channel.Url,
                                channel.ImageUrl, channel.Link,
                                _channels.GetChannelUnreadCount(channel.Id),
                                channel.Rank, _iconConverter);
    }

    public IEnumerable<ChannelModel> GetChannels()
    {
        var retVal = _channelsGroups.GetAll().Select(group =>
            new ChannelModel(group.Id, group.Name, group.Rank, _channels.GetByGroupId(group.Id).Select(x =>
            new ChannelModel(x.Id, x.Title, x.Description,
                    x.Url, x.ImageUrl, x.Link,
                    _channels.GetChannelUnreadCount(x.Id),
                    x.Rank, _iconConverter)))).ToList();

        retVal.AddRange(_channels.GetByGroupId(null).Select(x =>
            new ChannelModel(x.Id, x.Title, x.Description, x.Url, x.ImageUrl, x.Link,
            _channels.GetChannelUnreadCount(x.Id), x.Rank, _iconConverter)).ToList());


        foreach (var item in retVal)
        {
            item.WhenAnyValue(x => x.Title).Subscribe((x) => this.UpdateChannel(item));
            if (item.IsChannelsGroup && item.Children!.Any())
            {
                foreach (var child in item.Children!)
                {
                    child.WhenAnyValue(x => x.Title).Subscribe((x) => this.UpdateChannel(child));
                }
            }
        }

        return retVal;
    }

    public void UpdateChannel(ChannelModel channel)
    {
        if (channel == null || channel.ModelType != ChannelModelType.Default)
        {
            throw new ArgumentNullException(nameof(channel));
        }

        if (channel.IsChannelsGroup)
        {
            var channelGroup = _channelsGroups.Get(channel.Id);
            if (channelGroup != null)
            {
                channelGroup.Name = channel.Title;
                channelGroup.Rank = channel.Rank;
                _channelsGroups.Update(channelGroup);
            }
        }
        else
        {
            var feed = _channels.Get(channel.Id);
            if (feed != null)
            {
                feed.Title = channel.Title;
                feed.Rank = channel.Rank;
                if (channel.Parent != null)
                {
                    feed.ChannelsGroupId = channel.Parent.Id;
                }
                else
                {
                    feed.ChannelsGroupId = null;
                }

                _channels.Update(feed);
            }
        }

    }

    public ChannelItemModel GetChannelItem(long channelItemId)
    {
        return new ChannelItemModel(_channelItems.Get(channelItemId));
    }

    public void UpdateChannelItem(ChannelItemModel channelItem)
    {
        var item = _channelItems.Get(channelItem.Id);
        if (item != null)
        {
            if (item.IsRead != channelItem.IsRead)
            {
                _channelItems.SetRead(channelItem.Id, channelItem.IsRead);
            }

            if (item.IsFavorite != channelItem.IsFavorite)
            {
                _channelItems.SetFavorite(channelItem.Id, channelItem.IsFavorite);
            }

            if (item.IsReadLater != channelItem.IsReadLater)
            {
                _channelItems.SetReadLater(channelItem.Id, channelItem.IsReadLater);
            }

            if (item.IsDeleted != channelItem.IsDeleted)
            {
                _channelItems.SetDeleted(channelItem.Id, channelItem.IsDeleted);
            }
        }

    }

    public int GetStarredCount()
    {
        return _channels.GetStarredCount();
    }

    public int GetReadLaterCount()
    {
        return _channels.GetReadLaterCount();
    }

    public int GetChannelUnreadCount(int channelId)
    {
        return _channels.GetChannelUnreadCount(channelId);
    }

    public ChannelModel CreateNewChannel(string url)
    {
        return new ChannelModel(0, new Uri(url).Host, null, url, null, null, 0, 0, _iconConverter);
    }
}
