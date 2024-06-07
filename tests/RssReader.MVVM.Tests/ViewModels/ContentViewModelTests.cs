using Moq;
using Moq.AutoMock;
using RssReader.MVVM.Services.Interfaces;
using RssReader.MVVM.ViewModels;
using ReactiveUI;
using System.Reactive;

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
}

