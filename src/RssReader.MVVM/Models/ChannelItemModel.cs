using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Avalonia.Media.Imaging;
using ReactiveUI;
using RssReader.MVVM.DataAccess.Models;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.Models;

public class ChannelItemModel : ReactiveObject
{
    private readonly IIconConverter? _iconConverter;
    public ChannelItemModel(ChannelItem? channelItem, IIconConverter iconConverter)
    {
        if (channelItem == null)
        {
            Id = 0;
            return;
        }

        Id = channelItem.Id;
        ChannelId = channelItem.ChannelId;
        Title = channelItem.Title;
        Description = HttpUtility.HtmlDecode(channelItem.Description);
        Content = string.IsNullOrEmpty(channelItem.Content) ? channelItem.Description : channelItem.Content;
        Link = channelItem.Link;
        PublishingDate = GetPublishingDate(channelItem.PublishingDate);
        IsRead = channelItem.IsRead;
        IsDeleted = channelItem.IsDeleted;
        IsFavorite = channelItem.IsFavorite;
        IsReadLater = channelItem.IsReadLater;
        if (channelItem.Channel != null)
        {
            ChannelTitle = channelItem.Channel.Title;
            ChannelLink = channelItem.Channel.Link;
        }

        if (channelItem.ItemCategories != null)
        {
            Categories = channelItem.ItemCategories.Select(x => new KeyValuePair<int, string>(x.CategoryId, x.Category.Name)).ToList();
        }

        _iconConverter = iconConverter;
        UpdateImageSource();
    }
    public long Id { get; set; }
    public int ChannelId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription => string.IsNullOrWhiteSpace(Description) ? string.Empty : CleanHtml(HttpUtility.HtmlDecode(Description));
    private string? _content;
    public string? Content
    {
        get => $"<body>{_content}</body>";
        set => _content = value;
    }
    public string? Link { get; set; }
    public string? PublishingDate { get; set; }
    public string? ChannelTitle { get; set; }
    public string? ChannelLink { get; set; }

    private Bitmap? _imageSource;
    public Bitmap? ImageSource
    {
        get => _imageSource;
        set => this.RaiseAndSetIfChanged(ref _imageSource, value);
    }

    public void UpdateImageSource()
    {
        if (_iconConverter != null && (!string.IsNullOrEmpty(Link) || !string.IsNullOrEmpty(ChannelLink)))
        {
            var host = new Uri(string.IsNullOrEmpty(ChannelLink) ? Link : ChannelLink).Host;
            ImageSource = _iconConverter.GetImageByChannelHost(host);
        }
    }

    private bool _isRead;
    public bool IsRead
    {
        get => _isRead;
        set => this.RaiseAndSetIfChanged(ref _isRead, value);
    }

    private bool _isFavorite;
    public bool IsFavorite
    {
        get => _isFavorite;
        set => this.RaiseAndSetIfChanged(ref _isFavorite, value);
    }

    private bool _isReadLater;
    public bool IsReadLater
    {
        get => _isReadLater;
        set => this.RaiseAndSetIfChanged(ref _isReadLater, value);
    }
    private bool _isDeleted;
    public bool IsDeleted
    {
        get => _isDeleted;
        set => this.RaiseAndSetIfChanged(ref _isDeleted, value);
    }

    public List<KeyValuePair<int, string>>? Categories { get; set; }

    private string CleanHtml(string html)
    {
        string prevHtml;
        do
        {
            prevHtml = html;
            html = Regex.Replace(html, @"<[^>]+>|&nbsp;", "").Trim();
        } while (html != prevHtml);

        return html.Substring(0, Math.Min(html.Length, 200));
    }

    private string GetPublishingDate(DateTime? dateTime)
    {
        if (dateTime == null)
        {
            return string.Empty;
        }

        if (dateTime.Value.Date == DateTime.Today)
        {
            return $"Today at {dateTime.Value.ToShortTimeString()}";
        }

        if (dateTime.Value.Date == DateTime.Today.AddDays(-1))
        {
            return "Yesterday";
        }

        return dateTime.Value.ToLongDateString();
    }
}
