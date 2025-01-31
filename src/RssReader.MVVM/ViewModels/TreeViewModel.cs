using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Input;
using DynamicData;
using DynamicData.Binding;
using log4net;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using RssReader.MVVM.Extensions;
using RssReader.MVVM.Models;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.ViewModels;

public class TreeViewModel : ViewModelBase
{
    private readonly IChannelService _channelsService;
    private readonly IChannelReader _channelReader;
    private readonly ILog _log;
    public TreeViewModel(IChannelService channelsService, IChannelReader channelReader, ILog log)
    {
        _channelsService = channelsService;
        _channelReader = channelReader;
        _log = log;
        AddFolderCommand = CreateAddFolderCommand();
        AddFeedCommand = CreateAddFeedCommand();
        DeleteCommand = CreateDeleteCommand();
        GetFoldersCommand = CreateGetFoldersCommand();
        RowDragStartedCommand = CreateRowDragStartedCommand();
        RowDragOverCommand = CreateRowDragOverCommand();
        RowDropCommand = CreateRowDropCommand();

        SourceItems = new ObservableCollectionExtended<ChannelModel>();
        SourceItems.ToObservableChangeSet()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _items)
            .Subscribe();

        var source = new HierarchicalTreeDataGridSource<ChannelModel>(Array.Empty<ChannelModel>())
        {
            Columns =
            {
                new HierarchicalExpanderColumn<ChannelModel>(
                    new TemplateColumn<ChannelModel>(
                        string.Empty,
                        "ChannelNameCell",
                        "ChannelNameEditCell",
                        GridLength.Star),
                    x => x.Children),
                new TextColumn<ChannelModel, int>(
                    string.Empty,
                    x => x.UnreadItemsCount,
                    GridLength.Auto),
            }
        };

        source.RowSelection!.SelectionChanged += (sender, args) =>
        {
            SelectedChannelModel = args.SelectedItems.FirstOrDefault();
        };

