# Framework API Reference

## Core Components

### `StateMachineConfiguration`
The parsed representation of an XML state machine definition. Contains:
- `InitialState`: The starting state
- `States`: Dictionary of state definitions

### `StateConfiguration`
Represents a single state and its transitions:
- `Name`: The state's name
- `StateClassName`: Optional registry key for a state object
- `Transitions`: Dictionary of trigger name → list of `TransitionConfiguration` objects

### `TransitionConfiguration`
Represents a single transition rule:
- `ToState`: Target state (required)
- `Source`: Trigger source filter (optional; `null` means "any source")
- `GuardName`: Guard logic name (optional)
- `PermissionName`: Permission logic name (optional)
- `OnLeavingCallbackName`: Callback to execute before leaving (optional)
- `OnEnteringCallbackName`: Callback to execute before entering (optional)
- `OnEnteredCallbackName`: Callback to execute after entering (optional)

The XML `trigger` attribute is stored as the key in the containing state's
`Transitions` dictionary, not as a property of `TransitionConfiguration`. For
example, a transition with `trigger="Start"` is added to
`state.Transitions["Start"]`. The machine first looks up that trigger in the
current state, then selects a transition from the resulting list by source.

Consequently, a `TransitionConfiguration` inspected on its own does not reveal
its trigger name; inspect its containing dictionary entry to find it. Keeping
the name only as the key avoids duplicating it in each transition and needing
to keep the two values consistent.

### `StateMachine`
The runtime engine:
```csharp
public class StateMachine
{
    public string CurrentState { get; }
    public TransitionResult Trigger(string triggerName, string? source = null, 
        IDictionary<string, object?>? data = null);
}
```

### `TransitionResult`
Returned from each `Trigger()` call:
```csharp
public class TransitionResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }
    public string FromState { get; set; }
    public string ToState { get; set; }
}
```

### `TransitionContext`
Passed to all guards, permissions, and callbacks:
```csharp
public class TransitionContext
{
    public string FromState { get; set; }
    public string ToState { get; set; }
    public string TriggerName { get; set; }
    public string? TriggerSource { get; set; }
    public IDictionary<string, object?> Data { get; set; }
}
```

## Extension Point Interfaces

### `IGuard`
```csharp
public interface IGuard
{
    bool Evaluate(TransitionContext context);
}
```

### `IPermission`
```csharp
public interface IPermission
{
    bool IsAllowed(TransitionContext context);
}
```

### `ITransitionCallback`
```csharp
public interface ITransitionCallback
{
    void Execute(TransitionContext context);
}
```

## Registry Interfaces

- `IGuardRegistry`: Maps guard names to `IGuard` implementations
- `IPermissionRegistry`: Maps permission names to `IPermission` implementations
- `ICallbackRegistry`: Maps callback names to `ITransitionCallback` implementations

## Related guides

- [Architecture and states](ArchitectureAndStates.md)
- [XML configuration](XmlConfiguration.md)
- [Transition processing](TransitionProcessing.md)
- [Framework overview](Framework.md)
