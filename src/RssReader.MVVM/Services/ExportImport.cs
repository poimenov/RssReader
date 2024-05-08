using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using RssReader.MVVM.DataAccess.Interfaces;
using RssReader.MVVM.DataAccess.Models;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.Services;

public class ExportImport : IExportImport
{
    private readonly IChannelsGroups _channelsGroups;
    private readonly IChannels _channels;
    public ExportImport(IChannelsGroups channelsGroups, IChannels channels)
    {
        _channelsGroups = channelsGroups;
        _channels = channels;
    }

    public void Export(string filePath)
    {
        var xdoc = new XDocument();
        var opml = new XElement("opml", new XAttribute("version", "1.0"));
        xdoc.Add(opml);
        var head = new XElement("head");
        opml.Add(head);
        var title = new XElement("title")
        {
            Value = "RSS Reader Subscriptions"
        };
        head.Add(title);
        var body = new XElement("body");
        opml.Add(body);
        foreach (var group in _channelsGroups.GetAll())
        {
            var groupElement = new XElement("outline",
                new XAttribute("text", group.Name),
                new XAttribute("title", group.Name));
            foreach (var channel in _channels.GetByGroupId(group.Id))
            {
                groupElement.Add(GetOutlineByChannel(channel));
            }

            body.Add(groupElement);
        }

        foreach (var channel in _channels.GetByGroupId(null))
        {
            body.Add(GetOutlineByChannel(channel));
        }
        xdoc.Save(filePath);
    }

    public void Import(string filePath)
    {
        if (!Path.Exists(filePath) || Path.GetExtension(filePath) != ".opml")
        {
            return;
        }

        var xdoc = XDocument.Load(filePath);
        var body = xdoc.Element("opml")?.Element("body");
        var items = body?.Elements("outline");
        if (items != null)
        {
            foreach (var item in items)
            {
                if (item.Attributes().Any(x => x.Name == "xmlUrl") && item.Attribute("type")?.Value == "rss")
                {
                    ImportChannel(item);
                }
                else
                {
                    var groupName = item.Attribute("text")?.Value;
                    if (!string.IsNullOrEmpty(groupName))
                    {
                        var group = _channelsGroups.Get(groupName);
                        if (group == null)
                        {
                            group = new ChannelsGroup { Name = groupName };
                            _channelsGroups.Create(group);
                        }

                        foreach (var child in item.Elements("outline"))
                        {
                            ImportChannel(child, group.Id);
                        }
                    }
                }

            }

            Debug.WriteLine("Import completed");
        }
    }

    private XElement GetOutlineByChannel(Channel channel)
    {
        var siteLink = string.IsNullOrEmpty(channel.Link) ? new Uri(channel.Url).GetLeftPart(UriPartial.Authority) : channel.Link;
        return new XElement("outline",
            new XAttribute("type", "rss"),
            new XAttribute("text", channel.Title),
            new XAttribute("title", channel.Title),
            new XAttribute("xmlUrl", channel.Url),
            new XAttribute("htmlUrl", siteLink));
    }

    private void ImportChannel(XElement item, int? groupId = null)
    {
        Debug.WriteLine(item.Attribute("title")?.Value);
        if (item != null && item.Name == "outline" && item.Attribute("xmlUrl") != null)
        {
            var url = item.Attribute("xmlUrl")?.Value;
            var link = item.Attribute("htmlUrl")?.Value;
            var title = item.Attribute("title")?.Value;
            if (!string.IsNullOrEmpty(url) && !_channels.Exists(url) && !string.IsNullOrEmpty(title))
            {
                var channel = new Channel
                {
                    Url = url,
                    Link = link,
                    Title = title,
                    ChannelsGroupId = groupId
                };
                var id = _channels.Create(channel);
                Debug.WriteLine($"Channel {title} with xmlUrl='{url}' imported. Id={id}");
            }
        }
    }
}
