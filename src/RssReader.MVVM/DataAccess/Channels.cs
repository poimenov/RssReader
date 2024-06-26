using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RssReader.MVVM.DataAccess.Interfaces;
using RssReader.MVVM.DataAccess.Models;

namespace RssReader.MVVM.DataAccess;

public class Channels : IChannels
{
    public int Create(Channel channel)
    {
        using (var db = new Database())
        {
            if (!db.Channels.Any(x => x.Url.ToLower() == channel.Url.ToLower()))
            {
                var rank = 1;
                if (channel.ChannelsGroupId != null && db.Channels.Any(x => x.ChannelsGroupId == channel.ChannelsGroupId))
                {
                    rank = db.Channels.Where(x => x.ChannelsGroupId == channel.ChannelsGroupId).Max(x => x.Rank) + 1;
                }
                else if (db.Channels.Any(x => x.ChannelsGroupId == null))
                {
                    rank = db.Channels.Where(x => x.ChannelsGroupId == null).Max(x => x.Rank) + 1;
                }

                channel.Rank = rank;
                db.Channels.Add(channel);
                db.SaveChanges();
                return channel.Id;
            }

            return 0;
        }
    }

    public void Delete(int id)
    {
        using (var db = new Database())
        {
            if (db.Channels.Any(x => x.Id == id))
            {
                var item = db.Channels
                    .Include(x => x.Items)
                    .ThenInclude(x => x.ItemCategories)
                    .First(x => x.Id == id);
                db.ItemCategories.RemoveRange(item.Items.SelectMany(x => x.ItemCategories));
                db.ChannelItems.RemoveRange(item.Items);
                db.Channels.Remove(item);
                db.SaveChanges();
            }
        }
    }

    public bool Exists(string url)
    {
        using (var db = new Database())
        {
            return db.Channels.Any(x => x.Url.ToLower() == url.ToLower());
        }
    }

    public Channel? Get(int id)
    {
        using (var db = new Database())
        {
            return db.Channels.FirstOrDefault(x => x.Id == id);
        }
    }

    public IEnumerable<Channel> GetAll()
    {
        using (var db = new Database())
        {
            return db.Channels.OrderBy(x => x.Rank).ToList();
        }
    }

    public int GetAllUnreadCount()
    {
        using (var db = new Database())
        {
            return db.ChannelItems.Count(x => x.IsRead == false);
        }
    }

    public IEnumerable<Channel> GetByGroupId(int? groupId)
    {
        using (var db = new Database())
        {
            if (groupId == null)
            {
                return db.Channels.Where(x => x.ChannelsGroupId.HasValue == false).OrderBy(x => x.Rank).ToList();
            }
            else
            {
                return db.Channels.Where(x => x.ChannelsGroupId == groupId).OrderBy(x => x.Rank).ToList();
            }
        }
    }

    public int GetReadLaterCount()
    {
        using (var db = new Database())
        {
            return db.ChannelItems.Count(x => x.IsReadLater == true);
        }
    }

    public int GetStarredCount()
    {
        using (var db = new Database())
        {
            return db.ChannelItems.Count(x => x.IsFavorite == true);
        }
    }

    public int GetChannelUnreadCount(int channelId)
    {
        using (var db = new Database())
        {
            return db.ChannelItems.Count(x => x.IsRead == false && x.ChannelId == channelId);
        }
    }

    public void Update(Channel channel)
    {
        using (var db = new Database())
        {
            if (db.Channels.Any(x => x.Id == channel.Id))
            {
                var item = db.Channels.First(x => x.Id == channel.Id);
                item.Title = channel.Title;
                item.Description = channel.Description;
                item.ImageUrl = channel.ImageUrl;
                item.ChannelsGroupId = channel.ChannelsGroupId;
                item.Rank = channel.Rank;
                item.Language = channel.Language;
                item.Link = channel.Link;
                item.Url = channel.Url;
                db.SaveChanges();
            }
        }
    }
}
