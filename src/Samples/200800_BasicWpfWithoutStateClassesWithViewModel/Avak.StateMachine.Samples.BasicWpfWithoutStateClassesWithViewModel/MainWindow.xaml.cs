namespace Avak.StateMachine.Samples.BasicWpfWithoutStateClassesWithViewModel;

using System.IO;
using System.Windows;
using Avak.StateMachine.Core;
using Avak.StateMachine.Core.Configuration;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        var loader = new XmlConfigurationLoader();
        string configurationPath = Path.Combine(AppContext.BaseDirectory, "StateMachineConfiguration.xml");
        var configuration = loader.LoadFromFile(configurationPath);

        _viewModel = new MainWindowViewModel(new StateMachine(configuration));
        DataContext = _viewModel;
    }

    private void Start_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Start();
    }

    private void Stop_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Stop();
    }
}
