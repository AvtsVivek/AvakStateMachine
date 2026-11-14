namespace Avak.StateMachine.Samples.BasicWpfWithoutStateClasses;

using System.IO;
using System.Windows;
using Avak.StateMachine.Core;
using Avak.StateMachine.Core.Configuration;

public partial class MainWindow : Window
{
    private readonly StateMachine _machine;

    public MainWindow()
    {
        InitializeComponent();

        var loader = new XmlConfigurationLoader();
        string configurationPath = Path.Combine(AppContext.BaseDirectory, "StateMachineConfiguration.xml");
        var configuration = loader.LoadFromFile(configurationPath);
        _machine = new StateMachine(configuration);

        UpdateDisplay("Ready.");
    }

    private void Start_Click(object sender, RoutedEventArgs e)
    {
        ApplyTrigger("Start");
    }

    private void Stop_Click(object sender, RoutedEventArgs e)
    {
        ApplyTrigger("Stop");
    }

    private void ApplyTrigger(string triggerName)
    {
        var result = _machine.Trigger(triggerName, source: "UI");
        UpdateDisplay(
            result.Success
                ? $"Transitioned from {result.FromState} to {result.ToState}."
                : result.ErrorMessage ?? "The transition was rejected.");
    }

    private void UpdateDisplay(string statusMessage)
    {
        CurrentStateText.Text = _machine.CurrentState;
        StatusText.Text = statusMessage;
        StartButton.IsEnabled = _machine.CurrentState == "Idle";
        StopButton.IsEnabled = _machine.CurrentState == "Running";
    }
}
