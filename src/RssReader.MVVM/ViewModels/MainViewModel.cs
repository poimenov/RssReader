using System;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using RssReader.MVVM.DataAccess.Interfaces;

namespace RssReader.MVVM.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IChannelItems _channelItems;
    private readonly IServiceProvider _serviceProvider;

    public MainViewModel(IChannelItems channelItems, IServiceProvider serviceProvider)
    {
        _channelItems = channelItems;
        _serviceProvider = serviceProvider;
        _isPaneOpen = true;
        TriggerPaneCommand = CreateTriggerPaneCommand();

        ContentViewModel = _serviceProvider.GetRequiredService<ContentViewModel>();
        TreeViewModel = _serviceProvider.GetRequiredService<TreeViewModel>();
        HeaderViewModel = _serviceProvider.GetRequiredService<HeaderViewModel>();

        HeaderViewModel.WhenAnyValue(x => x.ImportCount)
        .Subscribe(x =>
        {
            TreeViewModel.LoadChannels();
        });

        TreeViewModel.WhenAnyValue(x => x.SelectedChannelModel)
            .WhereNotNull()
            .Subscribe(x =>
            {
                var model = _serviceProvider.GetRequiredService<ItemsViewModel>();
                model.ChannelModel = x;
                model.AllChannels = TreeViewModel.GetChannelsForUpdate();
                model.PaneCommand = TriggerPaneCommand;
                SelectedItemsViewModel = model;

                SelectedItemsViewModel?.WhenAnyValue(x => x.Items)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    ContentViewModel.ItemsSource = x;
                });

                SelectedItemsViewModel?.WhenAnyValue(x => x.SelectedChannelItem)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    ContentViewModel.SelectedChannelItem = x;
                });

                ContentViewModel.WhenAnyValue(x => x.SelectedChannelItem)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    if (SelectedItemsViewModel != null)
                    {
                        SelectedItemsViewModel.SelectedChannelItem = x;
                    }
                });
            });

        ContentViewModel.WhenAnyValue(x => x.UnreadItemsCountChanged)
        .Subscribe(x =>
        {
            if (ContentViewModel.SelectedChannelModel != null)
            {
                var targetChannel = TreeViewModel.GetChannelsForUpdate().FirstOrDefault(c => c.Id == ContentViewModel.SelectedChannelModel.Id);
                if (targetChannel != null)
                {
                    targetChannel.UnreadItemsCount = ContentViewModel.SelectedChannelModel.UnreadItemsCount;
                }
            }
        });
        ContentViewModel.WhenAnyValue(x => x.StarredCount)
        .Subscribe(x =>
        {
            var targetChannel = TreeViewModel.Items.FirstOrDefault(x => x.ModelType == Models.ChannelModelType.Starred);
            if (targetChannel != null)
            {
                targetChannel.UnreadItemsCount = x;
            }

            if (TreeViewModel.SelectedChannelModel == targetChannel)
            {
                SelectedItemsViewModel?.LoadItems();
            }
        });
        ContentViewModel.WhenAnyValue(x => x.ReadLaterCount)
        .Subscribe(x =>
        {
            var targetChannel = TreeViewModel.Items.FirstOrDefault(x => x.ModelType == Models.ChannelModelType.ReadLater);
            if (targetChannel != null)
            {
                targetChannel.UnreadItemsCount = x;
            }

            if (TreeViewModel.SelectedChannelModel == targetChannel)
            {
                SelectedItemsViewModel?.LoadItems();
            }
        });
        ContentViewModel.WhenAnyValue(x => x.SelectedCategory)
        .WhereNotNull()
        .Subscribe(x =>
        {
            SelectedItemsViewModel?.LoadItems(x);
        });
    }
    public void DeleteChannelItems()
    {
        _channelItems.Delete();
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
