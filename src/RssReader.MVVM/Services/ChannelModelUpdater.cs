using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RssReader.MVVM.DataAccess.Interfaces;
using RssReader.MVVM.Models;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.Services;

public class ChannelModelUpdater : IChannelModelUpdater
{
    private readonly IChannels _channels;
    private readonly IChannelReader _channelReader;
    private readonly AppSettings _appSettings;
    public ChannelModelUpdater(IChannels channels, IChannelReader channelReader, IOptions<AppSettings> options)
    {
        _channels = channels;
        _channelReader = channelReader;
        _appSettings = options.Value;
    }
    public async Task ReadChannelAsync(ChannelModel? channelModel, CancellationToken cancellationToken, IDispatcherWrapper? dispatcherWrapper = null)
    {
        if (channelModel == null || channelModel.Id <= 0 || string.IsNullOrEmpty(channelModel.Url))
        {
            throw new ArgumentNullException(nameof(channelModel));
        }

        var channel = await _channelReader.ReadChannelAsync(channelModel.Id, IconsDirectoryPath, cancellationToken);

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
}
