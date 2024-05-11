using System;
using System.Reactive.Linq;
using ReactiveUI;
using RssReader.MVVM.DataAccess;
using RssReader.MVVM.Services;
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
        SelectedItemsViewModel = new ItemsViewModel(new ChannelItems(), _channelReader)
        {
            PaneCommand = TriggerPaneCommand
        };
        ContentViewModel = new ContentViewModel(new ChannelItems());
        TreeViewModel = new TreeViewModel(_channelService, _channelReader);
        HeaderViewModel = new HeaderViewModel(new ExportImport(new ChannelsGroups(), new Channels()));

        HeaderViewModel.WhenAnyValue(x => x.ImportCount)
        .Subscribe(x =>
        {
            TreeViewModel.LoadChannels();
        });

        TreeViewModel.WhenAnyValue(x => x.SelectedChannelModel)
            .Where(x => x != null)
            .Subscribe(x =>
            {
                SelectedItemsViewModel = new ItemsViewModel(new ChannelItems(), _channelReader)
                {
                    ChannelModel = x,
                    AllChannels = TreeViewModel.GetChannelsForUpdate(),
                    PaneCommand = TriggerPaneCommand
                };

                SelectedItemsViewModel.WhenAnyValue(x => x.SelectedChannelItem)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    ContentViewModel.SelectedChannelItem = _channelService.GetChannelItem(x!.Id);
                });
            });
    }

    private bool _isPaneOpen;
    public bool IsPaneOpen
    {
        get => _isPaneOpen;
        set => this.RaiseAndSetIfChanged(ref _isPaneOpen, value);
    }

    public TreeViewModel TreeViewModel { get; private set; }

    private ItemsViewModel? _selectedItemsViewModel;
    public ItemsViewModel? SelectedItemsViewModel
    {
        get => _selectedItemsViewModel;
        set => this.RaiseAndSetIfChanged(ref _selectedItemsViewModel, value);
    }

    private ContentViewModel? _contentViewModel;
    public ContentViewModel? ContentViewModel
    {
        get => _contentViewModel;
        set => this.RaiseAndSetIfChanged(ref _contentViewModel, value);
    }

    private HeaderViewModel? _headerViewModel;
    public HeaderViewModel? HeaderViewModel
    {
        get => _headerViewModel;
        set => this.RaiseAndSetIfChanged(ref _headerViewModel, value);
    }

    #region Commands

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

    #endregion
}
