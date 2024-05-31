using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CodeHollow.FeedReader;
using HtmlAgilityPack;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.Services;

public class HttpHandler : IHttpHandler
{
    private const string USER_AGENT = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36";

    public async Task<byte[]> GetByteArrayAsync(Uri uri, CancellationToken cancellationToken)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
            var response = await client.GetAsync(uri, cancellationToken);
            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<string>> GetFeedUrlsFromUrlAsync(string url, CancellationToken cancellationToken)
    {
        var feedUrls = await FeedReader.GetFeedUrlsFromUrlAsync(url, cancellationToken);
        return feedUrls.Select(x => x.Url);
    }

    public async Task<string> GetStringAsync(string url, CancellationToken cancellationToken)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
            return await client.GetStringAsync(url, cancellationToken);
        }
    }

    public async Task<KeyValuePair<Uri?, HtmlDocument>> LoadFromWebAsync(string uri, CancellationToken cancellationToken)
    {
        Uri? responseUri = null;
        var webGet = new HtmlWeb
        {
            CaptureRedirect = true
        };
        var document = await webGet.LoadFromWebAsync(uri, cancellationToken);
        if (webGet.ResponseUri != null)
        {
            responseUri = webGet.ResponseUri;
            document = await webGet.LoadFromWebAsync(responseUri.ToString(), CancellationToken.None);
        }

        return new KeyValuePair<Uri?, HtmlDocument>(responseUri, document);
    }
}
