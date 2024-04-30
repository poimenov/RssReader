using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeHollow.FeedReader;
using log4net;
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
    public ChannelReader(IChannels channels, IChannelItems channelItems, ILog log)
    {
        _channels = channels;
        _channelItems = channelItems;
        _log = log;
    }

    public async Task ReadChannelAsync(ChannelModel channelModel)
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
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Debug.WriteLine($"Start read url = {channel.Url}");

            var feed = await FeedReader.ReadAsync(channel.Url);

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
                            ItemId = string.IsNullOrEmpty(x.Id) ? x.Link : x.Id,
                        },
                        Categories = x.Categories.ToArray()
                    }).ToList().ForEach(x => _channelItems.Create(x.Item, x.Categories));
                }
                channelModel.Title = channel.Title;
                channelModel.Description = channel.Description;
                channelModel.Link = channel.Link;
                channelModel.ImageUrl = channel.ImageUrl;
                channelModel.Url = channel.Url;
                channelModel.UnreadItemsCount = _channels.GetUnreadCount(channel.Id);
                Debug.WriteLine($"End read url = {channel.Url}");
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
