using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using ReactiveUI;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.ViewModels;

public class HeaderViewModel : ViewModelBase
{
    private const string SWITCH_TO_LIGHT = "Switch to light theme";
    private const string SWITCH_TO_DARK = "Switch to dark theme";
    private readonly IExportImport? _exportImport;
    public HeaderViewModel(IExportImport? exportImport)
    {
        _exportImport = exportImport;
        _importCount = 0;
        ExportCommand = CreateExportCommand();
        ImportCommand = CreateImportCommand();
        SwitchThemeCommand = CreateSwitchThemeCommand();
        if (Application.Current is App app)
        {
            SetSwitchThemeText(app.ActualThemeVariant);
        }
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

    private string? _switchThemeText;
    public string? SwitchThemeText
    {
        get => _switchThemeText;
        set => this.RaiseAndSetIfChanged(ref _switchThemeText, value);
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

    public IReactiveCommand SwitchThemeCommand { get; }
    private IReactiveCommand CreateSwitchThemeCommand()
    {
        return ReactiveCommand.Create(
            () =>
            {
                if (Application.Current is App app)
                {
                    var theme = (app.ActualThemeVariant == ThemeVariant.Dark) ? ThemeVariant.Light : ThemeVariant.Dark;
                    app.RequestedThemeVariant = theme;
                    var settings = app.Settings;
                    settings.SetTheme(theme);
                    settings.Save();
                    SetSwitchThemeText(theme);
                }
            }
        );
    }

    #endregion

    private void SetSwitchThemeText(ThemeVariant themeVariant)
    {
        if (Application.Current is App app)
        {
            SwitchThemeText = (themeVariant == ThemeVariant.Dark) ? SWITCH_TO_LIGHT : SWITCH_TO_DARK;
        }
    }
}
