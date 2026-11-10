namespace Avak.StateMachine.Core.Tests;

using Configuration;
using Extensions;

[TestClass]
public class ConfigurationLoaderTests
{
    private readonly IConfigurationLoader _loader = new XmlConfigurationLoader();

    [TestMethod]
    public void LoadFromXml_ValidConfiguration_LoadsSuccessfully()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
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

    [TestMethod]
    public void LoadFromXml_NoInitialState_ThrowsException()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
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

    [TestMethod]
    public void LoadFromXml_InvalidInitialState_ThrowsException()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
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

    [TestMethod]
    public void LoadFromXml_TransitionToNonExistentState_ThrowsException()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
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

    [TestMethod]
    public void LoadFromXml_WithGuardsAndPermissions_ParsedCorrectly()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
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

    [TestMethod]
    public void LoadFromXml_WithCallbacks_ParsedCorrectly()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
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

    [TestMethod]
    public void LoadFromXml_WithSourceAwareness_ParsedCorrectly()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
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

    [TestMethod]
    public void LoadFromXml_DTDProhibited_ThrowsException()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
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
