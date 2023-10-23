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
        (DataContext as MainViewModel)?.ExitApplication();
    }

    // Note: click event is used instead of command for IsDefault="True" to work when pressing enter in text input
    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var vm = (MainViewModel)DataContext;
        vm.SendTextCommand.Execute(null);
    }
}
