using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Media.Imaging;
using ReactiveUI;
using RssReader.MVVM.Services.Interfaces;

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
    public const string CHANNELMODELTYPE_ALL = "All";
    public const string CHANNELMODELTYPE_STARRED = "Starred";
    public const string CHANNELMODELTYPE_READLATER = "Read Later";
    private readonly IIconConverter? _iconConverter;
    public ChannelModel(ChannelModelType type, string title, int unreadItemsCount, IIconConverter iconConverter)
    {
        Id = (int)type;
        Title = title;
        IsChannelsGroup = false;
        ModelType = type;
        _unreadItemsCount = unreadItemsCount;
        _iconConverter = iconConverter;
        _children = new ObservableCollection<ChannelModel>();
        UpdateImageSource();
    }

    public ChannelModel(int id, string title, int rank, IEnumerable<ChannelModel>? children)
    {
        Id = id;
        Title = title ?? string.Empty;
        Rank = rank;
        IsChannelsGroup = true;

        ModelType = ChannelModelType.Default;
        if (children != null && children.Any())
        {
            _children = new ObservableCollection<ChannelModel>(children);
            foreach (var child in _children)
            {
                child.Parent = this;
                child.WhenAnyValue(x => x.UnreadItemsCount)
                    .Subscribe(count =>
                    {
                        this.RaisePropertyChanged(nameof(UnreadItemsCount));
                    });
            }
        }
        else
        {
            _children = new ObservableCollection<ChannelModel>();
        }
    }
    public ChannelModel(int id, string title, string? description, string url, string? imageUrl, string? link, int unreadItemsCount, int rank, IIconConverter iconConverter)
    {
        Id = id;
        Title = title;
        Description = description;
        Url = url;
        ImageUrl = imageUrl;
        Link = link;
        Rank = rank;
        _iconConverter = iconConverter;
        _unreadItemsCount = unreadItemsCount;
        IsChannelsGroup = false;
        ModelType = ChannelModelType.Default;
        UpdateImageSource();
    }

    private Bitmap? _imageSource;
    public Bitmap? ImageSource
    {
        get => _imageSource;
        set => this.RaiseAndSetIfChanged(ref _imageSource, value);
    }

    public void UpdateImageSource()
    {
        if (_iconConverter != null && !IsChannelsGroup)
        {
            ImageSource = _iconConverter.GetImageByChannelModel(this);
        }
    }

    public bool IsReadOnly => ModelType != ChannelModelType.Default;

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
        get => IsChannelsGroup ? Children?.Sum(c => c.UnreadItemsCount) ?? 0 : _unreadItemsCount;
        set => this.RaiseAndSetIfChanged(ref _unreadItemsCount, value);
    }
}
