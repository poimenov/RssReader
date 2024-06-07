using System.Linq;
using Avalonia;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using ReactiveUI;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.Services;

public class ThemeService : ReactiveObject, IThemeService
{
    private static App? _app = null;
    private static App? CurrentApplication
    {
        get
        {
            if (Application.Current is App app)
            {
                _app = app;
            }

            return _app;
        }
    }

    public ThemeVariant ActualThemeVariant => CurrentApplication?.ActualThemeVariant ?? ThemeVariant.Default;

    private ThemeVariant _requestedThemeVariant = CurrentApplication?.RequestedThemeVariant ?? ThemeVariant.Default;
    public ThemeVariant RequestedThemeVariant
    {
        get => _requestedThemeVariant;
        set
        {
            if (CurrentApplication is not null)
            {
                CurrentApplication.RequestedThemeVariant = value;
            }

            this.RaiseAndSetIfChanged(ref _requestedThemeVariant, value);
        }
    }

    public ColorPaletteResources? GetColorPaletteResource(ThemeVariant themeVariant)
    {
        ColorPaletteResources? pallete = null;
        if (CurrentApplication is not null)
        {
            var style = CurrentApplication.Styles.FirstOrDefault(x => x.GetType() == typeof(FluentTheme));
            if (style != null)
            {
                var theme = (FluentTheme)style;
                pallete = theme.Palettes[themeVariant];
            }
        }

        return pallete;
    }

    public T? GetResource<T>(string key)
    {
        T? retVal = default;
        if (CurrentApplication is not null)
        {
            if (CurrentApplication.Styles.TryGetResource(key, ActualThemeVariant, out object? obj))
            {
                if (obj is not null && obj is T value)
                {
                    retVal = value;
                }
            }
        }

        return retVal;
    }

}
