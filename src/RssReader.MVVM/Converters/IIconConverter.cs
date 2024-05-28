using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using RssReader.MVVM.Models;

namespace RssReader.MVVM.Converters;

public interface IIconConverter : IValueConverter
{
    Bitmap? GetImageByChannelModel(ChannelModel channelModel);
}
