namespace Avak.StateMachine.Core.Extensions;

/// <summary>Context for state lifecycle methods, including initial entry.</summary>
public sealed class StateContext(
    string? fromState,
    string toState,
    string? triggerName,
    string? triggerSource,
    IDictionary<string, object?> data)
{
    /// <summary>The source state, or null during initialization.</summary>
    public string? FromState { get; } = fromState;
    /// <summary>The target state, including the initial state during initialization.</summary>
    public string ToState { get; } = toState;
    /// <summary>The trigger name, or null during initialization.</summary>
    public string? TriggerName { get; } = triggerName;
    /// <summary>The trigger source, or null if absent or during initialization.</summary>
    public string? TriggerSource { get; } = triggerSource;
    /// <summary>Request data; initially empty during initialization.</summary>
    public IDictionary<string, object?> Data { get; } = data;
    /// <summary>Whether this context represents initial entry rather than a trigger.</summary>
    public bool IsInitialization => FromState is null;
}

/// <summary>Optional state-specific checks and lifecycle behavior.</summary>
public abstract class State
{
    /// <summary>Returns whether the target state may be entered.</summary>
    public virtual bool CanEnter(StateContext context) => true;
    /// <summary>Returns whether the source state may be exited.</summary>
    public virtual bool CanExit(StateContext context) => true;
    /// <summary>Performs work after the target state becomes current.</summary>
    public virtual void Enter(StateContext context) { }
    /// <summary>Performs work while the source state is still current.</summary>
    public virtual void Exit(StateContext context) { }
}

/// <summary>Resolves an application-supplied state object by its configured class name.</summary>
public interface IStateRegistry
{
    State? Resolve(string name);
}

/// <summary>In-memory registry of application-supplied state objects.</summary>
public class StateRegistry : IStateRegistry
{
    private readonly Dictionary<string, State> _states = [];

    public void Register(string name, State state)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _states[name] = state ?? throw new ArgumentNullException(nameof(state));
    }

    public State? Resolve(string name)
    {
        _states.TryGetValue(name, out var state);
        return state;
    }
}
