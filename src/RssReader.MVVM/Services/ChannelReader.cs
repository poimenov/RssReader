using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CodeHollow.FeedReader;
using log4net;
using RssReader.MVVM.DataAccess.Interfaces;
using RssReader.MVVM.DataAccess.Models;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.Services;

public class ChannelReader : IChannelReader
{
    private readonly object _locker = new object();
    private readonly ILog _log;
    private readonly IChannels _channels;
    private readonly IChannelItems _channelItems;
    public ChannelReader(IChannels channels, IChannelItems channelItems, ILog log)
    {
        _channels = channels;
        _channelItems = channelItems;
        _log = log;
    }

    public async Task<Channel> ReadChannelAsync(Uri uri)
    {
        var feed = await FeedReader.ReadAsync(uri.ToString());

        return new Channel
        {
            Title = feed.Title,
            Description = feed.Description,
            ImageUrl = feed.ImageUrl,
            Link = feed.Link,
            Language = feed.Language,
            Url = uri.ToString(),
            Items = feed.Items.Select(x => new ChannelItem
            {
                Title = x.Title,
                Description = x.Description,
                Content = x.Content,
                Link = x.Link,
                PublishingDate = x.PublishingDate,
                ItemId = x.Id
            }).ToList()
        };
    }

    public async Task ReadAllChannelsAsync()
    {
        var tasks = _channels.GetAll().Select(async x => await this.ReadAndSaveChannelAsync(x));
        await Task.WhenAll(tasks);
    }

    private async Task ReadAndSaveChannelAsync(Channel channel)
    {
        if (channel != null)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var feed = await FeedReader.ReadAsync(channel.Url);
                // Feed? feed = null;
                // try
                // {
                //     feed = await FeedReader.ReadAsync(channel.Url);
                // }
                // catch (XmlException)
                // {
                //     var doc = new XmlDocument();
                //     doc.Load(channel.Url);
                //     feed = FeedReader.ReadFromString(doc.OuterXml);
                //     Debug.WriteLine("From xml: " + channel.Url);
                // }
                // catch (System.Net.Http.HttpRequestException)
                // {
                //     if (!string.IsNullOrEmpty(channel.Link))
                //     {
                //         var uris = await FeedReader.ParseFeedUrlsAsStringAsync(channel.Link);
                //         if (uris.Any() && !string.IsNullOrWhiteSpace(uris.First()) && uris.First() != channel.Url)
                //         {
                //             feed = await FeedReader.ReadAsync(uris.First());
                //             Debug.WriteLine("Feed url: " + uris.First());
                //             channel.Url = uris.First();
                //         }
                //     }
                // }
                // catch (Exception)
                // {
                //     throw;
                // }

                if (feed != null)
                {
                    lock (_locker)
                    {
                        if (!string.IsNullOrEmpty(feed.Title) && !string.IsNullOrEmpty(feed.Link))
                        {
                            channel.Title = feed.Title;
                            channel.Link = feed.Link;
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
                                ItemId = x.Id
                            },
                            Categories = x.Categories.ToArray()
                        }).ToList().ForEach(x => _channelItems.Create(x.Item, x.Categories));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Url = {channel.Url}");
                Debug.WriteLine(ex);
                _log.InfoFormat("Url = {0}", channel.Url);
                _log.Error(ex);
            }

            await Task.Delay(TimeSpan.FromMinutes(1));
        }
    }

}
