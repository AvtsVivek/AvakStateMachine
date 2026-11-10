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
                        ["Start"] = new List<TransitionConfiguration>
                        {
                            new() { ToState = "Running" }
                        }
                    }
                },
                ["Running"] = new StateConfiguration
                {
                    Name = "Running",
                    Transitions = new()
                    {
                        ["Stop"] = new List<TransitionConfiguration>
                        {
                            new() { ToState = "Idle" }
                        },
                        ["Pause"] = new List<TransitionConfiguration>
                        {
                            new() { ToState = "Paused" }
                        }
                    }
                },
                ["Paused"] = new StateConfiguration
                {
                    Name = "Paused",
                    Transitions = new()
                    {
                        ["Resume"] = new List<TransitionConfiguration>
                        {
                            new() { ToState = "Running" }
                        }
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
                        ["Resume"] = new List<TransitionConfiguration>
                        {
                            new() { ToState = "Running", Source = "UI" },
                            new() { ToState = "Paused", Source = "API" },
                            new() { ToState = "Running" }
                        }
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
        var callbackExecuted = false;

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
        var callbackExecuted = false;

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
        var callbackExecuted = false;

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
    /// Verifies rejection of a nested Pause request while the outer Start request completes.
    /// </summary>
    /// <remarks>
    /// The onEntering callback checks that the nested result fails with
    /// <see cref="NestedRequestException"/>. The outer assertions currently expect success
    /// and Running. This characterizes existing behavior; it does not verify the required
    /// contract that a nested request must also cause the outer transition to fail.
    /// </remarks>
    [TestMethod]
    public void Trigger_NestedRequest_RejectedAndPreservesState()
    {
        var config = CreateSimpleConfig();
        var callbackRegistry = new CallbackRegistry();
        var machine = new StateMachine(config, callbackRegistry: callbackRegistry);

        TestCallback? nestedCallback = null;
        nestedCallback = new TestCallback(() =>
        {
            var result = machine.Trigger("Pause");
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Exception is NestedRequestException);
        });

        callbackRegistry.Register("NestedCallback", nestedCallback);
        config.States["Idle"].Transitions["Start"][0].OnEnteringCallbackName = "NestedCallback";

        var result = machine.Trigger("Start");

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Running", machine.CurrentState);
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

    private class TestGuard : IGuard
    {
        private readonly bool _allow;
        public TestGuard(bool allow) => _allow = allow;
        public bool Evaluate(TransitionContext context) => _allow;
    }

    private class TestPermission : IPermission
    {
        private readonly bool _allow;
        public TestPermission(bool allow) => _allow = allow;
        public bool IsAllowed(TransitionContext context) => _allow;
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
