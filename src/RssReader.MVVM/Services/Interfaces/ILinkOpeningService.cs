using System;

namespace RssReader.MVVM.Services.Interfaces;

public interface ILinkOpeningService
{
    void OpenUrl(string url);
}
