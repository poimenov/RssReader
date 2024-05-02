using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RssReader.MVVM.DataAccess.Models;
using RssReader.MVVM.Models;

namespace RssReader.MVVM.Services.Interfaces;

public interface IChannelReader
{
    Task ReadAllChannelsAsync();
    Task<Channel> ReadChannelAsync(Uri uri);
    Task ReadChannelAsync(ChannelModel channelModel, CancellationToken cancellationToken);
    Task DownloadIconAsync(Uri uri, CancellationToken cancellationToken);
}
