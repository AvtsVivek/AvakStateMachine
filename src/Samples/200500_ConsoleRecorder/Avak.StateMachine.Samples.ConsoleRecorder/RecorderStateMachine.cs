namespace Avak.StateMachine.Samples.ConsoleRecorder;

using Avak.StateMachine.Core;
using Avak.StateMachine.Core.Configuration;
using Avak.StateMachine.Core.Extensions;

/// <summary>
/// Simple audio recorder state machine sample.
/// Demonstrates XML configuration, guards, permissions, callbacks, and error handling.
/// </summary>
public class RecorderStateMachine
{
    public StateMachine Machine { get; }

    public RecorderStateMachine()
    {
        var loader = new XmlConfigurationLoader();
        var config = loader.LoadFromFile(GetConfigFilePath());

        var guardRegistry = new GuardRegistry();
        guardRegistry.Register("CheckAudioDeviceAvailable", new AudioDeviceAvailableGuard());

        var permissionRegistry = new PermissionRegistry();
        permissionRegistry.Register("AdminOnly", new AdminOnlyPermission());

        var callbackRegistry = new CallbackRegistry();
        callbackRegistry.Register("PrepareRecording", new PrepareRecordingCallback());
        callbackRegistry.Register("OnRecordingStarted", new OnRecordingStartedCallback());
        callbackRegistry.Register("OnRecordingPaused", new OnRecordingPausedCallback());
        callbackRegistry.Register("OnRecordingStopped", new OnRecordingStoppedCallback());
        callbackRegistry.Register("OnResumeRecording", new OnResumeRecordingCallback());
        callbackRegistry.Register("ValidateResumeState", new ValidateResumeStateCallback());

        Machine = new StateMachine(config, guardRegistry, permissionRegistry, callbackRegistry);
    }

    private static string GetConfigFilePath()
    {
        string assemblyPath = AppDomain.CurrentDomain.BaseDirectory;
        return Path.Combine(assemblyPath, "RecorderConfiguration.xml");
    }

    public TransitionResult Start()
    {
        return Machine.Trigger("Start");
    }

    public TransitionResult Pause()
    {
        return Machine.Trigger("Pause");
    }

    public TransitionResult Resume()
    {
        return Machine.Trigger("Resume");
    }

    public TransitionResult Stop()
    {
        return Machine.Trigger("Stop");
    }
}

// Guard implementations

public class AudioDeviceAvailableGuard : IGuard
{
    public bool Evaluate(TransitionContext context)
    {
        return true;
    }
}

// Permission implementations

public class AdminOnlyPermission : IPermission
{
    public bool IsAllowed(TransitionContext context)
    {
        return true;
    }
}

// Callback implementations

public class PrepareRecordingCallback : ITransitionCallback
{
    public void Execute(TransitionContext context)
    {
    }
}

public class OnRecordingStartedCallback : ITransitionCallback
{
    public void Execute(TransitionContext context)
    {
    }
}

public class OnRecordingPausedCallback : ITransitionCallback
{
    public void Execute(TransitionContext context)
    {
    }
}

public class OnRecordingStopped : ITransitionCallback
{
    public void Execute(TransitionContext context)
    {
    }
}

public class OnRecordingStoppedCallback : ITransitionCallback
{
    public void Execute(TransitionContext context)
    {
    }
}

public class OnResumeRecordingCallback : ITransitionCallback
{
    public void Execute(TransitionContext context)
    {
    }
}

public class ValidateResumeStateCallback : ITransitionCallback
{
    public void Execute(TransitionContext context)
    {
    }
}
