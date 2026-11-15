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
/// All configured extension names must resolve during construction.
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

        ValidateConfiguredExtensions();
        _currentState = _config.InitialState;
        _isProcessing = false;
    }

    private void ValidateConfiguredExtensions()
    {
        foreach (var state in _config.States.Values)
        {
            foreach (var (triggerName, transitions) in state.Transitions)
            {
                foreach (var transition in transitions)
                {
                    ValidateExtension(
                        transition.GuardName,
                        _guardRegistry.Resolve,
                        "guard",
                        state.Name,
                        triggerName);
                    ValidateExtension(
                        transition.PermissionName,
                        _permissionRegistry.Resolve,
                        "permission",
                        state.Name,
                        triggerName);
                    ValidateExtension(
                        transition.OnLeavingCallbackName,
                        _callbackRegistry.Resolve,
                        "callback",
                        state.Name,
                        triggerName);
                    ValidateExtension(
                        transition.OnEnteringCallbackName,
                        _callbackRegistry.Resolve,
                        "callback",
                        state.Name,
                        triggerName);
                    ValidateExtension(
                        transition.OnEnteredCallbackName,
                        _callbackRegistry.Resolve,
                        "callback",
                        state.Name,
                        triggerName);
                }
            }
        }
    }

    private static void ValidateExtension<T>(
        string? name,
        Func<string, T?> resolve,
        string extensionType,
        string stateName,
        string triggerName)
        where T : class
    {
        if (name is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new StateMachineException(
                $"Configured {extensionType} name cannot be empty for transition " +
                $"from state '{stateName}' with trigger '{triggerName}'");
        }

        if (resolve(name) is null)
        {
            throw new UnresolvedExtensionException(extensionType, name, stateName, triggerName);
        }
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

        var guardException = EvaluateGuard(transition, context);
        if (guardException is not null)
        {
            return FailureResult(guardException.Message, guardException);
        }

        var permissionException = EvaluatePermission(transition, context);
        if (permissionException is not null)
        {
            return FailureResult(permissionException.Message, permissionException);
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

    private Exception? EvaluateGuard(TransitionConfiguration transition, TransitionContext context)
    {
        if (transition.GuardName is null)
        {
            return null;
        }

        var guard = _guardRegistry.Resolve(transition.GuardName);
        if (guard is null)
        {
            return CreateUnresolvedExtensionException("guard", transition.GuardName, context);
        }

        try
        {
            if (!guard.Evaluate(context))
            {
                string message = $"Guard '{transition.GuardName}' denied transition from '{context.FromState}' to '{context.ToState}'";
                return new TransitionDeniedException(message, "Guard denied transition");
            }
            return null;
        }
        catch (Exception ex)
        {
            string message = $"Guard '{transition.GuardName}' threw exception: {ex.Message}";
            return new TransitionDeniedException(message, "Guard denied transition");
        }
    }

    private Exception? EvaluatePermission(TransitionConfiguration transition, TransitionContext context)
    {
        if (transition.PermissionName is null)
        {
            return null;
        }

        var permission = _permissionRegistry.Resolve(transition.PermissionName);
        if (permission is null)
        {
            return CreateUnresolvedExtensionException("permission", transition.PermissionName, context);
        }

        try
        {
            if (!permission.IsAllowed(context))
            {
                string message = $"Permission '{transition.PermissionName}' denied transition from '{context.FromState}' to '{context.ToState}'";
                return new TransitionDeniedException(message, "Permission denied transition");
            }
            return null;
        }
        catch (Exception ex)
        {
            string message = $"Permission '{transition.PermissionName}' threw exception: {ex.Message}";
            return new TransitionDeniedException(message, "Permission denied transition");
        }
    }

    private void ExecuteCallback(string? callbackName, TransitionContext context)
    {
        if (callbackName is null)
        {
            return;
        }

        var callback = _callbackRegistry.Resolve(callbackName)
            ?? throw CreateUnresolvedExtensionException("callback", callbackName, context);

        try
        {
            callback.Execute(context);
        }
        catch (Exception ex)
        {
            throw new CallbackException($"Callback '{callbackName}' failed", callbackName, ex);
        }
    }

    private static UnresolvedExtensionException CreateUnresolvedExtensionException(
        string extensionType,
        string extensionName,
        TransitionContext context)
    {
        return new UnresolvedExtensionException(
            extensionType,
            extensionName,
            context.FromState,
            context.TriggerName);
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
