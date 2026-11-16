# Basic WPF State Machine With Named States and ViewModel

This WPF sample demonstrates the framework's named `Idle` and `Running` states,
with UI state exposed through a small `MainWindowViewModel`. It does not define
a C# class for each state.

## What it demonstrates

- XML configuration loaded when the window is created
- UI-sourced `Start` and `Stop` triggers
- WPF bindings for current state, status, and button availability
- Rejected transitions reported in the UI without changing the state
- Testable state interaction separated from the WPF window

## Run

From the repository root on Windows with the .NET Windows Desktop runtime:

```shell
dotnet run --project src/Samples/200800_BasicWpfWithNamedStatesAndViewModel/BasicWpfWithNamedStatesAndViewModel/BasicWpfWithNamedStatesAndViewModel.csproj
```

The initial state is `Idle`. **Start** transitions to `Running`; **Stop**
returns to `Idle`. The unavailable action is disabled in each state.

## Test

The view-model tests run without creating a WPF window:

```shell
dotnet test src/Samples/200800_BasicWpfWithNamedStatesAndViewModel/BasicWpfWithNamedStatesAndViewModel.Tests/BasicWpfWithNamedStatesAndViewModel.Tests.csproj
```
