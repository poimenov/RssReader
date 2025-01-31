using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.Services;

public class DispatcherWrapper : IDispatcherWrapper
{
    async Task IDispatcherWrapper.InvokeAsync(Action action, CancellationToken cancellationToken)
    {
        await Dispatcher.UIThread.InvokeAsync(action, DispatcherPriority.Background, cancellationToken);
    }
}
