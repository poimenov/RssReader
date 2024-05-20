using System.Reactive;
using ReactiveUI;
using RssReader.MVVM.Models;

namespace RssReader.MVVM.ViewModels;

public class ItemButtonsViewModel : ViewModelBase
{
    private const string IS_READ_TRUE = "Mark as unread";
    private const string IS_READ_FALSE = "Mark as read";
    private const string IS_FAVORITE_TRUE = "Remove from favorites";
    private const string IS_FAVORITE_FALSE = "Add to favorites";
    private const string IS_READ_LATER_TRUE = "Remove from read later";
    private const string IS_READ_LATER_FALSE = "Add to read later";
    private const string IS_DELETED_TRUE = "Restore";
    private const string IS_DELETED_FALSE = "Delete";
    public ItemButtonsViewModel(ChannelItemModel selectedChannelItem)
    {
        SelectedChannelItem = selectedChannelItem;
        UpdateToolTip();
        ToggleReadCommand = CreateToggleReadCommand();
        ToggleFavoriteCommand = CreateToggleFavoriteCommand();
        ToggleReadLaterCommand = CreateToggleReadLaterCommand();
        ToggleDeleteCommand = CreateToggleDeleteCommand();
    }

    private void UpdateToolTip()
    {
        if (SelectedChannelItem != null)
        {
            IsReadToolTip = SelectedChannelItem.IsRead ? IS_READ_TRUE : IS_READ_FALSE;
            IsFavoriteToolTip = SelectedChannelItem.IsFavorite ? IS_FAVORITE_TRUE : IS_FAVORITE_FALSE;
            IsReadLaterToolTip = SelectedChannelItem.IsReadLater ? IS_READ_LATER_TRUE : IS_READ_LATER_FALSE;
            IsDeletedToolTip = SelectedChannelItem.IsDeleted ? IS_DELETED_TRUE : IS_DELETED_FALSE;
        }
    }

    private ChannelItemModel? _selectedChannelItem;
    public ChannelItemModel? SelectedChannelItem
    {
        get => _selectedChannelItem;
        set => this.RaiseAndSetIfChanged(ref _selectedChannelItem, value);
    }

    public IReactiveCommand ToggleReadCommand { get; }
    private IReactiveCommand CreateToggleReadCommand()
    {
        return ReactiveCommand.Create<bool, Unit>((isRead) =>
        {
            if (SelectedChannelItem != null)
            {
                UpdateToolTip();
            }
            return Unit.Default;
        });
    }
    public IReactiveCommand ToggleFavoriteCommand { get; }
    private IReactiveCommand CreateToggleFavoriteCommand()
    {
        return ReactiveCommand.Create<bool, Unit>((isFavorite) =>
        {
            if (SelectedChannelItem != null)
            {
                UpdateToolTip();
            }
            return Unit.Default;
        });
    }
    public IReactiveCommand ToggleReadLaterCommand { get; }
    private IReactiveCommand CreateToggleReadLaterCommand()
    {
        return ReactiveCommand.Create<bool, Unit>((isReadLater) =>
        {
            if (SelectedChannelItem != null)
            {
                UpdateToolTip();
            }
            return Unit.Default;
        });
    }
    public IReactiveCommand ToggleDeleteCommand { get; }
    private IReactiveCommand CreateToggleDeleteCommand()
    {
        return ReactiveCommand.Create<bool, Unit>((isDeleted) =>
        {
            if (SelectedChannelItem != null)
            {
                UpdateToolTip();
            }
            return Unit.Default;
        });
    }

    private string? _isReadToolTip;
    public string? IsReadToolTip
    {
        get => _isReadToolTip;
        set => this.RaiseAndSetIfChanged(ref _isReadToolTip, value);
    }

    private string? _isFavoriteToolTip;
    public string? IsFavoriteToolTip
    {
        get => _isFavoriteToolTip;
        set => this.RaiseAndSetIfChanged(ref _isFavoriteToolTip, value);
    }

    private string? _isReadLaterToolTip;
    public string? IsReadLaterToolTip
    {
        get => _isReadLaterToolTip;
        set => this.RaiseAndSetIfChanged(ref _isReadLaterToolTip, value);
    }

    private string? _isDeletedToolTip;
    public string? IsDeletedToolTip
    {
        get => _isDeletedToolTip;
        set => this.RaiseAndSetIfChanged(ref _isDeletedToolTip, value);
    }

}
