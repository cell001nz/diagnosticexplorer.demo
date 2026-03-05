using System.Threading.Tasks;
using Avalonia.Controls;
using WidgetSample.Avalonia.ViewModels;

namespace WidgetSample.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Closing += OnWindowClosing;
    }

    private void OnWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
            _ = vm.OnClosingAsync();
    }
}