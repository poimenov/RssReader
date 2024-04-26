using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;

namespace RssReader.MVVM.Models;

public class ChannelModel : ReactiveObject
{
    public ChannelModel(int id, string title, IEnumerable<ChannelModel> children)
    {
        Id = id;
        Title = title;
        _children = new ObservableCollection<ChannelModel>(children);
        IsChannelsGroup = true;
    }
    public ChannelModel(int id, string title, string? description, string url, string? imageUrl, string? link, int unreadItemsCount, int rank)
    {
        Id = id;
        Title = title;
        Description = description;
        Url = url;
        ImageUrl = imageUrl;
        Link = link;
        Rank = rank;
        _unreadItemsCount = unreadItemsCount;
        IsChannelsGroup = false;
    }
    private ObservableCollection<ChannelModel>? _children;
    public ObservableCollection<ChannelModel>? Children { get => _children; }
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsChannelsGroup { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Link { get; set; } = null;
    public string? ImageUrl { get; set; } = null;
    public string? Description { get; set; } = null;
    public int Rank { get; set; }
    private int _unreadItemsCount;
    public int UnreadItemsCount
    {
        get
        {
            return IsChannelsGroup ? Children?.Sum(c => c.UnreadItemsCount) ?? 0 : _unreadItemsCount;
        }
    }
}
