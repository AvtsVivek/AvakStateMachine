namespace WpfTrafficLightWithDI.Tests;

using Avak.StateMachine.Core;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Tests the WPF traffic-light sample's DI registrations and view-model behavior.
/// </summary>
/// <remarks>
/// Tests resolve the view model and its state-machine dependencies without
/// constructing a WPF window. Each test uses fresh service providers and scopes.
/// </remarks>
[TestClass]
public class TrafficLightDependencyInjectionTests
{
    /// <summary>
    /// Verifies that successive UI Next triggers cycle Red to Green to Yellow to Red.
    /// </summary>
    /// <remarks>
    /// The test resolves the view model and session from one DI scope, invokes
    /// Next three times, and checks each state and the transitions recorded by
    /// the injected callback in that same session.
    /// </remarks>
    [TestMethod]
    public void Next_WhenOperational_CyclesThroughTrafficLightAndRecordsTransitions()
    {
        using var provider = CreateProvider();
        using var scope = provider.CreateScope();
        var viewModel = scope.ServiceProvider.GetRequiredService<MainWindowViewModel>();
        var session = scope.ServiceProvider.GetRequiredService<TrafficLightSession>();

        Assert.AreEqual("Red", viewModel.CurrentState);
        Assert.IsTrue(viewModel.Next().Success);
        Assert.AreEqual("Green", viewModel.CurrentState);
        Assert.IsTrue(viewModel.Next().Success);
        Assert.AreEqual("Yellow", viewModel.CurrentState);
        Assert.IsTrue(viewModel.Next().Success);
        Assert.AreEqual("Red", viewModel.CurrentState);
        CollectionAssert.AreEqual(
            new[] { "Red -> Green", "Green -> Yellow", "Yellow -> Red" },
            session.TransitionHistory);
    }

    /// <summary>
    /// Verifies that the operational guard rejects Next from each traffic-light state when the signal is unavailable.
    /// </summary>
    /// <remarks>
    /// Each data row advances an operational signal to Red, Green, or Yellow,
    /// then marks the scoped session unavailable and attempts Next. Assertions
    /// check denial, preservation of the current state and light presentation,
    /// and that the callback does not record an event for the denied transition.
    /// </remarks>
    /// <param name="expectedState">The state from which the denied trigger is attempted.</param>
    /// <param name="successfulTransitions">The number of operational Next triggers needed to reach that state.</param>
    /// <param name="expectedRedOpacity">Expected opacity of the red light after denial.</param>
    /// <param name="expectedYellowOpacity">Expected opacity of the yellow light after denial.</param>
    /// <param name="expectedGreenOpacity">Expected opacity of the green light after denial.</param>
    [TestMethod]
    [DataRow("Red", 0, 1.0, 0.25, 0.25)]
    [DataRow("Green", 1, 0.25, 0.25, 1.0)]
    [DataRow("Yellow", 2, 0.25, 1.0, 0.25)]
    public void Next_WhenSignalNotOperational_IsDeniedWithoutChangingState(
        string expectedState,
        int successfulTransitions,
        double expectedRedOpacity,
        double expectedYellowOpacity,
        double expectedGreenOpacity)
    {
        using var provider = CreateProvider();
        using var scope = provider.CreateScope();
        var viewModel = scope.ServiceProvider.GetRequiredService<MainWindowViewModel>();
        var session = scope.ServiceProvider.GetRequiredService<TrafficLightSession>();

        for (int i = 0; i < successfulTransitions; i++)
        {
            Assert.IsTrue(viewModel.Next().Success);
        }

        Assert.AreEqual(expectedState, viewModel.CurrentState);
        int recordedTransitionsBeforeDenial = session.TransitionHistory.Count;
        viewModel.IsOperational = false;

        var result = viewModel.Next();

        Assert.IsFalse(result.Success);
        Assert.IsInstanceOfType<TransitionDeniedException>(result.Exception);
        Assert.AreEqual(expectedState, viewModel.CurrentState);
        Assert.AreEqual(expectedRedOpacity, viewModel.RedOpacity);
        Assert.AreEqual(expectedYellowOpacity, viewModel.YellowOpacity);
        Assert.AreEqual(expectedGreenOpacity, viewModel.GreenOpacity);
        Assert.AreEqual(recordedTransitionsBeforeDenial, session.TransitionHistory.Count);
    }

    /// <summary>
    /// Verifies that separate DI scopes have independent machines and traffic-light sessions.
    /// </summary>
    /// <remarks>
    /// The first scope is marked unavailable and the second remains operational.
    /// The test checks that their view models, machine states, and history are
    /// independent, and that repeated machine resolution in one scope returns
    /// the same instance used by its view model.
    /// </remarks>
    [TestMethod]
    public void SeparateScopes_HaveIndependentMachinesAndSessions()
    {
        using var provider = CreateProvider();
        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();
        var firstViewModel = firstScope.ServiceProvider.GetRequiredService<MainWindowViewModel>();
        var secondViewModel = secondScope.ServiceProvider.GetRequiredService<MainWindowViewModel>();
        var firstSession = firstScope.ServiceProvider.GetRequiredService<TrafficLightSession>();
        var secondSession = secondScope.ServiceProvider.GetRequiredService<TrafficLightSession>();
        firstViewModel.IsOperational = false;

        Assert.IsFalse(firstViewModel.Next().Success);
        Assert.IsTrue(secondViewModel.Next().Success);
        Assert.AreEqual("Red", firstViewModel.CurrentState);
        Assert.AreEqual("Green", secondViewModel.CurrentState);
        Assert.AreEqual(0, firstSession.TransitionHistory.Count);
        CollectionAssert.AreEqual(new[] { "Red -> Green" }, secondSession.TransitionHistory);
        Assert.AreSame(
            secondViewModel,
            secondScope.ServiceProvider.GetRequiredService<MainWindowViewModel>());
    }

    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        string configurationPath = Path.Combine(AppContext.BaseDirectory, "StateMachineConfiguration.xml");
        services.AddWpfTrafficLightServices(configurationPath);
        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
    }
}
