namespace Avak.StateMachine.Core.Tests;

using Configuration;
using Extensions;

/// <summary>Verifies optional state-object initialization and transition behavior.</summary>
[TestClass]
public class StateClassTests
{
    /// <summary>Initial entry invokes the target's checks and action with an initialization context.</summary>
    [TestMethod]
    public void Constructor_InitialClassState_RunsEntryWithInitializationContext()
    {
        var fixture = new Fixture();
        var machine = fixture.CreateMachine();

        CollectionAssert.AreEqual(new[] { "A.CanEnter", "A.Enter" }, fixture.Events);
        Assert.AreEqual("A", machine.CurrentState);
        Assert.IsNotNull(fixture.InitialContext);
        Assert.IsTrue(fixture.InitialContext.IsInitialization);
        Assert.IsNull(fixture.InitialContext.FromState);
        Assert.IsNull(fixture.InitialContext.TriggerName);
        Assert.IsNull(fixture.InitialContext.TriggerSource);
        Assert.AreEqual("A", fixture.InitialContext.ToState);
        Assert.AreEqual(0, fixture.InitialContext.Data.Count);
    }

    /// <summary>Initialization stops on denial or a thrown state method and does not return a machine.</summary>
    /// <param name="failure">The initialization outcome to exercise.</param>
    [TestMethod]
    [DataRow("deny")]
    [DataRow("CanEnter")]
    [DataRow("Enter")]
    public void Constructor_InitialEntryFails_SurfacesFailure(string failure)
    {
        var fixture = new Fixture { Failure = failure == "deny" ? null : "A." + failure, Denial = failure == "deny" ? "A.CanEnter" : null };
        if (failure == "deny")
        {
            Assert.ThrowsExactly<TransitionDeniedException>(() => fixture.CreateMachine());
            CollectionAssert.AreEqual(new[] { "A.CanEnter" }, fixture.Events);
        }
        else
        {
            var exception = Assert.ThrowsExactly<StateExecutionException>(() => fixture.CreateMachine());
            Assert.AreEqual(failure, exception.MethodName);
            Assert.AreSame(fixture.Exception, exception.InnerException);
        }
    }

    /// <summary>All state classes are resolved before initial entry, and blank configured names are rejected.</summary>
    [TestMethod]
    public void Constructor_MissingOrBlankStateClass_FailsBeforeInitialEntry()
    {
        var fixture = new Fixture();
        fixture.Config.States["B"].StateClassName = "Missing";
        var missing = Assert.ThrowsExactly<StateMachineException>(() => fixture.CreateMachine());
        StringAssert.Contains(missing.Message, "Missing");
        Assert.AreEqual(0, fixture.Events.Count);

        fixture.Config.States["B"].StateClassName = " ";
        Assert.ThrowsExactly<StateMachineException>(() => fixture.CreateMachine());
        Assert.AreEqual(0, fixture.Events.Count);
    }

    /// <summary>XML state-class names are parsed and invalid blank names are rejected.</summary>
    [TestMethod]
    public void Loader_StateClassAttribute_ValidatesAndRetainsName()
    {
        var loader = new XmlConfigurationLoader();
        var config = loader.LoadFromXml("<StateMachine initialState='A'><States><State name='A' stateClass='First'/></States></StateMachine>");
        Assert.AreEqual("First", config.States["A"].StateClassName);
        var exception = Assert.ThrowsExactly<StateMachineException>(() =>
            loader.LoadFromXml("<StateMachine initialState='A'><States><State name='A' stateClass=' '/></States></StateMachine>"));
        StringAssert.Contains(exception.Message, "stateClass");
    }

