namespace Avak.StateMachine.Samples.BasicWpfWithoutStateClasses.Tests;

using Avak.StateMachine.Core;
using Avak.StateMachine.Core.Configuration;
using Avak.StateMachine.Samples.BasicWpfWithoutStateClasses;

/// <summary>
/// Tests WPF sample state interaction without constructing a window or starting an application.
/// </summary>
[TestClass]
public class MainWindowViewModelTests
{
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
