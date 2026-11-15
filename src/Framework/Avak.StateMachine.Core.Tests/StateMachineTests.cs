namespace Avak.StateMachine.Core.Tests;

using Configuration;
using Extensions;

/// <summary>
/// Tests runtime transition resolution, extension execution, failure results, and state preservation.
/// </summary>
/// <remarks>
/// Tests use in-memory configurations and local test doubles without devices or external services.
/// Each method documents its actual assertions rather than implying broader contract coverage.
/// </remarks>
[TestClass]
public class StateMachineTests
{
    /// <summary>
    /// Creates a fresh, unguarded Idle/Running/Paused configuration for an individual test.
    /// </summary>
    /// <returns>
    /// A configuration starting at Idle, with Start to Running, Stop to Idle,
    /// Pause to Paused, and Resume back to Running.
    /// </returns>
    private StateMachineConfiguration CreateSimpleConfig()
    {
        return new StateMachineConfiguration
        {
            InitialState = "Idle",
            States = new()
            {
                ["Idle"] = new StateConfiguration
                {
                    Name = "Idle",
                    Transitions = new()
                    {
                        ["Start"] =
                        [
                            new() { ToState = "Running" }
                        ]
                    }
                },
                ["Running"] = new StateConfiguration
                {
                    Name = "Running",
                    Transitions = new()
                    {
                        ["Stop"] =
                        [
                            new() { ToState = "Idle" }
                        ],
                        ["Pause"] =
                        [
                            new() { ToState = "Paused" }
                        ]
                    }
                },
                ["Paused"] = new StateConfiguration
                {
                    Name = "Paused",
                    Transitions = new()
                    {
                        ["Resume"] =
                        [
                            new() { ToState = "Running" }
                        ]
                    }
                }
            }
        };
    }

    /// <summary>
    /// Verifies that construction exposes the configured initial state before any trigger is issued.
    /// </summary>
    /// <remarks>
    /// The shared configuration selects Idle; the assertion checks CurrentState immediately
    /// after creating the machine.
    /// </remarks>
    [TestMethod]
    public void Constructor_InitializesWithInitialState()
    {
        var config = CreateSimpleConfig();
        var machine = new StateMachine(config);

        Assert.AreEqual("Idle", machine.CurrentState);
    }

    /// <summary>
    /// Verifies that a configured guard must be registered before the machine is created.
    /// </summary>
    /// <remarks>
    /// Start names an unregistered guard. The exception must identify its type, name,
    /// source state, and trigger.
    /// </remarks>
    [TestMethod]
    public void Constructor_UnresolvedGuard_Throws()
    {
        var config = CreateSimpleConfig();
        config.States["Idle"].Transitions["Start"][0].GuardName = "MissingGuard";

        var exception = Assert.ThrowsExactly<UnresolvedExtensionException>(
            () => new StateMachine(config));

        Assert.AreEqual("guard", exception.ExtensionType);
        Assert.AreEqual("MissingGuard", exception.ExtensionName);
        Assert.AreEqual("Idle", exception.StateName);
        Assert.AreEqual("Start", exception.TriggerName);
    }

    /// <summary>
    /// Verifies that permissions on any configured transition are resolved during construction.
    /// </summary>
    /// <remarks>
    /// Pause on the non-initial Running state names an unregistered permission.
    /// The exception must identify its type, name, source state, and trigger.
    /// </remarks>
    [TestMethod]
    public void Constructor_UnresolvedPermissionOnNonInitialState_Throws()
    {
        var config = CreateSimpleConfig();
        config.States["Running"].Transitions["Pause"][0].PermissionName = "MissingPermission";

        var exception = Assert.ThrowsExactly<UnresolvedExtensionException>(
            () => new StateMachine(config));

        Assert.AreEqual("permission", exception.ExtensionType);
        Assert.AreEqual("MissingPermission", exception.ExtensionName);
        Assert.AreEqual("Running", exception.StateName);
        Assert.AreEqual("Pause", exception.TriggerName);
    }

