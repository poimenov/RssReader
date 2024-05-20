using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RssReader.MVVM.Views;

public partial class ItemButtonsView : UserControl
{
    public ItemButtonsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

