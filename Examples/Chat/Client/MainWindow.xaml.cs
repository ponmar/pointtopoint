using Client.ViewModels;
using System.Windows;

namespace Client;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    private void Window_Closed(object sender, System.EventArgs e)
    {
        (DataContext as MainViewModel)?.Close();
    }
}
