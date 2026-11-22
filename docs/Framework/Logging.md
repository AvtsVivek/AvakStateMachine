# Logging

## Optional Logging

The source API accepts an optional `ILogger<StateMachine>` constructor argument:

```csharp
var machine = new StateMachine(configuration, logger: logger);
```

Omitting the logger uses a no-op logger. The core references
`Microsoft.Extensions.Logging.Abstractions`, not a logging provider or DI
container. Applications choose and configure their providers. When using .NET
DI, registering logging makes `ILogger<StateMachine>` available for constructor
injection; logging is not required to resolve the machine.

The machine emits outcome records for completed trigger requests and one
separate record when a nested trigger is first attempted:

| Event ID | Name | Level | Outcome |
| --- | --- | --- | --- |
| 100 | `TransitionSucceeded` | Information | Transition completed successfully |
| 101 | `TransitionRejected` | Debug | No matching transition, or guard/permission denial |
| 102 | `NestedRequestDetected` | Warning | A nested trigger was rejected when first attempted |
| 103 | `TransitionFailed` | Error | Callback failure or another returned transition error |

Outcome records use `TriggerName`, `FromState`, `ToState`, `TriggerSource`, and
`Success`. `ToState` describes the resulting state, not the attempted target:
on failure it is the restored source state. Event 102 instead records the
nested attempt with `TriggerName`, `TriggerSource`, and `CurrentState` (the
state at the time the nested request was made). Only the first nested attempt
is logged; when the outer result is a nested-request failure, it does not emit
a duplicate outcome record.

Logs supplement, rather than replace, `TransitionResult` and explicit exception
handling. Exceptions returned in a failure result are attached to the record.
Guards and permissions that throw are still classified as denials under the
existing runtime contract.

Request data is not automatically logged. Attached exceptions can contain
user-supplied messages or inner exceptions; configure access and redaction
appropriately, and avoid putting secrets in names or exception messages.

Custom loggers must not trigger the machine recursively. If a logger throws,
the exception propagates, the machine restores its internal source state, and
the request-processing flag is cleared. Already executed callbacks or emitted
logs cannot be undone. Construction/validation exceptions and exceptions
that escape trigger processing are surfaced directly, not automatically logged.

Optional logging is available in the published `0.1.0-preview.2` package.

Default implementations (`GuardRegistry`, `PermissionRegistry`, `CallbackRegistry`) are provided for simple in-memory registration.

## Related guides

- [Transition processing](TransitionProcessing.md)
- [Failure handling and rollback](FailureHandling.md)
- [Scope, testing, and development](ScopeAndDevelopment.md)
- [Framework overview](Framework.md)
