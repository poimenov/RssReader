using Avalonia;
using ReactiveUI;

namespace RssReader.MVVM.ViewModels;

public class ViewModelBase : ReactiveObject
{
    protected App CurrentApplication => (App)Application.Current;
}
