using Moq;
using RssReader.MVVM.Converters;
using RssReader.MVVM.DataAccess.Interfaces;
using RssReader.MVVM.DataAccess.Models;
using RssReader.MVVM.Models;
using RssReader.MVVM.Services;

namespace RssReader.MVVM.Tests.Services
{
    public class ChannelServiceTests
    {
        [Fact]
        public void GetChannels_ReturnsExpectedChannels()
        {
            // Arrange
            var mockChannelsGroups = new Mock<IChannelsGroups>();
            mockChannelsGroups.Setup(m => m.GetAll())
                .Returns(new List<ChannelsGroup>
                {
                    new() { Id = 1, Name= "Channel Group 1", Rank = 1 },
                });

            var mockChannels = new Mock<IChannels>();
            mockChannels.Setup(m => m.GetByGroupId(1))
            .Returns(new List<Channel>
            {
                new() { Id = 1, Title = "Channel 1", Url = "http://channel1.com", ChannelsGroupId = 1 , Rank = 1 },
                new() { Id = 2, Title = "Channel 2", Url = "http://channel2.com", ChannelsGroupId = 1 , Rank = 2 },
            });
            mockChannels.Setup(m => m.GetByGroupId(null))
            .Returns(new List<Channel>
            {
                new() { Id = 3, Title = "Channel 3", Url = "http://channel3.com", ChannelsGroupId = null , Rank = 1 },
            });
            mockChannels.Setup(m => m.GetChannelUnreadCount(1)).Returns(10);
            mockChannels.Setup(m => m.GetChannelUnreadCount(2)).Returns(12);
            mockChannels.Setup(m => m.GetChannelUnreadCount(3)).Returns(14);

            var mockChannelItems = new Mock<IChannelItems>();
            var mockIconConverter = new Mock<IIconConverter>();

            var channelService = new ChannelService(mockChannelsGroups.Object, mockChannels.Object,
                                                    mockChannelItems.Object, mockIconConverter.Object);

            // Act
            var channels = channelService.GetChannels();

            // Assert
            Assert.Equal(2, channels.Count());

            var group = channels.First();
            Assert.Equal(1, group.Id);
            Assert.Equal(1, group.Rank);
            Assert.Equal("Channel Group 1", group.Title);
            Assert.True(group.IsChannelsGroup);
            Assert.Equal(22, group.UnreadItemsCount);

            var children = group.Children;
            Assert.NotNull(children);
            Assert.Equal(2, children!.Count());

            Assert.Equal(1, children!.First().Id);
            Assert.Equal(1, children!.First().Rank);
            Assert.False(children!.First().IsChannelsGroup);
            Assert.Equal(group, children!.First().Parent);
            Assert.Null(children!.First().Children);
            Assert.Equal("Channel 1", children!.First().Title);
            Assert.Equal("http://channel1.com", children!.First().Url);

            Assert.Equal(2, children!.Last().Id);
            Assert.Equal(2, channels.First().Children!.Last().Rank);
            Assert.False(children!.Last().IsChannelsGroup);
            Assert.Equal(group, children!.Last().Parent);
            Assert.Null(children!.Last().Children);
            Assert.Equal("Channel 2", children!.Last().Title);
            Assert.Equal("http://channel2.com", children!.Last().Url);

            Assert.Equal(3, channels.Last().Id);
            Assert.Equal(1, channels.Last().Rank);
            Assert.False(channels.Last().IsChannelsGroup);
            Assert.Null(channels.Last().Parent);
            Assert.Null(channels.Last().Children);
            Assert.Equal("Channel 3", channels.Last().Title);
            Assert.Equal("http://channel3.com", channels.Last().Url);
            Assert.Equal(14, channels.Last().UnreadItemsCount);
        }

        [Fact]
        public void GetChannels_ReturnsEmptyListWhenNoChannels()
        {
            // Arrange
            var mockChannelsGroups = new Mock<IChannelsGroups>();
            mockChannelsGroups.Setup(m => m.GetAll())
                .Returns(new List<ChannelsGroup>());

            var mockChannels = new Mock<IChannels>();
            mockChannels.Setup(m => m.GetByGroupId(null))
            .Returns(new List<Channel>());

            var mockChannelItems = new Mock<IChannelItems>();
            var mockIconConverter = new Mock<IIconConverter>();

            var channelService = new ChannelService(mockChannelsGroups.Object, mockChannels.Object,
                                                    mockChannelItems.Object, mockIconConverter.Object);

            // Act
            var channels = channelService.GetChannels();

            // Assert
            Assert.Empty(channels);
        }

        [Fact]
        public void GetChannels_ThrowsExceptionWhenChannelsGroupsThrows()
        {
            // Arrange
            var mockChannelsGroups = new Mock<IChannelsGroups>();
            mockChannelsGroups.Setup(m => m.GetAll())
                .Throws(new Exception("Error occurred"));

            var mockChannels = new Mock<IChannels>();
            mockChannels.Setup(m => m.GetByGroupId(null))
                .Returns(new List<Channel>());

            var mockChannelItems = new Mock<IChannelItems>();
            var mockIconConverter = new Mock<IIconConverter>();

            var channelService = new ChannelService(mockChannelsGroups.Object, mockChannels.Object,
                                                    mockChannelItems.Object, mockIconConverter.Object);

            // Act & Assert
            Assert.Throws<Exception>(() => channelService.GetChannels());
        }

