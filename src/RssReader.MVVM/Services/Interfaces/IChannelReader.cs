using System;
using System.Threading;
using System.Threading.Tasks;
using RssReader.MVVM.DataAccess.Models;

namespace RssReader.MVVM.Services.Interfaces;

public interface IChannelReader
{
    Task<Channel> ReadChannelAsync(int channelId, string iconsDirectoryPath, CancellationToken cancellationToken);
    Task DownloadIconAsync(Uri? imageUri, Uri? siteUri, string iconsDirectoryPath, CancellationToken cancellationToken);
}
