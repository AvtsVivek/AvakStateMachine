namespace Avak.StateMachine.Core;

using System.Diagnostics;
using Configuration;
using Extensions;

/// <summary>
/// Transition result indicating success or failure of a state transition.
/// </summary>
[DebuggerDisplay("Success = {Success}, FromState = {FromState,nq}, ToState = {ToState,nq}")]
public class TransitionResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }
    public string FromState { get; set; } = string.Empty;
    public string ToState { get; set; } = string.Empty;
}

/// <summary>
/// The core state machine engine.
/// Manages state transitions based on XML configuration, guards, permissions, and callbacks.
/// Thread-safe is not guaranteed; single-threaded use only in this version.
/// </summary>
[DebuggerDisplay("CurrentState = {CurrentState,nq}")]
public class StateMachine
{
    private readonly StateMachineConfiguration _config;
    private readonly IGuardRegistry _guardRegistry;
    private readonly IPermissionRegistry _permissionRegistry;
    private readonly ICallbackRegistry _callbackRegistry;

    private string _currentState;
    private bool _isProcessing;

    public string CurrentState => _currentState;

    public StateMachine(
        StateMachineConfiguration config,
        IGuardRegistry? guardRegistry = null,
        IPermissionRegistry? permissionRegistry = null,
        ICallbackRegistry? callbackRegistry = null)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _guardRegistry = guardRegistry ?? new GuardRegistry();
        _permissionRegistry = permissionRegistry ?? new PermissionRegistry();
        _callbackRegistry = callbackRegistry ?? new CallbackRegistry();

        _currentState = _config.InitialState;
        _isProcessing = false;
    }

    /// <summary>
    /// Triggers a transition by trigger name and optional source.
    /// </summary>
    public TransitionResult Trigger(string triggerName, string? source = null, IDictionary<string, object?>? data = null)
    {
        if (_isProcessing)
        {
            return FailureResult(
                $"Nested request detected for trigger '{triggerName}' while already processing a transition",
                new NestedRequestException($"Nested request detected for trigger '{triggerName}'"));
        }

        _isProcessing = true;
        try
        {
            return TriggerInternal(triggerName, source, data);
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private TransitionResult TriggerInternal(string triggerName, string? source, IDictionary<string, object?>? data)
    {
        var currentStateConfig = _config.States[_currentState];

        if (!currentStateConfig.Transitions.TryGetValue(triggerName, out var transitionList))
        {
            return FailureResult($"No transition configured for trigger '{triggerName}' from state '{_currentState}'", null);
        }

        var transition = ResolveTransition(transitionList, source);
        if (transition == null)
        {
            return FailureResult($"No matching transition for trigger '{triggerName}' from state '{_currentState}' with source '{source}'", null);
        }

        var context = new TransitionContext
        {
            FromState = _currentState,
            ToState = transition.ToState,
            TriggerName = triggerName,
            TriggerSource = source,
            Data = data ?? new Dictionary<string, object?>()
        };

        if (!EvaluateGuard(transition, context, out string? guardError))
        {
            return FailureResult(guardError!, new TransitionDeniedException(guardError!, "Guard denied transition"));
        }

        if (!EvaluatePermission(transition, context, out string? permissionError))
        {
            return FailureResult(permissionError!, new TransitionDeniedException(permissionError!, "Permission denied transition"));
        }

        string sourceState = _currentState;

        try
        {
            ExecuteCallback(transition.OnLeavingCallbackName, context);
            ExecuteCallback(transition.OnEnteringCallbackName, context);

            _currentState = transition.ToState;

            ExecuteCallback(transition.OnEnteredCallbackName, context);

            return new TransitionResult
            {
                Success = true,
                FromState = sourceState,
                ToState = transition.ToState
            };
        }
        catch (Exception ex)
        {
            _currentState = sourceState;

            if (ex is CallbackException)
            {
                return FailureResult(ex.Message, ex);
            }

            return FailureResult($"Transition failed: {ex.Message}", ex);
        }
    }

    private static TransitionConfiguration? ResolveTransition(List<TransitionConfiguration> transitions, string? source)
    {
        var sourceSpecific = transitions.FirstOrDefault(t => t.Source == source);
        if (sourceSpecific != null)
        {
            return sourceSpecific;
        }

        var anySource = transitions.FirstOrDefault(t => string.IsNullOrEmpty(t.Source));
        return anySource;
    }

    private bool EvaluateGuard(TransitionConfiguration transition, TransitionContext context, out string? error)
    {
        error = null;

        if (string.IsNullOrEmpty(transition.GuardName))
        {
            return true;
        }

        var guard = _guardRegistry.Resolve(transition.GuardName);
        if (guard == null)
        {
            return true;
        }

        try
        {
            if (!guard.Evaluate(context))
            {
                error = $"Guard '{transition.GuardName}' denied transition from '{context.FromState}' to '{context.ToState}'";
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            error = $"Guard '{transition.GuardName}' threw exception: {ex.Message}";
            return false;
        }
    }

    private bool EvaluatePermission(TransitionConfiguration transition, TransitionContext context, out string? error)
    {
        error = null;

        if (string.IsNullOrEmpty(transition.PermissionName))
        {
            return true;
        }

        var permission = _permissionRegistry.Resolve(transition.PermissionName);
        if (permission == null)
        {
            return true;
        }

        try
        {
            if (!permission.IsAllowed(context))
            {
                error = $"Permission '{transition.PermissionName}' denied transition from '{context.FromState}' to '{context.ToState}'";
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            error = $"Permission '{transition.PermissionName}' threw exception: {ex.Message}";
            return false;
        }
    }

    private void ExecuteCallback(string? callbackName, TransitionContext context)
    {
        if (string.IsNullOrEmpty(callbackName))
        {
            return;
        }

        var callback = _callbackRegistry.Resolve(callbackName);
        if (callback == null)
        {
            return;
        }

        try
        {
            callback.Execute(context);
        }
        catch (Exception ex)
        {
            throw new CallbackException($"Callback '{callbackName}' failed", callbackName, ex);
        }
    }

    private TransitionResult FailureResult(string message, Exception? ex)
    {
        return new TransitionResult
        {
            Success = false,
            ErrorMessage = message,
            Exception = ex,
            FromState = _currentState,
            ToState = _currentState
        };
    }
}
