using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RssReader.MVVM.Extensions;

public static class StringExtensions
{
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
}
