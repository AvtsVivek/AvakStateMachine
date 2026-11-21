namespace WpfTrafficLightWithDI;

using System.Windows;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    private void Next_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Next();
    }
}
