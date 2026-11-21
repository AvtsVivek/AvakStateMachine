namespace BasicWpfWithNamedStatesAndViewModel.Tests;

using Avak.StateMachine.Core;
using Avak.StateMachine.Core.Configuration;
using BasicWpfWithNamedStatesAndViewModel;

/// <summary>
/// Tests WPF sample state interaction without constructing a window or starting an application.
/// </summary>
[TestClass]
public class MainWindowViewModelTests
{
    /// <summary>
    /// Verifies that Start updates the view model's state, available actions, status, and notifications.
    /// </summary>
    /// <remarks>
    /// The test observes <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/>
    /// while invoking Start. It checks the resulting Running state and bindable
    /// values, and verifies notifications for the state, action availability,
    /// and status properties without creating a WPF window.
    /// </remarks>
    [TestMethod]
    public void Start_FromIdle_TransitionsToRunningAndUpdatesBindableProperties()
    {
        var viewModel = CreateViewModel();
        List<string?> changedProperties = [];
        viewModel.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

        var result = viewModel.Start();

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Running", viewModel.CurrentState);
        Assert.IsFalse(viewModel.CanStart);
        Assert.IsTrue(viewModel.CanStop);
        Assert.AreEqual("Transitioned from Idle to Running.", viewModel.StatusMessage);
        CollectionAssert.Contains(changedProperties, nameof(MainWindowViewModel.CurrentState));
        CollectionAssert.Contains(changedProperties, nameof(MainWindowViewModel.CanStart));
        CollectionAssert.Contains(changedProperties, nameof(MainWindowViewModel.CanStop));
        CollectionAssert.Contains(changedProperties, nameof(MainWindowViewModel.StatusMessage));
    }

    /// <summary>
    /// Verifies that Stop transitions the view model from Running to Idle and updates its displayed values.
    /// </summary>
    /// <remarks>
    /// Start establishes the Running state before Stop is called. The assertions
    /// check the successful result, Idle state, enabled Start action, disabled
    /// Stop action, and updated status text.
    /// </remarks>
    [TestMethod]
    public void Stop_FromRunning_TransitionsToIdle()
    {
        var viewModel = CreateViewModel();
        viewModel.Start();

        var result = viewModel.Stop();

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Idle", viewModel.CurrentState);
        Assert.IsTrue(viewModel.CanStart);
        Assert.IsFalse(viewModel.CanStop);
        Assert.AreEqual("Transitioned from Running to Idle.", viewModel.StatusMessage);
    }

    /// <summary>
    /// Verifies that Stop from Idle is reported as rejected while the view model remains unchanged.
    /// </summary>
    /// <remarks>
    /// The test calls Stop on a fresh view model and checks the failed result,
    /// preserved Idle state, action availability, and a status message containing
    /// the missing-transition diagnostic.
    /// </remarks>
    [TestMethod]
    public void Stop_FromIdle_ReportsRejectedTransitionWithoutChangingState()
    {
        var viewModel = CreateViewModel();

        var result = viewModel.Stop();

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Idle", viewModel.CurrentState);
        Assert.IsTrue(viewModel.CanStart);
        Assert.IsFalse(viewModel.CanStop);
        StringAssert.Contains(viewModel.StatusMessage, "No transition configured");
    }

    private static MainWindowViewModel CreateViewModel()
    {
        StateMachineConfiguration configuration = new()
        {
            InitialState = "Idle",
            States = new()
            {
                ["Idle"] = new StateConfiguration
                {
                    Name = "Idle",
                    Transitions = new()
                    {
                        ["Start"] = [new TransitionConfiguration { ToState = "Running", Source = "UI" }]
                    }
                },
                ["Running"] = new StateConfiguration
                {
                    Name = "Running",
                    Transitions = new()
                    {
                        ["Stop"] = [new TransitionConfiguration { ToState = "Idle", Source = "UI" }]
                    }
                }
            }
        };

        return new MainWindowViewModel(new StateMachine(configuration));
    }
}
