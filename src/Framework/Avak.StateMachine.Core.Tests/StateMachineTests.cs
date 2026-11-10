namespace Avak.StateMachine.Core.Tests;

using Configuration;
using Extensions;

[TestClass]
public class StateMachineTests
{
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

    [TestMethod]
    public void Constructor_InitializesWithInitialState()
    {
        var config = CreateSimpleConfig();
        var machine = new StateMachine(config);

        Assert.AreEqual("Idle", machine.CurrentState);
    }

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
