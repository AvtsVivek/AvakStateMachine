namespace Avak.StateMachine.Samples.BasicWpfWithoutStateClasses.Tests;

using Avak.StateMachine.Core;
using Avak.StateMachine.Core.Configuration;

/// <summary>
/// Tests the WPF sample's XML-configured transitions without creating a window.
/// </summary>
[TestClass]
public class StateMachineConfigurationTests
{
    [TestMethod]
    public void Start_FromIdle_TransitionsToRunning()
    {
        var machine = CreateMachine();

        var result = machine.Trigger("Start", source: "UI");

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Running", machine.CurrentState);
    }

    [TestMethod]
    public void Stop_FromRunning_TransitionsToIdle()
    {
        var machine = CreateMachine();
        machine.Trigger("Start", source: "UI");

        var result = machine.Trigger("Stop", source: "UI");

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Idle", machine.CurrentState);
    }

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
