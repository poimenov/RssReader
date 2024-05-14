using System.Diagnostics;
using Avalonia.Input;
using Avalonia.Xaml.Interactions.DragAndDrop;

namespace RssReader.MVVM.Behaviors;

public class ChannelModelDropHandler : DropHandlerBase
{
    public ChannelModelDropHandler()
    {

    }

    public override void Over(object? sender, DragEventArgs e, object? sourceContext, object? targetContext)
    {
        Debug.WriteLine($"Over DragEventArgs: {e}");
        //Debug.WriteLine($"sourceContext: {sourceContext}, targetContext {targetContext}");
        base.Over(sender, e, sourceContext, targetContext);
    }

    public override void Drop(object? sender, DragEventArgs e, object? sourceContext, object? targetContext)
    {
        Debug.WriteLine($"Drop DragEventArgs: {e}");
        base.Drop(sender, e, sourceContext, targetContext);
    }

    public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        Debug.WriteLine($"Validate DragEventArgs: {e}");
        return base.Validate(sender, e, sourceContext, targetContext, state);
    }

    public override bool Execute(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        Debug.WriteLine($"Execute DragEventArgs: {e}");
        return base.Execute(sender, e, sourceContext, targetContext, state);
    }
}
