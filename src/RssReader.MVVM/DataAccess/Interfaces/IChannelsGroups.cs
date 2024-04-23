using System.Collections.Generic;
using RssReader.MVVM.DataAccess.Models;

namespace RssReader.MVVM.DataAccess.Interfaces;

public interface IChannelsGroups
{
    int Create(ChannelsGroup channelsGroup);
    void Update(ChannelsGroup channelsGroup);
    void Delete(int id);
    ChannelsGroup? Get(int id);
    ChannelsGroup? Get(string name);
    IEnumerable<ChannelsGroup> GetAll();
}
