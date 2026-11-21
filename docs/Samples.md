# Samples

This page summarizes the examples included with AvakStateMachine. It will be
updated as new samples are added. Each sample has its own guide and unit tests;
the samples solution is `src/Samples/Samples.slnx`. The framework supports named
states with optional C# state objects. See
[State Representation](Framework.md#state-representation).

## Console Recorder

**Location:** `src/Samples/200500_ConsoleRecorder/`

A small recorder lifecycle example with `Stopped`, `Recording`, and `Paused`
states. It demonstrates XML configuration, transitions, lifecycle callbacks,
and preserving state when a trigger is invalid.

- [Sample guide and state graph](../src/Samples/200500_ConsoleRecorder/README.md)
- [Sample project](../src/Samples/200500_ConsoleRecorder/ConsoleRecorder/)
- [Unit tests](../src/Samples/200500_ConsoleRecorder/ConsoleRecorder.Tests/)

## Basic WPF State Machine With Named States

**Location:** `src/Samples/200600_BasicWpfWithNamedStates/`

A Windows WPF UI showing `Idle` and `Running` states without defining a C# class
for each state or using a view model. The window code-behind loads the XML,
triggers the machine, and updates the controls directly.

- [Sample guide](../src/Samples/200600_BasicWpfWithNamedStates/README.md)
- [WPF application](../src/Samples/200600_BasicWpfWithNamedStates/BasicWpfWithNamedStates/)
- [Unit tests](../src/Samples/200600_BasicWpfWithNamedStates/BasicWpfWithNamedStates.Tests/)

## Basic WPF State Machine With Named States and ViewModel

**Location:** `src/Samples/200800_BasicWpfWithNamedStatesAndViewModel/`

The same `Idle`/`Running` example using a small view model and WPF data
bindings. Its tests exercise the view model without launching a WPF window.

- [Sample guide](../src/Samples/200800_BasicWpfWithNamedStatesAndViewModel/README.md)
- [WPF application](../src/Samples/200800_BasicWpfWithNamedStatesAndViewModel/BasicWpfWithNamedStatesAndViewModel/)
- [Unit tests](../src/Samples/200800_BasicWpfWithNamedStatesAndViewModel/BasicWpfWithNamedStatesAndViewModel.Tests/)

## Console With Dependency Injection

**Location:** `src/Samples/200900_ConsoleWithDependencyInjection/`

A console application using .NET dependency injection to construct a scoped
machine and its guard and callback dependencies. Its tests demonstrate guard
denial, callback execution, and isolation between scopes.

- [Sample guide](../src/Samples/200900_ConsoleWithDependencyInjection/README.md)
- [Console application](../src/Samples/200900_ConsoleWithDependencyInjection/ConsoleWithDependencyInjection/)
- [Unit tests](../src/Samples/200900_ConsoleWithDependencyInjection/ConsoleWithDependencyInjection.Tests/)

## WPF Traffic Light With Dependency Injection

**Location:** `src/Samples/201500_WpfTrafficLightWithDI/`

A WPF traffic-light application using DI to construct a scoped state machine,
guard, callback, and view model. The operational checkbox demonstrates guard
denial, while tests cover the light cycle and scope isolation.

- [Sample guide and state graph](../src/Samples/201500_WpfTrafficLightWithDI/README.md)
- [WPF application](../src/Samples/201500_WpfTrafficLightWithDI/WpfTrafficLightWithDI/)
- [Unit tests](../src/Samples/201500_WpfTrafficLightWithDI/WpfTrafficLightWithDI.Tests/)

## Console Document Workflow

**Location:** `src/Samples/201900_ConsoleDocumentWorkflow/`

A document approval workflow demonstrating registered C# state classes,
source-aware transitions, reviewer permissions, and an in-memory audit.

- [Sample guide and state graph](../src/Samples/201900_ConsoleDocumentWorkflow/README.md)
- [Console application](../src/Samples/201900_ConsoleDocumentWorkflow/ConsoleDocumentWorkflow/)
- [Unit tests](../src/Samples/201900_ConsoleDocumentWorkflow/ConsoleDocumentWorkflow.Tests/)

## Windows Forms Self-Transition: Network Connection

**Location:** `src/Samples/202400_WinFormsSelfTransitionNetworkConnection/`

A Windows Forms application demonstrating a `Connected` self-transition with
a fake network connection and a visible lifecycle trace. Tests run without a
form or real network access.

- [Sample guide and state graph](../src/Samples/202400_WinFormsSelfTransitionNetworkConnection/README.md)
- [Windows Forms application](../src/Samples/202400_WinFormsSelfTransitionNetworkConnection/WinFormsSelfTransitionNetworkConnection/)
- [Unit tests](../src/Samples/202400_WinFormsSelfTransitionNetworkConnection/WinFormsSelfTransitionNetworkConnection.Tests/)

## Adding a sample

When adding a sample, include its project and unit tests in the samples
solution, provide a README alongside the sample, and add a short summary and
links here.
