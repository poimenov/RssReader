using System.Collections.Generic;
using RssReader.MVVM.Models;

namespace RssReader.MVVM.Services.Interfaces;

public interface IChannelService
{
    IEnumerable<ChannelModel> GetChannels();
    ChannelItemModel GetChannelItem(long channelItemId);
    void AddChannel(ChannelModel channel);
    void UpdateChannel(ChannelModel channel);
    void DeleteChannel(ChannelModel channel);
}
