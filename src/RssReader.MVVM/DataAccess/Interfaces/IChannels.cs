using System.Collections.Generic;
using RssReader.MVVM.DataAccess.Models;

namespace RssReader.MVVM.DataAccess.Interfaces;

public interface IChannels
{
    int Create(Channel channel);
    void Update(Channel channel);
    void Delete(int id);
    Channel? Get(int id);
    bool Exists(string url);
    IEnumerable<Channel> GetAll();
    IEnumerable<Channel> GetByGroupId(int? groupId);
    int GetChannelUnreadCount(int channelId);
    int GetAllUnreadCount();
    int GetStarredCount();
    int GetReadLaterCount();
}
