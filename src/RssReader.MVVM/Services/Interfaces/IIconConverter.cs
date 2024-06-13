using Avalonia.Media.Imaging;
using RssReader.MVVM.Models;

namespace RssReader.MVVM.Services.Interfaces;

public interface IIconConverter
{
    Bitmap? GetImageByChannelModel(ChannelModel channelModel);
    Bitmap? GetImageByChannelHost(string? host);
}
