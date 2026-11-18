namespace ConsoleWithDependencyInjection.Tests;

using Avak.StateMachine.Core;
using Microsoft.Extensions.DependencyInjection;

[TestClass]
public class DependencyInjectionTests
{
    [TestMethod]
    public void Start_WhenNotReady_IsDeniedWithoutRecordingAnEvent()
    {
        using var provider = CreateProvider();
        using var scope = provider.CreateScope();
        var machine = scope.ServiceProvider.GetRequiredService<StateMachine>();
        var session = scope.ServiceProvider.GetRequiredService<SessionData>();

        var result = machine.Trigger("Start", source: "Console");

        Assert.IsFalse(result.Success);
        Assert.IsInstanceOfType<TransitionDeniedException>(result.Exception);
        Assert.AreEqual("Idle", machine.CurrentState);
        Assert.AreEqual(0, session.Events.Count);
    }

    [TestMethod]
    public void Start_WhenReady_UsesTheScopedGuardAndCallback()
    {
        using var provider = CreateProvider();
        using var scope = provider.CreateScope();
        var machine = scope.ServiceProvider.GetRequiredService<StateMachine>();
        var session = scope.ServiceProvider.GetRequiredService<SessionData>();
        session.IsReady = true;

        var result = machine.Trigger("Start", source: "Console");

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Running", machine.CurrentState);
        CollectionAssert.AreEqual(new[] { "Idle -> Running" }, session.Events);
    }

    [TestMethod]
    public void SeparateScopes_HaveIndependentMachinesAndSessionData()
    {
        using var provider = CreateProvider();
        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();
        var firstMachine = firstScope.ServiceProvider.GetRequiredService<StateMachine>();
        var firstSession = firstScope.ServiceProvider.GetRequiredService<SessionData>();
        var secondMachine = secondScope.ServiceProvider.GetRequiredService<StateMachine>();
        var secondSession = secondScope.ServiceProvider.GetRequiredService<SessionData>();
        firstSession.IsReady = true;

        Assert.IsTrue(firstMachine.Trigger("Start", source: "Console").Success);
        Assert.IsFalse(secondMachine.Trigger("Start", source: "Console").Success);
        Assert.AreEqual("Running", firstMachine.CurrentState);
        Assert.AreEqual("Idle", secondMachine.CurrentState);
        Assert.AreEqual(1, firstSession.Events.Count);
        Assert.AreEqual(0, secondSession.Events.Count);
        Assert.AreSame(firstMachine, firstScope.ServiceProvider.GetRequiredService<StateMachine>());
    }

    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        string configurationPath = Path.Combine(AppContext.BaseDirectory, "StateMachineConfiguration.xml");
        services.AddSampleServices(configurationPath);
        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
    }
}
