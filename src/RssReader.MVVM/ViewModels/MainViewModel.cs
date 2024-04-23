using ReactiveUI;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IExportImport _exportImport;
    private readonly IChannelReader _channelReader;

    public MainViewModel(IExportImport exportImport, IChannelReader channelReader)
    {
        _exportImport = exportImport;
        _channelReader = channelReader;
        _isPaneOpen = true;
        TriggerPaneCommand = CreateTriggerPaneCommand();
        // _exportImport.Import("/home/poimenov/Desktop/feedly.opml");
        // _channelReader.ReadAllChannelsAsync();        
    }

    private bool _isPaneOpen;
    public bool IsPaneOpen
    {
        get => _isPaneOpen;
        set => this.RaiseAndSetIfChanged(ref _isPaneOpen, value);
    }

    public IReactiveCommand TriggerPaneCommand { get; }

    private IReactiveCommand CreateTriggerPaneCommand()
    {
        return ReactiveCommand.Create(
        () =>
            {
                IsPaneOpen = !IsPaneOpen;
            }
        );
    }
}