    /// <summary>A state class can inherit the default permissive checks and no-op lifecycle methods.</summary>
    [TestMethod]
    public void Trigger_DefaultStateMethods_AllowTransition()
    {
        var config = new XmlConfigurationLoader().LoadFromXml("""
            <StateMachine initialState="A"><States>
              <State name="A" stateClass="Default"><Transitions><Transition trigger="Go" to="B" /></Transitions></State>
              <State name="B" stateClass="Default" />
            </States></StateMachine>
            """);
        var registry = new StateRegistry();
        registry.Register("Default", new DefaultState());
        var machine = new StateMachine(config, null, null, null, null, registry);

        Assert.IsTrue(machine.Trigger("Go").Success);
        Assert.AreEqual("B", machine.CurrentState);
    }

    /// <summary>Checks and lifecycle methods run in order with the correct visible current state.</summary>
    [TestMethod]
    public void Trigger_ClassStates_RunsOrderedChecksAndCallbacks()
    {
        var fixture = new Fixture();
        var machine = fixture.CreateMachine();
        fixture.Machine = machine;
        fixture.Events.Clear();
        var data = new Dictionary<string, object?> { ["id"] = 42 };

        var result = machine.Trigger("Go", "UI", data);

        Assert.IsTrue(result.Success);
        Assert.AreEqual("B", machine.CurrentState);
        CollectionAssert.AreEqual(new[]
        {
            "guard:A", "permission:A", "A.CanExit:A", "B.CanEnter:A",
            "leaving:A", "A.Exit:A", "entering:A", "B.Enter:B", "entered:B"
        }, fixture.Events);
        Assert.IsNotNull(fixture.LastContext);
        Assert.IsFalse(fixture.LastContext.IsInitialization);
        Assert.AreEqual("A", fixture.LastContext.FromState);
        Assert.AreEqual("B", fixture.LastContext.ToState);
        Assert.AreEqual("Go", fixture.LastContext.TriggerName);
        Assert.AreEqual("UI", fixture.LastContext.TriggerSource);
        Assert.AreSame(data, fixture.LastContext.Data);
    }

    /// <summary>A self-transition runs both exit and entry methods on the same state object.</summary>
    [TestMethod]
    public void Trigger_SelfTransition_RunsExitAndEntry()
    {
        var fixture = new Fixture();
        var machine = fixture.CreateMachine();
        fixture.Machine = machine;
        fixture.Events.Clear();

        var result = machine.Trigger("Again");

        Assert.IsTrue(result.Success);
        Assert.AreEqual("A", machine.CurrentState);
        CollectionAssert.AreEqual(new[] { "A.CanExit:A", "A.CanEnter:A", "A.Exit:A", "A.Enter:A" }, fixture.Events);
    }

    /// <summary>A class-backed state can transition to or from a state without a class.</summary>
    [TestMethod]
    public void Trigger_MixedStates_UsesOnlyConfiguredStateMethods()
    {
        var fixture = new Fixture();
        fixture.Config.States["B"].StateClassName = null;
        var machine = fixture.CreateMachine();
        fixture.Machine = machine;
        fixture.Events.Clear();

        Assert.IsTrue(machine.Trigger("Go", "UI").Success);
        Assert.IsTrue(machine.Trigger("Back").Success);
        Assert.AreEqual("A", machine.CurrentState);
        CollectionAssert.AreEqual(new[]
        {
            "guard:A", "permission:A", "A.CanExit:A", "leaving:A", "A.Exit:A", "entering:A", "entered:B",
            "A.CanEnter:B", "A.Enter:A"
        }, fixture.Events);
    }

    /// <summary>A state check returning false stops later stages and preserves the source state.</summary>
    /// <param name="stage">The state check that denies the transition.</param>
    [TestMethod]
    [DataRow("A.CanExit")]
    [DataRow("B.CanEnter")]
    public void Trigger_StateCheckDenies_StopsAndRestoresSource(string stage)
    {
        var fixture = new Fixture();
        var machine = fixture.CreateMachine();
        fixture.Machine = machine;
        fixture.Events.Clear();
        fixture.Denial = stage;

        var result = machine.Trigger("Go", "UI");

        Assert.IsFalse(result.Success);
        Assert.IsInstanceOfType<TransitionDeniedException>(result.Exception);
        Assert.AreEqual("A", machine.CurrentState);
        Assert.AreEqual(stage + ":A", fixture.Events[^1]);
        Assert.IsFalse(fixture.Events.Any(x => x.StartsWith("leaving:", StringComparison.Ordinal)));
    }

