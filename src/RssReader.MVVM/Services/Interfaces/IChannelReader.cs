using System;
using System.Threading;
using System.Threading.Tasks;
using RssReader.MVVM.Models;

namespace RssReader.MVVM.Services.Interfaces;

public interface IChannelReader
{
    Task ReadChannelAsync(ChannelModel channelModel, CancellationToken cancellationToken);
    Task DownloadIconAsync(Uri? imageUri, Uri? siteUri, CancellationToken cancellationToken);
}
