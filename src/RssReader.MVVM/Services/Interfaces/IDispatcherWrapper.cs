using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace RssReader.MVVM.Services.Interfaces;

public interface IDispatcherWrapper
{
    DispatcherOperation InvokeAsync(Action action, DispatcherPriority priority, CancellationToken cancellationToken);

    //DispatcherOperation<TResult> InvokeAsync<TResult>(Func<TResult> callback);    
}
