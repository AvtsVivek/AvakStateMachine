namespace Avak.StateMachine.Core.Tests;

using Configuration;
using Extensions;
using Microsoft.Extensions.Logging;

/// <summary>
/// Verifies the state machine's structured logging behavior for successful,
/// rejected, failed, and nested trigger requests.
/// </summary>
/// <remarks>
/// Tests use a recording logger to inspect event levels, IDs, structured
/// properties, and attached exceptions. They also verify that logging does not
/// expose request data and that logger failures preserve machine state.
/// </remarks>
[TestClass]
public class LoggingTests
{
    /// <summary>
    /// Verifies that a successful transition emits one information-level
    /// outcome with structured transition fields, but does not log request data.
    /// </summary>
    /// <remarks>
    /// The test triggers <c>Start</c> from <c>Idle</c> using the <c>UI</c>
    /// source and includes a secret-like value in request data. It checks the
    /// success event ID, source and target states, trigger fields, absence of
    /// an exception, and that the supplied data value appears nowhere in the
    /// recorded log entry.
    /// </remarks>
    [TestMethod]
    public void SuccessfulTransition_LogsStructuredOutcomeWithoutRequestData()
    {
        var logger = new RecordingLogger();
        var machine = new StateMachine(CreateConfiguration(), logger: logger);

        var result = machine.Trigger("Start", source: "UI",
            data: new Dictionary<string, object?> { ["Secret"] = "do-not-log-this" });

        Assert.IsTrue(result.Success);
        var entry = logger.Entries.Single();
        Assert.AreEqual(LogLevel.Information, entry.Level);
        Assert.AreEqual("TransitionSucceeded", entry.EventId.Name);
        Assert.AreEqual("Start", entry.Fields["TriggerName"]);
        Assert.AreEqual("Idle", entry.Fields["FromState"]);
        Assert.AreEqual("Running", entry.Fields["ToState"]);
        Assert.AreEqual("UI", entry.Fields["TriggerSource"]);
        Assert.AreEqual(true, entry.Fields["Success"]);
        Assert.IsNull(entry.Exception);
        Assert.IsFalse(entry.Fields.ContainsKey("Secret"));
        Assert.IsFalse(entry.Message.Contains("do-not-log-this", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that a guard or permission denial emits a debug-level rejection
    /// and leaves the machine in its source state.
    /// </summary>
    /// <param name="extension">
    /// Selects whether the test denies the transition through a guard or a
    /// permission.
    /// </param>
    /// <remarks>
    /// Each data row configures exactly one denying extension on the
    /// <c>Idle</c>-to-<c>Running</c> transition. The assertions verify the
    /// returned failure, unchanged <c>Idle</c> state, rejection event, and
    /// identity between the result exception and the exception attached to
    /// the log record.
    /// </remarks>
    [TestMethod]
    [DataRow("Guard")]
    [DataRow("Permission")]
    public void DeniedTransition_LogsAtDebugAndPreservesState(string extension)
    {
        var configuration = CreateConfiguration();
        var transition = configuration.States["Idle"].Transitions["Start"][0];
        var guards = new GuardRegistry();
        var permissions = new PermissionRegistry();
        if (extension == "Guard")
        {
            transition.GuardName = "Deny";
            guards.Register("Deny", new DenyingGuard());
        }
        else
        {
            transition.PermissionName = "Deny";
            permissions.Register("Deny", new DenyingPermission());
        }
        var logger = new RecordingLogger();
        var machine = new StateMachine(configuration, guards, permissions, logger: logger);

        var result = machine.Trigger("Start", source: "UI");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Idle", machine.CurrentState);
        var entry = logger.Entries.Single();
        Assert.AreEqual(LogLevel.Debug, entry.Level);
        Assert.AreEqual("TransitionRejected", entry.EventId.Name);
        Assert.AreSame(result.Exception, entry.Exception);
        Assert.AreEqual("Idle", entry.Fields["ToState"]);
    }

    /// <summary>
    /// Verifies that an unknown trigger or a trigger with a nonmatching source
    /// emits a debug-level rejection.
    /// </summary>
    /// <param name="trigger">
    /// The attempted trigger, either unconfigured or configured for another source.
    /// </param>
    /// <param name="source">The source supplied with the trigger request.</param>
    /// <remarks>
    /// Both cases must fail without changing the state from <c>Idle</c>. The
    /// test also verifies that the logged structured fields identify the
    /// attempted trigger and source.
    /// </remarks>
    [TestMethod]
    [DataRow("Unknown", "UI")]
    [DataRow("Start", "OtherSource")]
    public void UnmatchedTransition_LogsRejection(string trigger, string source)
    {
        var logger = new RecordingLogger();
        var machine = new StateMachine(CreateConfiguration(), logger: logger);

        var result = machine.Trigger(trigger, source);

        Assert.IsFalse(result.Success);
        var entry = logger.Entries.Single();
        Assert.AreEqual(LogLevel.Debug, entry.Level);
        Assert.AreEqual("TransitionRejected", entry.EventId.Name);
        Assert.AreEqual(trigger, entry.Fields["TriggerName"]);
        Assert.AreEqual(source, entry.Fields["TriggerSource"]);
        Assert.AreEqual("Idle", machine.CurrentState);
    }

    /// <summary>
    /// Verifies that a lifecycle callback failure at any configured callback
    /// stage emits an error record after restoring the source state.
    /// </summary>
    /// <param name="stage">
    /// Selects whether the failing callback runs on leaving, entering, or
    /// entered.
    /// </param>
    /// <remarks>
    /// Each data row places the same throwing callback at a different stage of
    /// the transition. The test checks that the machine returns to
    /// <c>Idle</c>, the log is an error-level transition failure, the logged
    /// exception is the result exception, and the original callback exception
    /// remains its inner exception.
    /// </remarks>
    [TestMethod]
    [DataRow("OnLeaving")]
    [DataRow("OnEntering")]
    [DataRow("OnEntered")]
    public void CallbackFailure_LogsErrorAfterRestoringSourceState(string stage)
    {
        var configuration = CreateConfiguration();
        var transition = configuration.States["Idle"].Transitions["Start"][0];
        switch (stage)
        {
            case "OnLeaving":
                transition.OnLeavingCallbackName = "Fail";
                break;
            case "OnEntering":
                transition.OnEnteringCallbackName = "Fail";
                break;
            case "OnEntered":
                transition.OnEnteredCallbackName = "Fail";
                break;
        }
        var failure = new InvalidOperationException("Callback failed");
        var callbacks = new CallbackRegistry();
        callbacks.Register("Fail", new DelegateCallback(() => throw failure));
        var logger = new RecordingLogger();
        var machine = new StateMachine(configuration, callbackRegistry: callbacks, logger: logger);

        var result = machine.Trigger("Start", source: "UI");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Idle", machine.CurrentState);
        var entry = logger.Entries.Single();
        Assert.AreEqual(LogLevel.Error, entry.Level);
        Assert.AreEqual("TransitionFailed", entry.EventId.Name);
        Assert.AreSame(result.Exception, entry.Exception);
        Assert.AreSame(failure, entry.Exception?.InnerException);
        Assert.AreEqual("Idle", entry.Fields["FromState"]);
        Assert.AreEqual("Idle", entry.Fields["ToState"]);
    }

    /// <summary>
    /// Verifies that an ignored nested trigger is logged once when detected,
    /// while still causing the outer transition to fail and roll back.
    /// </summary>
    /// <remarks>
    /// The callback attempts a nested <c>Start</c> from source
    /// <c>Callback</c> after the outer transition provisionally enters
    /// <c>Running</c>, then ignores the nested result. The assertions verify
    /// the warning event's nested-request fields, confirm only one log entry
    /// is produced instead of a duplicate outer warning, and ensure the outer
    /// request fails with the machine restored to <c>Idle</c>.
    /// </remarks>
    [TestMethod]
    public void IgnoredNestedRequest_LogsNestedAttemptOnceAndRejectsOuterTransition()
    {
        var configuration = CreateConfiguration();
        configuration.States["Idle"].Transitions["Start"][0].OnEnteredCallbackName = "Nest";
        StateMachine? machine = null;
        var callbacks = new CallbackRegistry();
        callbacks.Register("Nest", new DelegateCallback(() => machine!.Trigger("Start", source: "Callback")));
        var logger = new RecordingLogger();
        machine = new StateMachine(configuration, callbackRegistry: callbacks, logger: logger);

        var result = machine.Trigger("Start", source: "UI");

        Assert.IsFalse(result.Success);
        Assert.IsInstanceOfType<NestedRequestException>(result.Exception);
        Assert.AreEqual("Idle", machine.CurrentState);
        var entry = logger.Entries.Single();
        Assert.AreEqual(LogLevel.Warning, entry.Level);
        Assert.AreEqual("NestedRequestDetected", entry.EventId.Name);
        Assert.AreEqual("Start", entry.Fields["TriggerName"]);
        Assert.AreEqual("Callback", entry.Fields["TriggerSource"]);
        Assert.AreEqual("Running", entry.Fields["CurrentState"]);
        Assert.AreSame(result.Exception, entry.Exception);
    }

    /// <summary>
    /// Verifies that a logger failure while recording a nested request is
    /// surfaced to the caller and the outer transition's source state is restored.
    /// </summary>
    /// <remarks>
    /// The callback makes one nested request, and the recording logger throws
    /// while writing its warning. The test checks that the exact logger
    /// exception reaches the caller, the machine rolls back to <c>Idle</c>,
    /// and the machine can complete a later transition after the logger stops
    /// throwing.
    /// </remarks>
    [TestMethod]
    public void NestedRequestLoggingFailure_IsSurfacedAndRestoresOuterSourceState()
    {
        var configuration = CreateConfiguration();
        configuration.States["Idle"].Transitions["Start"][0].OnEnteredCallbackName = "Nest";
        StateMachine? machine = null;
        bool nestedAttempted = false;
        var callbacks = new CallbackRegistry();
        callbacks.Register("Nest", new DelegateCallback(() =>
        {
            if (!nestedAttempted)
            {
                nestedAttempted = true;
                machine!.Trigger("Start", source: "Callback");
            }
        }));
        var failure = new InvalidOperationException("Logging failed");
        var logger = new RecordingLogger { Failure = failure };
        machine = new StateMachine(configuration, callbackRegistry: callbacks, logger: logger);

        var actual = Assert.ThrowsExactly<InvalidOperationException>(
            () => machine.Trigger("Start", source: "UI"));

        Assert.AreSame(failure, actual);
        Assert.AreEqual("Idle", machine.CurrentState);
        logger.Failure = null;
        Assert.IsTrue(machine.Trigger("Start", source: "UI").Success);
    }

    /// <summary>
    /// Verifies that a logger failure while recording a normal transition
    /// outcome is surfaced and does not leave the machine in a changed state.
    /// </summary>
    /// <remarks>
    /// The logger throws when the successful transition is logged. The test
    /// checks that the same exception reaches the caller, the provisional
    /// transition is rolled back to <c>Idle</c>, and a subsequent trigger
    /// succeeds once logging is restored.
    /// </remarks>
    [TestMethod]
    public void ThrowingLogger_RestoresStateAndSurfacesFailure()
    {
        var failure = new InvalidOperationException("Logging failed");
        var logger = new RecordingLogger { Failure = failure };
        var machine = new StateMachine(CreateConfiguration(), logger: logger);

        var actual = Assert.ThrowsExactly<InvalidOperationException>(
            () => machine.Trigger("Start", source: "UI"));

        Assert.AreSame(failure, actual);
        Assert.AreEqual("Idle", machine.CurrentState);
        logger.Failure = null;
        Assert.IsTrue(machine.Trigger("Start", source: "UI").Success);
    }

    private static StateMachineConfiguration CreateConfiguration()
    {
        return new XmlConfigurationLoader().LoadFromXml("""
            <StateMachine initialState="Idle">
              <States>
                <State name="Idle">
                  <Transitions>
                    <Transition trigger="Start" source="UI" to="Running" />
                  </Transitions>
                </State>
                <State name="Running" />
              </States>
            </StateMachine>
            """);
    }

    private sealed class DenyingGuard : IGuard
    {
        public bool Evaluate(TransitionContext context) => false;
    }

    private sealed class DenyingPermission : IPermission
    {
        public bool IsAllowed(TransitionContext context) => false;
    }

    private sealed class DelegateCallback(Action execute) : ITransitionCallback
    {
        public void Execute(TransitionContext context) => execute();
    }

    private sealed record LogEntry(
        LogLevel Level, EventId EventId, Dictionary<string, object?> Fields,
        Exception? Exception, string Message);

    private sealed class RecordingLogger : ILogger<StateMachine>
    {
        public List<LogEntry> Entries { get; } = [];
        public Exception? Failure { get; set; }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (Failure is not null)
            {
                throw Failure;
            }

            var fields = ((IEnumerable<KeyValuePair<string, object?>>)state!)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            Entries.Add(new LogEntry(logLevel, eventId, fields, exception, formatter(state, exception)));
        }
    }
}
