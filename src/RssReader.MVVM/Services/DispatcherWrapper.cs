using System;
using System.Threading;
using Avalonia.Threading;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.Services;

public class DispatcherWrapper : IDispatcherWrapper
{
    public DispatcherOperation InvokeAsync(Action action, DispatcherPriority priority, CancellationToken cancellationToken)
    {
        return Dispatcher.UIThread.InvokeAsync(action, priority, cancellationToken);
    }
}
