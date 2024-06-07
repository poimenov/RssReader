using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace RssReader.MVVM.Services.Interfaces;

public interface IFilePickerService
{
    Task<IStorageFile?> SaveFilePickerAsync(FilePickerSaveOptions options);
    Task<IReadOnlyList<IStorageFile>?> OpenFilePickerAsync(FilePickerOpenOptions options);
}
