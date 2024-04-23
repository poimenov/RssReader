using System.Diagnostics;
using System.IO;
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
        throw new System.NotImplementedException();
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
                        var url = child.Attribute("xmlUrl")?.Value;
                        var link = child.Attribute("htmlUrl")?.Value;
                        var title = child.Attribute("text")?.Value;
                        if (!string.IsNullOrEmpty(url) && !_channels.Exists(url) && !string.IsNullOrEmpty(title))
                        {
                            var channel = new Channel
                            {
                                Url = url,
                                Link = link,
                                ChannelsGroupId = group.Id,
                                Title = title
                            };
                            _channels.Create(channel);
                        }
                    }
                }
            }

            Debug.WriteLine("Import completed");
        }
    }
}