    /// <summary>A denied guard or permission prevents state methods from running.</summary>
    /// <param name="stage">The transition-specific check that rejects the request.</param>
    [TestMethod]
    [DataRow("guard")]
    [DataRow("permission")]
    public void Trigger_GuardOrPermissionDenies_DoesNotInvokeStateMethods(string stage)
    {
        var fixture = new Fixture();
        var machine = fixture.CreateMachine();
        fixture.Machine = machine;
        fixture.Events.Clear();
        fixture.Denial = stage;

        var result = machine.Trigger("Go", "UI");

        Assert.IsFalse(result.Success);
        Assert.IsInstanceOfType<TransitionDeniedException>(result.Exception);
        Assert.AreEqual("A", machine.CurrentState);
        Assert.AreEqual(stage + ":A", fixture.Events[^1]);
        Assert.IsFalse(fixture.Events.Any(x => x.StartsWith("A.", StringComparison.Ordinal) || x.StartsWith("B.", StringComparison.Ordinal)));
    }

    /// <summary>A state method exception stops later work, reports the cause, and restores the source.</summary>
    /// <param name="stage">The state lifecycle method that throws.</param>
    [TestMethod]
    [DataRow("A.CanExit")]
    [DataRow("B.CanEnter")]
    [DataRow("A.Exit")]
    [DataRow("B.Enter")]
    public void Trigger_StateMethodThrows_ReturnsFailureAndRestoresSource(string stage)
    {
        var fixture = new Fixture();
        var machine = fixture.CreateMachine();
        fixture.Machine = machine;
        fixture.Events.Clear();
        fixture.Failure = stage;

        var result = machine.Trigger("Go", "UI");

        Assert.IsFalse(result.Success);
        Assert.IsInstanceOfType<StateExecutionException>(result.Exception);
        Assert.AreSame(fixture.Exception, result.Exception.InnerException);
        Assert.AreEqual("A", machine.CurrentState);
        Assert.IsTrue(fixture.Events[^1].StartsWith(stage + ":", StringComparison.Ordinal));
    }

    /// <summary>A callback failure after state entry restores the source but does not undo state-object work.</summary>
    [TestMethod]
    public void Trigger_CallbackFailsAfterStateEntry_RestoresSourceWithoutUndoingStateWork()
    {
        var fixture = new Fixture();
        var machine = fixture.CreateMachine();
        fixture.Machine = machine;
        fixture.Events.Clear();
        fixture.Failure = "entered";

        var result = machine.Trigger("Go", "UI");

        Assert.IsFalse(result.Success);
        Assert.IsInstanceOfType<CallbackException>(result.Exception);
        Assert.AreSame(fixture.Exception, result.Exception.InnerException);
        Assert.AreEqual("A", machine.CurrentState);
        CollectionAssert.Contains(fixture.Events, "B.Enter:B");
        Assert.AreEqual("entered:B", fixture.Events[^1]);
    }

    /// <summary>An ignored nested request from any state method aborts the outer transition.</summary>
    /// <param name="stage">The state lifecycle method making the nested request.</param>
    [TestMethod]
    [DataRow("A.CanExit")]
    [DataRow("B.CanEnter")]
    [DataRow("A.Exit")]
    [DataRow("B.Enter")]
    public void Trigger_NestedRequestInStateMethod_FailsOuterAndRestoresSource(string stage)
    {
        var fixture = new Fixture();
        var machine = fixture.CreateMachine();
        fixture.Machine = machine;
        fixture.Events.Clear();
        fixture.Nested = stage;

        var result = machine.Trigger("Go", "UI");

        Assert.IsFalse(result.Success);
        Assert.IsInstanceOfType<NestedRequestException>(result.Exception);
        Assert.AreEqual("A", machine.CurrentState);
        Assert.IsTrue(fixture.Events[^1].StartsWith(stage + ":", StringComparison.Ordinal));
        fixture.Nested = null;
        Assert.IsTrue(machine.Trigger("Go", "UI").Success);
    }

