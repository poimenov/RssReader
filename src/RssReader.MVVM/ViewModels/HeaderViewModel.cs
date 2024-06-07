using Avalonia;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using ReactiveUI;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.ViewModels;

public class HeaderViewModel : ViewModelBase
{
    private const string SWITCH_TO_LIGHT = "Switch to light theme";
    private const string SWITCH_TO_DARK = "Switch to dark theme";
    private const string ICON_TO_LIGHT = "ToLightTheme";
    private const string ICON_TO_DARK = "ToDarkTheme";
    private readonly IExportImport? _exportImport;
    private readonly ILinkOpeningService _linkOpeningService;
    private readonly IFilePickerService _filePickerService;
    private readonly IThemeService _themeService;
    private readonly AppSettings _settings;
    public HeaderViewModel(IExportImport? exportImport, ILinkOpeningService linkOpeningService, IFilePickerService filePickerService, IThemeService themeService, AppSettings settings)
    {
        _exportImport = exportImport;
        _linkOpeningService = linkOpeningService;
        _filePickerService = filePickerService;
        _themeService = themeService;
        _settings = settings;
        _importCount = 0;
        ExportCommand = CreateExportCommand();
        ImportCommand = CreateImportCommand();
        SwitchThemeCommand = CreateSwitchThemeCommand();
        OpenSourceCodeCommand = CreateOpenSourceCodeCommand();
        SetSwitchThemeText(_themeService.ActualThemeVariant);
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

    private Geometry? _switchIcon;
    public Geometry? SwitchIcon
    {
        get => _switchIcon;
        set => this.RaiseAndSetIfChanged(ref _switchIcon, value);
    }

    #region Commands

    public IReactiveCommand ExportCommand { get; }

    private IReactiveCommand CreateExportCommand()
    {
        return ReactiveCommand.Create(
        async () =>
            {
                var file = await _filePickerService.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save Opml File"
                });

                if (file is not null)
                {
                    _exportImport?.Export(file.Path.LocalPath);
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
                var files = await _filePickerService.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Open Opml File",
                    AllowMultiple = false,
                    FileTypeFilter = new[] { Opml }
                });

                if (files is not null && files.Count >= 1)
                {
                    _exportImport?.Import(files[0].Path.LocalPath);
                    ImportCount++;
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
                var theme = (_themeService.ActualThemeVariant != ThemeVariant.Light) ? ThemeVariant.Light : ThemeVariant.Dark;
                _themeService.RequestedThemeVariant = theme;
                _settings.SetTheme(theme);
                _settings.Save();
                SetSwitchThemeText(theme);
            }
        );
    }

    public IReactiveCommand OpenSourceCodeCommand { get; }
    private IReactiveCommand CreateOpenSourceCodeCommand()
    {
        return ReactiveCommand.Create(
            () =>
            {
                _linkOpeningService.OpenUrl("https://github.com/poimenov/RssReader");
            }
        );
    }

    #endregion

    private void SetSwitchThemeText(ThemeVariant themeVariant)
    {
        SwitchThemeText = (themeVariant == ThemeVariant.Dark) ? SWITCH_TO_LIGHT : SWITCH_TO_DARK;
        SwitchIcon = _themeService.GetResource<StreamGeometry>((themeVariant == ThemeVariant.Dark) ? ICON_TO_LIGHT : ICON_TO_DARK);
    }
}
