namespace Avak.StateMachine.Core.Tests;

using Configuration;
using Extensions;

/// <summary>
/// Tests XML configuration loading, reference validation, and named extension metadata.
/// </summary>
/// <remarks>
/// Each test supplies an in-memory XML document; no configuration files or external
/// resources are required. These tests inspect configuration rather than execute transitions.
/// </remarks>
[TestClass]
public class ConfigurationLoaderTests
{
    private readonly IConfigurationLoader _loader = new XmlConfigurationLoader();

    /// <summary>
    /// Verifies that a valid two-state document preserves its initial state and both state definitions.
    /// </summary>
    /// <remarks>
    /// The document defines Start and Stop transitions between Idle and Running.
    /// Assertions check the initial-state name, state count, and presence of both states.
    /// </remarks>
    [TestMethod]
    public void LoadFromXml_ValidConfiguration_LoadsSuccessfully()
    {
        string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<StateMachine initialState=""Idle"">
  <States>
    <State name=""Idle"">
      <Transitions>
        <Transition trigger=""Start"" to=""Running"" />
      </Transitions>
    </State>
    <State name=""Running"">
      <Transitions>
        <Transition trigger=""Stop"" to=""Idle"" />
      </Transitions>
    </State>
  </States>
</StateMachine>";

        var config = _loader.LoadFromXml(xml);

        Assert.AreEqual("Idle", config.InitialState);
        Assert.AreEqual(2, config.States.Count);
        Assert.IsTrue(config.States.ContainsKey("Idle"));
        Assert.IsTrue(config.States.ContainsKey("Running"));
    }

    /// <summary>
    /// Verifies that omitting the required initialState attribute rejects the configuration.
    /// </summary>
    /// <remarks>
    /// A state is present so that the missing initial-state declaration is the failure under test.
    /// The loader must throw <see cref="StateMachineException"/> with a message naming the attribute.
    /// </remarks>
    [TestMethod]
    public void LoadFromXml_NoInitialState_ThrowsException()
    {
        string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<StateMachine>
  <States>
    <State name=""Idle"" />
  </States>
</StateMachine>";

        try
        {
            _loader.LoadFromXml(xml);
            Assert.Fail("Expected StateMachineException");
        }
        catch (StateMachineException ex)
        {
            StringAssert.Contains(ex.Message, "initialState");
        }
    }

    /// <summary>
    /// Verifies that an initial-state name must reference a state defined in the document.
    /// </summary>
    /// <remarks>
    /// The document declares only Idle but selects NonExistent as its initial state.
    /// The loader must throw <see cref="StateMachineException"/> with an initial-state diagnostic.
    /// </remarks>
    [TestMethod]
    public void LoadFromXml_InvalidInitialState_ThrowsException()
    {
        string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<StateMachine initialState=""NonExistent"">
  <States>
    <State name=""Idle"" />
  </States>
</StateMachine>";

        try
        {
            _loader.LoadFromXml(xml);
            Assert.Fail("Expected StateMachineException");
        }
        catch (StateMachineException ex)
        {
            StringAssert.Contains(ex.Message, "InitialState");
        }
    }

    /// <summary>
    /// Verifies that a transition cannot reference an undefined target state.
    /// </summary>
    /// <remarks>
    /// Idle is a valid initial state, but its Start transition targets NonExistent.
    /// The loader must throw <see cref="StateMachineException"/> with a missing-state diagnostic.
    /// </remarks>
    [TestMethod]
    public void LoadFromXml_TransitionToNonExistentState_ThrowsException()
    {
        string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<StateMachine initialState=""Idle"">
  <States>
    <State name=""Idle"">
      <Transitions>
        <Transition trigger=""Start"" to=""NonExistent"" />
      </Transitions>
    </State>
  </States>
</StateMachine>";

        try
        {
            _loader.LoadFromXml(xml);
            Assert.Fail("Expected StateMachineException");
        }
        catch (StateMachineException ex)
        {
            StringAssert.Contains(ex.Message, "not found");
        }
    }

