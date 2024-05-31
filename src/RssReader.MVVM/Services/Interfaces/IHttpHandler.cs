using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace RssReader.MVVM.Services.Interfaces;

public interface IHttpHandler
{
    Task<string> GetStringAsync(string url, CancellationToken cancellationToken);
    Task<byte[]> GetByteArrayAsync(Uri uri, CancellationToken cancellationToken);
    Task<IEnumerable<string>> GetFeedUrlsFromUrlAsync(string url, CancellationToken cancellationToken);
    Task<KeyValuePair<Uri?, HtmlDocument>> LoadFromWebAsync(string url, CancellationToken cancellationToken);
}
