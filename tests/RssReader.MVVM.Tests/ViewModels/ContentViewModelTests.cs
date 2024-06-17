using Moq;
using Moq.AutoMock;
using RssReader.MVVM.Services.Interfaces;
using RssReader.MVVM.ViewModels;
using ReactiveUI;
using System.Reactive;
using ReactiveUI.Testing;
using Microsoft.Reactive.Testing;
using RssReader.MVVM.DataAccess.Interfaces;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Media;
using RssReader.MVVM.DataAccess.Models;
using RssReader.MVVM.Models;
using System.Reflection;
using Avalonia.Media.Imaging;
using Avalonia.Headless.XUnit;

namespace RssReader.MVVM.Tests.ViewModels;

public class ContentViewModelTests
{
    private readonly AutoMocker _autoMocker;
    public ContentViewModelTests()
    {
        _autoMocker = new AutoMocker();
    }

    [Fact]
    public void OpenLinkCommand_WorksCorrectly()
    {
        //Arrange
        var url = "https://google.com";
        _autoMocker.Setup<ILinkOpeningService>(m => m.OpenUrl(It.Is<string>(arg => arg == url))).Verifiable();

        var contentViewModel = _autoMocker.CreateInstance<ContentViewModel>();
        var command = (ReactiveCommand<string, Unit>)contentViewModel.OpenLinkCommand;

        // Act
        command.Execute(url).Subscribe();

        // Assert
        _autoMocker.Verify<ILinkOpeningService>(m => m.OpenUrl(It.Is<string>(arg => arg == url)), Times.Once);
    }

    [Fact]
    public void CopyLinkCommand_WorksCorrectly()
    {
        //Arrange
        var url = "https://google.com";
        _autoMocker.Setup<IClipboardService>(m => m.SetTextAsync(It.Is<string>(arg => arg == url))).Verifiable();

        var contentViewModel = _autoMocker.CreateInstance<ContentViewModel>();
        var command = (ReactiveCommand<string, Unit>)contentViewModel.CopyLinkCommand;

        // Act
        command.Execute(url).Subscribe();

        // Assert
        _autoMocker.Verify<IClipboardService>(m => m.SetTextAsync(It.Is<string>(arg => arg == url)), Times.Once);
    }

    [Fact]
    public void ViewPostsCommand_WorksCorrectly()
    {
        //Arrange
        var searchCategoryName = "CategoryName";
        var categories = new List<Category>
        {
            new Category { Id = 1, Name = searchCategoryName + "1" },
            new Category { Id = 2, Name = searchCategoryName },
            new Category { Id = 3, Name = searchCategoryName + "3" },
        };
        var mockCategories = new Mock<ICategories>();
        mockCategories.Setup(c => c.GetByName(It.Is<string>(arg => arg == searchCategoryName))).Returns(categories);

        var mockChannelService = new Mock<IChannelService>();
        var mockLinkOpeningService = new Mock<ILinkOpeningService>();
        var mockClipboardService = new Mock<IClipboardService>();
        var mockThemeService = new Mock<IThemeService>();
        var canExecuteViewPostsCommand = false;

        // Act        
        var contentViewModel = new ContentViewModel(mockChannelService.Object, mockCategories.Object, mockLinkOpeningService.Object, mockClipboardService.Object, mockThemeService.Object);
        contentViewModel.ViewPostsCommand.CanExecute.Subscribe(x => canExecuteViewPostsCommand = x);
        var command = (ReactiveCommand<string, Unit>)contentViewModel.ViewPostsCommand;

        // Assert
        Assert.False(canExecuteViewPostsCommand);
        contentViewModel.SearchName = searchCategoryName;
        Assert.True(canExecuteViewPostsCommand);
        command.Execute(searchCategoryName).Subscribe();
        Assert.Equal(categories.First(x => x.Name == searchCategoryName), contentViewModel.SelectedCategory);
    }

