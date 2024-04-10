using Avalonia.Controls;
using ClientAvalonia.ViewModels;

namespace ClientAvalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Closed(object? sender, System.EventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.ExitApplication();
        }
    }
}
