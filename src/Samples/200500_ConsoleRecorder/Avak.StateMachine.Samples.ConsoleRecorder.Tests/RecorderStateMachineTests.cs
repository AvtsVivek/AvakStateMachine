namespace Avak.StateMachine.Samples.ConsoleRecorder.Tests;

[TestClass]
public class RecorderStateMachineTests
{
    [TestMethod]
    public void RecorderInitializesInStoppedState()
    {
        var recorder = new RecorderStateMachine();
        Assert.AreEqual("Stopped", recorder.Machine.CurrentState);
    }

    [TestMethod]
    public void Start_FromStopped_TransitionsToRecording()
    {
        var recorder = new RecorderStateMachine();
        var result = recorder.Start();

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Recording", recorder.Machine.CurrentState);
    }

    [TestMethod]
    public void Pause_FromRecording_TransitionsToPaused()
    {
        var recorder = new RecorderStateMachine();
        recorder.Start();

        var result = recorder.Pause();

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Paused", recorder.Machine.CurrentState);
    }

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

    [TestMethod]
    public void Stop_FromRecording_TransitionsToStopped()
    {
        var recorder = new RecorderStateMachine();
        recorder.Start();

        var result = recorder.Stop();

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Stopped", recorder.Machine.CurrentState);
    }

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

    [TestMethod]
    public void Resume_FromStopped_TransitionsToRecording()
    {
        var recorder = new RecorderStateMachine();

        var result = recorder.Resume();

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Recording", recorder.Machine.CurrentState);
    }

    [TestMethod]
    public void Pause_FromStopped_FailsAndPreservesState()
    {
        var recorder = new RecorderStateMachine();

        var result = recorder.Pause();

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Stopped", recorder.Machine.CurrentState);
    }

    [TestMethod]
    public void Stop_FromStopped_FailsAndPreservesState()
    {
        var recorder = new RecorderStateMachine();

        var result = recorder.Stop();

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Stopped", recorder.Machine.CurrentState);
    }

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