        Source = source;
        LoadChannels();
    }

    public void LoadChannels()
    {
        SourceItems.Clear();
        var channelAll = _channelsService.GetChannel(ChannelModelType.All);
        var items = new ObservableCollectionExtended<ChannelModel>
        {
            channelAll,
            _channelsService.GetChannel(ChannelModelType.Starred),
            _channelsService.GetChannel(ChannelModelType.ReadLater)
        };
        items.AddRange(_channelsService.GetChannels());
        SourceItems.AddRange(items);
        ((HierarchicalTreeDataGridSource<ChannelModel>)Source).Items = Items;
        SelectedChannelModel = channelAll;

        var channelsForUpdate = GetChannelsForUpdate();

        channelsForUpdate.ForEach(x => x.WhenAnyValue(m => m.UnreadItemsCount)
            .Subscribe(c =>
            {
                channelAll.UnreadItemsCount = GetAllUnreadCount();
            }));

        Parallel.ForEachAsync(channelsForUpdate, cancellationToken: default,
            async (x, ct) =>
            {
                await _channelReader.ReadChannelAsync(x, ct);
            });
    }

    public List<ChannelModel> GetChannelsForUpdate()
    {
        var retVal = SourceItems.Where(x => x.IsChannelsGroup == true && x.Children != null && x.Children.Count > 0)
            .SelectMany(x => x.Children!, (group, channel) => new
            {
                GroupId = group.Id,
                Channel = channel
            }).Where(x => x != null).Select(x => x.Channel).ToList();
        retVal.AddRange(SourceItems.Where(x => x.IsChannelsGroup == false && x.ModelType == ChannelModelType.Default).ToList());
        return retVal;
    }

    private int GetAllUnreadCount()
    {
        var channelsForUpdate = GetChannelsForUpdate();
        return channelsForUpdate.Sum(x => x.UnreadItemsCount);
    }

    private bool IsValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        return Uri.IsWellFormedUriString(url, UriKind.Absolute) &&
            !GetChannelsForUpdate().Any(x => x.Url.Equals(FeedUrl, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsValidFolderName(string? folderName)
    {
        var reservedNames = new string[] { ChannelModel.CHANNELMODELTYPE_ALL, ChannelModel.CHANNELMODELTYPE_STARRED, ChannelModel.CHANNELMODELTYPE_READLATER };

        return !string.IsNullOrWhiteSpace(folderName) && !reservedNames.Contains(folderName, StringComparer.OrdinalIgnoreCase) &&
                    Folders!.All(x => !x.Equals(folderName, StringComparison.OrdinalIgnoreCase));
    }

    private ChannelModel? Moved { get; set; }

    #region FolderName
    private string? _folderName;
    public string? FolderName
    {
        get => _folderName;
        set => this.RaiseAndSetIfChanged(ref _folderName, value);
    }
    #endregion

    #region FeedUrl
    private string? _feedUrl;
    public string? FeedUrl
    {
        get => _feedUrl;
        set => this.RaiseAndSetIfChanged(ref _feedUrl, value);
    }
    #endregion

    #region Folders
    private IEnumerable<string>? _folders;
    public IEnumerable<string>? Folders
    {
        get => _folders;
        set => this.RaiseAndSetIfChanged(ref _folders, value);
    }
    #endregion

    #region SelectedFolder
    private string? _selectedFolder;
    public string? SelectedFolder
    {
        get => _selectedFolder;
        set => this.RaiseAndSetIfChanged(ref _selectedFolder, value);
    }
    #endregion

    #region Items    
    private readonly ReadOnlyObservableCollection<ChannelModel> _items;
    public ReadOnlyObservableCollection<ChannelModel> Items => _items;
    #endregion    

    public ObservableCollectionExtended<ChannelModel> SourceItems;
    public ITreeDataGridSource<ChannelModel> Source { get; private set; }

    #region SelectedChannelModel
    private ChannelModel? _selectedChannelModel;
    public ChannelModel? SelectedChannelModel
    {
        get => _selectedChannelModel;
        set => this.RaiseAndSetIfChanged(ref _selectedChannelModel, value);
    }
    #endregion

    #region GetFoldersCommand
    public IReactiveCommand GetFoldersCommand { get; }
    private IReactiveCommand CreateGetFoldersCommand()
    {
        return ReactiveCommand.Create(
            () =>
            {
                var folders = new List<string>
                {
                    string.Empty
                };
                folders.AddRange(SourceItems.Where(x => x.IsChannelsGroup == true).Select(x => x.Title));
                Folders = folders;
            }
        );
    }
    #endregion

    #region AddFolderCommand
    public IReactiveCommand AddFolderCommand { get; }
    private IReactiveCommand CreateAddFolderCommand()
    {
        return ReactiveCommand.Create<string, Unit>(
            (folderName) =>
            {
                if (IsValidFolderName(folderName))
                {
                    var folder = new ChannelModel(0, folderName, 0, null);
                    _channelsService.AddChannel(folder);
                    if (SourceItems.Any(x => x.IsChannelsGroup == true))
                    {
                        var maxIndex = SourceItems.Where(x => x.IsChannelsGroup == true).Select(x => SourceItems.IndexOf(x)).Max();
                        SourceItems.Insert(maxIndex + 1, folder);
                    }
                    else
                    {
                        var index = SourceItems.IndexOf(SourceItems.First(x => x.ModelType == ChannelModelType.ReadLater));
                        SourceItems.Insert(index + 1, folder);
                    }

                    folder.WhenAnyValue(x => x.Title).Subscribe((x) => _channelsService.UpdateChannel(folder));
                    FolderName = null;
                }

                return Unit.Default;
            }, this.WhenAnyValue(x => x.FolderName, (folderName) => IsValidFolderName(folderName)));
    }
    #endregion

    #region AddFeedCommand
    public IReactiveCommand AddFeedCommand { get; }
    private IReactiveCommand CreateAddFeedCommand()
    {
        return ReactiveCommand.CreateFromTask<string, Unit>(
            async (feedUrl) =>
            {
                if (IsValidUrl(feedUrl))
                {
                    try
                    {
                        var feed = _channelsService.CreateNewChannel(feedUrl);

                        if (!string.IsNullOrEmpty(SelectedFolder))
                        {
                            var folder = SourceItems.FirstOrDefault(x => x.Title.Equals(SelectedFolder, StringComparison.OrdinalIgnoreCase));
                            if (folder != null)
                            {
                                feed.Parent = folder;
                                folder.Children!.Add(feed);
                                feed.WhenAnyValue(x => x.UnreadItemsCount)
                                    .Subscribe(count =>
                                    {
                                        folder.RaisePropertyChanged(nameof(ChannelModel.UnreadItemsCount));
                                    });
                            }
                        }
                        else
                        {
                            SourceItems.Add(feed);
                        }

                        _channelsService.AddChannel(feed);
                        var channelAll = SourceItems.First(x => x.ModelType == ChannelModelType.All);
                        feed.WhenAnyValue(m => m.UnreadItemsCount).Subscribe(c => { channelAll.UnreadItemsCount = GetAllUnreadCount(); });
                        await _channelReader.ReadChannelAsync(feed, default);
                        feed.UpdateImageSource();
                        var source = (HierarchicalTreeDataGridSource<ChannelModel>)Source;
                        source.RowSelection?.Select(source.Items.IndexOf(feed));
                        feed.WhenAnyValue(x => x.Title).Subscribe((x) => _channelsService.UpdateChannel(feed));
                    }
                    catch (System.Exception ex)
                    {
                        _log.Error(ex);
                        var dialog = this.GetMessageBox("Exception", ex.Message, ButtonEnum.Ok, Icon.Error);
                        await dialog.ShowAsync();
                    }
                    finally
                    {
                        SelectedFolder = string.Empty;
                        FeedUrl = null;
                    }
                }

                return Unit.Default;
            }, this.WhenAnyValue(x => x.FeedUrl, (feedUrl) => IsValidUrl(feedUrl)));
    }
    #endregion

    #region DeleteCommand
    public IReactiveCommand DeleteCommand { get; }
    private IReactiveCommand CreateDeleteCommand()
    {
        return ReactiveCommand.Create(
            async () =>
            {
                if (SelectedChannelModel != null &&
                    SelectedChannelModel.ModelType == ChannelModelType.Default)
                {
                    var message = $"Delete {SelectedChannelModel.Title} {(SelectedChannelModel.IsChannelsGroup ? "folder" : "feed")}?";
                    var dialog = this.GetMessageBox("Delete", message, ButtonEnum.YesNo, Icon.Question);
                    var result = await dialog.ShowAsync();
                    if (result == ButtonResult.No)
                    {
                        return;
                    }

                    _channelsService.DeleteChannel(SelectedChannelModel);
                    if (SelectedChannelModel.IsChannelsGroup)
                    {
                        SourceItems.Remove(SelectedChannelModel);
                    }
                    else
                    {
                        var parent = SelectedChannelModel.Parent;
                        if (parent != null)
                        {
                            parent.Children!.Remove(SelectedChannelModel);
                            parent.RaisePropertyChanged(nameof(ChannelModel.Children));
                        }
                        else
                        {
                            SourceItems.Remove(SelectedChannelModel);
                        }
                    }

                    SelectedFolder = string.Empty;
                    var source = (HierarchicalTreeDataGridSource<ChannelModel>)Source;
                    source.RowSelection?.Select(0);
                    var channelAll = SourceItems.First(x => x.ModelType == ChannelModelType.All);
                    channelAll.UnreadItemsCount = GetAllUnreadCount();
                }
            }, this.WhenAnyValue(x => x.SelectedChannelModel, x => x != null && x.ModelType == ChannelModelType.Default));
    }
    #endregion

    #region RowDragStartedCommand
    public IReactiveCommand RowDragStartedCommand { get; }
    private IReactiveCommand CreateRowDragStartedCommand()
    {
        return ReactiveCommand.Create(
            (TreeDataGridRowDragStartedEventArgs e) =>
            {
                if (e.Models.Count() == 1 && e.Models.First() is ChannelModel m)
                {
                    if (m.ModelType == ChannelModelType.Default)
                    {
                        Moved = m;
                    }
                    else
                    {
                        e.AllowedEffects = DragDropEffects.None;
                    }
                }
            });
    }
    #endregion

    #region RowDragOverCommand
    public IReactiveCommand RowDragOverCommand { get; }
    private IReactiveCommand CreateRowDragOverCommand()
    {
        return ReactiveCommand.Create(
            (TreeDataGridRowDragEventArgs e) =>
            {
                e.Inner.DragEffects = DragDropEffects.None;
                if (Moved != null && e.TargetRow.Model is ChannelModel Target && Target.ModelType == ChannelModelType.Default)
                {
                    if (Moved.IsChannelsGroup && Target.IsChannelsGroup && e.Position != TreeDataGridRowDropPosition.Inside)
                    {
                        e.Inner.DragEffects = DragDropEffects.Move;
                    }
                    else if (!Moved.IsChannelsGroup)
                    {
                        if (Target.IsChannelsGroup)
                        {
                            if (e.Position == TreeDataGridRowDropPosition.Inside)
                            {
                                if (Moved.Parent != Target)
                                {
                                    e.Inner.DragEffects = DragDropEffects.Move;
                                }
                            }
                            else if (e.Position == TreeDataGridRowDropPosition.After)
                            {
                                if (SourceItems!.Last(x => x.IsChannelsGroup == true) == Target)
                                {
                                    e.Inner.DragEffects = DragDropEffects.Move;
                                }
                            }
                        }
                        else
                        {
                            if (e.Position != TreeDataGridRowDropPosition.Inside)
                            {
                                e.Inner.DragEffects = DragDropEffects.Move;
                            }
                        }
                    }
                }
            });
    }
    #endregion

    #region RowDropCommand
    public IReactiveCommand RowDropCommand { get; }
    private IReactiveCommand CreateRowDropCommand()
    {
        return ReactiveCommand.Create(
            (TreeDataGridRowDragEventArgs e) =>
            {
                if (Moved != null && e.TargetRow.Model is ChannelModel Target && Target.ModelType == ChannelModelType.Default)
                {
                    e.Handled = true;
                    Debug.WriteLine($"Position: {e.Position}");
                    var sourceIndex = SourceItems!.IndexOf(Moved);
                    Debug.WriteLine($"Source: {Moved.Title}, Index: {sourceIndex}, Rank: {Moved.Rank}");
                    var targetIndex = SourceItems!.IndexOf(Target);
                    Debug.WriteLine($"Target: {Target.Title}, Index: {targetIndex}, Rank: {Target.Rank}");

                    if (Moved.IsChannelsGroup && Target.IsChannelsGroup && e.Position != TreeDataGridRowDropPosition.Inside)
                    {
                        //Moved folders
                        SourceItems.RemoveAt(sourceIndex);
                        SourceItems.Insert(targetIndex, Moved);
                        var i = 1;
                        foreach (var group in SourceItems.Where(x => x.IsChannelsGroup == true))
                        {
                            group.Rank = i;
                            _channelsService.UpdateChannel(group);
                            i++;
                        }
                    }
                    else if (!Moved.IsChannelsGroup)
                    {
                        if (Target.IsChannelsGroup)
                        {
                            if (e.Position == TreeDataGridRowDropPosition.Inside)
                            {
                                //Moved channel into folder
                                if (Moved.Parent != Target)
                                {
                                    if (Moved.Parent != null)
                                    {
                                        Moved.Parent.Children!.Remove(Moved);
                                    }
                                    else
                                    {
                                        SourceItems.RemoveAt(sourceIndex);
                                    }

                                    Moved.Parent = Target;
                                    Target.Children!.Add(Moved);
                                    var i = 1;
                                    foreach (var item in Target.Children)
                                    {
                                        item.Rank = i;
                                        _channelsService.UpdateChannel(item);
                                        i++;
                                    }
                                }
                            }
                            else if (e.Position == TreeDataGridRowDropPosition.After)
                            {
                                //Moved channel after last folder
                                if (SourceItems!.Last(x => x.IsChannelsGroup == true) == Target)
                                {
                                    if (Moved.Parent != null)
                                    {
                                        Moved.Parent.Children!.Remove(Moved);
                                        Moved.Parent = null;
                                    }
                                    else
                                    {
                                        SourceItems.RemoveAt(sourceIndex);
                                    }

                                    SourceItems.Add(Moved);
                                    var i = 1;
                                    foreach (var item in SourceItems.Where(x => x.IsChannelsGroup == false))
                                    {
                                        item.Rank = i;
                                        _channelsService.UpdateChannel(item);
                                        i++;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (e.Position != TreeDataGridRowDropPosition.Inside)
                            {
                                if (Target.Parent != null)
                                {
                                    //Moved channel from one folder to other folder 
                                    //(before or after target channel)
                                    targetIndex = Target.Parent.Children!.IndexOf(Target);
                                    if (Moved.Parent != null)
                                    {
                                        Moved.Parent.Children!.Remove(Moved);
                                    }
                                    else
                                    {
                                        SourceItems.RemoveAt(sourceIndex);
                                    }

                                    Target.Parent.Children.Insert(targetIndex, Moved);
                                    Moved.Parent = Target.Parent;
                                    var i = 1;
                                    foreach (var item in Target.Parent.Children)
                                    {
                                        item.Rank = i;
                                        _channelsService.UpdateChannel(item);
                                        i++;
                                    }
                                }
                                else
                                {
                                    //Moved channel from one folder to root of tree
                                    //(before or after target channel)
                                    if (Moved.Parent != null)
                                    {
                                        Moved.Parent.Children!.Remove(Moved);
                                        Moved.Parent = null;
                                    }
                                    else
                                    {
                                        SourceItems.RemoveAt(sourceIndex);
                                    }

                                    SourceItems.Insert(targetIndex, Moved);
                                    var i = 1;
                                    foreach (var item in SourceItems.Where(x => x.IsChannelsGroup == false))
                                    {
                                        item.Rank = i;
                                        _channelsService.UpdateChannel(item);
                                        i++;
                                    }
                                }
                            }
                        }
                    }
                }
            });
    }
    #endregion
}
