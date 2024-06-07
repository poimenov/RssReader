using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.Services;

public class PlatformService : IPlatformService
{
    public Platform GetPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Platform.Linux;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Platform.MacOs;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Platform.Windows;
        }

        return Platform.Unknown;
    }
}
