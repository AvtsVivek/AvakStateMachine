# Basic WPF State Machine With Named States

This WPF sample drives the framework directly from the window's code-behind.
It uses the framework's named `Idle` and `Running` states without defining a
C# class for each state or using a view model.

## What it demonstrates

- XML configuration loaded when the window is created
- UI-sourced `Start` and `Stop` triggers
- Code-behind updating the state label, status message, and button availability
- Rejected transitions leaving the machine in its current state

## Run

From the repository root on Windows with the .NET Windows Desktop runtime:

```shell
dotnet run --project src/Samples/200600_BasicWpfWithNamedStates/BasicWpfWithNamedStates/BasicWpfWithNamedStates.csproj
```

The initial state is `Idle`. **Start** transitions to `Running`; **Stop**
returns to `Idle`. The unavailable action is disabled in each state.

## Test

The tests load the sample XML and verify its transitions without creating a
WPF window:

```shell
dotnet test src/Samples/200600_BasicWpfWithNamedStates/BasicWpfWithNamedStates.Tests/BasicWpfWithNamedStates.Tests.csproj
```

The sample project uses WPF and therefore requires Windows to run.
