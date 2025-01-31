using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace RssReader.MVVM.Services.Interfaces;

public interface IDispatcherWrapper
{
    Task InvokeAsync(Action action, CancellationToken cancellationToken);
}
