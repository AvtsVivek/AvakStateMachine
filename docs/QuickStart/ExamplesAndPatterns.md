# Examples and Patterns

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

For a complete WPF application using dependency injection, see the
[WPF traffic-light sample](../../src/Samples/201500_WpfTrafficLightWithDI/README.md).

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

## Related guides

- [Quick Start](QuickStart.md)
- [Guards, callbacks, and triggers](ExtensionsAndTriggers.md)
- [Troubleshooting](Troubleshooting.md)
