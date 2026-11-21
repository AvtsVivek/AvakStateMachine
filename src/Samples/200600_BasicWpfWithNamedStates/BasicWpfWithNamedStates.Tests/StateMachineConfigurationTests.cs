namespace BasicWpfWithNamedStates.Tests;

using Avak.StateMachine.Core;
using Avak.StateMachine.Core.Configuration;

/// <summary>
/// Tests the WPF sample's XML-configured transitions without creating a window.
/// </summary>
[TestClass]
public class StateMachineConfigurationTests
{
    /// <summary>
    /// Verifies that the UI-sourced Start trigger moves the configured machine from Idle to Running.
    /// </summary>
    /// <remarks>
    /// The sample XML is loaded without constructing a WPF window. The assertions
    /// check the trigger result and the machine's resulting state.
    /// </remarks>
    [TestMethod]
    public void Start_FromIdle_TransitionsToRunning()
    {
        var machine = CreateMachine();

        var result = machine.Trigger("Start", source: "UI");

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Running", machine.CurrentState);
    }

    /// <summary>
    /// Verifies that Stop returns the machine to Idle after it has entered Running.
    /// </summary>
    /// <remarks>
    /// The test first performs the configured UI-sourced Start transition, then
    /// checks that UI-sourced Stop succeeds and restores the Idle state.
    /// </remarks>
    [TestMethod]
    public void Stop_FromRunning_TransitionsToIdle()
    {
        var machine = CreateMachine();
        machine.Trigger("Start", source: "UI");

        var result = machine.Trigger("Stop", source: "UI");

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Idle", machine.CurrentState);
    }

    /// <summary>
    /// Verifies that Stop is rejected from Idle without changing the current state.
    /// </summary>
    /// <remarks>
    /// The test issues Stop on a freshly configured machine, where no such
    /// transition exists, and checks both the failed result and preserved Idle state.
    /// </remarks>
    [TestMethod]
    public void Stop_FromIdle_IsRejectedWithoutChangingState()
    {
        var machine = CreateMachine();

        var result = machine.Trigger("Stop", source: "UI");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Idle", machine.CurrentState);
    }

    private static StateMachine CreateMachine()
    {
        string configurationPath = Path.Combine(AppContext.BaseDirectory, "StateMachineConfiguration.xml");
        var configuration = new XmlConfigurationLoader().LoadFromFile(configurationPath);
        return new StateMachine(configuration);
    }
}
