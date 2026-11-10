namespace Avak.StateMachine.Samples.ConsoleRecorder.Tests;

/// <summary>
/// Tests the recorder sample's initial state, valid lifecycle transitions, and invalid triggers.
/// </summary>
/// <remarks>
/// Each test creates a recorder using the sample XML configuration and checks its observable
/// state. These scenarios do not exercise real audio devices or assert guard and permission logic.
/// </remarks>
[TestClass]
public class RecorderStateMachineTests
{
    /// <summary>
    /// Verifies that a newly constructed recorder starts in Stopped.
    /// </summary>
    /// <remarks>
    /// No trigger is issued. The assertion checks that sample initialization uses
    /// the initial state declared in the XML configuration.
    /// </remarks>
    [TestMethod]
    public void RecorderInitializesInStoppedState()
    {
        var recorder = new RecorderStateMachine();
        Assert.AreEqual("Stopped", recorder.Machine.CurrentState);
    }

    /// <summary>
    /// Verifies that Start begins the recorder lifecycle from Stopped.
    /// </summary>
    /// <remarks>
    /// A fresh recorder receives Start. The result must succeed and the machine must
    /// report Recording; no actual audio capture is asserted.
    /// </remarks>
    [TestMethod]
    public void Start_FromStopped_TransitionsToRecording()
    {
        var recorder = new RecorderStateMachine();
        var result = recorder.Start();

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Recording", recorder.Machine.CurrentState);
    }

    /// <summary>
    /// Verifies that Pause moves an active recorder to Paused.
    /// </summary>
    /// <remarks>
    /// Start establishes Recording before Pause is issued.
    /// Assertions check Pause succeeds and the resulting state is Paused.
    /// </remarks>
    [TestMethod]
    public void Pause_FromRecording_TransitionsToPaused()
    {
        var recorder = new RecorderStateMachine();
        recorder.Start();

        var result = recorder.Pause();

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Paused", recorder.Machine.CurrentState);
    }

    /// <summary>
    /// Verifies that Resume returns a paused recorder to Recording.
    /// </summary>
    /// <remarks>
    /// Start followed by Pause establishes the source state.
    /// Assertions check Resume succeeds and the resulting state is Recording.
    /// </remarks>
    [TestMethod]
    public void Resume_FromPaused_TransitionsToRecording()
    {
        var recorder = new RecorderStateMachine();
        recorder.Start();
        recorder.Pause();

        var result = recorder.Resume();

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Recording", recorder.Machine.CurrentState);
    }

    /// <summary>
    /// Verifies that Stop returns an active recorder to Stopped.
    /// </summary>
    /// <remarks>
    /// Start establishes Recording before Stop is issued.
    /// Assertions check Stop succeeds and the final machine state is Stopped.
    /// </remarks>
    [TestMethod]
    public void Stop_FromRecording_TransitionsToStopped()
    {
        var recorder = new RecorderStateMachine();
        recorder.Start();

        var result = recorder.Stop();

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Stopped", recorder.Machine.CurrentState);
    }

    /// <summary>
    /// Verifies that Stop can end the recorder lifecycle directly from Paused.
    /// </summary>
    /// <remarks>
    /// Start and Pause establish Paused. Stop must succeed and reach Stopped
    /// without requiring an intervening Resume.
    /// </remarks>
    [TestMethod]
    public void Stop_FromPaused_TransitionsToStopped()
    {
        var recorder = new RecorderStateMachine();
        recorder.Start();
        recorder.Pause();

        var result = recorder.Stop();

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Stopped", recorder.Machine.CurrentState);
    }

    /// <summary>
    /// Verifies the sample's explicit Resume transition from Stopped to Recording.
    /// </summary>
    /// <remarks>
    /// Resume is issued on a fresh recorder, without a prior Start or Pause.
    /// The test checks this configured behavior by asserting success and Recording.
    /// </remarks>
    [TestMethod]
    public void Resume_FromStopped_TransitionsToRecording()
    {
        var recorder = new RecorderStateMachine();

        var result = recorder.Resume();

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Recording", recorder.Machine.CurrentState);
    }

    /// <summary>
    /// Verifies that Pause is rejected before recording has begun.
    /// </summary>
    /// <remarks>
    /// A fresh recorder has no Pause transition from Stopped.
    /// Assertions check a failed result and preservation of Stopped.
    /// </remarks>
    [TestMethod]
    public void Pause_FromStopped_FailsAndPreservesState()
    {
        var recorder = new RecorderStateMachine();

        var result = recorder.Pause();

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Stopped", recorder.Machine.CurrentState);
    }

    /// <summary>
    /// Verifies that Stop is rejected when the recorder is already Stopped.
    /// </summary>
    /// <remarks>
    /// Stop is issued without first starting a recording.
    /// The result must fail and the machine must remain Stopped.
    /// </remarks>
    [TestMethod]
    public void Stop_FromStopped_FailsAndPreservesState()
    {
        var recorder = new RecorderStateMachine();

        var result = recorder.Stop();

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Stopped", recorder.Machine.CurrentState);
    }

    /// <summary>
    /// Verifies that a second Start is rejected while the recorder is Recording.
    /// </summary>
    /// <remarks>
    /// The first Start establishes Recording. Assertions check that the repeated
    /// request fails without changing the existing Recording state.
    /// </remarks>
    [TestMethod]
    public void Start_FromRecording_FailsAndPreservesState()
    {
        var recorder = new RecorderStateMachine();
        recorder.Start();

        var result = recorder.Start();

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Recording", recorder.Machine.CurrentState);
    }
}