    /// <summary>
    /// Verifies that a configured lifecycle callback must be registered before machine creation.
    /// </summary>
    /// <remarks>
    /// Start names an unregistered onEntered callback. The exception must identify
    /// its type, name, source state, and trigger.
    /// </remarks>
    [TestMethod]
    public void Constructor_UnresolvedCallback_Throws()
    {
        var config = CreateSimpleConfig();
        config.States["Idle"].Transitions["Start"][0].OnEnteredCallbackName = "MissingCallback";

        var exception = Assert.ThrowsExactly<UnresolvedExtensionException>(
            () => new StateMachine(config));

        Assert.AreEqual("callback", exception.ExtensionType);
        Assert.AreEqual("MissingCallback", exception.ExtensionName);
        Assert.AreEqual("Idle", exception.StateName);
        Assert.AreEqual("Start", exception.TriggerName);
    }

    /// <summary>
    /// Verifies that a present but blank extension name is rejected instead of treated as optional.
    /// </summary>
    /// <remarks>
    /// Start names a whitespace-only guard. Construction must throw a
    /// <see cref="StateMachineException"/> with a diagnostic about the blank name.
    /// </remarks>
    [TestMethod]
    public void Constructor_BlankGuardName_Throws()
    {
        var config = CreateSimpleConfig();
        config.States["Idle"].Transitions["Start"][0].GuardName = " ";

        var exception = Assert.ThrowsExactly<StateMachineException>(
            () => new StateMachine(config));

        StringAssert.Contains(exception.Message, "guard name cannot be empty");
    }

    /// <summary>
    /// Verifies that Start moves Idle to Running and returns matching transition endpoints.
    /// </summary>
    /// <remarks>
    /// Assertions check success, the result's source and target names, and the machine's
    /// resulting current state.
    /// </remarks>
    [TestMethod]
    public void Trigger_ValidTransition_ChangesState()
    {
        var config = CreateSimpleConfig();
        var machine = new StateMachine(config);

        var result = machine.Trigger("Start");

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Idle", result.FromState);
        Assert.AreEqual("Running", result.ToState);
        Assert.AreEqual("Running", machine.CurrentState);
    }

    /// <summary>
    /// Verifies that an unknown trigger returns failure without leaving Idle.
    /// </summary>
    /// <remarks>
    /// InvalidTrigger has no configured transition. The result must report a missing
    /// transition diagnostic and the current state must remain unchanged.
    /// </remarks>
    [TestMethod]
    public void Trigger_InvalidTransition_FailsAndPreservesState()
    {
        var config = CreateSimpleConfig();
        var machine = new StateMachine(config);

        var result = machine.Trigger("InvalidTrigger");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Idle", machine.CurrentState);
        Assert.IsTrue(result.ErrorMessage!.Contains("No transition configured"));
    }

    /// <summary>
    /// Verifies that a trigger configured for another state is rejected in the current state.
    /// </summary>
    /// <remarks>
    /// Start first moves the machine to Running. Resume exists only on Paused,
    /// so issuing it from Running must fail and preserve Running.
    /// </remarks>
    [TestMethod]
    public void Trigger_TransitionFromWrongState_FailsAndPreservesState()
    {
        var config = CreateSimpleConfig();
        var machine = new StateMachine(config);
        machine.Trigger("Start");

        var result = machine.Trigger("Resume");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Running", machine.CurrentState);
    }

    /// <summary>
    /// Verifies source-specific routing and selection of the source-agnostic entry when no source is supplied.
    /// </summary>
    /// <remarks>
    /// Separate machines start at Idle for each case. Resume from UI reaches Running,
    /// Resume from API reaches Paused, and Resume without a source reaches Running.
    /// Every case checks success and the resulting state.
    /// </remarks>
    [TestMethod]
    public void Trigger_SourceAware_ResolvesCorrectTransition()
    {
        var config = new StateMachineConfiguration
        {
            InitialState = "Idle",
            States = new()
            {
                ["Idle"] = new StateConfiguration
                {
                    Name = "Idle",
                    Transitions = new()
                    {
                        ["Resume"] =
                        [
                            new() { ToState = "Running", Source = "UI" },
                            new() { ToState = "Paused", Source = "API" },
                            new() { ToState = "Running" }
                        ]
                    }
                },
                ["Running"] = new StateConfiguration { Name = "Running" },
                ["Paused"] = new StateConfiguration { Name = "Paused" }
            }
        };

        var machine = new StateMachine(config);

        var resultUI = machine.Trigger("Resume", source: "UI");
        Assert.IsTrue(resultUI.Success);
        Assert.AreEqual("Running", machine.CurrentState);

        machine = new StateMachine(config);
        var resultAPI = machine.Trigger("Resume", source: "API");
        Assert.IsTrue(resultAPI.Success);
        Assert.AreEqual("Paused", machine.CurrentState);

        machine = new StateMachine(config);
        var resultNoSource = machine.Trigger("Resume");
        Assert.IsTrue(resultNoSource.Success);
        Assert.AreEqual("Running", machine.CurrentState);
    }

