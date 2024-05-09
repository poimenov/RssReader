using System.Collections.Generic;
using RssReader.MVVM.DataAccess.Models;

namespace RssReader.MVVM.DataAccess.Interfaces;

public interface IChannelItems
{
    long Create(ChannelItem channelItem, string[] categories);
    void SetRead(long id, bool isRead);
    void SetReadByGroupId(int groupId, bool isRead);
    void SetReadByChannelId(int channelId, bool isRead);
    void SetReadAll(bool isRead);
    void SetFavorite(long id, bool isFavorite);
    void SetDeleted(long id, bool isDeleted);
    void SetReadLater(long id, bool isReadLater);
    void Delete(long id);
    ChannelItem? Get(long id);
    IEnumerable<ChannelItem> GetByChannelId(int channelId);
    IEnumerable<ChannelItem> GetByGroupId(int groupId);
    IEnumerable<ChannelItem> GetByReadLater(bool isReadLater);
    IEnumerable<ChannelItem> GetByRead(bool isRead);
    IEnumerable<ChannelItem> GetByFavorite(bool isFavorite);
    IEnumerable<ChannelItem> GetByDeleted(bool isDeleted);
    IEnumerable<ChannelItem> GetByCategory(int categoryId);
}
