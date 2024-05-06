using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RssReader.MVVM.Views;

public partial class HeaderView : UserControl
{
    public HeaderView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