        [Fact]
        public void GetChannels_ThrowsExceptionWhenChannelsThrows()
        {
            // Arrange
            var mockChannelsGroups = new Mock<IChannelsGroups>();
            mockChannelsGroups.Setup(m => m.GetAll())
                .Returns(new List<ChannelsGroup>());

            var mockChannels = new Mock<IChannels>();
            mockChannels.Setup(m => m.GetByGroupId(null))
            .Throws(new Exception("Error occurred"));

            var mockChannelItems = new Mock<IChannelItems>();
            var mockIconConverter = new Mock<IIconConverter>();

            var channelService = new ChannelService(mockChannelsGroups.Object, mockChannels.Object,
                                                    mockChannelItems.Object, mockIconConverter.Object);

            // Act & Assert
            Assert.Throws<Exception>(() => channelService.GetChannels());
        }

        [Fact]
        public void AddChannel_WhenChannelIsGroup()
        {
            // Arrange
            var mockChannelsGroups = new Mock<IChannelsGroups>();
            mockChannelsGroups.Setup(m => m.Create(It.IsAny<ChannelsGroup>())).Returns(1);
            var mockChannels = new Mock<IChannels>();
            var mockChannelItems = new Mock<IChannelItems>();
            var mockIconConverter = new Mock<IIconConverter>();
            var channelService = new ChannelService(mockChannelsGroups.Object, mockChannels.Object,
                mockChannelItems.Object, mockIconConverter.Object);

            ChannelModel channelGroup = new ChannelModel(0, "Channel Group 1", 0, null);
            // Act & Assert
            channelService.AddChannel(channelGroup);
            mockChannelsGroups.Verify(m => m.Create(It.Is<ChannelsGroup>(arg => arg.Name == channelGroup.Title)), Times.Once);
            Assert.Equal(1, channelGroup.Id);
        }

        [Fact]
        public void AddChannel_WhenChannelWithoutParent()
        {
            // Arrange
            var mockChannelsGroups = new Mock<IChannelsGroups>();
            var mockChannels = new Mock<IChannels>();
            mockChannels.Setup(m => m.Create(It.IsAny<Channel>())).Returns(1);
            var mockChannelItems = new Mock<IChannelItems>();
            var mockIconConverter = new Mock<IIconConverter>();
            var channelService = new ChannelService(mockChannelsGroups.Object, mockChannels.Object,
                mockChannelItems.Object, mockIconConverter.Object);

            ChannelModel channel = new ChannelModel(0, "Channel 1", null, "http://channel.com", null, null, 0, 0, mockIconConverter.Object);
            // Act & Assert
            channelService.AddChannel(channel);
            mockChannels.Verify(m => m.Create(It.Is<Channel>(arg => arg.Title == channel.Title &&
                                                                arg.Url == channel.Url)), Times.Once);
            Assert.Equal(1, channel.Id);
        }

        [Fact]
        public void AddChannel_WhenChannelWithParent()
        {
            // Arrange
            var mockChannelsGroups = new Mock<IChannelsGroups>();
            var mockChannels = new Mock<IChannels>();
            mockChannels.Setup(m => m.Create(It.IsAny<Channel>())).Returns(1);
            var mockChannelItems = new Mock<IChannelItems>();
            var mockIconConverter = new Mock<IIconConverter>();
            var channelService = new ChannelService(mockChannelsGroups.Object, mockChannels.Object,
                mockChannelItems.Object, mockIconConverter.Object);

            ChannelModel channel = new ChannelModel(0, "Channel 1", null, "http://channel.com", null, null, 0, 0, mockIconConverter.Object);
            ChannelModel channelGroup = new ChannelModel(1, "Channel Group 1", 0, null);
            channel.Parent = channelGroup;
            // Act & Assert
            channelService.AddChannel(channel);
            mockChannels.Verify(m => m.Create(It.Is<Channel>(arg => arg.Title == channel.Title &&
                                                                arg.Url == channel.Url &&
                                                                arg.ChannelsGroupId == channelGroup.Id)), Times.Once);
            Assert.Equal(1, channel.Id);
        }

        [Fact]
        public void AddChannel_ThrowsExceptionWhenChannelModelInNotValid()
        {
            // Arrange
            var mockChannelsGroups = new Mock<IChannelsGroups>();
            var mockChannels = new Mock<IChannels>();
            var mockChannelItems = new Mock<IChannelItems>();
            var mockIconConverter = new Mock<IIconConverter>();
            var channelService = new ChannelService(mockChannelsGroups.Object, mockChannels.Object,
                mockChannelItems.Object, mockIconConverter.Object);

            ChannelModel channelTypeAll = new ChannelModel(ChannelModelType.All, ChannelModel.CHANNELMODELTYPE_ALL, 0, mockIconConverter.Object);
            ChannelModel channelTypeStarred = new ChannelModel(ChannelModelType.Starred, ChannelModel.CHANNELMODELTYPE_STARRED, 0, mockIconConverter.Object);
            ChannelModel channelTypeReadLater = new ChannelModel(ChannelModelType.ReadLater, ChannelModel.CHANNELMODELTYPE_READLATER, 0, mockIconConverter.Object);
            ChannelModel channelTitleNull = new ChannelModel(ChannelModelType.Default, null, 0, mockIconConverter.Object);
            ChannelModel channelTitleIsEmpty = new ChannelModel(ChannelModelType.Default, string.Empty, 0, mockIconConverter.Object);
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => channelService.AddChannel(null));
            Assert.Throws<ArgumentNullException>(() => channelService.AddChannel(channelTypeAll));
            Assert.Throws<ArgumentNullException>(() => channelService.AddChannel(channelTypeStarred));
            Assert.Throws<ArgumentNullException>(() => channelService.AddChannel(channelTypeReadLater));
            Assert.Throws<ArgumentNullException>(() => channelService.AddChannel(channelTitleNull));
            Assert.Throws<ArgumentNullException>(() => channelService.AddChannel(channelTitleIsEmpty));
        }



    }
}
