using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using RssReader.MVVM.DataAccess.Models;

namespace RssReader.MVVM.Models;

public class ChannelItemModel
{
    public ChannelItemModel(ChannelItem? channelItem)
    {
        if (channelItem == null)
        {
            Id = 0;
            return;
        }

        Id = channelItem.Id;
        Title = channelItem.Title;
        Description = HttpUtility.HtmlDecode(channelItem.Description);
        Content = string.IsNullOrEmpty(channelItem.Content) ? channelItem.Description : channelItem.Content;
        Link = channelItem.Link;
        PublishingDate = GetPublishingDate(channelItem.PublishingDate);
        if (channelItem.Channel != null)
        {
            ChannelTitle = channelItem.Channel.Title;
        }

        if (channelItem.ItemCategories != null)
        {
            Categories = channelItem.ItemCategories.Select(x => new KeyValuePair<int, string>(x.CategoryId, x.Category.Name)).ToList();
        }
    }
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription => string.IsNullOrWhiteSpace(Description) ? string.Empty : CleanHtml(HttpUtility.HtmlDecode(Description));
    public string? Content { get; set; }
    public string? Link { get; set; }
    public string? PublishingDate { get; set; }
    public string? ChannelTitle { get; set; }
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
