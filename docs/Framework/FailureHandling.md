# Failure Handling

## Rollback Behavior

When a transition fails at any point:
- The machine's `CurrentState` is restored to the state before the transition attempt
- Callbacks already executed (before the failure) are **not** rolled back
- Changes to state objects made by lifecycle methods are **not** rolled back
- External side effects (file writes, API calls, etc.) performed by callbacks are **not** automatically undone

### Recommendation for Callbacks with Side Effects
Applications should either:
1. Avoid irreversible work in callbacks, or
2. Implement compensation logic within callbacks (e.g., try/catch with explicit undo)

Example:
```csharp
public class WriteLogCallback : ITransitionCallback
{
    public void Execute(TransitionContext context)
    {
        // Good: Append-only operation, safe to leave partial
        File.AppendAllText("log.txt", $"{context.TriggerName} at {DateTime.Now}\n");
    }
}

public class RiskyUpdateCallback : ITransitionCallback
{
    private string? _originalValue;
    
    public void Execute(TransitionContext context)
    {
        // Less safe: Modifying external state without compensation
        // If a later callback throws, this update persists
        _originalValue = Database.GetValue("key");
        Database.SetValue("key", "new_value");
        // Better: Provide compensation if needed
    }
}
```
## Usage Example

### 1. Define Configuration (XML)
```xml
<?xml version="1.0" encoding="utf-8"?>
<StateMachine initialState="Stopped">
  <States>
    <State name="Stopped">
      <Transitions>
        <Transition trigger="Start" to="Running" onLeaving="PrepareStart" onEntered="OnStarted" />
      </Transitions>
    </State>
    <State name="Running">
      <Transitions>
        <Transition trigger="Stop" to="Stopped" onLeaving="Cleanup" />
      </Transitions>
    </State>
  </States>
</StateMachine>
```

### 2. Implement Guards/Permissions/Callbacks (C#)
```csharp
public class CanStartGuard : IGuard
{
    private readonly ISystemStatus _status;
    
    public CanStartGuard(ISystemStatus status) => _status = status;
    
    public bool Evaluate(TransitionContext context)
    {
        return _status.IsHealthy;
    }
}

public class PrepareStartCallback : ITransitionCallback
{
    private readonly ILogger _logger;
    
    public void Execute(TransitionContext context)
    {
        _logger.Info($"Preparing to start from {context.FromState}");
    }
}
```

### 3. Build and Use the State Machine
```csharp
var configLoader = new XmlConfigurationLoader();
var config = configLoader.LoadFromFile("machine-config.xml");

var guardRegistry = new GuardRegistry();
guardRegistry.Register("CanStart", new CanStartGuard(systemStatus));

var callbackRegistry = new CallbackRegistry();
callbackRegistry.Register("PrepareStart", new PrepareStartCallback(logger));

var machine = new StateMachine(config, guardRegistry: guardRegistry, 
    callbackRegistry: callbackRegistry);

var result = machine.Trigger("Start");
if (result.Success)
{
    Console.WriteLine($"Transitioned to {result.ToState}");
}
else
{
    Console.WriteLine($"Transition failed: {result.ErrorMessage}");
    if (result.Exception is TransitionDeniedException tde)
        Console.WriteLine($"Reason: {tde.Reason}");
}
```
## Exception Hierarchy

```
StateMachineException (base)
├── TransitionDeniedException (Guard or Permission denied)
├── NestedRequestException (Trigger during transition)
└── CallbackException (Callback threw exception)
```

## Related guides

- [Transition processing](TransitionProcessing.md)
- [API reference](ApiReference.md)
- [Logging](Logging.md)
- [Framework overview](Framework.md)
