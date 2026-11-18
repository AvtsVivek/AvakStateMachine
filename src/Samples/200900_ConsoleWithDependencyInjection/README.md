# Console Sample With Dependency Injection

This sample uses `Microsoft.Extensions.DependencyInjection` to construct a
named-state machine, its guard and callback registries, and the dependencies
used by those extensions. The framework itself does not depend on a DI
container; the registration code belongs to the application.

## How it works

- XML defines the `Idle` to `Running` transition and names its guard and callback.
- `AddSampleServices` loads the configuration and registers a scoped machine,
  scoped registries, and scoped guard, callback, and `SessionData` instances.
- The guard checks `SessionData.IsReady`. On success, the callback records the
  transition in the same scope's `SessionData`.
- The console application creates a scope, marks the session ready, and triggers
  `Start`. It reports a failed transition as an error.

Each scope has its own machine and session data; resolving the machine again
within that scope returns the same instance. Dispose the scope when the session
ends. A scope does not make the machine thread-safe: use each machine
single-threadedly, and choose lifetimes appropriate to your application.
The configuration is shared and must not be modified after registration.

## Sessions and DI scopes

A session and a DI scope are not the same concept:

- A **session** is an application concept, such as one recording session or
  workflow. `SessionData` is an ordinary sample class, not a special .NET DI type.
- A **DI scope** is a container lifetime boundary. Within it, each scoped service
  is created when needed and reused on subsequent resolutions. Separate scopes
  receive separate instances.

This sample chooses one scope per machine session. The application, guard, and
callback therefore share the same `SessionData` instance: when the application
sets `IsReady`, the guard sees that value, and the callback records its event in
that same object.

```text
DI scope
  StateMachine
  SessionData
  Guard ------> same SessionData
  Callback ---> same SessionData
```

DI does not understand application sessions automatically; the application
establishes that relationship through registrations and scope usage. Scoping
is a sample design choice, not a framework requirement.

In `Program.cs`, `provider.CreateScope()` creates the scope, and `using var scope`
disposes it at the end of the method. Disposing a scope also disposes the
disposable services owned by that scope.

## Service provider validation

`Program.cs` builds the provider with two safety checks enabled:

```csharp
using var provider = services.BuildServiceProvider(new ServiceProviderOptions
{
    ValidateScopes = true,
    ValidateOnBuild = true
});
```

Neither option creates a scope or changes the lifetime registrations.

### ValidateScopes

`ValidateScopes = true` detects incorrect scoped-service usage, including
resolving a scoped service directly from the root provider and injecting a
scoped service into a singleton through a dependency chain the container can
inspect.

For example, resolving the sample's machine from the root provider would throw:

```csharp
var machine = provider.GetRequiredService<StateMachine>();
```

Resolve it from the explicit scope instead:

```csharp
var machine = scope.ServiceProvider.GetRequiredService<StateMachine>();
```

Setting `ValidateScopes` to `false` does not disable scoping. The sample still
works because it uses scopes correctly, but accidental root resolution would
be allowed. Such an instance would belong to the root scope and remain there
until the root provider is disposed, potentially sharing mutable machine and
session state longer than intended.

### ValidateOnBuild

`ValidateOnBuild = true` checks registered dependency graphs that the container
can inspect when `BuildServiceProvider()` runs. For example, omitting the
`SessionData` registration would expose missing constructor dependencies in the
registered guard and callback.

Setting it to `false` postpones those checks until the affected services are
resolved, so the provider could build successfully and fail later.

This option does not instantiate every service or execute registration factory
lambdas. Errors inside the registry factories, for example, can still surface
when the machine is resolved. It is not a substitute for exercising the
application and testing its registrations.

Keep both options enabled in this sample to detect lifetime and dependency
wiring mistakes early.

## Run and test

From the repository root:

```shell
dotnet run --project src/Samples/200900_ConsoleWithDependencyInjection/ConsoleWithDependencyInjection/ConsoleWithDependencyInjection.csproj
dotnet test src/Samples/200900_ConsoleWithDependencyInjection/ConsoleWithDependencyInjection.slnx
```

The program prints `Current state: Running` and
`Recorded event: Idle -> Running`. Tests cover guard denial, successful
callback execution, and isolation between scopes.
