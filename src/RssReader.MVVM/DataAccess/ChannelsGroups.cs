using System.Collections.Generic;
using System.Linq;
using RssReader.MVVM.DataAccess.Interfaces;
using RssReader.MVVM.DataAccess.Models;

namespace RssReader.MVVM.DataAccess;

public class ChannelsGroups : IChannelsGroups
{
    public int Create(ChannelsGroup channelsGroup)
    {
        using (var db = new Database())
        {
            if (!db.ChannelsGroups.Any(x => x.Name.ToLower() == channelsGroup.Name.ToLower()))
            {
                var rank = 1;
                if (db.ChannelsGroups.Any())
                {
                    rank = db.ChannelsGroups.Max(x => x.Rank) + 1;
                }

                channelsGroup.Rank = rank;
                db.ChannelsGroups.Add(channelsGroup);
                db.SaveChanges();
                return channelsGroup.Id;
            }

            return 0;
        }
    }

    public void Delete(int id)
    {
        using (var db = new Database())
        {
            if (db.ChannelsGroups.Any(x => x.Id == id))
            {
                var item = db.ChannelsGroups.First(x => x.Id == id);
                db.ChannelsGroups.Remove(item);
                db.SaveChanges();
            }
        }
    }

    public ChannelsGroup? Get(int id)
    {
        using (var db = new Database())
        {
            return db.ChannelsGroups.FirstOrDefault(x => x.Id == id);
        }
    }

    public ChannelsGroup? Get(string name)
    {
        using (var db = new Database())
        {
            return db.ChannelsGroups.FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
        }
    }

    public IEnumerable<ChannelsGroup> GetAll()
    {
        using (var db = new Database())
        {
            return db.ChannelsGroups.ToList();
        }
    }

    public void Update(ChannelsGroup channelsGroup)
    {
        using (var db = new Database())
        {
            if (db.ChannelsGroups.Any(x => x.Id == channelsGroup.Id))
            {
                var item = db.ChannelsGroups.First(x => x.Id == channelsGroup.Id);
                item.Name = channelsGroup.Name;
                item.Rank = channelsGroup.Rank;
                db.SaveChanges();
            }
        }
    }
}
