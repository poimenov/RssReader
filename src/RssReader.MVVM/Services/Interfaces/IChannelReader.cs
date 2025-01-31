using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using RssReader.MVVM.Models;

namespace RssReader.MVVM.Services.Interfaces;

public interface IChannelReader
{
    Task<DataAccess.Models.Channel> ReadChannelAsync(int channelId, CancellationToken cancellationToken);
    Task DownloadIconAsync(Uri? imageUri, Uri? siteUri, CancellationToken cancellationToken);
    string IconsDirectoryPath { get; set; }
}
