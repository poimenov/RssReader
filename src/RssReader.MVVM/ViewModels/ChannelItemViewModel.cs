using System;
using System.Text.RegularExpressions;
using System.Web;
using RssReader.MVVM.DataAccess.Models;

namespace RssReader.MVVM.ViewModels;

public class ChannelItemViewModel : ViewModelBase
{
    private readonly ChannelItem _channelItem;
    public ChannelItemViewModel(ChannelItem channelItem)
    {
        _channelItem = channelItem;
    }

    public ChannelItem Source => _channelItem;

    public string Description
    {
        get
        {
            if (Source != null && !string.IsNullOrWhiteSpace(Source.Description))
            {
                return CleanHtml(HttpUtility.HtmlDecode(Source.Description));
            }

            return string.Empty;

        }
    }

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
}
