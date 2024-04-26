using System;
using System.Reactive.Linq;
using ReactiveUI;
using RssReader.MVVM.DataAccess;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IChannelService _channelService;
    private readonly IExportImport _exportImport;
    private readonly IChannelReader _channelReader;

    public MainViewModel(IChannelService channelService, IExportImport exportImport, IChannelReader channelReader)
    {
        _channelService = channelService;
        _exportImport = exportImport;
        _channelReader = channelReader;
        _isPaneOpen = true;
        TriggerPaneCommand = CreateTriggerPaneCommand();
        ChannelsTreeViewModel = new ChannelsTreeViewModel(_channelService);
        ChannelsTreeViewModel.WhenAnyValue(x => x.SelectedChannelModel)
            .Where(x => x != null)
            .Subscribe(x =>
            {
                SelectedChannelItemsViewModel = new ChannelItemsViewModel(new ChannelItems())
                {
                    ChannelModel = x
                };

                SelectedChannelItemsViewModel.WhenAnyValue(x => x.SelectedChannelItem)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    if (!string.IsNullOrEmpty(x?.Content))
                    {
                        HtmlContent = x.Content;
                    }
                    else if (!string.IsNullOrEmpty(x?.Description))
                    {
                        HtmlContent = x.Description;
                    }
                });
            });


        // _exportImport.Import("/home/poimenov/Desktop/feedly.opml");
        // _channelReader.ReadAllChannelsAsync();        
    }

    private bool _isPaneOpen;
    public bool IsPaneOpen
    {
        get => _isPaneOpen;
        set => this.RaiseAndSetIfChanged(ref _isPaneOpen, value);
    }

    private string _htmlContent;
    public string HtmlContent
    {
        get => _htmlContent;
        set => this.RaiseAndSetIfChanged(ref _htmlContent, value);
    }

    public ChannelsTreeViewModel ChannelsTreeViewModel { get; private set; }
    private ChannelItemsViewModel? _selectedChannelItemsViewModel;
    public ChannelItemsViewModel? SelectedChannelItemsViewModel
    {
        get => _selectedChannelItemsViewModel;
        set => this.RaiseAndSetIfChanged(ref _selectedChannelItemsViewModel, value);
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
