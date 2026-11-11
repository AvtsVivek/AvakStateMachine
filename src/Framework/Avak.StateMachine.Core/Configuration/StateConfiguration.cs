namespace Avak.StateMachine.Core.Configuration;

/// <summary>
/// Configuration for a single state in the state machine.
/// </summary>
public class StateConfiguration
{
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Transitions triggered from this state, keyed by trigger name.
    /// </summary>
    public Dictionary<string, List<TransitionConfiguration>> Transitions { get; set; } = [];
}

/// <summary>
/// Configuration for a state machine transition.
/// Resolved by: current state, trigger name, and trigger source.
/// </summary>
public class TransitionConfiguration
{
    /// <summary>
    /// The target state for this transition.
    /// </summary>
    public string ToState { get; set; } = string.Empty;
    
    /// <summary>
    /// The source (origin) of the trigger that activates this transition.
    /// Used to distinguish multiple transitions from the same state triggered by the same trigger name but from different sources.
    /// Null or empty means "any source".
    /// </summary>
    public string? Source { get; set; }
    
    /// <summary>
    /// Optional guard logic name. If present, the framework will invoke the injected guard registry to evaluate this guard.
    /// </summary>
    public string? GuardName { get; set; }
    
    /// <summary>
    /// Optional permission logic name. If present, the framework will invoke the injected permission registry to evaluate this permission.
    /// </summary>
    public string? PermissionName { get; set; }
    
    /// <summary>
    /// Optional callback to invoke before transition begins.
    /// </summary>
    public string? OnEnteringCallbackName { get; set; }
    
    /// <summary>
    /// Optional callback to invoke after transition succeeds.
    /// </summary>
    public string? OnEnteredCallbackName { get; set; }
    
    /// <summary>
    /// Optional callback to invoke when leaving the current state.
    /// </summary>
    public string? OnLeavingCallbackName { get; set; }
}

/// <summary>
/// Complete configuration for a state machine, loaded from XML.
/// </summary>
public class StateMachineConfiguration
{
    /// <summary>
    /// Initial state name.
    /// </summary>
    public string InitialState { get; set; } = string.Empty;
    
    /// <summary>
    /// All states in the machine, keyed by state name.
    /// </summary>
    public Dictionary<string, StateConfiguration> States { get; set; } = [];
}
