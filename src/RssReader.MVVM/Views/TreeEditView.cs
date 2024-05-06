using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RssReader.MVVM.Views;

public partial class TreeEditView : UserControl
{
    public TreeEditView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

