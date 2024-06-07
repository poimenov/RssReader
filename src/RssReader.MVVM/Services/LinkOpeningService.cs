using System;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.Services;

public class LinkOpeningService : ILinkOpeningService
{
    private readonly IPlatformService _platformService;
    private readonly IProcessService _processService;
    public LinkOpeningService(IPlatformService platformService, IProcessService processService)
    {
        _platformService = platformService;
        _processService = processService;
    }
    public void OpenUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            switch (_platformService.GetPlatform())
            {
                case Platform.Windows:
                    _processService.Run("cmd", $"/c start {url}");
                    break;
                case Platform.Linux:
                    _processService.Run("xdg-open", url);
                    break;
                case Platform.MacOs:
                    _processService.Run("open", url);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();

            }
        }
    }
    /*
    public static void OpenUrl(this string? obj)
    {
        if (string.IsNullOrEmpty(obj))
        {
            return;
        }

        if (Uri.IsWellFormedUriString(obj, UriKind.Absolute))
        {
            obj = $"\"{obj}\"";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = obj });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", obj);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", obj);
            }
        }
    }    
    */
}
