using Moq;
using RssReader.MVVM.DataAccess.Interfaces;
using RssReader.MVVM.DataAccess.Models;
using RssReader.MVVM.Models;
using RssReader.MVVM.Services;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.Tests.Services
{
    public class ChannelServiceTests
    {
        #region GetChannels

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
        #endregion

        #region AddChannel
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
        public void AddChannel_ThrowsExceptionWhenChannelModelIsNotValid()
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
        #endregion

        #region DeleteChannel
        [Fact]
        public void DeleteChannel_WhenChannelIsGroup()
        {
            // Arrange
            var mockChannelsGroups = new Mock<IChannelsGroups>();
            var mockChannels = new Mock<IChannels>();
            var mockChannelItems = new Mock<IChannelItems>();
            var mockIconConverter = new Mock<IIconConverter>();
            var channelService = new ChannelService(mockChannelsGroups.Object, mockChannels.Object,
                                                    mockChannelItems.Object, mockIconConverter.Object);

            ChannelModel channelGroup = new ChannelModel(1, "Channel Group 1", 0, null);
            // Act & Assert
            channelService.DeleteChannel(channelGroup);
            mockChannelsGroups.Verify(m => m.Delete(It.Is<int>(arg => arg == channelGroup.Id)), Times.Once);
        }

        [Fact]
        public void DeleteChannel_WhenChannelIsNotGroup()
        {
            // Arrange
            var mockChannelsGroups = new Mock<IChannelsGroups>();
            var mockChannels = new Mock<IChannels>();
            var mockChannelItems = new Mock<IChannelItems>();
            var mockIconConverter = new Mock<IIconConverter>();
            var channelService = new ChannelService(mockChannelsGroups.Object, mockChannels.Object,
                                                    mockChannelItems.Object, mockIconConverter.Object);

            ChannelModel channel = new ChannelModel(1, "Channel 1", null, "http://channel.com", null, null, 0, 0, mockIconConverter.Object);
            // Act & Assert
            channelService.DeleteChannel(channel);
            mockChannels.Verify(m => m.Delete(It.Is<int>(arg => arg == channel.Id)), Times.Once);
        }

        [Fact]
        public void DeleteChannel_ThrowsExceptionWhenChannelModelIsNotValid()
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
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => channelService.AddChannel(null));
            Assert.Throws<ArgumentNullException>(() => channelService.AddChannel(channelTypeAll));
            Assert.Throws<ArgumentNullException>(() => channelService.AddChannel(channelTypeStarred));
            Assert.Throws<ArgumentNullException>(() => channelService.AddChannel(channelTypeReadLater));
        }
        #endregion

        #region GetChannel
        [Fact]
        public void GetChannel_ReturnsChannel_WhenChannelModelTypeIsAll()
        {
            // Arrange
            var mockChannelsGroups = new Mock<IChannelsGroups>();
            var mockChannels = new Mock<IChannels>();
            int count = 1;
            mockChannels.Setup(m => m.GetAllUnreadCount()).Returns(count);
            var mockChannelItems = new Mock<IChannelItems>();
            var mockIconConverter = new Mock<IIconConverter>();
            var channelService = new ChannelService(mockChannelsGroups.Object, mockChannels.Object,
                                                    mockChannelItems.Object, mockIconConverter.Object);
            // Act & Assert
            var channel = channelService.GetChannel(ChannelModelType.All);
            mockChannels.Verify(m => m.GetAllUnreadCount(), Times.Once);
            Assert.True(channel.ModelType == ChannelModelType.All);
            Assert.True(channel.Title == ChannelModel.CHANNELMODELTYPE_ALL);
            Assert.True(channel.IsChannelsGroup == false);
            Assert.Equal((int)ChannelModelType.All, channel.Id);
            Assert.Equal(count, channel.UnreadItemsCount);
        }

        [Fact]
        public void GetChannel_ReturnsChannel_WhenChannelModelTypeIsStarred()
        {
            // Arrange
            var mockChannelsGroups = new Mock<IChannelsGroups>();
            var mockChannels = new Mock<IChannels>();
            int count = 1;
            mockChannels.Setup(m => m.GetStarredCount()).Returns(count);
            var mockChannelItems = new Mock<IChannelItems>();
            var mockIconConverter = new Mock<IIconConverter>();
            var channelService = new ChannelService(mockChannelsGroups.Object, mockChannels.Object,
                                                    mockChannelItems.Object, mockIconConverter.Object);
            // Act & Assert
            var channel = channelService.GetChannel(ChannelModelType.Starred);
            mockChannels.Verify(m => m.GetStarredCount(), Times.Once);
            Assert.True(channel.ModelType == ChannelModelType.Starred);
            Assert.True(channel.Title == ChannelModel.CHANNELMODELTYPE_STARRED);
            Assert.True(channel.IsChannelsGroup == false);
            Assert.Equal((int)ChannelModelType.Starred, channel.Id);
            Assert.Equal(count, channel.UnreadItemsCount);
        }

        [Fact]
        public void GetChannel_ReturnsChannel_WhenChannelModelTypeIsReadLater()
        {
            // Arrange
            var mockChannelsGroups = new Mock<IChannelsGroups>();
            var mockChannels = new Mock<IChannels>();
            int count = 1;
            mockChannels.Setup(m => m.GetReadLaterCount()).Returns(count);
            var mockChannelItems = new Mock<IChannelItems>();
            var mockIconConverter = new Mock<IIconConverter>();
            var channelService = new ChannelService(mockChannelsGroups.Object, mockChannels.Object,
                                                    mockChannelItems.Object, mockIconConverter.Object);
            // Act & Assert
            var channel = channelService.GetChannel(ChannelModelType.ReadLater);
            mockChannels.Verify(m => m.GetReadLaterCount(), Times.Once);
            Assert.True(channel.ModelType == ChannelModelType.ReadLater);
            Assert.True(channel.Title == ChannelModel.CHANNELMODELTYPE_READLATER);
            Assert.True(channel.IsChannelsGroup == false);
            Assert.Equal((int)ChannelModelType.ReadLater, channel.Id);
            Assert.Equal(count, channel.UnreadItemsCount);
        }

        [Fact]
        public void GetChannel_ReturnsChannel_WhenChannelModelTypeIsDeafault()
        {
            // Arrange
            var mockChannelsGroups = new Mock<IChannelsGroups>();
            var mockChannels = new Mock<IChannels>();
            int count = 1;
            mockChannels.Setup(m => m.GetAllUnreadCount()).Returns(count);
            var mockChannelItems = new Mock<IChannelItems>();
            var mockIconConverter = new Mock<IIconConverter>();
            var channelService = new ChannelService(mockChannelsGroups.Object, mockChannels.Object,
                                                    mockChannelItems.Object, mockIconConverter.Object);
            // Act & Assert
            var channel = channelService.GetChannel(ChannelModelType.Default);
            mockChannels.Verify(m => m.GetAllUnreadCount(), Times.Once);
            Assert.True(channel.ModelType == ChannelModelType.All);
            Assert.True(channel.Title == ChannelModel.CHANNELMODELTYPE_ALL);
            Assert.True(channel.IsChannelsGroup == false);
            Assert.Equal((int)ChannelModelType.All, channel.Id);
            Assert.Equal(count, channel.UnreadItemsCount);
        }
        #endregion

        #region GetChannelModel
        [Fact]
        public void GetChannelModel_ReturnsChannelModel()
        {
            // Arrange            
            var mockChannelsGroups = new Mock<IChannelsGroups>();
            var mockChannels = new Mock<IChannels>();
            int id = 1; int count = 1;
            mockChannels.Setup(m => m.GetChannelUnreadCount(It.Is<int>(arg => arg == id))).Returns(count);
            var channel = new Channel
            {
                Id = id,
                Title = "title",
                Description = "description",
                Url = "http://url.com",
                ImageUrl = "http://image.com",
                Link = "http://link.com",
                Rank = 1,
            };
            mockChannels.Setup(m => m.Get(It.Is<int>(arg => arg == id))).Returns(channel);
            var mockChannelItems = new Mock<IChannelItems>();
            var mockIconConverter = new Mock<IIconConverter>();
            var channelService = new ChannelService(mockChannelsGroups.Object, mockChannels.Object,
                                                    mockChannelItems.Object, mockIconConverter.Object);
            // Act & Assert
            var channelModel = channelService.GetChannelModel(id);
            mockChannels.Verify(m => m.Get(It.Is<int>(arg => arg == id)), Times.Once);
            mockChannels.Verify(m => m.GetChannelUnreadCount(It.Is<int>(arg => arg == id)), Times.Once);
            Assert.NotNull(channelModel);
            Assert.True(channelModel.Id == id);
            Assert.True(channelModel.Title == channel.Title);
            Assert.True(channelModel.Description == channel.Description);
            Assert.True(channelModel.Url == channel.Url);
            Assert.True(channelModel.ImageUrl == channel.ImageUrl);
            Assert.True(channelModel.Link == channel.Link);
            Assert.True(channelModel.UnreadItemsCount == count);
            Assert.True(channelModel.Rank == channel.Rank);
        }

        [Fact]
        public void GetChannelModel_ReturnsNull_WhenChannelNotFound()
        {
            // Arrange            
            var mockChannelsGroups = new Mock<IChannelsGroups>();
            var mockChannels = new Mock<IChannels>();
            int id = 1; Channel? channel = null;
            mockChannels.Setup(m => m.Get(It.Is<int>(arg => arg == id))).Returns(channel);
            var mockChannelItems = new Mock<IChannelItems>();
            var mockIconConverter = new Mock<IIconConverter>();
            var channelService = new ChannelService(mockChannelsGroups.Object, mockChannels.Object,
                                                    mockChannelItems.Object, mockIconConverter.Object);
            // Act & Assert
            var channelModel = channelService.GetChannelModel(id);
            mockChannels.Verify(m => m.Get(It.Is<int>(arg => arg == id)), Times.Once);
            mockChannels.Verify(m => m.GetChannelUnreadCount(It.IsAny<int>()), Times.Never);
            Assert.Null(channelModel);
        }
        #endregion

        #region UpdateChannel
        [Fact]
        public void UpdateChannel_WhenChannelModelIsChannelGroup()
        {
            // Arrange
            var mockChannelsGroups = new Mock<IChannelsGroups>();
            ChannelModel channelGroup = new ChannelModel(1, "New Channel Group Name", 2, null);
            var group = new ChannelsGroup
            {
                Id = 1,
                Name = "Channel Group 1",
                Rank = 1,
            };
            mockChannelsGroups.Setup(m => m.Get(It.Is<int>(arg => arg == channelGroup.Id))).Returns(group);
            var mockChannels = new Mock<IChannels>();
            var mockChannelItems = new Mock<IChannelItems>();
            var mockIconConverter = new Mock<IIconConverter>();
            var channelService = new ChannelService(mockChannelsGroups.Object, mockChannels.Object,
                                                    mockChannelItems.Object, mockIconConverter.Object);
            // Act & Assert
            channelService.UpdateChannel(channelGroup);
            mockChannelsGroups.Verify(m => m.Get(It.Is<int>(arg => arg == channelGroup.Id)), Times.Once);
            mockChannelsGroups.Verify(m => m.Update(It.Is<ChannelsGroup>(arg => arg.Id == channelGroup.Id &&
                        arg.Name == channelGroup.Title && arg.Rank == channelGroup.Rank)), Times.Once);
        }

        [Fact]
        public void UpdateChannel_WhenChannelModelIsChannelGroup_AndChannelGroupNotFound()
        {
            // Arrange
            var mockChannelsGroups = new Mock<IChannelsGroups>();
            ChannelModel channelGroup = new ChannelModel(1, "New Channel Group Name", 2, null);
            ChannelsGroup? group = null;
            mockChannelsGroups.Setup(m => m.Get(It.Is<int>(arg => arg == channelGroup.Id))).Returns(group);
            var mockChannels = new Mock<IChannels>();
            var mockChannelItems = new Mock<IChannelItems>();
            var mockIconConverter = new Mock<IIconConverter>();
            var channelService = new ChannelService(mockChannelsGroups.Object, mockChannels.Object,
                                                    mockChannelItems.Object, mockIconConverter.Object);

            // Act & Assert
            channelService.UpdateChannel(channelGroup);
            mockChannelsGroups.Verify(m => m.Get(It.Is<int>(arg => arg == channelGroup.Id)), Times.Once);
            mockChannelsGroups.Verify(m => m.Update(It.IsAny<ChannelsGroup>()), Times.Never);
        }

        [Fact]
        public void UpdateChannel_WhenChannelModelIsNotChannelGroup_WithoutParent()
        {
            // Arrange
            var mockChannelsGroups = new Mock<IChannelsGroups>();
            var mockIconConverter = new Mock<IIconConverter>();
            var mockChannels = new Mock<IChannels>();
            ChannelModel channelModel = new ChannelModel(1, "New Channel Title", null, "http://url.com", null, null, 1, 2, mockIconConverter.Object);
            var channel = new Channel
            {
                Id = 1,
                Title = "Channel 1",
                Rank = 1,
            };
            mockChannels.Setup(m => m.Get(It.Is<int>(arg => arg == channelModel.Id))).Returns(channel);
            var mockChannelItems = new Mock<IChannelItems>();
            var channelService = new ChannelService(mockChannelsGroups.Object, mockChannels.Object,
                                                    mockChannelItems.Object, mockIconConverter.Object);
            // Act & Assert
            channelService.UpdateChannel(channelModel);
            mockChannels.Verify(m => m.Get(It.Is<int>(arg => arg == channelModel.Id)), Times.Once);
            mockChannels.Verify(m => m.Update(It.Is<Channel>(arg => arg.Id == channelModel.Id &&
                        arg.Title == channelModel.Title && arg.Rank == channelModel.Rank &&
                        arg.ChannelsGroupId == null)), Times.Once);
        }

        [Fact]
        public void UpdateChannel_WhenChannelModelIsNotChannelGroup_WithParent()
        {
            // Arrange
            var mockChannelsGroups = new Mock<IChannelsGroups>();
            var mockIconConverter = new Mock<IIconConverter>();
            var mockChannels = new Mock<IChannels>();
            ChannelModel channelGroup = new ChannelModel(1, "Channel Group 1", 1, null);
            ChannelModel channelModel = new ChannelModel(1, "New Channel Title", null, "http://url.com", null, null, 1, 2, mockIconConverter.Object);
            channelModel.Parent = channelGroup;
            var channel = new Channel
            {
                Id = 1,
                Title = "Channel 1",
                Rank = 1,
                ChannelsGroupId = 2
            };
            mockChannels.Setup(m => m.Get(It.Is<int>(arg => arg == channelModel.Id))).Returns(channel);
            var mockChannelItems = new Mock<IChannelItems>();
            var channelService = new ChannelService(mockChannelsGroups.Object, mockChannels.Object,
                                                    mockChannelItems.Object, mockIconConverter.Object);
            // Act & Assert
            channelService.UpdateChannel(channelModel);
            mockChannels.Verify(m => m.Get(It.Is<int>(arg => arg == channelModel.Id)), Times.Once);
            mockChannels.Verify(m => m.Update(It.Is<Channel>(arg => arg.Id == channelModel.Id &&
                        arg.Title == channelModel.Title && arg.Rank == channelModel.Rank &&
                        arg.ChannelsGroupId == channelGroup.Id)), Times.Once);
        }

        [Fact]
        public void UpdateChannel_ThrowsExceptionWhenChannelModelIsNotValid()
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
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => channelService.UpdateChannel(null));
            Assert.Throws<ArgumentNullException>(() => channelService.UpdateChannel(channelTypeAll));
            Assert.Throws<ArgumentNullException>(() => channelService.UpdateChannel(channelTypeStarred));
            Assert.Throws<ArgumentNullException>(() => channelService.UpdateChannel(channelTypeReadLater));
        }
        #endregion

        [Fact]
        public void GetChannelItem_ReturnsChannelItem()
        {
            // Arrange
            var mockChannelsGroups = new Mock<IChannelsGroups>();
            var mockIconConverter = new Mock<IIconConverter>();
            var mockChannels = new Mock<IChannels>();
            var mockChannelItems = new Mock<IChannelItems>();
            long channelItemId = 1;
            var channelItem = new ChannelItem
            {
                Id = 1,
                Title = "Channel Item 1",
                Description = "Channel Item 1 Description",
                Content = "Channel Item 1 Content",
                Link = "http://url.com",
                PublishingDate = DateTime.Now,
                ChannelId = 1,
                IsRead = false,
                IsFavorite = false,
                IsReadLater = false,
                IsDeleted = false,
                ItemCategories = new List<ItemCategory>
                {
                    new ItemCategory
                    {
                        Id = 1,
                        CategoryId = 1,
                        ChannelItemId = 1,
                        Category = new Category
                        {
                            Id = 1,
                            Name = "Category 1"
                        }
                    },
                    new ItemCategory
                    {
                        Id = 2,
                        CategoryId = 2,
                        ChannelItemId = 1,
                        Category = new Category
                        {
                            Id = 2,
                            Name = "Category 2"
                        }
                    }
                }
            };
            mockChannelItems.Setup(m => m.Get(It.Is<long>(arg => arg == channelItemId))).Returns(channelItem);
            var channelService = new ChannelService(mockChannelsGroups.Object, mockChannels.Object,
                                                    mockChannelItems.Object, mockIconConverter.Object);
            // Act & Assert
            var channelItemModel = channelService.GetChannelItem(channelItemId);
            mockChannelItems.Verify(m => m.Get(It.Is<long>(arg => arg == channelItemId)), Times.Once);
            Assert.NotNull(channelItemModel);
            Assert.Equal(channelItemModel.Id, channelItem.Id);
            Assert.Equal(channelItemModel.Title, channelItem.Title);
            Assert.Equal(channelItemModel.Description, channelItem.Description);
            Assert.Equal(channelItemModel.Content, $"<body>{channelItem.Content}</body>");
            Assert.Equal(channelItemModel.Link, channelItem.Link);
            Assert.Equal(channelItemModel.PublishingDate, $"Today at {channelItem.PublishingDate.Value.ToShortTimeString()}");
            Assert.NotNull(channelItemModel.Categories);
            Assert.Equal(channelItem.ItemCategories.Count(), channelItemModel.Categories.Count());
            Assert.Equal(channelItem.ItemCategories.First().CategoryId, channelItemModel.Categories.First().Key);
            Assert.Equal(channelItem.ItemCategories.First().Category.Name, channelItemModel.Categories.First().Value);
            Assert.Equal(channelItem.ItemCategories.Last().CategoryId, channelItemModel.Categories.Last().Key);
            Assert.Equal(channelItem.ItemCategories.Last().Category.Name, channelItemModel.Categories.Last().Value);
        }

        #region UpdateChannelItem
        [Fact]
        public void UpdateChannelItem_SetIsRead()
        {
            // Arrange
            var mockChannelsGroups = new Mock<IChannelsGroups>();
            var mockIconConverter = new Mock<IIconConverter>();
            var mockChannels = new Mock<IChannels>();
            var mockChannelItems = new Mock<IChannelItems>();
            var channelItem = ChannelItem;
            var channelItemModel = new ChannelItemModel(channelItem, mockIconConverter.Object)
            {
                IsRead = true
            };
            mockChannelItems.Setup(m => m.Get(It.Is<long>(arg => arg == channelItemModel.Id))).Returns(channelItem);
            var channelService = new ChannelService(mockChannelsGroups.Object, mockChannels.Object,
                                                    mockChannelItems.Object, mockIconConverter.Object);
            // Act & Assert
            channelService.UpdateChannelItem(channelItemModel);
            mockChannelItems.Verify(m => m.SetRead(It.Is<long>(arg => arg == channelItemModel.Id),
                                                It.Is<bool>(arg => arg == channelItemModel.IsRead)), Times.Once);
            mockChannelItems.Verify(m => m.SetFavorite(It.IsAny<long>(), It.IsAny<bool>()), Times.Never);
            mockChannelItems.Verify(m => m.SetReadLater(It.IsAny<long>(), It.IsAny<bool>()), Times.Never);
            mockChannelItems.Verify(m => m.SetDeleted(It.IsAny<long>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public void UpdateChannelItem_SetFavorite()
        {
            // Arrange
            var mockChannelsGroups = new Mock<IChannelsGroups>();
            var mockIconConverter = new Mock<IIconConverter>();
            var mockChannels = new Mock<IChannels>();
            var mockChannelItems = new Mock<IChannelItems>();
            var channelItem = ChannelItem;
            var channelItemModel = new ChannelItemModel(channelItem, mockIconConverter.Object)
            {
                IsFavorite = true
            };
            mockChannelItems.Setup(m => m.Get(It.Is<long>(arg => arg == channelItemModel.Id))).Returns(channelItem);
            var channelService = new ChannelService(mockChannelsGroups.Object, mockChannels.Object,
                                                    mockChannelItems.Object, mockIconConverter.Object);
            // Act & Assert
            channelService.UpdateChannelItem(channelItemModel);
            mockChannelItems.Verify(m => m.SetFavorite(It.Is<long>(arg => arg == channelItemModel.Id),
                                                It.Is<bool>(arg => arg == channelItemModel.IsFavorite)), Times.Once);
            mockChannelItems.Verify(m => m.SetRead(It.IsAny<long>(), It.IsAny<bool>()), Times.Never);
            mockChannelItems.Verify(m => m.SetReadLater(It.IsAny<long>(), It.IsAny<bool>()), Times.Never);
            mockChannelItems.Verify(m => m.SetDeleted(It.IsAny<long>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public void UpdateChannelItem_SetReadLater()
        {
            // Arrange
            var mockChannelsGroups = new Mock<IChannelsGroups>();
            var mockIconConverter = new Mock<IIconConverter>();
            var mockChannels = new Mock<IChannels>();
            var mockChannelItems = new Mock<IChannelItems>();
            var channelItem = ChannelItem;
            var channelItemModel = new ChannelItemModel(channelItem, mockIconConverter.Object)
            {
                IsReadLater = true
            };
            mockChannelItems.Setup(m => m.Get(It.Is<long>(arg => arg == channelItemModel.Id))).Returns(channelItem);
            var channelService = new ChannelService(mockChannelsGroups.Object, mockChannels.Object,
                                                    mockChannelItems.Object, mockIconConverter.Object);
            // Act & Assert
            channelService.UpdateChannelItem(channelItemModel);
            mockChannelItems.Verify(m => m.SetReadLater(It.Is<long>(arg => arg == channelItemModel.Id),
                                                It.Is<bool>(arg => arg == channelItemModel.IsReadLater)), Times.Once);
            mockChannelItems.Verify(m => m.SetRead(It.IsAny<long>(), It.IsAny<bool>()), Times.Never);
            mockChannelItems.Verify(m => m.SetFavorite(It.IsAny<long>(), It.IsAny<bool>()), Times.Never);
            mockChannelItems.Verify(m => m.SetDeleted(It.IsAny<long>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public void UpdateChannelItem_SetDeleted()
        {
            // Arrange
            var mockChannelsGroups = new Mock<IChannelsGroups>();
            var mockIconConverter = new Mock<IIconConverter>();
            var mockChannels = new Mock<IChannels>();
            var mockChannelItems = new Mock<IChannelItems>();
            var channelItem = ChannelItem;
            var channelItemModel = new ChannelItemModel(channelItem, mockIconConverter.Object)
            {
                IsDeleted = true
            };
            mockChannelItems.Setup(m => m.Get(It.Is<long>(arg => arg == channelItemModel.Id))).Returns(channelItem);
            var channelService = new ChannelService(mockChannelsGroups.Object, mockChannels.Object,
                                                    mockChannelItems.Object, mockIconConverter.Object);
            // Act & Assert
            channelService.UpdateChannelItem(channelItemModel);
            mockChannelItems.Verify(m => m.SetDeleted(It.Is<long>(arg => arg == channelItemModel.Id),
                                                It.Is<bool>(arg => arg == channelItemModel.IsDeleted)), Times.Once);
            mockChannelItems.Verify(m => m.SetRead(It.IsAny<long>(), It.IsAny<bool>()), Times.Never);
            mockChannelItems.Verify(m => m.SetFavorite(It.IsAny<long>(), It.IsAny<bool>()), Times.Never);
            mockChannelItems.Verify(m => m.SetReadLater(It.IsAny<long>(), It.IsAny<bool>()), Times.Never);
        }

        private ChannelItem ChannelItem
        {
            get
            {
                return new ChannelItem
                {
                    Id = 1,
                    Title = "Channel Item 1",
                    Description = "Channel Item 1 Description",
                    Content = "Channel Item 1 Content",
                    Link = "http://url.com",
                    PublishingDate = DateTime.Now,
                    ChannelId = 1,
                    IsRead = false,
                    IsFavorite = false,
                    IsReadLater = false,
                    IsDeleted = false,
                };
            }
        }
        #endregion

        [Fact]
        public void GetStarredCount_ReturnsExpectedValue()
        {
            // Arrange
            var mockChannelsGroups = new Mock<IChannelsGroups>();
            var mockIconConverter = new Mock<IIconConverter>();
            var mockChannels = new Mock<IChannels>();
            var expectedResult = 10;
            mockChannels.Setup(m => m.GetStarredCount()).Returns(expectedResult);
            var mockChannelItems = new Mock<IChannelItems>();
            var channelService = new ChannelService(mockChannelsGroups.Object, mockChannels.Object,
                                                    mockChannelItems.Object, mockIconConverter.Object);
            // Act & Assert
            var result = channelService.GetStarredCount();
            mockChannels.Verify(m => m.GetStarredCount(), Times.Once);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void GetReadLaterCount_ReturnsExpectedValue()
        {
            // Arrange
            var mockChannelsGroups = new Mock<IChannelsGroups>();
            var mockIconConverter = new Mock<IIconConverter>();
            var mockChannels = new Mock<IChannels>();
            var expectedResult = 10;
            mockChannels.Setup(m => m.GetReadLaterCount()).Returns(expectedResult);
            var mockChannelItems = new Mock<IChannelItems>();
            var channelService = new ChannelService(mockChannelsGroups.Object, mockChannels.Object,
                                                    mockChannelItems.Object, mockIconConverter.Object);
            // Act & Assert
            var result = channelService.GetReadLaterCount();
            mockChannels.Verify(m => m.GetReadLaterCount(), Times.Once);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void GetChannelUnreadCount_ReturnsExpectedValue()
        {
            // Arrange
            var mockChannelsGroups = new Mock<IChannelsGroups>();
            var mockIconConverter = new Mock<IIconConverter>();
            var mockChannels = new Mock<IChannels>();
            var channelId = 1; var expectedResult = 10;
            mockChannels.Setup(m => m.GetChannelUnreadCount(It.Is<int>(arg => arg == channelId))).Returns(expectedResult);
            var mockChannelItems = new Mock<IChannelItems>();
            var channelService = new ChannelService(mockChannelsGroups.Object, mockChannels.Object,
                                                    mockChannelItems.Object, mockIconConverter.Object);
            // Act & Assert
            var result = channelService.GetChannelUnreadCount(channelId);
            mockChannels.Verify(m => m.GetChannelUnreadCount(It.Is<int>(arg => arg == channelId)), Times.Once);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void CreateNewChannel_ReturnsExpectedValue()
        {
            // Arrange
            var mockChannelsGroups = new Mock<IChannelsGroups>();
            var mockIconConverter = new Mock<IIconConverter>();
            var mockChannels = new Mock<IChannels>();
            var url = "http://url.com";
            var mockChannelItems = new Mock<IChannelItems>();
            var channelService = new ChannelService(mockChannelsGroups.Object, mockChannels.Object,
                                                    mockChannelItems.Object, mockIconConverter.Object);
            // Act & Assert
            var result = channelService.CreateNewChannel(url);
            Assert.Equal(0, result.Id);
            Assert.Equal(url, result.Url);
            Assert.Equal(new Uri(url).Host, result.Title);
            Assert.Equal(0, result.UnreadItemsCount);
            Assert.Equal(0, result.Rank);
            Assert.Null(result.Description);
            Assert.Null(result.ImageUrl);
            Assert.Null(result.Link);
        }
    }
}
