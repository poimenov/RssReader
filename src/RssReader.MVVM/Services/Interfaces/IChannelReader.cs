using System;
using System.Threading;
using System.Threading.Tasks;
using RssReader.MVVM.Models;

namespace RssReader.MVVM.Services.Interfaces;

public interface IChannelReader
{
    Task ReadChannelAsync(ChannelModel? channelModel, CancellationToken cancellationToken, IDispatcherWrapper? dispatcherWrapper = null);
    Task DownloadIconAsync(Uri? imageUri, Uri? siteUri, CancellationToken cancellationToken);
    string IconsDirectoryPath { get; set; }
}
