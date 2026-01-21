using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.Services;

public class ClipboardService : IClipboardService
{
    private IClipboard? _avaloniaClipboard = null;
    private IClipboard? AvaloniaClipboard
    {
        get
        {
            if (Application.Current is App app && app.TopWindow is not null)
            {
                _avaloniaClipboard = TopLevel.GetTopLevel(app.TopWindow)?.Clipboard;
            }

            return _avaloniaClipboard;
        }
    }

    public Task<string?> GetTextAsync()
    {
        if (AvaloniaClipboard is not null)
        {
            return AvaloniaClipboard.TryGetTextAsync();
        }

        return Task.FromResult<string?>(null);
    }

    public async Task SetTextAsync(string text)
    {
        if (AvaloniaClipboard is not null)
        {
            await AvaloniaClipboard.SetTextAsync(text);
        }
    }
}
