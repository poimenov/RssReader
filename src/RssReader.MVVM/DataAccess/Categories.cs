using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RssReader.MVVM.DataAccess.Interfaces;
using RssReader.MVVM.DataAccess.Models;

namespace RssReader.MVVM.DataAccess;

public class Categories : ICategories
{
    public Category? Get(int categoryId)
    {
        using (var db = new Database())
        {
            return db.Categories.FirstOrDefault(x => x.Id == categoryId);
        }
    }

    public IEnumerable<Category> GetByChannelItem(long channelItemId)
    {
        using (var db = new Database())
        {
            return db.ItemCategories
                .Include(x => x.Category)
                .Where(x => x.ChannelItemId == channelItemId)
                .Select(x => x.Category)
                .Distinct()
                .ToList();
        }
    }

    public IEnumerable<Category> GetByName(string categoryName)
    {
        using (var db = new Database())
        {
            return db.Categories
                .Where(x => EF.Functions.Like(x.Name, $"{categoryName}%"))
                .OrderBy(x => x.Name)
                .Take(10)
                .ToList();
        }
    }
}
