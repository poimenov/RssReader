using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RssReader.MVVM.Views;

public partial class TreeView : UserControl
{
    public TreeView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

