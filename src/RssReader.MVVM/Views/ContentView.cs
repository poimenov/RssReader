using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RssReader.MVVM.Views;

public partial class ContentView : UserControl
{
    public ContentView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

