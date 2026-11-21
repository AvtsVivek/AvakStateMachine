namespace WinFormsSelfTransitionNetworkConnection.Tests;

using Avak.StateMachine.Core;

/// <summary>Tests the fake connection workflow without opening a form or using a network.</summary>
[TestClass]
public class ConnectionWorkflowTests
{
    /// <summary>Connect opens the fake connection, and Disconnect closes it and restores the named state.</summary>
    [TestMethod]
    public void ConnectAndDisconnect_ChangeStateAndFakeConnection()
    {
        var workflow = new ConnectionWorkflow();
        Assert.AreEqual("Disconnected", workflow.CurrentState);
        Assert.IsFalse(workflow.Connection.IsOpen);

        Assert.IsTrue(workflow.Connect().Success);
        Assert.AreEqual("Connected", workflow.CurrentState);
        Assert.IsTrue(workflow.Connection.IsOpen);
        Assert.IsTrue(workflow.Disconnect().Success);
        Assert.AreEqual("Disconnected", workflow.CurrentState);
        Assert.IsFalse(workflow.Connection.IsOpen);
        CollectionAssert.AreEqual(new[]
        {
            "CanEnter Connected", "Enter Connected", "Open connection",
            "CanExit Connected", "Exit Connected", "Close connection"
        }, workflow.Connection.Events.ToArray());
    }

    /// <summary>Refresh performs both sides of a self-transition while keeping the state Connected.</summary>
    /// <remarks>The lifecycle trace confirms that checks precede the close and reopen actions.</remarks>
    [TestMethod]
    public void Refresh_WhenConnected_ExitsAndReentersConnected()
    {
        var workflow = new ConnectionWorkflow();
        Assert.IsTrue(workflow.Connect().Success);
        int previousEvents = workflow.Connection.Events.Count;

        var result = workflow.Refresh();

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Connected", result.FromState);
        Assert.AreEqual("Connected", result.ToState);
        Assert.AreEqual("Connected", workflow.CurrentState);
        Assert.IsTrue(workflow.Connection.IsOpen);
        CollectionAssert.AreEqual(new[]
        {
            "CanExit Connected", "CanEnter Connected",
            "Exit Connected", "Close connection", "Enter Connected", "Open connection"
        }, workflow.Connection.Events.Skip(previousEvents).ToArray());
    }

    /// <summary>Clearing the event history preserves the connection and allows new events to be recorded.</summary>
    [TestMethod]
    public void ClearEvents_RemovesHistoryWithoutChangingConnection()
    {
        var workflow = new ConnectionWorkflow();
        Assert.IsTrue(workflow.Connect().Success);
        Assert.IsTrue(workflow.Connection.Events.Count > 0);

        workflow.Connection.ClearEvents();

        Assert.AreEqual(0, workflow.Connection.Events.Count);
        Assert.AreEqual("Connected", workflow.CurrentState);
        Assert.IsTrue(workflow.Connection.IsOpen);
        Assert.IsTrue(workflow.Disconnect().Success);
        CollectionAssert.AreEqual(
            new[] { "CanExit Connected", "Exit Connected", "Close connection" },
            workflow.Connection.Events.ToArray());
    }

    /// <summary>An unavailable connection denies Connect before any open action.</summary>
    [TestMethod]
    public void Connect_WhenUnavailable_IsDeniedWithoutOpeningConnection()
    {
        var workflow = new ConnectionWorkflow();
        workflow.Connection.IsAvailable = false;

        var result = workflow.Connect();

        Assert.IsFalse(result.Success);
        Assert.IsInstanceOfType<TransitionDeniedException>(result.Exception);
        Assert.AreEqual("Disconnected", workflow.CurrentState);
        Assert.IsFalse(workflow.Connection.IsOpen);
        CollectionAssert.AreEqual(new[] { "CanEnter Connected" }, workflow.Connection.Events.ToArray());
    }

    /// <summary>A denied self-transition leaves the existing fake connection open without executing exit or entry.</summary>
    [TestMethod]
    public void Refresh_WhenUnavailable_IsDeniedBeforeClosingConnection()
    {
        var workflow = new ConnectionWorkflow();
        Assert.IsTrue(workflow.Connect().Success);
        workflow.Connection.IsAvailable = false;
        int previousEvents = workflow.Connection.Events.Count;

        var result = workflow.Refresh();

        Assert.IsFalse(result.Success);
        Assert.IsInstanceOfType<TransitionDeniedException>(result.Exception);
        Assert.AreEqual("Connected", workflow.CurrentState);
        Assert.IsTrue(workflow.Connection.IsOpen);
        CollectionAssert.AreEqual(
            new[] { "CanExit Connected", "CanEnter Connected" },
            workflow.Connection.Events.Skip(previousEvents).ToArray());
    }

    /// <summary>Refresh while disconnected is rejected without opening the fake connection.</summary>
    [TestMethod]
    public void Refresh_WhenDisconnected_IsRejectedWithoutChangingState()
    {
        var workflow = new ConnectionWorkflow();

        var result = workflow.Refresh();

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Disconnected", workflow.CurrentState);
        Assert.IsFalse(workflow.Connection.IsOpen);
        Assert.AreEqual(0, workflow.Connection.Events.Count);
    }
}
