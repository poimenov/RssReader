using System.Reflection;
using System.Xml;
using Moq;
using RssReader.MVVM.DataAccess.Interfaces;
using RssReader.MVVM.DataAccess.Models;
using RssReader.MVVM.Services;

namespace RssReader.MVVM.Tests.Services;

public class ExportImportTests
{
    [Fact]
    public void Export_SavesValidOpmlFile()
    {
        // Arrange
        var mockChannelsGroups = new Mock<IChannelsGroups>();
        var channelsGroup = new ChannelsGroup
        {
            Id = 1,
            Name = "Group 1"
        };
        mockChannelsGroups.Setup(m => m.GetAll()).Returns(new List<ChannelsGroup> { channelsGroup });
        var mockChannels = new Mock<IChannels>();
        var channelsInGroup = new List<Channel>()
        {
            new Channel
            {
                Id = 1,
                Title = "Channel 1",
                Url = "http://channel1.com",
                Link = "http://channel1.com/link",
                ChannelsGroupId = 1,
                Rank = 1
            },
            new Channel
            {
                Id = 2,
                Title = "Channel 2",
                Url = "http://channel2.com",
                Link = "http://channel2.com/link",
                ChannelsGroupId = 1,
                Rank = 2
            }
        };
        mockChannels.Setup(m => m.GetByGroupId(channelsGroup.Id)).Returns(channelsInGroup);
        var channelsOutGroup = new List<Channel>()
        {
            new Channel
            {
                Id = 3,
                Title = "Channel 3",
                Url = "http://channel3.com",
                Link = "http://channel3.com/link",
                ChannelsGroupId = null,
                Rank = 1
            },
            new Channel
            {
                Id = 4,
                Title = "Channel 4",
                Url = "http://channel4.com",
                Link = "http://channel4.com/link",
                ChannelsGroupId = null,
                Rank = 2
            }
        };
        mockChannels.Setup(m => m.GetByGroupId(null)).Returns(channelsOutGroup);

        var exportImport = new ExportImport(mockChannelsGroups.Object, mockChannels.Object);

        var filePath = GetFullFilename("export.opml");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        // // Act
        exportImport.Export(filePath);

        // // Assert
        Assert.True(File.Exists(filePath));

        var xmlDocument = new XmlDocument();
        xmlDocument.Load(filePath);
        var groupAttribute = xmlDocument.SelectSingleNode("/opml/body/outline[not(@type)]/@text");
        var itemsInGroup = xmlDocument.SelectNodes("/opml/body/outline[not(@type)]/outline");
        var itemsOutGroup = xmlDocument.SelectNodes("/opml/body/outline[@type]");

        Assert.Equal(channelsGroup.Name, groupAttribute?.Value);
        Assert.Equal(channelsInGroup.Count, itemsInGroup!.Count);

        foreach (XmlNode item in itemsInGroup)
        {
            var title = item.Attributes["text"]!.Value;
            var url = item.Attributes["xmlUrl"]!.Value;
            var link = item.Attributes["htmlUrl"]!.Value;
            Assert.True(channelsInGroup.Any(c => c.Title == title && c.Url == url && c.Link == link) == true);
        }

        Assert.Equal(channelsOutGroup.Count, itemsOutGroup!.Count);
        foreach (XmlNode item in itemsOutGroup)
        {
            var title = item.Attributes["text"]!.Value;
            var url = item.Attributes["xmlUrl"]!.Value;
            var link = item.Attributes["htmlUrl"]!.Value;
            Assert.True(channelsOutGroup.Any(c => c.Title == title && c.Url == url && c.Link == link) == true);
        }
    }

    [Fact]
    public void Import_LoadsValidOpmlFile()
    {
        // Arrange
        var mockChannelsGroups = new Mock<IChannelsGroups>();
        ChannelsGroup? channelsGroup = null; var groupId = 1;
        mockChannelsGroups.Setup(m => m.Get(It.IsAny<string>())).Returns(channelsGroup);
        mockChannelsGroups.Setup(m => m.Create(It.IsAny<ChannelsGroup>())).Returns(groupId);
        var mockChannels = new Mock<IChannels>();
        mockChannels.Setup(m => m.Exists(It.IsAny<string>())).Returns(false);
        mockChannels.Setup(m => m.Create(It.IsAny<Channel>())).Returns(1);
        var exportImport = new ExportImport(mockChannelsGroups.Object, mockChannels.Object);
        var filePath = GetFullFilename("import.opml");

        var xmlDocument = new XmlDocument();
        xmlDocument.Load(filePath);
        var groupAttribute = xmlDocument.SelectSingleNode("/opml/body/outline[not(@type)]/@text");
        var itemsInGroup = xmlDocument.SelectNodes("/opml/body/outline[not(@type)]/outline");
        var itemsOutGroup = xmlDocument.SelectNodes("/opml/body/outline[@type]");

        // // Act
        exportImport.Import(filePath);

        // Assert
        mockChannelsGroups.Verify(m => m.Get(It.Is<string>(arg => arg == groupAttribute!.Value)), Times.Once);
        mockChannelsGroups.Verify(m => m.Create(It.Is<ChannelsGroup>(arg => arg.Name == groupAttribute!.Value)), Times.Once);
        foreach (XmlNode item in itemsInGroup!)
        {
            var title = item.Attributes["text"]!.Value;
            var url = item.Attributes["xmlUrl"]!.Value;
            var link = item.Attributes["htmlUrl"]!.Value;
            mockChannels.Verify(m => m.Exists(It.Is<string>(arg => arg == url)), Times.Once);
            mockChannels.Verify(m => m.Create(It.Is<Channel>(arg => arg.Title == title &&
                                                                    arg.Url == url &&
                                                                    arg.Link == link &&
                                                                    arg.ChannelsGroupId == groupId)), Times.Once);
        }
        foreach (XmlNode item in itemsOutGroup!)
        {
            var title = item.Attributes["text"]!.Value;
            var url = item.Attributes["xmlUrl"]!.Value;
            var link = item.Attributes["htmlUrl"]!.Value;
            mockChannels.Verify(m => m.Exists(It.Is<string>(arg => arg == url)), Times.Once);
            mockChannels.Verify(m => m.Create(It.Is<Channel>(arg => arg.Title == title &&
                                                                    arg.Url == url &&
                                                                    arg.Link == link &&
                                                                    arg.ChannelsGroupId == null)), Times.Once);
        }
    }

    static string GetFullFilename(string filename)
    {
        string executable = new Uri(Assembly.GetExecutingAssembly().Location).LocalPath;
        return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(executable)!, filename));
    }
}
