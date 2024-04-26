using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using RssReader.MVVM.DataAccess.Interfaces;
using RssReader.MVVM.Models;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.Services;

public class ChannelService : IChannelService
{
    private readonly IChannelsGroups _channelsGroups;
    private readonly IChannels _channels;
    public ChannelService(IChannelsGroups channelsGroups, IChannels channels)
    {
        _channelsGroups = channelsGroups;
        _channels = channels;
    }
    public void AddChannel(ChannelModel channel)
    {
        throw new System.NotImplementedException();
    }

    public void DeleteChannel(ChannelModel channel)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerable<ChannelModel> GetChannels()
    {
        var retVal = _channelsGroups.GetAll().Select(group =>
            new ChannelModel(group.Id, group.Name, _channels.GetByGroupId(group.Id).Select(x =>
            new ChannelModel(x.Id, x.Title, x.Description, x.Url, x.ImageUrl, x.Link, _channels.GetUnreadCount(x.Id), x.Rank)))).ToList();

        return retVal;
    }

    public void UpdateChannel(ChannelModel channel)
    {
        throw new System.NotImplementedException();
    }
}
