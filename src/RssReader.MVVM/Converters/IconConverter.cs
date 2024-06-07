using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.Extensions.Options;
using RssReader.MVVM.Models;

namespace RssReader.MVVM.Converters;

public class IconConverter : IIconConverter
{
    private readonly Dictionary<string, Bitmap> _icons;
    private readonly AppSettings _appSettings;
    private readonly Bitmap _defaultIcon;
    private readonly Bitmap _allIcon;
    private readonly Bitmap _starredIcon;
    private readonly Bitmap _readLaterIcon;
    private const string DEFAULT = "default";

    public IconConverter(IOptions<AppSettings> options)
    {
        _icons = new Dictionary<string, Bitmap>();
        _appSettings = options.Value;
        using (var defaultStream = AssetLoader.Open(new Uri($"{AppSettings.AvaResPath}/rss-button-orange.32.png")))
        using (var allStream = AssetLoader.Open(new Uri($"{AppSettings.AvaResPath}/document-documents-file-page-svgrepo-com.png")))
        using (var starredStream = AssetLoader.Open(new Uri($"{AppSettings.AvaResPath}/bookmark-favorite-rating-star-svgrepo-com.png")))
        using (var readLaterStream = AssetLoader.Open(new Uri($"{AppSettings.AvaResPath}/flag-location-map-marker-pin-pointer-svgrepo-com.png")))
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
            return GetImageByChannelModel(channel);
        }

        return _defaultIcon;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public Bitmap? GetImageByChannelModel(ChannelModel channelModel)
    {
        var retVal = _defaultIcon;
        switch (channelModel.ModelType)
        {
            case ChannelModelType.All:
                retVal = _allIcon;
                break;
            case ChannelModelType.Starred:
                retVal = _starredIcon;
                break;
            case ChannelModelType.ReadLater:
                retVal = _readLaterIcon;
                break;
            default:
                var url = string.IsNullOrEmpty(channelModel.Link) ? channelModel.Url : channelModel.Link;
                if (!channelModel.IsChannelsGroup && !string.IsNullOrEmpty(url))
                {
                    var key = new Uri(url).Host;
                    if (_icons.ContainsKey(key))
                    {
                        retVal = _icons[key];
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
                                retVal = img;
                            }
                        }
                    }
                }
                break;
        }

        return retVal;
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
            return Path.Combine(_appSettings.AppDataPath, "Icons");
        }
    }
}
