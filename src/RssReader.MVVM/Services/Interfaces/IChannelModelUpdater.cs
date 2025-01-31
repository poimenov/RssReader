using System.Threading;
using System.Threading.Tasks;
using RssReader.MVVM.Models;

namespace RssReader.MVVM.Services.Interfaces;

public interface IChannelModelUpdater
{
    Task ReadChannelAsync(ChannelModel? channelModel, CancellationToken cancellationToken, IDispatcherWrapper? dispatcherWrapper = null);
}
