namespace Avak.StateMachine.Samples.BasicWpfStateMachineWithoutStateClasses;

using System.IO;
using System.Windows;
using Avak.StateMachine.Core;
using Avak.StateMachine.Core.Configuration;

public partial class MainWindow : Window
{
    private readonly StateMachineViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        var loader = new XmlConfigurationLoader();
        string configurationPath = Path.Combine(AppContext.BaseDirectory, "StateMachineConfiguration.xml");
        var configuration = loader.LoadFromFile(configurationPath);

        _viewModel = new StateMachineViewModel(new StateMachine(configuration));
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
