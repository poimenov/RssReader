using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RssReader.MVVM.DataAccess.Interfaces;
using RssReader.MVVM.DataAccess.Models;

namespace RssReader.MVVM.DataAccess;

public class ChannelItems : IChannelItems
{
    public long Create(ChannelItem channelItem, string[] categories)
    {
        using (var db = new Database())
        {
            if (!db.ChannelItems.Any(x => x.ItemId == channelItem.ItemId))
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    db.ChannelItems.Add(channelItem);
                    db.SaveChanges();

                    foreach (var category in categories.Select(x => x.Trim().ToLower()).Distinct())
                    {
                        var cat = db.Categories.FirstOrDefault(x => x.Name == category);
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
            return db.ChannelItems.Include(x => x.Channel)
                .Include(x => x.ItemCategories)
                .ThenInclude(x => x.Category)
                .FirstOrDefault(x => x.Id == id);
        }
    }

    public IEnumerable<ChannelItem> GetByCategory(int categoryId)
    {
        using (var db = new Database())
        {
            return db.ChannelItems.Include(x => x.Channel).Where(x => x.ItemCategories.Select(c => c.CategoryId).Contains(categoryId))
                    .OrderByDescending(x => x.Id)
                    .ToList();
        }
    }

    public IEnumerable<ChannelItem> GetByChannelId(int channelId)
    {
        using (var db = new Database())
        {
            return db.ChannelItems.Include(x => x.Channel).Where(x => x.ChannelId == channelId && x.IsRead == false)
                    .OrderByDescending(x => x.PublishingDate)
                    .ToList();
        }
    }

    public IEnumerable<ChannelItem> GetByDeleted(bool isDeleted)
    {
        using (var db = new Database())
        {
            return db.ChannelItems.Where(x => x.IsDeleted == isDeleted).ToList();
        }
    }

    public IEnumerable<ChannelItem> GetByFavorite(bool isFavorite)
    {
        using (var db = new Database())
        {
            return db.ChannelItems.Include(x => x.Channel).Where(x => x.IsFavorite == isFavorite)
                    .OrderByDescending(x => x.PublishingDate)
                    .ToList();
        }
    }

    public IEnumerable<ChannelItem> GetByGroupId(int groupId)
    {
        using (var db = new Database())
        {
            return db.ChannelItems.Include(x => x.Channel).Where(x => x.Channel.ChannelsGroupId == groupId && x.IsRead == false)
                    .OrderByDescending(x => x.PublishingDate)
                    .ToList();
        }
    }

    public IEnumerable<ChannelItem> GetByRead(bool isRead)
    {
        using (var db = new Database())
        {
            return db.ChannelItems.Include(x => x.Channel).Where(x => x.IsRead == isRead)
                    .OrderByDescending(x => x.PublishingDate)
                    .ToList();
        }
    }

    public IEnumerable<ChannelItem> GetByReadLater(bool isReadLater)
    {
        using (var db = new Database())
        {
            return db.ChannelItems.Include(x => x.Channel).Where(x => x.IsReadLater == isReadLater)
                    .OrderByDescending(x => x.PublishingDate)
                    .ToList();
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

    public void SetReadAll(bool isRead)
    {
        using (var db = new Database())
        {
            if (db.ChannelItems.Where(x => x.IsRead == !isRead).Any())
            {
                FormattableString sql = $"UPDATE [ChannelItems] SET [IsRead] = {(isRead ? 1 : 0)}";
                Debug.WriteLine(sql);
                db.Database.ExecuteSql(sql);
            }
        }
    }

    public void SetReadByChannelId(int channelId, bool isRead)
    {
        using (var db = new Database())
        {
            if (db.ChannelItems.Where(x => x.ChannelId == channelId && x.IsRead == !isRead).Any())
            {
                FormattableString sql = $"UPDATE [ChannelItems] SET [IsRead] = {(isRead ? 1 : 0)} WHERE [ChannelId] = {channelId}";
                Debug.WriteLine(sql);
                db.Database.ExecuteSql(sql);
            }
        }
    }

    public void SetReadByGroupId(int groupId, bool isRead)
    {
        using (var db = new Database())
        {
            if (db.ChannelItems.Where(x => x.Channel.ChannelsGroupId == groupId && x.IsRead == !isRead).Any())
            {
                FormattableString sql = $@"
                    UPDATE [ChannelItems]
                        SET [IsRead] = {(isRead ? 1 : 0)}
                    WHERE [ChannelItems].[Id] IN (
                        SELECT CI.[Id]
                        FROM [ChannelItems] AS CI
                        INNER JOIN [Channels] AS C ON C.[Id] = CI.[ChannelId]
                        WHERE CI.[IsRead] = {(isRead ? 0 : 1)} AND C.[ChannelsGroupId] = {groupId})";
                Debug.WriteLine(sql);
                db.Database.ExecuteSql(sql);
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
