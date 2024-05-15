using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using RssReader.MVVM.Models;

namespace RssReader.MVVM.Converters;

public class IconConverter : IValueConverter
{
    private readonly Dictionary<string, Bitmap> _icons;
    private readonly Bitmap _defaultIcon;
    private readonly Bitmap _allIcon;
    private readonly Bitmap _starredIcon;
    private readonly Bitmap _readLaterIcon;

    public IconConverter(Dictionary<string, Bitmap> icons)
    {
        _icons = icons;
        _defaultIcon = icons["default"];
        _allIcon = icons[ChannelModelType.All.ToString()];
        _starredIcon = icons[ChannelModelType.Starred.ToString()];
        _readLaterIcon = icons[ChannelModelType.ReadLater.ToString()];
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ChannelModel channel)
        {
            switch (channel.ModelType)
            {
                case ChannelModelType.All:
                    return _allIcon;
                case ChannelModelType.Starred:
                    return _starredIcon;
                case ChannelModelType.ReadLater:
                    return _readLaterIcon;
                default:
                    var url = string.IsNullOrEmpty(channel.Link) ? channel.Url : channel.Link;
                    if (!string.IsNullOrEmpty(url))
                    {
                        var key = new Uri(url).Host;
                        if (_icons.ContainsKey(key))
                        {
                            return _icons[key];
                        }
                    }
                    else
                    {
                        return null;
                    }
                    break;
            }
        }

        return _defaultIcon;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
