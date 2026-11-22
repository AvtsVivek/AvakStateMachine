# Extensions and Triggers

## Adding Guards and Callbacks

### Define Guard Logic

```csharp
public class CanStartGuard : IGuard
{
    public bool Evaluate(TransitionContext context)
    {
        // Return false to deny the transition
        return DateTime.Now.Hour >= 9; // Only allow starting after 9 AM
    }
}
```

### Reference in XML

```xml
<Transition trigger="Start" to="Running" guard="CanStart" />
```

### Register and Inject

```csharp
var guardRegistry = new GuardRegistry();
guardRegistry.Register("CanStart", new CanStartGuard());

var machine = new StateMachine(config, guardRegistry);

// Now triggers are guarded
var result = machine.Trigger("Start");
if (!result.Success && result.Exception is TransitionDeniedException)
    Console.WriteLine("Guard denied the transition");
```

### Add Callbacks

```csharp
public class OnStartCallback : ITransitionCallback
{
    public void Execute(TransitionContext context)
    {
        Console.WriteLine($"Transitioning from {context.FromState} to {context.ToState}");
    }
}
```

```xml
<Transition trigger="Start" to="Running" onEntered="OnStart" />
```

```csharp
var callbackRegistry = new CallbackRegistry();
callbackRegistry.Register("OnStart", new OnStartCallback());

var machine = new StateMachine(config, callbackRegistry: callbackRegistry);
machine.Trigger("Start"); // Callback executes
```
## Source-Aware Triggers

Distinguish transitions by trigger source:

```xml
<Transition trigger="Start" source="UI" to="Running" />
<Transition trigger="Start" source="API" to="Paused" />
<Transition trigger="Start" to="Running" /> <!-- Default, any source -->
```

```csharp
machine.Trigger("Start", source: "UI");  // Goes to Running
machine.Trigger("Start", source: "API"); // Goes to Paused
machine.Trigger("Start");                // Goes to Running (default)
```
## Passing Data to Callbacks

Include context data in transitions:

```csharp
var result = machine.Trigger("Start", data: new Dictionary<string, object?>
{
    ["priority"] = "high",
    ["userId"] = "alice@example.com"
});
```

Access in callbacks:
```csharp
public class LogStartCallback : ITransitionCallback
{
    public void Execute(TransitionContext context)
    {
        var priority = context.Data["priority"];
        var userId = context.Data["userId"];
        Console.WriteLine($"Start requested by {userId} with priority {priority}");
    }
}
```
## Error Handling

Check `TransitionResult` for detailed failure information:

```csharp
var result = machine.Trigger("InvalidTrigger");

if (!result.Success)
{
    Console.WriteLine($"Error: {result.ErrorMessage}");
    
    if (result.Exception is TransitionDeniedException)
        Console.WriteLine("Guard or permission denied the transition");
    else if (result.Exception is NestedRequestException)
        Console.WriteLine("Trigger during transition detected");
    else if (result.Exception is CallbackException)
        Console.WriteLine("Callback threw an exception");
}

// Machine state is always consistent after a failed transition
Console.WriteLine($"Current state: {machine.CurrentState}");
```
## Callback Lifecycle

Callbacks are configured on transitions and run in this order:

1. **OnLeaving** - Perform source-state departure work while the source is current.
2. **OnEntering** - Prepare the target while the source is still current.
3. The machine changes `CurrentState` to the target.
4. **OnEntered** - Perform target-entry work while the target is current.

```xml
<Transition trigger="Start" to="Running"
  onLeaving="Cleanup"
  onEntering="Initialize"
  onEntered="PostInit" />
```

`OnEntered` runs before the transition succeeds. If any callback throws, the
machine restores its internal source state, but it cannot undo external side
effects. The framework currently has no `OnLeft` callback. See the
[framework transition reference](../Framework/TransitionProcessing.md#lifecycle-callbacks)
for details.

## Related guides

- [Quick Start](QuickStart.md)
- [Examples and patterns](ExamplesAndPatterns.md)
- [Troubleshooting](Troubleshooting.md)
