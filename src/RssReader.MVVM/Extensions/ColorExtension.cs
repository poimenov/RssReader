using Avalonia.Media;

namespace RssReader.MVVM.Extensions;

public static class ColorExtension
{
    public static string ToCssString(this Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}