    /// <summary>
    /// Verifies that a registered guard returning false denies Start and preserves Idle.
    /// </summary>
    /// <remarks>
    /// The transition references AlwaysDeny. Assertions check failure, unchanged state,
    /// a guard diagnostic, and a <see cref="TransitionDeniedException"/> in the result.
    /// </remarks>
    [TestMethod]
    public void Trigger_GuardDenies_FailsAndPreservesState()
    {
        var config = CreateSimpleConfig();
        var guardRegistry = new GuardRegistry();
        guardRegistry.Register("AlwaysDeny", new TestGuard(false));

        config.States["Idle"].Transitions["Start"][0].GuardName = "AlwaysDeny";

        var machine = new StateMachine(config, guardRegistry);

        var result = machine.Trigger("Start");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Idle", machine.CurrentState);
        Assert.IsTrue(result.ErrorMessage!.Contains("Guard"));
        Assert.IsTrue(result.Exception is TransitionDeniedException);
    }

    /// <summary>
    /// Verifies that a registered permission returning false denies Start and preserves Idle.
    /// </summary>
    /// <remarks>
    /// The transition references AlwaysDeny. Assertions check failure, unchanged state,
    /// a permission diagnostic, and a <see cref="TransitionDeniedException"/> in the result.
    /// </remarks>
    [TestMethod]
    public void Trigger_PermissionDenies_FailsAndPreservesState()
    {
        var config = CreateSimpleConfig();
        var permissionRegistry = new PermissionRegistry();
        permissionRegistry.Register("AlwaysDeny", new TestPermission(false));

        config.States["Idle"].Transitions["Start"][0].PermissionName = "AlwaysDeny";

        var machine = new StateMachine(config, permissionRegistry: permissionRegistry);

        var result = machine.Trigger("Start");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Idle", machine.CurrentState);
        Assert.IsTrue(result.ErrorMessage!.Contains("Permission"));
        Assert.IsTrue(result.Exception is TransitionDeniedException);
    }

    /// <summary>
    /// Verifies that Start succeeds when its named guard and permission both return true.
    /// </summary>
    /// <remarks>
    /// Both implementations are registered and attached to the transition.
    /// Assertions check a successful result and the resulting Running state.
    /// </remarks>
    [TestMethod]
    public void Trigger_GuardAndPermissionAllow_TransitionSucceeds()
    {
        var config = CreateSimpleConfig();
        var guardRegistry = new GuardRegistry();
        var permissionRegistry = new PermissionRegistry();

        guardRegistry.Register("AllowGuard", new TestGuard(true));
        permissionRegistry.Register("AllowPermission", new TestPermission(true));

        config.States["Idle"].Transitions["Start"][0].GuardName = "AllowGuard";
        config.States["Idle"].Transitions["Start"][0].PermissionName = "AllowPermission";

        var machine = new StateMachine(config, guardRegistry, permissionRegistry);

        var result = machine.Trigger("Start");

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Running", machine.CurrentState);
    }

    /// <summary>
    /// Verifies that Start invokes the callback named by onLeaving.
    /// </summary>
    /// <remarks>
    /// The callback sets a local flag, which is asserted after the trigger returns.
    /// This test checks invocation only, not ordering or state visibility inside the callback.
    /// </remarks>
    [TestMethod]
    public void Trigger_OnLeavingCallbackExecutes()
    {
        var config = CreateSimpleConfig();
        var callbackRegistry = new CallbackRegistry();
        bool callbackExecuted = false;

        callbackRegistry.Register("LeaveCallback", new TestCallback(() => callbackExecuted = true));
        config.States["Idle"].Transitions["Start"][0].OnLeavingCallbackName = "LeaveCallback";

        var machine = new StateMachine(config, callbackRegistry: callbackRegistry);
        machine.Trigger("Start");

        Assert.IsTrue(callbackExecuted);
    }

