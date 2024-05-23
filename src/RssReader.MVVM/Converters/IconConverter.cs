using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using RssReader.MVVM.Models;

namespace RssReader.MVVM.Converters;

public class IconConverter : IValueConverter
{
    private readonly Dictionary<string, Bitmap> _icons;
    private readonly Bitmap _defaultIcon;
    private readonly Bitmap _allIcon;
    private readonly Bitmap _starredIcon;
    private readonly Bitmap _readLaterIcon;
    public const string ASSETS_PATH = "avares://RssReader.MVVM/Assets";
    private const string DEFAULT = "default";

    public static readonly IconConverter Instance = new();

    public IconConverter()
    {
        _icons = new Dictionary<string, Bitmap>();
        using (var defaultStream = AssetLoader.Open(new Uri($"{ASSETS_PATH}/rss-button-orange.32.png")))
        using (var allStream = AssetLoader.Open(new Uri($"{ASSETS_PATH}/document-documents-file-page-svgrepo-com.png")))
        using (var starredStream = AssetLoader.Open(new Uri($"{ASSETS_PATH}/bookmark-favorite-rating-star-svgrepo-com.png")))
        using (var readLaterStream = AssetLoader.Open(new Uri($"{ASSETS_PATH}/flag-location-map-marker-pin-pointer-svgrepo-com.png")))
        {
            _icons.Add(DEFAULT, new Bitmap(defaultStream));
            _icons.Add(ChannelModelType.All.ToString(), new Bitmap(allStream));
            _icons.Add(ChannelModelType.Starred.ToString(), new Bitmap(starredStream));
            _icons.Add(ChannelModelType.ReadLater.ToString(), new Bitmap(readLaterStream));
        }

        if (Directory.Exists(IconsDirectoryPath))
        {
            foreach (var fileIcon in Directory.GetFiles(IconsDirectoryPath))
            {
                if (AllowedExtensions!.Contains(Path.GetExtension(fileIcon)))
                {
                    using (var stream = File.OpenRead(fileIcon))
                    {
                        _icons.Add(Path.GetFileNameWithoutExtension(fileIcon), new Bitmap(stream));
                    }
                }
            }
        }

        _defaultIcon = _icons[DEFAULT];
        _allIcon = _icons[ChannelModelType.All.ToString()];
        _starredIcon = _icons[ChannelModelType.Starred.ToString()];
        _readLaterIcon = _icons[ChannelModelType.ReadLater.ToString()];
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
                    if (!channel.IsChannelsGroup && !string.IsNullOrEmpty(url))
                    {
                        var key = new Uri(url).Host;
                        if (_icons.ContainsKey(key))
                        {
                            return _icons[key];
                        }
                        else if (Directory.Exists(IconsDirectoryPath))
                        {
                            var files = Directory.GetFiles(IconsDirectoryPath, key);
                            if (files.Any() && AllowedExtensions!.Contains(Path.GetExtension(files.First())))
                            {
                                var fileIcon = files.First();
                                using (var stream = File.OpenRead(fileIcon))
                                {
                                    var img = new Bitmap(stream);
                                    _icons.Add(key, img);
                                    return img;
                                }
                            }
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

    private string[]? _allowedExtensions;
    private string[]? AllowedExtensions
    {
        get
        {
            if (_allowedExtensions == null)
            {
                _allowedExtensions = new[] { ".ico", ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp" };
            }

            return _allowedExtensions;
        }
    }

    private string IconsDirectoryPath
    {
        get
        {
            return Path.Combine(AppSettings.AppDataPath, "Icons");
        }
    }
}
