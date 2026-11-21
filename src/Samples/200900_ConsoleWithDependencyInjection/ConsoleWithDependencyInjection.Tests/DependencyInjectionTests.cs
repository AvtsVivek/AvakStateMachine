namespace ConsoleWithDependencyInjection.Tests;

using Avak.StateMachine.Core;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Tests the dependency-injection sample's guard, callback, and scoped-service behavior.
/// </summary>
/// <remarks>
/// Each test builds an isolated service provider using the sample registrations.
/// No console process, network service, or external device is required.
/// </remarks>
[TestClass]
public class DependencyInjectionTests
{
    /// <summary>
    /// Verifies that the registered guard denies Start when the scoped session is not ready.
    /// </summary>
    /// <remarks>
    /// The test resolves the machine and session data from one scope without
    /// setting <see cref="SessionData.IsReady"/>. It checks the denial result,
    /// unchanged Idle state, and absence of callback-recorded events.
    /// </remarks>
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

    /// <summary>
    /// Verifies that a ready scoped session allows Start and shares its data with the callback.
    /// </summary>
    /// <remarks>
    /// The test marks the scope's session ready, triggers Start, and checks the
    /// Running state and the transition event recorded by the callback in that
    /// same session's event list.
    /// </remarks>
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

    /// <summary>
    /// Verifies that separate DI scopes receive independent machines and session data.
    /// </summary>
    /// <remarks>
    /// Only the first scope's session is marked ready. The first machine must
    /// transition and record an event while the second remains Idle without
    /// events. The test also checks repeated resolution returns the same machine
    /// instance within a scope.
    /// </remarks>
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
