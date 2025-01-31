using System;
using System.Threading;
using System.Threading.Tasks;

namespace RssReader.MVVM.Services.Interfaces;

public interface IDispatcherWrapper
{
    Task InvokeAsync(Action action, CancellationToken cancellationToken);
}
