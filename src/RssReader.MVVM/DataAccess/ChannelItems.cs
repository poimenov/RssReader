using System.Collections.Generic;
using System.Linq;
using RssReader.MVVM.DataAccess.Interfaces;
using RssReader.MVVM.DataAccess.Models;

namespace RssReader.MVVM.DataAccess;

public class ChannelItems : IChannelItems
{
    public long Create(ChannelItem channelItem, string[] categories)
    {
        using (var db = new Database())
        {
            if (!db.ChannelItems.Any(x => x.ItemId.ToLower() == channelItem.ItemId.ToLower()))
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    db.ChannelItems.Add(channelItem);
                    db.SaveChanges();

                    foreach (var category in categories.Distinct())
                    {
                        var cat = db.Categories.FirstOrDefault(x => x.Name.ToLower() == category.ToLower());
                        if (cat == null)
                        {
                            cat = new Category
                            {
                                Name = category
                            };
                            db.Categories.Add(cat);
                            db.SaveChanges();
                        }

                        var itemCat = new ItemCategory
                        {
                            CategoryId = cat.Id,
                            ChannelItemId = channelItem.Id
                        };
                        db.ItemCategories.Add(itemCat);
                    }

                    db.SaveChanges();
                    transaction.Commit();
                }

                return channelItem.Id;
            }

            return 0;
        }
    }

    public void Delete(long id)
    {
        using (var db = new Database())
        {
            if (db.ChannelItems.Any(x => x.Id == id))
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    if (db.ItemCategories.Any(x => x.ChannelItemId == id))
                    {
                        var itemCats = db.ItemCategories.Where(x => x.ChannelItemId == id);
                        foreach (var itemCat in itemCats)
                        {
                            db.ItemCategories.Remove(itemCat);
                        }
                    }

                    var item = db.ChannelItems.First(x => x.Id == id);
                    db.ChannelItems.Remove(item);
                    db.SaveChanges();
                    transaction.Commit();
                }
            }
        }
    }

    public ChannelItem? Get(long id)
    {
        using (var db = new Database())
        {
            return db.ChannelItems.FirstOrDefault(x => x.Id == id);
        }
    }

    public IEnumerable<ChannelItem> GetByCategory(int categoryId)
    {
        using (var db = new Database())
        {
            return db.ChannelItems.Where(x => x.ItemCategories.Select(c => c.CategoryId).Contains(categoryId));
        }
    }

    public IEnumerable<ChannelItem> GetByChannelId(int channelId)
    {
        using (var db = new Database())
        {
            return db.ChannelItems.Where(x => x.ChannelId == channelId);
        }
    }

    public IEnumerable<ChannelItem> GetByDeleted(bool isDeleted)
    {
        using (var db = new Database())
        {
            return db.ChannelItems.Where(x => x.IsDeleted == isDeleted);
        }
    }

    public IEnumerable<ChannelItem> GetByFavorite(bool isFavorite)
    {
        using (var db = new Database())
        {
            return db.ChannelItems.Where(x => x.IsFavorite == isFavorite);
        }
    }

    public IEnumerable<ChannelItem> GetByGroupId(int groupId)
    {
        using (var db = new Database())
        {
            return db.ChannelItems.Where(x => x.Channel.ChannelsGroupId == groupId);
        }
    }

    public IEnumerable<ChannelItem> GetByRead(bool isRead)
    {
        using (var db = new Database())
        {
            return db.ChannelItems.Where(x => x.IsRead == isRead);
        }
    }

    public IEnumerable<ChannelItem> GetByReadLater(bool isReadLater)
    {
        using (var db = new Database())
        {
            return db.ChannelItems.Where(x => x.IsReadLater == isReadLater);
        }
    }

    public void SetDeleted(long id, bool isDeleted)
    {
        using (var db = new Database())
        {
            if (db.ChannelItems.Any(x => x.Id == id))
            {
                var item = db.ChannelItems.First(x => x.Id == id);
                item.IsDeleted = isDeleted;
                db.SaveChanges();
            }
        }
    }

    public void SetFavorite(long id, bool isFavorite)
    {
        using (var db = new Database())
        {
            if (db.ChannelItems.Any(x => x.Id == id))
            {
                var item = db.ChannelItems.First(x => x.Id == id);
                item.IsFavorite = isFavorite;
                db.SaveChanges();
            }
        }
    }

    public void SetRead(long id, bool isRead)
    {
        using (var db = new Database())
        {
            if (db.ChannelItems.Any(x => x.Id == id))
            {
                var item = db.ChannelItems.First(x => x.Id == id);
                item.IsRead = isRead;
                db.SaveChanges();
            }
        }
    }

    public void SetReadLater(long id, bool isReadLater)
    {
        using (var db = new Database())
        {
            if (db.ChannelItems.Any(x => x.Id == id))
            {
                var item = db.ChannelItems.First(x => x.Id == id);
                item.IsReadLater = isReadLater;
                db.SaveChanges();
            }
        }
    }
}
