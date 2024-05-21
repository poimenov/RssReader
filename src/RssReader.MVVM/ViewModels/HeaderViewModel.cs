using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ReactiveUI;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.ViewModels;

public class HeaderViewModel : ViewModelBase
{
    private readonly IExportImport? _exportImport;
    public HeaderViewModel(IExportImport? exportImport)
    {
        _exportImport = exportImport;
        _importCount = 0;
        ExportCommand = CreateExportCommand();
        ImportCommand = CreateImportCommand();
    }

    private static FilePickerFileType Opml { get; } = new("opml file")
    {
        Patterns = new[] { "*.opml" },
        MimeTypes = new[] { "text/xml" }
    };

    private int _importCount;
    public int ImportCount
    {
        get => _importCount;
        set => this.RaiseAndSetIfChanged(ref _importCount, value);
    }

    #region Commands

    public IReactiveCommand ExportCommand { get; }

    private IReactiveCommand CreateExportCommand()
    {
        return ReactiveCommand.Create(
        async () =>
            {
                if (Application.Current is App app)
                {
                    var topLevel = TopLevel.GetTopLevel(app.TopWindow);

                    if (topLevel == null)
                    {
                        return;
                    }

                    var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                    {
                        Title = "Save Opml File"
                    });

                    if (file is not null)
                    {
                        _exportImport?.Export(file.Path.LocalPath);
                    }
                }
            }
        );
    }

    public IReactiveCommand ImportCommand { get; }

    private IReactiveCommand CreateImportCommand()
    {
        return ReactiveCommand.Create(
        async () =>
            {
                if (Application.Current is App app)
                {
                    var topLevel = TopLevel.GetTopLevel((app.TopWindow));

                    if (topLevel == null)
                    {
                        return;
                    }

                    var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                    {
                        Title = "Open Opml File",
                        AllowMultiple = false,
                        FileTypeFilter = new[] { Opml }
                    });

                    if (files.Count >= 1)
                    {
                        _exportImport?.Import(files[0].Path.LocalPath);
                        ImportCount++;
                    }
                }

            }
        );
    }

    #endregion
}
