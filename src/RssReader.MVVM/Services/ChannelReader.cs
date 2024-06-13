using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Avalonia.Threading;
using CodeHollow.FeedReader;
using log4net;
using Microsoft.Extensions.Options;
using RssReader.MVVM.DataAccess.Interfaces;
using RssReader.MVVM.DataAccess.Models;
using RssReader.MVVM.Models;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.Services;

public class ChannelReader : IChannelReader
{
    private readonly object _locker = new object();
    private readonly ILog _log;
    private readonly IChannels _channels;
    private readonly IChannelItems _channelItems;
    private readonly IHttpHandler _httpHandler;
    private readonly IDispatcherWrapper _dispatcherWrapper;
    private readonly AppSettings _appSettings;

    public ChannelReader(IHttpHandler httpHandler, IDispatcherWrapper dispatcherWrapper, IChannels channels, IChannelItems channelItems, IOptions<AppSettings> options, ILog log)
    {
        _httpHandler = httpHandler;
        _dispatcherWrapper = dispatcherWrapper;
        _channels = channels;
        _channelItems = channelItems;
        _appSettings = options.Value;
        _log = log;
    }

    public async Task ReadChannelAsync(ChannelModel? channelModel, CancellationToken cancellationToken)
    {
        if (channelModel == null || channelModel.Id <= 0 || string.IsNullOrEmpty(channelModel.Url))
        {
            throw new ArgumentNullException(nameof(channelModel));
        }

        var channel = _channels.Get(channelModel.Id);

        if (channel == null)
        {
            throw new ArgumentException("Channel not found", nameof(channelModel));
        }

        try
        {
            Debug.WriteLine($"Start read url = {channel.Url}, ThreadId = {Thread.CurrentThread.ManagedThreadId}");

            Feed? feed = null;

            try
            {
                feed = await GetFeedAsync(channel.Url, cancellationToken);
            }
            catch (System.Exception)
            {
                var feedLinks = await FeedReader.GetFeedUrlsFromUrlAsync(channel.Url, cancellationToken);
                if (feedLinks != null && feedLinks.Count() > 0)
                {
                    var url = feedLinks.First().Url;
                    feed = await GetFeedAsync(url, cancellationToken);
                    channel.Url = url;
                }
            }

            if (feed != null)
            {
                Uri? imageUri = null;
                if (feed.ImageUrl != null)
                {
                    imageUri = new Uri(feed.ImageUrl);
                }

                Uri? siteUri = null;
                var siteLink = string.IsNullOrEmpty(channelModel.Link) ? new Uri(channelModel.Url).GetLeftPart(UriPartial.Authority) : channelModel.Link;
                if (!string.IsNullOrEmpty(siteLink))
                {
                    siteUri = new Uri(siteLink);
                }

                await DownloadIconAsync(imageUri, siteUri, cancellationToken);

                lock (_locker)
                {
                    if (!string.IsNullOrEmpty(feed.Title))
                    {
                        channel.Title = (new Uri(channel.Url).Host == channel.Title) ? feed.Title : channel.Title;
                        channel.Link = feed.Link ?? siteLink;
                        channel.Description = feed.Description;
                        channel.ImageUrl = feed.ImageUrl;
                        channel.Language = feed.Language;
                        channel.LastUpdatedDate = feed.LastUpdatedDate;

                        _channels.Update(channel);
                    }

                    feed.Items.Select(x => new
                    {
                        Item = new ChannelItem
                        {
                            ChannelId = channel.Id,
                            Title = x.Title,
                            Description = x.Description,
                            Content = x.Content,
                            Link = x.Link,
                            PublishingDate = x.PublishingDate,
                            ItemId = string.IsNullOrEmpty(x.Id) ? x.Link : x.Id,
                        },
                        Categories = x.Categories.ToArray()
                    }).ToList().ForEach(x => _channelItems.Create(x.Item, x.Categories));
                }

                await _dispatcherWrapper.InvokeAsync(() =>
                {
                    channelModel.Title = channel.Title;
                    channelModel.Description = channel.Description;
                    channelModel.Link = channel.Link;
                    channelModel.ImageUrl = channel.ImageUrl;
                    channelModel.Url = channel.Url;
                    channelModel.UnreadItemsCount = _channels.GetChannelUnreadCount(channel.Id);
                }, DispatcherPriority.Background, cancellationToken);
                Debug.WriteLine($"End read url = {channel.Url}, ThreadId = {Thread.CurrentThread.ManagedThreadId}");
            }
            else
            {
                Debug.WriteLine($"Can't load feed from url = {channel.Url}");
                _log.InfoFormat($"Can't load feed from url = {channel.Url}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Url = {channel.Url}");
            Debug.WriteLine(ex);
            _log.InfoFormat($"Url = {channel.Url}");
            _log.Error(ex);
        }
    }

    public async Task DownloadIconAsync(Uri? imageUri, Uri? siteUri, CancellationToken cancellationToken)
    {
        if (imageUri != null || siteUri != null)
        {
            try
            {
                if (!Directory.Exists(IconsDirectoryPath))
                {
                    Directory.CreateDirectory(IconsDirectoryPath);
                }

                if (siteUri != null && Directory.GetFiles(IconsDirectoryPath, $"{siteUri.Host}.*").Length == 0)
                {
                    if (imageUri == null)
                    {
                        var responseUri = siteUri;
                        var result = await _httpHandler.LoadFromWebAsync(siteUri.ToString(), cancellationToken);
                        if (result.Key != null)
                        {
                            responseUri = result.Key;
                        }

                        var document = result.Value;
                        var iconLink = document.DocumentNode.SelectSingleNode("//head/link[@rel=\"icon\"]");
                        if (iconLink != null)
                        {
                            imageUri = new Uri(responseUri, iconLink.Attributes["href"].Value);
                        }
                        else
                        {
                            var shortcutIconLink = document.DocumentNode.SelectSingleNode("//head/link[@rel=\"shortcut icon\"]");
                            if (shortcutIconLink != null)
                            {
                                imageUri = new Uri(responseUri, shortcutIconLink.Attributes["href"].Value);
                            }
                            else
                            {
                                var appleTouchIconLink = document.DocumentNode.SelectSingleNode("//head/link[@rel=\"apple-touch-icon\"]");
                                if (appleTouchIconLink != null)
                                {
                                    imageUri = new Uri(responseUri, appleTouchIconLink.Attributes["href"].Value);
                                }
                            }
                        }
                    }

                    if (imageUri != null)
                    {
                        Debug.WriteLine($"Downloading icon : {imageUri}");
                        var data = await _httpHandler.GetByteArrayAsync(imageUri, cancellationToken);
                        var fileExtension = GetExtension(data);
                        if (data != null && data.Length > 0 && !string.IsNullOrEmpty(fileExtension))
                        {
                            var fileName = $"{siteUri.Host}{fileExtension}";
                            File.WriteAllBytes(Path.Combine(IconsDirectoryPath, fileName), data);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                _log.Error(ex);
            }
        }
    }

    private string _iconsDirectoryPath = string.Empty;
    public string IconsDirectoryPath
    {
        get
        {
            if (string.IsNullOrEmpty(_iconsDirectoryPath))
            {
                _iconsDirectoryPath = Path.Combine(_appSettings.AppDataPath, "Icons");
            }

            return _iconsDirectoryPath;
        }
        set
        {
            _iconsDirectoryPath = value;
        }
    }

    private static string GetExtension(byte[] fileBytes)
    {
        var retVal = string.Empty;
        byte[] jpegMagic = { 0xFF, 0xD8 };
        byte[] pngMagic = { 0x89, 0x50, 0x4E, 0x47 };
        byte[] gifMagic = { 0x47, 0x49, 0x46, 0x38 };
        byte[] icoMagic = { 0x00, 0x00, 0x01, 0x00 };
        byte[] bmpMagic = { 0x42, 0x4D };
        byte[] webpMagic = { 0x57, 0x45, 0x42, 0x50 };

        if (fileBytes.Length >= 4)
        {
            if (fileBytes[0] == jpegMagic[0] && fileBytes[1] == jpegMagic[1])
                retVal = ".jpg";
            else if (fileBytes[0] == pngMagic[0] && fileBytes[1] == pngMagic[1])
                retVal = ".png";
            else if (fileBytes[0] == gifMagic[0] && fileBytes[1] == gifMagic[1])
                retVal = ".gif";
            else if (fileBytes[0] == icoMagic[0] && fileBytes[1] == icoMagic[1] &&
                     fileBytes[2] == icoMagic[2] && fileBytes[3] == icoMagic[3])
                retVal = ".ico";
            if (fileBytes[0] == bmpMagic[0] && fileBytes[1] == bmpMagic[1])
                retVal = ".bmp";
            else if (fileBytes[0] == webpMagic[0] && fileBytes[1] == webpMagic[1] &&
                     fileBytes[2] == webpMagic[2] && fileBytes[3] == webpMagic[3])
                retVal = ".webp";
        }

        return retVal;
    }

    private async Task<Feed?> GetFeedAsync(string url, CancellationToken cancellationToken)
    {
        string response = await _httpHandler.GetStringAsync(url, cancellationToken);
        var doc = XDocument.Parse(response);
        return FeedReader.ReadFromString(doc.ToString());
    }
}