    /// <summary>
    /// Verifies that guard and permission names are retained on the parsed transition.
    /// </summary>
    /// <remarks>
    /// The Start transition references CheckReady and AllowStart. Assertions inspect those
    /// names only; resolving or executing the named implementations is outside this test.
    /// </remarks>
    [TestMethod]
    public void LoadFromXml_WithGuardsAndPermissions_ParsedCorrectly()
    {
        string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<StateMachine initialState=""Idle"">
  <States>
    <State name=""Idle"">
      <Transitions>
        <Transition trigger=""Start"" to=""Running"" guard=""CheckReady"" permission=""AllowStart"" />
      </Transitions>
    </State>
    <State name=""Running"" />
  </States>
</StateMachine>";

        var config = _loader.LoadFromXml(xml);
        var idleState = config.States["Idle"];
        var startTransition = idleState.Transitions["Start"][0];

        Assert.AreEqual("CheckReady", startTransition.GuardName);
        Assert.AreEqual("AllowStart", startTransition.PermissionName);
    }

    /// <summary>
    /// Verifies that all three lifecycle callback attributes map to their configuration properties.
    /// </summary>
    /// <remarks>
    /// Distinct names for onLeaving, onEntering, and onEntered make incorrect mappings visible.
    /// The test checks parsed names, not callback execution order.
    /// </remarks>
    [TestMethod]
    public void LoadFromXml_WithCallbacks_ParsedCorrectly()
    {
        string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<StateMachine initialState=""Idle"">
  <States>
    <State name=""Idle"">
      <Transitions>
        <Transition trigger=""Start"" to=""Running"" onLeaving=""LeaveIdle"" onEntering=""EnterRunning"" onEntered=""PostRunning"" />
      </Transitions>
    </State>
    <State name=""Running"" />
  </States>
</StateMachine>";

        var config = _loader.LoadFromXml(xml);
        var transition = config.States["Idle"].Transitions["Start"][0];

        Assert.AreEqual("LeaveIdle", transition.OnLeavingCallbackName);
        Assert.AreEqual("EnterRunning", transition.OnEnteringCallbackName);
        Assert.AreEqual("PostRunning", transition.OnEnteredCallbackName);
    }

    /// <summary>
    /// Verifies that multiple transitions for one trigger retain their individual source filters.
    /// </summary>
    /// <remarks>
    /// Resume has UI, API, and source-agnostic entries. Assertions check that all three
    /// entries remain in document order and that an omitted source is represented by null.
    /// </remarks>
    [TestMethod]
    public void LoadFromXml_WithSourceAwareness_ParsedCorrectly()
    {
        string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<StateMachine initialState=""Idle"">
  <States>
    <State name=""Idle"">
      <Transitions>
        <Transition trigger=""Resume"" source=""UI"" to=""Running"" />
        <Transition trigger=""Resume"" source=""API"" to=""Paused"" />
        <Transition trigger=""Resume"" to=""Running"" />
      </Transitions>
    </State>
    <State name=""Running"" />
    <State name=""Paused"" />
  </States>
</StateMachine>";

        var config = _loader.LoadFromXml(xml);
        var transitions = config.States["Idle"].Transitions["Resume"];

        Assert.AreEqual(3, transitions.Count);
        Assert.AreEqual("UI", transitions[0].Source);
        Assert.AreEqual("API", transitions[1].Source);
        Assert.IsNull(transitions[2].Source);
    }

    /// <summary>
    /// Verifies that XML containing a DTD declaration is rejected.
    /// </summary>
    /// <remarks>
    /// The input declares an external entity and must produce <see cref="StateMachineException"/>.
    /// The test does not depend on the referenced file existing or on any external resource.
    /// </remarks>
    [TestMethod]
    public void LoadFromXml_DTDProhibited_ThrowsException()
    {
        string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE foo [<!ENTITY xxe SYSTEM ""file:///etc/passwd"">]>
<StateMachine initialState=""Idle"">
  <States>
    <State name=""Idle"" />
  </States>
</StateMachine>";

        try
        {
            _loader.LoadFromXml(xml);
            Assert.Fail("Expected StateMachineException");
        }
        catch (StateMachineException)
        {
        }
    }
}
