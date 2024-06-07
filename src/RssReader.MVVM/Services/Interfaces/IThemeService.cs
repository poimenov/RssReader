using Avalonia.Styling;
using Avalonia.Themes.Fluent;

namespace RssReader.MVVM.Services.Interfaces;

public interface IThemeService
{
    ThemeVariant ActualThemeVariant { get; }
    ThemeVariant RequestedThemeVariant { get; set; }
    ColorPaletteResources? GetColorPaletteResource(ThemeVariant themeVariant);
    T? GetResource<T>(string key);
}
