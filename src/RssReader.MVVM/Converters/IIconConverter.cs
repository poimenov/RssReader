using Avalonia.Media.Imaging;
using RssReader.MVVM.Models;

namespace RssReader.MVVM.Converters;

public interface IIconConverter
{
    Bitmap? GetImageByChannelModel(ChannelModel channelModel);
    Bitmap? GetImageByChannelHost(string? host);
}
