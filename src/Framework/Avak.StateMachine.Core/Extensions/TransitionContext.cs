namespace Avak.StateMachine.Core.Extensions;

/// <summary>
/// Context passed to guards, permissions, and callbacks.
/// </summary>
public class TransitionContext
{
    /// <summary>
    /// The current state before the transition.
    /// </summary>
    public string FromState { get; set; } = string.Empty;
    
    /// <summary>
    /// The target state of the transition.
    /// </summary>
    public string ToState { get; set; } = string.Empty;
    
    /// <summary>
    /// The name of the trigger that initiated this transition.
    /// </summary>
    public string TriggerName { get; set; } = string.Empty;
    
    /// <summary>
    /// The source of the trigger (e.g., which component triggered it).
    /// </summary>
    public string? TriggerSource { get; set; }
    
    /// <summary>
    /// Additional context data supplied by the caller.
    /// </summary>
    public IDictionary<string, object?> Data { get; set; } = new Dictionary<string, object?>();
}

/// <summary>
/// Guard logic: evaluates whether a transition may proceed based on conditions.
/// If a guard returns false, the transition is denied and the state machine remains unchanged.
/// </summary>
public interface IGuard
{
    /// <summary>
    /// Evaluates whether the transition should be allowed.
    /// </summary>
    /// <returns>True if the transition is allowed; false to deny it.</returns>
    bool Evaluate(TransitionContext context);
}

/// <summary>
/// Permission logic: evaluates whether the caller has permission to perform a transition.
/// If a permission returns false, the transition is denied and the state machine remains unchanged.
/// </summary>
public interface IPermission
{
    /// <summary>
    /// Evaluates whether the caller has permission to perform the transition.
    /// </summary>
    /// <returns>True if permitted; false to deny it.</returns>
    bool IsAllowed(TransitionContext context);
}

/// <summary>
/// Lifecycle callback: executed at specific points during a transition.
/// If a callback throws an exception, the transition fails and the machine rolls back to the source state.
/// </summary>
public interface ITransitionCallback
{
    /// <summary>
    /// Executes the callback logic.
    /// </summary>
    void Execute(TransitionContext context);
}
