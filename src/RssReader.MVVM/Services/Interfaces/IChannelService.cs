using System.Collections.Generic;
using RssReader.MVVM.Models;

namespace RssReader.MVVM.Services.Interfaces;

public interface IChannelService
{
    IEnumerable<ChannelModel> GetChannels();
    ChannelModel? GetChannelModel(int channelId);
    ChannelModel GetChannel(ChannelModelType channelModelType);
    ChannelModel CreateNewChannel(string url);
    void AddChannel(ChannelModel channel);
    void UpdateChannel(ChannelModel channel);
    void DeleteChannel(ChannelModel channel);
    ChannelItemModel GetChannelItem(long channelItemId);
    void UpdateChannelItem(ChannelItemModel channelItem);
    int GetStarredCount();
    int GetReadLaterCount();
    int GetChannelUnreadCount(int channelId);
}
