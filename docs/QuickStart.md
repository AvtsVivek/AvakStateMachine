# AvakStateMachine Quick Start

## 30-Second Overview

AvakStateMachine is a .NET framework for building state machines from XML configuration. Define states and transitions in XML, implement guards/permissions/callbacks in C#, inject them into the machine, and trigger transitions with automatic validation and error handling.

## Installation

The framework is available in:
- Framework library: `Avak.StateMachine.Core` (in `src/Framework/Avak.StateMachine.Core`)
- Reference this project from your application

## 5-Minute Setup

### Step 1: Define State Machine (XML)

Create a file `machine.xml`:
```xml
<?xml version="1.0" encoding="utf-8"?>
<StateMachine initialState="Idle">
  <States>
    <State name="Idle">
      <Transitions>
        <Transition trigger="Start" to="Running" />
      </Transitions>
    </State>
    <State name="Running">
      <Transitions>
        <Transition trigger="Stop" to="Idle" />
      </Transitions>
    </State>
  </States>
</StateMachine>
```

### Step 2: Load and Use (C#)

```csharp
using Avak.StateMachine.Core;
using Avak.StateMachine.Core.Configuration;
using Avak.StateMachine.Core.Extensions;

// Load configuration
var loader = new XmlConfigurationLoader();
var config = loader.LoadFromFile("machine.xml");

// Create state machine
var machine = new StateMachine(config);

// Trigger transitions
var result = machine.Trigger("Start");
Console.WriteLine(result.Success); // true
Console.WriteLine(machine.CurrentState); // "Running"

result = machine.Trigger("Stop");
Console.WriteLine(machine.CurrentState); // "Idle"
```

That's it! You now have a working state machine.

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

Callbacks are executed in order during a transition:

1. **OnLeaving** - Before leaving source state
2. **OnEntering** - Before entering target state
3. **OnEntered** - After successfully entering target state

```xml
<Transition trigger="Start" to="Running"
  onLeaving="Cleanup"
  onEntering="Initialize"
  onEntered="PostInit" />
```

If any callback throws, the transition fails and the machine rolls back to the source state.

## Full Example: Traffic Light

### Configuration
```xml
<?xml version="1.0" encoding="utf-8"?>
<StateMachine initialState="Red">
  <States>
    <State name="Red">
      <Transitions>
        <Transition trigger="Next" to="Green" onEntered="AllowTraffic" />
      </Transitions>
    </State>
    <State name="Green">
      <Transitions>
        <Transition trigger="Next" to="Yellow" />
      </Transitions>
    </State>
    <State name="Yellow">
      <Transitions>
        <Transition trigger="Next" to="Red" onLeaving="WarnTraffic" />
      </Transitions>
    </State>
  </States>
</StateMachine>
```

### Implementation
```csharp
public class AllowTrafficCallback : ITransitionCallback
{
    public void Execute(TransitionContext context)
        => Console.WriteLine("🟢 Traffic allowed");
}

public class WarnTrafficCallback : ITransitionCallback
{
    public void Execute(TransitionContext context)
        => Console.WriteLine("🟡 Prepare to stop");
}

// Usage
var loader = new XmlConfigurationLoader();
var config = loader.LoadFromFile("traffic.xml");

var callbackRegistry = new CallbackRegistry();
callbackRegistry.Register("AllowTraffic", new AllowTrafficCallback());
callbackRegistry.Register("WarnTraffic", new WarnTrafficCallback());

var light = new StateMachine(config, callbackRegistry: callbackRegistry);

// Cycle through lights
for (int i = 0; i < 3; i++)
{
    Console.WriteLine($"Current: {light.CurrentState}");
    light.Trigger("Next");
}
```

Output:
```
Current: Red
🟢 Traffic allowed
Current: Green
Current: Yellow
🟡 Prepare to stop
Current: Red
```

## Common Patterns

### Multiple Transitions from Same State

```xml
<State name="Editing">
  <Transitions>
    <Transition trigger="Save" to="Saved" />
    <Transition trigger="Cancel" to="Initial" />
    <Transition trigger="Error" to="ErrorState" />
  </Transitions>
</State>
```

### Source-Specific Behavior

```xml
<State name="Processing">
  <Transitions>
    <Transition trigger="Complete" source="Worker1" to="Done" />
    <Transition trigger="Complete" source="Worker2" to="Review" />
    <Transition trigger="Complete" to="Done" /> <!-- Default -->
  </Transitions>
</State>
```

### Guarded Transitions

```xml
<Transition trigger="Publish" to="Published" 
  guard="IsApproved" 
  permission="EditorRole" />
```

Only allows transition if guard returns `true` AND permission returns `true`.

### Conditional Callbacks

If you need conditional logic in callbacks:

```csharp
public class ConditionalCallback : ITransitionCallback
{
    public void Execute(TransitionContext context)
    {
        if (context.Data.TryGetValue("requiresNotification", out var value) 
            && value is bool requires && requires)
        {
            SendNotification(context);
        }
    }
}
```

## Best Practices

1. **Keep XML Configuration Simple**: Define structure, not logic
2. **Put Logic in Typed Implementations**: Guards and callbacks are C# classes
3. **Use Source for Intent Distinction**: Not just for routing, but to document where triggers originate
4. **Avoid Side Effects in Guards**: Guards should be pure read-only checks
5. **Keep Callbacks Focused**: Each callback should have a single responsibility
6. **Handle Failures Explicitly**: Always check `TransitionResult.Success`
7. **Document Callback Dependencies**: What state should exist when callback executes?

## Troubleshooting

### Configuration Won't Load
- Check XML syntax (must be valid XML)
- Verify `initialState` references an existing state
- All transition `to` attributes must reference existing states

### Guard Not Working
- Verify guard name in XML matches registration name
- Check that `IGuard.Evaluate()` returns `false` (not throws exception)
- Guard returns `false` = transition denied

### Callback Not Executing
- Verify callback name in XML matches registration name
- Check callback is registered before creating the machine
- Remember: if any callback throws, entire transition fails

### State Didn't Change
- Check `TransitionResult.Success` - transition may have failed
- Verify trigger name and current state are correct
- Look at `TransitionResult.ErrorMessage` for details

## Documentation Links

- **[Framework Reference](Framework.md)**: Complete API documentation
- **[Samples](Samples.md)**: Overview of available samples
- **[Recorder Sample](../src/Samples/200500_ConsoleRecorder/README.md)**: Full example with tests
- **[Basic Requirements](BasicRequirements.md)**: Original specification

## Next Steps

1. **Try the Quick Start** above
2. **Read the Recorder Sample** for a complete example
3. **Review Framework Reference** for detailed API documentation
4. **Build Your Own State Machine**: Apply to your domain

Happy state machine building! 🎯
