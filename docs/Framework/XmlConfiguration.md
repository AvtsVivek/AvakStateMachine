# XML Configuration

## XML Configuration Format

### Basic Structure
```xml
<?xml version="1.0" encoding="utf-8"?>
<StateMachine initialState="InitialStateName">
  <States>
    <State name="StateName1">
      <Transitions>
        <Transition trigger="TriggerName" to="StateName2" />
      </Transitions>
    </State>
    <State name="StateName2" />
  </States>
</StateMachine>
```

### Transition with Guards, Permissions, and Callbacks
```xml
<Transition 
  trigger="TriggerName" 
  source="SourceIdentifier"
  to="TargetState" 
  guard="GuardLogicName"
  permission="PermissionLogicName"
  onLeaving="OnLeavingCallbackName"
  onEntering="OnEnteringCallbackName"
  onEntered="OnEnteredCallbackName" />
```

### Attributes

| Attribute | Required | Type | Description |
|-----------|----------|------|-------------|
| `initialState` | Yes (on `StateMachine`) | String | Name of the starting state |
| `name` | Yes (on `State`) | String | Unique state name |
| `stateClass` | No (on `State`) | String | Registry key for an optional state object |
| `trigger` | Yes (on `Transition`) | String | Trigger identifier |
| `to` | Yes (on `Transition`) | String | Target state name |
| `source` | No | String | Trigger source filter |
| `guard` | No | String | Guard logic name |
| `permission` | No | String | Permission logic name |
| `onLeaving` | No | String | Callback name for leaving |
| `onEntering` | No | String | Callback name for entering |
| `onEntered` | No | String | Callback name for post-entry |

### Validation Rules

1. `initialState` must be nonblank and reference an existing state.
2. State names, trigger names, and target state names must be nonblank; each
   target must reference an existing state.
3. State names must be unique. For each state and trigger, each source may
   occur only once. An omitted or empty source represents the same
   source-agnostic transition.
4. Configured state-class, guard, permission, and callback names must be nonblank.
   State-class keys must resolve during machine construction.
5. Unknown elements, attributes, non-whitespace text, duplicate `States` or
   `Transitions` containers, and XML namespaces are rejected.
6. DTDs and external entity resolution are prohibited.

## Related guides

- [Architecture and states](ArchitectureAndStates.md)
- [Transition processing](TransitionProcessing.md)
- [Quick Start: guards and triggers](../QuickStart/ExtensionsAndTriggers.md)
- [Framework overview](Framework.md)
