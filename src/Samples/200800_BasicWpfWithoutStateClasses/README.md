# Basic WPF State Machine Without State Classes

This sample demonstrates a small WPF interface driven by AvakStateMachine. It
uses the framework's string-based state names (`Idle` and `Running`); it does
not define a C# class for each state.

## What it demonstrates

- XML configuration loaded when the window is created
- UI-sourced `Start` and `Stop` triggers
- WPF bindings for current state, status, and button availability
- Rejected transitions reported in the UI without changing the state
- Testable state interaction separated from the WPF window

The sample has no guards or permissions and does not use real services.

## Run

From the repository root on Windows with the .NET Windows Desktop runtime:

```shell
dotnet run --project src/Samples/200800_BasicWpfWithoutStateClasses/Avak.StateMachine.Samples.BasicWpfWithoutStateClasses/Avak.StateMachine.Samples.BasicWpfWithoutStateClasses.csproj
```

The initial state is `Idle`. **Start** transitions to `Running`; **Stop**
returns to `Idle`. The unavailable action is disabled in each state.

## Test

The tests exercise the view model and XML-independent state handling without
launching a WPF window:

```shell
dotnet test src/Samples/Avak.StateMachine.Samples.slnx
```

The sample project uses WPF and therefore requires Windows to run.
