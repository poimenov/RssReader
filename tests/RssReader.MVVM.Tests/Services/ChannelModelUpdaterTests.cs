using System.Reflection;
using System.Xml;
using log4net;
using Microsoft.Extensions.Options;
using Moq;
using RssReader.MVVM.DataAccess.Interfaces;
using RssReader.MVVM.DataAccess.Models;
using RssReader.MVVM.Models;
using RssReader.MVVM.Services;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.Tests.Services
{
    public class ChannelModelUpdaterTests
    {
        [Fact]
        public async Task ReadChannelAsync_InvalidChannelModel_ThrowsException()
        {
            //Arrange
            var mockHttpHandler = new Mock<IHttpHandler>();
            var mockChannels = new Mock<IChannels>();
            var mockChannelItems = new Mock<IChannelItems>();
            var mockLog = new Mock<ILog>();
            var mockIconConverter = new Mock<IIconConverter>();
            var mockDispatherWrapper = new Mock<IDispatcherWrapper>();
            var mockAppSettings = new Mock<IOptions<AppSettings>>();
            var mockChannelReader = new Mock<IChannelReader>();

            var ChannelModelUpdater = new ChannelModelUpdater(mockChannels.Object, mockChannelReader.Object, mockAppSettings.Object);

            ChannelModel? channelModel = null;
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => ChannelModelUpdater.ReadChannelAsync(channelModel, CancellationToken.None, mockDispatherWrapper.Object));

            channelModel = new ChannelModel(0, "Test", null, "https://example.com/rss", null, null, 0, 0, mockIconConverter.Object);
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => ChannelModelUpdater.ReadChannelAsync(channelModel, CancellationToken.None, mockDispatherWrapper.Object));

            channelModel = new ChannelModel(1, "Test", null, string.Empty, null, null, 0, 0, mockIconConverter.Object);
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => ChannelModelUpdater.ReadChannelAsync(channelModel, CancellationToken.None, mockDispatherWrapper.Object));

            channelModel = new ChannelModel(ChannelModelType.All, ChannelModel.CHANNELMODELTYPE_ALL, 0, mockIconConverter.Object);
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => ChannelModelUpdater.ReadChannelAsync(channelModel, CancellationToken.None, mockDispatherWrapper.Object));

            channelModel = new ChannelModel(ChannelModelType.ReadLater, ChannelModel.CHANNELMODELTYPE_READLATER, 0, mockIconConverter.Object);
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => ChannelModelUpdater.ReadChannelAsync(channelModel, CancellationToken.None, mockDispatherWrapper.Object));

            channelModel = new ChannelModel(ChannelModelType.Starred, ChannelModel.CHANNELMODELTYPE_STARRED, 0, mockIconConverter.Object);
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => ChannelModelUpdater.ReadChannelAsync(channelModel, CancellationToken.None, mockDispatherWrapper.Object));

            channelModel = new ChannelModel(1, "Test", null, "https://example.com/rss", null, null, 0, 0, mockIconConverter.Object);
        }

        [Fact]
        public async Task ReadChannelAsync_ValidChannelModel_WorksCorrectly()
        {
            //Arrange
            var rssUrl = "https://test.com/rss";
            var imageUri = new Uri("http://www.test.com/image.png");

            var mockHttpHandler = new Mock<IHttpHandler>();
            mockHttpHandler.Setup(h => h.GetStringAsync(It.Is<string>(arg => arg == rssUrl),
                                                        It.IsAny<CancellationToken>()))
                                                        .Returns(GetStringAsync("rss.xml"));
            mockHttpHandler.Setup(h => h.GetByteArrayAsync(It.Is<Uri>(arg => arg.ToString() == imageUri.ToString()),
                                                        It.IsAny<CancellationToken>()))
                                                        .Returns(GetBytesAsync("rss.png"));

            var mockChannels = new Mock<IChannels>();
            Channel? channel = new Channel
            {
                Id = 1,
                Title = new Uri(rssUrl).Host,
                Url = rssUrl,
                Rank = 1,
                LastUpdatedDate = DateTime.Now.AddYears(-10),
            };
            mockChannels.Setup(c => c.Get(It.Is<int>(arg => arg == channel.Id))).Returns(channel);
            mockChannels.Setup(c => c.GetChannelUnreadCount(It.Is<int>(arg => arg == channel.Id))).Returns(3);

            var mockChannelItems = new Mock<IChannelItems>();
            mockChannelItems.Setup(c => c.Create(It.IsAny<ChannelItem>(), It.IsAny<string[]>())).Returns(1);

            var mockLog = new Mock<ILog>();
            var mockIconConverter = new Mock<IIconConverter>();
            var mockDispatherWrapper = new Mock<IDispatcherWrapper>();
            mockDispatherWrapper.Setup(d => d.InvokeAsync(It.IsAny<Action>(),
                                                        It.IsAny<CancellationToken>()))
                                                        .Callback((Action action,
                                                            CancellationToken cancellationToken) => action());

            var mockAppSettings = new Mock<IOptions<AppSettings>>();
            var channelReader = new ChannelReader(mockHttpHandler.Object, mockChannels.Object, mockChannelItems.Object, mockLog.Object);

            var channelModel = new ChannelModel(1, "Test", null, rssUrl, null, null, 0, 0, mockIconConverter.Object);
            var ChannelModelUpdater = new ChannelModelUpdater(mockChannels.Object, channelReader, mockAppSettings.Object)
            {
                IconsDirectoryPath = GetFullPath("Icons")
            };            

            var iconFileName = GetFullPath($"Icons{Path.DirectorySeparatorChar}test.com.png");
            if (File.Exists(iconFileName))
            {
                File.Delete(iconFileName);
            }

            var document = new XmlDocument();
            document.LoadXml(await GetStringAsync("rss.xml"));
            var namespaceManagersMngr = new XmlNamespaceManager(document.NameTable);
            namespaceManagersMngr.AddNamespace("content", "http://purl.org/rss/1.0/modules/content/");
            var title = document.SelectSingleNode("/rss/channel/title")!.InnerText;
            var link = document.SelectSingleNode("/rss/channel/link")!.InnerText;
            var language = document.SelectSingleNode("/rss/channel/language")!.InnerText;
            var description = document.SelectSingleNode("/rss/channel/description")!.InnerText;
            var imageUrl = document.SelectSingleNode("/rss/channel/image/url")!.InnerText;
            var lastBuildDate = document.SelectSingleNode("/rss/channel/lastBuildDate")!.InnerText;
            var lastUpdatedDate = CodeHollow.FeedReader.Helpers.TryParseDateTime(lastBuildDate);
            var items = document.SelectNodes("/rss/channel/item");

            // Act
            await ChannelModelUpdater.ReadChannelAsync(channelModel, CancellationToken.None, mockDispatherWrapper.Object);

            // Assert            
            mockChannels.Verify(c => c.Get(It.Is<int>(arg => arg == channel.Id)), Times.Once);
            mockChannels.Verify(c => c.GetChannelUnreadCount(It.Is<int>(arg => arg == channel.Id)), Times.Once);
            mockChannels.Verify(c => c.Update(It.Is<Channel>(arg => arg.Id == 1 &&
                                                                    arg.Title == title &&
                                                                    arg.Link == link &&
                                                                    arg.Language == language &&
                                                                    arg.Url == rssUrl &&
                                                                    arg.Rank == channel.Rank &&
                                                                    arg.LastUpdatedDate == lastUpdatedDate &&
                                                                    arg.ImageUrl == imageUrl)),
                                                                    Times.Once);
            mockHttpHandler.Verify(h => h.GetStringAsync(It.Is<string>(arg => arg == rssUrl), It.IsAny<CancellationToken>()), Times.Once);
            mockHttpHandler.Verify(h => h.GetByteArrayAsync(It.Is<Uri>(arg => arg.ToString() == imageUri.ToString()), It.IsAny<CancellationToken>()), Times.Once);

            foreach (XmlNode item in items)
            {
                var itemTitle = item.SelectSingleNode("title")!.InnerText;
                var itemDescription = item.SelectSingleNode("description")!.InnerText;
                var itemUrl = item.SelectSingleNode("link")!.InnerText;
                var itemPublishedDate = CodeHollow.FeedReader.Helpers.TryParseDateTime(item.SelectSingleNode("pubDate")!.InnerText);
                var itemContent = item.SelectSingleNode("content:encoded", namespaceManagersMngr)!.InnerText;
                var itemCategories = item.SelectNodes("category").OfType<XmlNode>().Select(c => c.InnerText).ToArray();
                mockChannelItems.Verify(c => c.Create(It.Is<ChannelItem>(
                                                    arg => arg.Title == itemTitle &&
                                                    arg.ChannelId == channel.Id &&
                                                    arg.Description == itemDescription &&
                                                    arg.Link == itemUrl &&
                                                    arg.ItemId == itemUrl &&
                                                    arg.PublishingDate == itemPublishedDate &&
                                                    arg.Content == itemContent),
                                                    It.Is<string[]>(
                                                    arg => arg.SequenceEqual(itemCategories))),
                                                    Times.Once);
            }

            mockDispatherWrapper.Verify(d => d.InvokeAsync(It.IsAny<Action>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.True(File.Exists(iconFileName));
        }

        #region private methods
        private static async Task<string> GetStringAsync(string filename)
        {
            return await File.ReadAllTextAsync(GetFullPath(filename));
        }

        private static async Task<byte[]> GetBytesAsync(string filename)
        {
            return await File.ReadAllBytesAsync(GetFullPath(filename));
        }

        private static string GetFullPath(string path)
        {
            return Path.GetFullPath(Path.Combine(GetLocalDirectory()!, path));
        }

        private static string? GetLocalDirectory()
        {
            return Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().Location).LocalPath);
        }
        #endregion
    }
}
