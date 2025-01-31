using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using RssReader.MVVM.DataAccess.Interfaces;
using RssReader.MVVM.Models;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.Services;

public class ChannelModelUpdater : IChannelModelUpdater
{
    private readonly IChannels _channels;
    private readonly IChannelReader _channelReader;
    public ChannelModelUpdater(IChannels channels, IChannelReader channelReader)
    {
        _channels = channels;
        _channelReader = channelReader;
    }
    public async Task ReadChannelAsync(ChannelModel? channelModel, CancellationToken cancellationToken, IDispatcherWrapper? dispatcherWrapper = null)
    {
        if (channelModel == null || channelModel.Id <= 0 || string.IsNullOrEmpty(channelModel.Url))
        {
            throw new ArgumentNullException(nameof(channelModel));
        }

        var channel = await _channelReader.ReadChannelAsync(channelModel.Id, cancellationToken);

        if (dispatcherWrapper != null)
        {
            await dispatcherWrapper.InvokeAsync(() =>
            {
                channelModel.Title = channel.Title;
                channelModel.Description = channel.Description;
                channelModel.Link = channel.Link;
                channelModel.ImageUrl = channel.ImageUrl;
                channelModel.Url = channel.Url;
                channelModel.UnreadItemsCount = _channels.GetChannelUnreadCount(channel.Id);
            }, cancellationToken);
        }
    }
}