    /// <summary>
    /// Verifies that Start invokes the callback named by onEntering.
    /// </summary>
    /// <remarks>
    /// The callback sets a local flag, which is asserted after the trigger returns.
    /// This test checks invocation only, not ordering or state visibility inside the callback.
    /// </remarks>
    [TestMethod]
    public void Trigger_OnEnteringCallbackExecutes()
    {
        var config = CreateSimpleConfig();
        var callbackRegistry = new CallbackRegistry();
        bool callbackExecuted = false;

        callbackRegistry.Register("EnterCallback", new TestCallback(() => callbackExecuted = true));
        config.States["Idle"].Transitions["Start"][0].OnEnteringCallbackName = "EnterCallback";

        var machine = new StateMachine(config, callbackRegistry: callbackRegistry);
        machine.Trigger("Start");

        Assert.IsTrue(callbackExecuted);
    }

    /// <summary>
    /// Verifies that Start invokes the callback named by onEntered.
    /// </summary>
    /// <remarks>
    /// The callback sets a local flag, which is asserted after the trigger returns.
    /// This test checks invocation only, not ordering or state visibility inside the callback.
    /// </remarks>
    [TestMethod]
    public void Trigger_OnEnteredCallbackExecutes()
    {
        var config = CreateSimpleConfig();
        var callbackRegistry = new CallbackRegistry();
        bool callbackExecuted = false;

        callbackRegistry.Register("EnteredCallback", new TestCallback(() => callbackExecuted = true));
        config.States["Idle"].Transitions["Start"][0].OnEnteredCallbackName = "EnteredCallback";

        var machine = new StateMachine(config, callbackRegistry: callbackRegistry);
        machine.Trigger("Start");

        Assert.IsTrue(callbackExecuted);
    }

    /// <summary>
    /// Verifies that an exception in onLeaving fails Start and leaves the machine at Idle.
    /// </summary>
    /// <remarks>
    /// A callback deliberately throws <see cref="InvalidOperationException"/>.
    /// Assertions check failure, restored source state, a failure diagnostic,
    /// and a <see cref="CallbackException"/> in the result.
    /// </remarks>
    [TestMethod]
    public void Trigger_OnLeavingCallbackThrows_RollsBackAndPreservesState()
    {
        var config = CreateSimpleConfig();
        var callbackRegistry = new CallbackRegistry();

        callbackRegistry.Register("ThrowCallback", new TestCallback(() => throw new InvalidOperationException("Callback failed")));
        config.States["Idle"].Transitions["Start"][0].OnLeavingCallbackName = "ThrowCallback";

        var machine = new StateMachine(config, callbackRegistry: callbackRegistry);
        var result = machine.Trigger("Start");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Idle", machine.CurrentState);
        Assert.IsTrue(result.ErrorMessage!.Contains("Callback") || result.ErrorMessage!.Contains("failed"));
        Assert.IsTrue(result.Exception is CallbackException);
    }

    /// <summary>
    /// Verifies that an exception in onEntering fails Start and preserves its source state.
    /// </summary>
    /// <remarks>
    /// The named callback deliberately throws. The test checks a failed result and Idle
    /// as the final current state; it does not assert exception wrapping or external compensation.
    /// </remarks>
    [TestMethod]
    public void Trigger_OnEnteringCallbackThrows_RollsBackAndPreservesState()
    {
        var config = CreateSimpleConfig();
        var callbackRegistry = new CallbackRegistry();

        callbackRegistry.Register("ThrowCallback", new TestCallback(() => throw new InvalidOperationException("Callback failed")));
        config.States["Idle"].Transitions["Start"][0].OnEnteringCallbackName = "ThrowCallback";

        var machine = new StateMachine(config, callbackRegistry: callbackRegistry);
        var result = machine.Trigger("Start");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Idle", machine.CurrentState);
    }

