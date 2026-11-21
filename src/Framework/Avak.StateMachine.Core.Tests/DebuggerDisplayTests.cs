namespace Avak.StateMachine.Core.Tests;

using System.Diagnostics;
using System.Reflection;
using Configuration;
using Extensions;

/// <summary>
/// Tests debugger summaries that require calculated or conditional values.
/// </summary>
[TestClass]
public class DebuggerDisplayTests
{
    /// <summary>
    /// Verifies that simple debugger displays reference the intended public properties.
    /// </summary>
    /// <remarks>
    /// The test inspects the attributes on the configuration, transition context,
    /// result, and machine types. For each type, it checks that every listed
    /// property appears in the display expression, including the nested state count.
    /// </remarks>
    [TestMethod]
    public void SimpleDisplays_ReferenceRequestedProperties()
    {
        (Type Type, string[] Properties)[] cases =
        [
            (typeof(StateMachineConfiguration), ["InitialState", "States.Count"]),
            (typeof(TransitionContext), ["FromState", "ToState", "TriggerName", "TriggerSource"]),
            (typeof(TransitionResult), ["Success", "FromState", "ToState"]),
            (typeof(StateMachine), ["CurrentState"])
        ];

        foreach (var (type, properties) in cases)
        {
            var display = type.GetCustomAttribute<DebuggerDisplayAttribute>();
            Assert.IsNotNull(display, $"Missing DebuggerDisplay on {type.Name}.");
            foreach (string property in properties)
            {
                Assert.IsTrue(
                    display.Value.Contains($"{{{property}}}") || display.Value.Contains($"{{{property},nq}}"),
                    $"{type.Name} display does not evaluate {property}.");
            }
        }
    }

    /// <summary>
    /// Verifies that a state's debugger display count includes transitions in all trigger groups.
    /// </summary>
    /// <remarks>
    /// The test creates two trigger groups containing three transitions total,
    /// reads the internal calculated transition-count property, then removes the
    /// transitions in one group and checks that the count updates to one.
    /// </remarks>
    [TestMethod]
    public void StateDisplay_CountsTransitionsAcrossTriggerGroups()
    {
        var state = new StateConfiguration
        {
            Name = "Idle",
            Transitions = new()
            {
                ["Start"] = [new(), new()],
                ["Stop"] = [new()]
            }
        };

        var display = typeof(StateConfiguration).GetCustomAttribute<DebuggerDisplayAttribute>();
        Assert.IsNotNull(display);
        Assert.AreEqual("Name = {Name,nq}, Transitions = {TransitionCount}", display.Value);
        var countProperty = typeof(StateConfiguration).GetProperty("TransitionCount", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(countProperty);
        Assert.AreEqual(3, countProperty.GetValue(state));

        state.Transitions["Start"].Clear();
        Assert.AreEqual(1, countProperty.GetValue(state));
    }

    /// <summary>
    /// Verifies that the transition debugger display includes non-null optional properties only.
    /// </summary>
    /// <remarks>
    /// The test first checks the display for a transition with only a target.
    /// It then sets each optional field and verifies its rendered value, including
    /// an explicitly empty source, before clearing selected fields and confirming
    /// null-valued properties are omitted.
    /// </remarks>
    [TestMethod]
    public void TransitionDisplay_IncludesOnlyNonNullOptionalProperties()
    {
        var transition = new TransitionConfiguration { ToState = "Running" };
        var display = typeof(TransitionConfiguration).GetCustomAttribute<DebuggerDisplayAttribute>();
        Assert.IsNotNull(display);
        Assert.AreEqual("{DebuggerDisplay,nq}", display.Value);
        var displayProperty = typeof(TransitionConfiguration).GetProperty("DebuggerDisplay", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(displayProperty);
        Assert.AreEqual("ToState = Running", displayProperty.GetValue(transition));

        transition.Source = "";
        transition.GuardName = "Ready";
        transition.PermissionName = "Allowed";
        transition.OnLeavingCallbackName = "Leave";
        transition.OnEnteringCallbackName = "Enter";
        transition.OnEnteredCallbackName = "Entered";
        Assert.AreEqual(
            "ToState = Running, Source = , GuardName = Ready, PermissionName = Allowed, " +
            "OnLeavingCallbackName = Leave, OnEnteringCallbackName = Enter, OnEnteredCallbackName = Entered",
            displayProperty.GetValue(transition));

        transition.GuardName = null;
        transition.OnEnteringCallbackName = null;
        Assert.AreEqual(
            "ToState = Running, Source = , PermissionName = Allowed, " +
            "OnLeavingCallbackName = Leave, OnEnteredCallbackName = Entered",
            displayProperty.GetValue(transition));
    }
}