    [Fact]
    public void CopyLinkCommand_IgnoreWhenNullOrEmpty()
    {
        //Arrange        
        _autoMocker.Setup<IClipboardService>(m => m.SetTextAsync(It.IsAny<string>())).Verifiable();

        var contentViewModel = _autoMocker.CreateInstance<ContentViewModel>();
        var command = (ReactiveCommand<string, Unit>)contentViewModel.CopyLinkCommand;

        // Act
        command.Execute(string.Empty).Subscribe();

        // Assert
        _autoMocker.Verify<IClipboardService>(m => m.SetTextAsync(It.IsAny<string>()), Times.Never);

        // Act
        command.Execute(null).Subscribe();

        // Assert
        _autoMocker.Verify<IClipboardService>(m => m.SetTextAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Constructor_WorksCorrectly()
    {
        new TestScheduler().With(scheduler =>
        {
            //Arrange
            var mockChannelService = new Mock<IChannelService>();
            var starredCount = 10;
            var readLaterCount = 5;
            mockChannelService.Setup(m => m.GetStarredCount()).Returns(starredCount);
            mockChannelService.Setup(m => m.GetReadLaterCount()).Returns(readLaterCount);
            var mockCategories = new Mock<ICategories>();
            var mockLinkOpeningService = new Mock<ILinkOpeningService>();
            var mockClipboardService = new Mock<IClipboardService>();
            var mockThemeService = new Mock<IThemeService>();
            var themeVariant = ThemeVariant.Light;
            var pallete = new ColorPaletteResources
            {
                BaseHigh = Colors.Black,
            };
            mockThemeService.SetupGet(m => m.RequestedThemeVariant).Returns(themeVariant);
            mockThemeService.SetupGet(m => m.ActualThemeVariant).Returns(themeVariant);
            mockThemeService.Setup(m => m.GetColorPaletteResource(It.Is<ThemeVariant>(arg => arg == themeVariant))).Returns(pallete);
            var canExecuteOpenLinkCommand = false;
            var canExecuteCopyLinkCommand = false;
            var CanExecuteViewPostsCommand = false;

            //Act
            var contentViewModel = new ContentViewModel(mockChannelService.Object, mockCategories.Object, mockLinkOpeningService.Object, mockClipboardService.Object, mockThemeService.Object);
            contentViewModel.OpenLinkCommand.CanExecute.Subscribe(x => canExecuteOpenLinkCommand = x);
            contentViewModel.CopyLinkCommand.CanExecute.Subscribe(x => canExecuteCopyLinkCommand = x);
            contentViewModel.ViewPostsCommand.CanExecute.Subscribe(x => CanExecuteViewPostsCommand = x);

            //Assert
            var expectedCss = @$"body {{ color: Black;}}
                div {{ color: Black;}}
                span {{ color: Black;}}
                table {{ color: Black;}}
                b {{ color: Black;}}
                h1, h2, h3 {{ color: Black; }}
                p {{ color: Black;}}";
            Assert.Equal(expectedCss, contentViewModel.Css);
            Assert.False(contentViewModel.UnreadItemsCountChanged);
            Assert.Equal(starredCount, contentViewModel.StarredCount);
            Assert.Equal(readLaterCount, contentViewModel.ReadLaterCount);
            Assert.Null(contentViewModel.SelectedChannelItem);
            Assert.Null(contentViewModel.SelectedChannelModel);
            Assert.Null(contentViewModel.ItemsSource);
            Assert.Null(contentViewModel.ChannelImageSource);
            Assert.Null(contentViewModel.ItemCategories);
            Assert.Null(contentViewModel.SearchCategories);
            Assert.NotNull(contentViewModel.OpenLinkCommand);
            Assert.True(canExecuteOpenLinkCommand);
            Assert.True(canExecuteCopyLinkCommand);
            Assert.False(CanExecuteViewPostsCommand);
            Assert.NotNull(contentViewModel.CopyLinkCommand);
            Assert.NotNull(contentViewModel.ViewPostsCommand);
            mockChannelService.Verify(m => m.GetStarredCount(), Times.Once);
            mockChannelService.Verify(m => m.GetReadLaterCount(), Times.Once);
        });
    }

    [AvaloniaFact]
    public void SelectedChannelItem_Changed_WorksCorrectly()
    {
        //Arrange
        var mockIconConverter = new Mock<IIconConverter>();
        mockIconConverter.Setup(m => m.GetImageByChannelModel(It.Is<ChannelModel>(arg => arg.Id == 1))).Returns(ChannelImage());
        var channelModel = new ChannelModel(1, "Test Channel", null, "http://test.com/feed/", "http://test.com/image/", "http://test.com/", 1, 1, mockIconConverter.Object);
        var channelItem = new ChannelItem
        {
            Id = 1,
            ChannelId = channelModel.Id,
            Title = "Test",
            Description = "Test description",
            Content = "Test content",
            Link = "https://test.com/items/1/",
            ItemId = "https://test.com/items/1/",
            IsDeleted = false,
            IsFavorite = false,
            IsReadLater = false,
            IsRead = false,
            PublishingDate = DateTime.Now,
        };
        var channelItemModel = new ChannelItemModel(channelItem, mockIconConverter.Object);
        var mockCategories = new Mock<ICategories>();
        var categories = new List<Category>
        {
            new Category { Id = 1, Name = "Category 1" },
        };
        mockCategories.Setup(m => m.GetByChannelItem(It.Is<long>(arg => arg == channelItemModel.Id))).Returns(categories);
        var mockChannelService = new Mock<IChannelService>();
        mockChannelService.Setup(m => m.GetChannelModel(It.Is<int>(arg => arg == channelItemModel.ChannelId))).Returns(channelModel);
        var mockLinkOpeningService = new Mock<ILinkOpeningService>();
        var mockClipboardService = new Mock<IClipboardService>();
        var mockThemeService = new Mock<IThemeService>();

        // Act        
        var contentViewModel = new ContentViewModel(mockChannelService.Object, mockCategories.Object, mockLinkOpeningService.Object, mockClipboardService.Object, mockThemeService.Object);
    }

    private static string? GetLocalDirectory()
    {
        return Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().Location).LocalPath);
    }

    private static string GetFullPath(string path)
    {
        return Path.GetFullPath(Path.Combine(GetLocalDirectory()!, path));
    }

    private static Bitmap ChannelImage()
    {
        return new Bitmap(GetFullPath("rss.png"));
    }
}