    /// <summary>
    /// Verifies that an exception in onEntered fails Start and restores Idle.
    /// </summary>
    /// <remarks>
    /// This exercises failure in the post-entry callback. Assertions check the failed
    /// result and final source state, not reversal of any callback side effects.
    /// </remarks>
    [TestMethod]
    public void Trigger_OnEnteredCallbackThrows_RollsBackAndPreservesState()
    {
        var config = CreateSimpleConfig();
        var callbackRegistry = new CallbackRegistry();

        callbackRegistry.Register("ThrowCallback", new TestCallback(() => throw new InvalidOperationException("Callback failed")));
        config.States["Idle"].Transitions["Start"][0].OnEnteredCallbackName = "ThrowCallback";

        var machine = new StateMachine(config, callbackRegistry: callbackRegistry);
        var result = machine.Trigger("Start");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Idle", machine.CurrentState);
    }

    /// <summary>
    /// Verifies that an ignored nested request aborts the outer transition at every extension stage.
    /// </summary>
    /// <remarks>
    /// Each case attempts Pause during one stage of Start and ignores the nested result.
    /// Assertions check both failures, the restored Idle state, and that no later stages run.
    /// A subsequent Start without nesting must succeed.
    /// </remarks>
    /// <param name="stage">The guard, permission, or callback stage that attempts the nested request.</param>
    [TestMethod]
    [DataRow("guard")]
    [DataRow("permission")]
    [DataRow("leaving")]
    [DataRow("entering")]
    [DataRow("entered")]
    public void Trigger_NestedRequest_AbortsOuterTransition(string stage)
    {
        var config = CreateSimpleConfig();
        var transition = config.States["Idle"].Transitions["Start"][0];
        var guardRegistry = new GuardRegistry();
        var permissionRegistry = new PermissionRegistry();
        var callbackRegistry = new CallbackRegistry();
        StateMachine? machine = null;
        TransitionResult? nestedResult = null;
        bool attemptNested = true;
        var executed = new List<string>();
        void Visit(string currentStage)
        {
            executed.Add(currentStage);
            if (attemptNested && currentStage == stage)
            {
                nestedResult = machine!.Trigger("Pause");
            }
        }

        guardRegistry.Register("Guard", new DelegateGuard(() => { Visit("guard"); return true; }));
        permissionRegistry.Register("Permission", new DelegatePermission(() => { Visit("permission"); return true; }));
        callbackRegistry.Register("Leaving", new TestCallback(() => Visit("leaving")));
        callbackRegistry.Register("Entering", new TestCallback(() => Visit("entering")));
        callbackRegistry.Register("Entered", new TestCallback(() => Visit("entered")));
        transition.GuardName = "Guard";
        transition.PermissionName = "Permission";
        transition.OnLeavingCallbackName = "Leaving";
        transition.OnEnteringCallbackName = "Entering";
        transition.OnEnteredCallbackName = "Entered";
        machine = new StateMachine(config, guardRegistry, permissionRegistry, callbackRegistry);

        var result = machine.Trigger("Start");

        Assert.IsNotNull(nestedResult);
        Assert.IsFalse(nestedResult.Success);
        Assert.IsInstanceOfType<NestedRequestException>(nestedResult.Exception);
        Assert.IsFalse(result.Success);
        Assert.IsInstanceOfType<NestedRequestException>(result.Exception);
        Assert.AreEqual("Idle", result.FromState);
        Assert.AreEqual("Idle", result.ToState);
        Assert.AreEqual("Idle", machine.CurrentState);
        CollectionAssert.AreEqual(
            new[] { "guard", "permission", "leaving", "entering", "entered" }.Take(executed.Count).ToArray(),
            executed);
        Assert.AreEqual(stage, executed[^1]);

        attemptNested = false;
        result = machine.Trigger("Start");
        Assert.IsTrue(result.Success);
        Assert.AreEqual("Running", machine.CurrentState);
    }

