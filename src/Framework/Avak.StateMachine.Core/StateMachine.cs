namespace Avak.StateMachine.Core;

using System.Diagnostics;
using Configuration;
using Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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
    private readonly ILogger<StateMachine> _logger;
    private readonly Dictionary<string, State> _states = [];

    private string _currentState;
    private bool _isProcessing;
    private NestedRequestException? _nestedRequest;
    private Exception? _nestedRequestLoggingFailure;

    public string CurrentState => _currentState;

    public StateMachine(
        StateMachineConfiguration config,
        IGuardRegistry? guardRegistry = null,
        IPermissionRegistry? permissionRegistry = null,
        ICallbackRegistry? callbackRegistry = null,
        ILogger<StateMachine>? logger = null)
        : this(config, guardRegistry, permissionRegistry, callbackRegistry, logger, new StateRegistry())
    {
    }

    public StateMachine(
        StateMachineConfiguration config,
        IGuardRegistry? guardRegistry,
        IPermissionRegistry? permissionRegistry,
        ICallbackRegistry? callbackRegistry,
        ILogger<StateMachine>? logger,
        IStateRegistry stateRegistry)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        ArgumentNullException.ThrowIfNull(stateRegistry);
        _guardRegistry = guardRegistry ?? new GuardRegistry();
        _permissionRegistry = permissionRegistry ?? new PermissionRegistry();
        _callbackRegistry = callbackRegistry ?? new CallbackRegistry();
        _logger = logger ?? NullLogger<StateMachine>.Instance;

        ValidateConfiguredExtensions();
        foreach (var (name, stateConfig) in _config.States)
        {
            if (stateConfig.StateClassName is null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(stateConfig.StateClassName))
            {
                throw new StateMachineException($"Configured state class name cannot be empty for state '{name}'");
            }

            _states[name] = stateRegistry.Resolve(stateConfig.StateClassName)
                ?? throw new StateMachineException(
                    $"Configured state class '{stateConfig.StateClassName}' could not be resolved for state '{name}'");
        }

        _currentState = _config.InitialState;
        if (_states.TryGetValue(_currentState, out var initialState))
        {
            _isProcessing = true;
            try
            {
                var context = new StateContext(null, _currentState, null, null, new Dictionary<string, object?>());
                if (!ExecuteStateCheck(_currentState, nameof(State.CanEnter), () => initialState.CanEnter(context)))
                {
                    throw new TransitionDeniedException($"Initial state '{_currentState}' denied entry", "State denied entry");
                }

                ExecuteStateAction(_currentState, nameof(State.Enter), () => initialState.Enter(context));
                if (_nestedRequestLoggingFailure is not null)
                {
                    throw _nestedRequestLoggingFailure;
                }
            }
            finally
            {
                _isProcessing = false;
                _nestedRequest = null;
                _nestedRequestLoggingFailure = null;
            }
        }
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
            if (_nestedRequest is null)
            {
                _nestedRequest = new NestedRequestException(
                    $"Nested request detected for trigger '{triggerName}'");
                LogNestedRequest(triggerName, source, _currentState, _nestedRequest);
            }

            return FailureResult(
                $"Nested request detected for trigger '{triggerName}' while already processing a transition",
                _nestedRequest);
        }

        string sourceState = _currentState;
        _isProcessing = true;
        try
        {
            TransitionResult result;
            try
            {
                result = TriggerInternal(triggerName, source, data);
            }
            catch (NestedRequestException ex)
            {
                _currentState = sourceState;
                result = FailureResult(ex.Message, ex);
            }

            if (_nestedRequestLoggingFailure is not null)
            {
                throw _nestedRequestLoggingFailure;
            }

            if (result.Exception is not NestedRequestException)
            {
                LogResult(result, triggerName, source);
            }

            if (result.Success)
            {
                ThrowIfNestedRequest();
            }
            return result;
        }
        catch
        {
            _currentState = sourceState;
            throw;
        }
        finally
        {
            _nestedRequest = null;
            _nestedRequestLoggingFailure = null;
            _isProcessing = false;
        }
    }

    private void LogNestedRequest(
        string triggerName,
        string? source,
        string currentState,
        NestedRequestException exception)
    {
        const LogLevel level = LogLevel.Warning;
        if (!_logger.IsEnabled(level))
        {
            return;
        }

        try
        {
            _logger.Log(level, new EventId(102, "NestedRequestDetected"), exception,
                "Nested trigger {TriggerName} with source {TriggerSource} was rejected while the machine was in {CurrentState}",
                triggerName, source, currentState);
        }
        catch (Exception ex)
        {
            _nestedRequestLoggingFailure = ex;
        }
    }

    private void LogResult(TransitionResult result, string triggerName, string? source)
    {
        LogLevel level;
        EventId eventId;
        if (result.Success)
        {
            level = LogLevel.Information;
            eventId = new EventId(100, "TransitionSucceeded");
        }
        else if (result.Exception is null or TransitionDeniedException)
        {
            level = LogLevel.Debug;
            eventId = new EventId(101, "TransitionRejected");
        }
        else
        {
            level = LogLevel.Error;
            eventId = new EventId(103, "TransitionFailed");
        }

        if (_logger.IsEnabled(level))
        {
            _logger.Log(level, eventId, result.Exception,
                "Transition {TriggerName} from {FromState} ended in {ToState} with source {TriggerSource}: {Success}",
                triggerName, result.FromState, result.ToState, source, result.Success);
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
        ThrowIfNestedRequest();
        if (guardException is not null)
        {
            return FailureResult(guardException.Message, guardException);
        }

        var permissionException = EvaluatePermission(transition, context);
        ThrowIfNestedRequest();
        if (permissionException is not null)
        {
            return FailureResult(permissionException.Message, permissionException);
        }

        string sourceState = _currentState;

        try
        {
            _states.TryGetValue(sourceState, out var sourceObject);
            _states.TryGetValue(transition.ToState, out var targetObject);
            var stateContext = new StateContext(sourceState, transition.ToState, triggerName, source, context.Data);
            if (sourceObject is not null &&
                !ExecuteStateCheck(sourceState, nameof(State.CanExit), () => sourceObject.CanExit(stateContext)))
            {
                throw new TransitionDeniedException($"State '{sourceState}' denied exit", "State denied exit");
            }
            if (targetObject is not null &&
                !ExecuteStateCheck(transition.ToState, nameof(State.CanEnter), () => targetObject.CanEnter(stateContext)))
            {
                throw new TransitionDeniedException($"State '{transition.ToState}' denied entry", "State denied entry");
            }

            ExecuteCallback(transition.OnLeavingCallbackName, context);
            ThrowIfNestedRequest();
            if (sourceObject is not null)
            {
                ExecuteStateAction(sourceState, nameof(State.Exit), () => sourceObject.Exit(stateContext));
            }
            ExecuteCallback(transition.OnEnteringCallbackName, context);
            ThrowIfNestedRequest();

            _currentState = transition.ToState;

            if (targetObject is not null)
            {
                ExecuteStateAction(transition.ToState, nameof(State.Enter), () => targetObject.Enter(stateContext));
            }
            ExecuteCallback(transition.OnEnteredCallbackName, context);
            ThrowIfNestedRequest();

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

            if (ex is NestedRequestException)
            {
                return FailureResult(ex.Message, ex);
            }

            if (ex is CallbackException)
            {
                return FailureResult(ex.Message, ex);
            }

            if (ex is StateExecutionException or TransitionDeniedException)
            {
                return FailureResult(ex.Message, ex);
            }

            return FailureResult($"Transition failed: {ex.Message}", ex);
        }
    }

    private bool ExecuteStateCheck(string stateName, string methodName, Func<bool> execute)
    {
        bool result;
        try
        {
            result = execute();
        }
        catch (Exception ex)
        {
            throw new StateExecutionException(stateName, methodName, ex);
        }
        ThrowIfNestedRequest();
        return result;
    }

    private void ExecuteStateAction(string stateName, string methodName, Action execute)
    {
        try
        {
            execute();
        }
        catch (Exception ex)
        {
            throw new StateExecutionException(stateName, methodName, ex);
        }
        ThrowIfNestedRequest();
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
        ThrowIfNestedRequest();
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
        ThrowIfNestedRequest();
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

        var callback = _callbackRegistry.Resolve(callbackName);
        ThrowIfNestedRequest();
        if (callback is null)
        {
            throw CreateUnresolvedExtensionException("callback", callbackName, context);
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

    private void ThrowIfNestedRequest()
    {
        if (_nestedRequestLoggingFailure is not null)
        {
            throw _nestedRequestLoggingFailure;
        }

        if (_nestedRequest is not null)
        {
            throw _nestedRequest;
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
