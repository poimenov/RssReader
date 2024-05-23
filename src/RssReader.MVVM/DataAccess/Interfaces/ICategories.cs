using System.Collections.Generic;
using Avalonia.Animation.Easings;
using RssReader.MVVM.DataAccess.Models;

namespace RssReader.MVVM.DataAccess.Interfaces;

public interface ICategories
{
    IEnumerable<Category> GetByChannelItem(long channelItemId);
    IEnumerable<Category> GetByName(string categoryName);
    Category? Get(int categoryId);
}