    /// <summary>
    /// Verifies that catching a nested-request exception does not allow the outer transition to succeed.
    /// </summary>
    /// <remarks>
    /// The onEntered callback throws and catches the exception from its nested Pause result.
    /// Start must still fail with <see cref="NestedRequestException"/> and restore Idle.
    /// </remarks>
    [TestMethod]
    public void Trigger_NestedRequestCaughtByCallback_StillFails()
    {
        var config = CreateSimpleConfig();
        var callbacks = new CallbackRegistry();
        StateMachine? machine = null;
        callbacks.Register("Nested", new TestCallback(() =>
        {
            try
            {
                throw machine!.Trigger("Pause").Exception!;
            }
            catch (NestedRequestException)
            {
            }
        }));
        config.States["Idle"].Transitions["Start"][0].OnEnteredCallbackName = "Nested";
        machine = new StateMachine(config, callbackRegistry: callbacks);

        var result = machine.Trigger("Start");

        Assert.IsFalse(result.Success);
        Assert.IsInstanceOfType<NestedRequestException>(result.Exception);
        Assert.AreEqual("Idle", machine.CurrentState);
    }

    /// <summary>
    /// Verifies that a callback exception is reported when the callback also attempts a nested request.
    /// </summary>
    /// <remarks>
    /// The onEntered callback attempts Pause and then throws <see cref="InvalidOperationException"/>.
    /// Start must fail with a <see cref="CallbackException"/> containing that exception
    /// and restore Idle.
    /// </remarks>
    [TestMethod]
    public void Trigger_CallbackThrowsAfterNestedRequest_ReportsCallbackFailure()
    {
        var config = CreateSimpleConfig();
        var callbacks = new CallbackRegistry();
        StateMachine? machine = null;
        callbacks.Register("Nested", new TestCallback(() =>
        {
            machine!.Trigger("Pause");
            throw new InvalidOperationException("Callback failed");
        }));
        config.States["Idle"].Transitions["Start"][0].OnEnteredCallbackName = "Nested";
        machine = new StateMachine(config, callbackRegistry: callbacks);

        var result = machine.Trigger("Start");

        Assert.IsFalse(result.Success);
        Assert.IsInstanceOfType<CallbackException>(result.Exception);
        Assert.IsInstanceOfType<InvalidOperationException>(result.Exception.InnerException);
        Assert.AreEqual("Idle", machine.CurrentState);
    }

    /// <summary>
    /// Verifies that a lifecycle callback receives transition endpoints, trigger name, and caller data.
    /// </summary>
    /// <remarks>
    /// The onEntering callback captures its context during Start. Assertions check
    /// Idle, Running, Start, and the supplied Key1 value on the captured context.
    /// </remarks>
    [TestMethod]
    public void Trigger_ContextDataPassedToCallbacks()
    {
        var config = CreateSimpleConfig();
        var callbackRegistry = new CallbackRegistry();
        TransitionContext? capturedContext = null;

        callbackRegistry.Register("CaptureContext", new TestCallback(ctx => capturedContext = ctx));
        config.States["Idle"].Transitions["Start"][0].OnEnteringCallbackName = "CaptureContext";

        var machine = new StateMachine(config, callbackRegistry: callbackRegistry);
        var data = new Dictionary<string, object?> { ["Key1"] = "Value1" };

        machine.Trigger("Start", data: data);

        Assert.IsNotNull(capturedContext);
        Assert.AreEqual("Idle", capturedContext.FromState);
        Assert.AreEqual("Running", capturedContext.ToState);
        Assert.AreEqual("Start", capturedContext.TriggerName);
        Assert.AreEqual("Value1", capturedContext.Data["Key1"]);
    }

    private class TestGuard(bool allow) : IGuard
    {
        private readonly bool _allow = allow;

        public bool Evaluate(TransitionContext context) => _allow;
    }

    private class TestPermission(bool allow) : IPermission
    {
        private readonly bool _allow = allow;

        public bool IsAllowed(TransitionContext context) => _allow;
    }

    private class DelegateGuard(Func<bool> evaluate) : IGuard
    {
        public bool Evaluate(TransitionContext context) => evaluate();
    }

    private class DelegatePermission(Func<bool> evaluate) : IPermission
    {
        public bool IsAllowed(TransitionContext context) => evaluate();
    }

    private class TestCallback : ITransitionCallback
    {
        private readonly Action<TransitionContext>? _action;
        private readonly Action? _actionNoContext;

        public TestCallback(Action action)
        {
            _actionNoContext = action;
        }

        public TestCallback(Action<TransitionContext> action)
        {
            _action = action;
        }

        public void Execute(TransitionContext context)
        {
            _action?.Invoke(context);
            _actionNoContext?.Invoke();
        }
    }
}
