using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using DynamicData.Binding;
using ReactiveUI;

namespace RssReader.MVVM.Models;

public enum ChannelModelType
{
    Default = 0,
    All = -1,
    Starred = -2,
    ReadLater = -3
}

public class ChannelModel : ReactiveObject
{
    public ChannelModel(ChannelModelType type, string title, int unreadItemsCount)
    {
        Id = (int)type;
        Title = title;
        IsChannelsGroup = false;
        ModelType = type;
        _unreadItemsCount = unreadItemsCount;
    }

    public ChannelModel(int id, string title, IEnumerable<ChannelModel>? children)
    {
        Id = id;
        Title = title ?? string.Empty;
        IsChannelsGroup = true;
        ModelType = ChannelModelType.Default;
        if (children != null)
        {
            //UnreadItemsCount = children.Sum(x => x.UnreadItemsCount);
            foreach (var child in children)
            {
                child.Parent = this;
                // child.WhenAnyValue(x => x.UnreadItemsCount).Subscribe(x =>
                // {
                //     UnreadItemsCount = children.Sum(x => x.UnreadItemsCount);
                // });
            }

            _children = new ObservableCollection<ChannelModel>(children);
        }
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
        ModelType = ChannelModelType.Default;
    }
    public ChannelModelType ModelType { get; private set; }

    private ChannelModel? _parent;
    public ChannelModel? Parent
    {
        get => _parent;
        set => this.RaiseAndSetIfChanged(ref _parent, value);
    }

    private ObservableCollection<ChannelModel>? _children;
    public ObservableCollection<ChannelModel>? Children { get => _children; }
    public int Id { get; set; }
    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }
    public bool IsChannelsGroup { get; set; }
    private string _url = string.Empty;
    public string Url
    {
        get => _url;
        set => this.RaiseAndSetIfChanged(ref _url, value);
    }
    private string? _link = null;
    public string? Link
    {
        get => _link;
        set => this.RaiseAndSetIfChanged(ref _link, value);
    }
    private string? _imageUrl = null;
    public string? ImageUrl
    {
        get => _imageUrl;
        set => this.RaiseAndSetIfChanged(ref _imageUrl, value);
    }

    private string? _description = null;
    public string? Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }
    private int _rank;
    public int Rank
    {
        get => _rank;
        set => this.RaiseAndSetIfChanged(ref _rank, value);
    }
    private int _unreadItemsCount;
    public int UnreadItemsCount
    {
        // get => _unreadItemsCount;
        // set
        // {
        //     var diff = value - _unreadItemsCount;
        //     this.RaiseAndSetIfChanged(ref _unreadItemsCount, value);
        //     if (Parent != null)
        //     {
        //         Parent.UnreadItemsCount += diff;
        //     }
        // }
        get
        {
            return IsChannelsGroup ? Children?.Sum(c => c.UnreadItemsCount) ?? 0 : _unreadItemsCount;
        }
        set => this.RaiseAndSetIfChanged(ref _unreadItemsCount, value);
        // set
        // {
        //     this.RaiseAndSetIfChanged(ref _unreadItemsCount, value);
        //     if (Parent != null)
        //     {
        //         Parent.RaisePropertyChanged(nameof(UnreadItemsCount));
        //     }
        // }
    }
}
