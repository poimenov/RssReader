using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RssReader.MVVM.DataAccess.Models;

namespace RssReader.MVVM.Services.Interfaces;

public interface IChannelReader
{
    Task ReadAllChannelsAsync();
    Task<Channel> ReadChannelAsync(Uri uri);
}
