using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.Services;

public class FilePickerService : IFilePickerService
{
    private IStorageProvider? _avaloniaStorageProvider = null;
    private IStorageProvider? AvaloniaIStorageProvider
    {
        get
        {
            if (Application.Current is App app && app.TopWindow is not null)
            {
                _avaloniaStorageProvider = TopLevel.GetTopLevel(app.TopWindow)?.StorageProvider;
            }

            return _avaloniaStorageProvider;
        }
    }

    public async Task<IReadOnlyList<IStorageFile>?> OpenFilePickerAsync(FilePickerOpenOptions options)
    {
        if (AvaloniaIStorageProvider != null)
        {
            return await AvaloniaIStorageProvider.OpenFilePickerAsync(options);
        }
        
        return null;
    }

    public Task<IStorageFile?> SaveFilePickerAsync(FilePickerSaveOptions options)
    {
        if (AvaloniaIStorageProvider!= null)
        {
            return AvaloniaIStorageProvider.SaveFilePickerAsync(options);
        }
        
        return Task.FromResult<IStorageFile?>(null);
    }
}