    private sealed class Fixture
    {
        public readonly List<string> Events = [];
        public readonly InvalidOperationException Exception = new("State failed");
        public StateMachine? Machine;
        public StateContext? InitialContext;
        public StateContext? LastContext;
        public string? Denial;
        public string? Failure;
        public string? Nested;
        public StateMachineConfiguration Config { get; } = new XmlConfigurationLoader().LoadFromXml("""
            <StateMachine initialState="A">
              <States>
                <State name="A" stateClass="First">
                  <Transitions>
                    <Transition trigger="Go" source="UI" to="B" guard="guard" permission="permission"
                                onLeaving="leaving" onEntering="entering" onEntered="entered" />
                    <Transition trigger="Again" to="A" />
                  </Transitions>
                </State>
                <State name="B" stateClass="Second">
                  <Transitions><Transition trigger="Back" to="A" /></Transitions>
                </State>
              </States>
            </StateMachine>
            """);

        public StateMachine CreateMachine()
        {
            var states = new StateRegistry();
            states.Register("First", new TestState("A", this));
            states.Register("Second", new TestState("B", this));
            var guards = new GuardRegistry();
            guards.Register("guard", new TestGuard(this));
            var permissions = new PermissionRegistry();
            permissions.Register("permission", new TestPermission(this));
            var callbacks = new CallbackRegistry();
            foreach (string name in new[] { "leaving", "entering", "entered" })
            {
                callbacks.Register(name, new TestCallback(this, name));
            }
            return new StateMachine(Config, guards, permissions, callbacks, null, states);
        }

        public bool Visit(string stage, StateContext context)
        {
            if (context.IsInitialization)
            {
                InitialContext = context;
                Events.Add(stage);
            }
            else
            {
                LastContext = context;
                Events.Add($"{stage}:{Machine!.CurrentState}");
            }
            if (Nested == stage)
            {
                Machine!.Trigger("Again");
            }
            if (Failure == stage)
            {
                throw Exception;
            }
            return Denial != stage;
        }
    }

    private sealed class TestState(string name, Fixture fixture) : State
    {
        public override bool CanEnter(StateContext context) => fixture.Visit(name + ".CanEnter", context);
        public override bool CanExit(StateContext context) => fixture.Visit(name + ".CanExit", context);
        public override void Enter(StateContext context) => fixture.Visit(name + ".Enter", context);
        public override void Exit(StateContext context) => fixture.Visit(name + ".Exit", context);
    }

    private sealed class DefaultState : State { }

    private sealed class TestGuard(Fixture fixture) : IGuard
    {
        public bool Evaluate(TransitionContext context)
        {
            fixture.Events.Add($"guard:{fixture.Machine!.CurrentState}");
            return fixture.Denial != "guard";
        }
    }

    private sealed class TestPermission(Fixture fixture) : IPermission
    {
        public bool IsAllowed(TransitionContext context)
        {
            fixture.Events.Add($"permission:{fixture.Machine!.CurrentState}");
            return fixture.Denial != "permission";
        }
    }

    private sealed class TestCallback(Fixture fixture, string name) : ITransitionCallback
    {
        public void Execute(TransitionContext context)
        {
            fixture.Events.Add($"{name}:{fixture.Machine!.CurrentState}");
            if (fixture.Failure == name)
            {
                throw fixture.Exception;
            }
        }
    }
}
