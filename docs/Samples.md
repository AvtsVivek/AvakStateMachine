# Samples

This page summarizes the examples included with AvakStateMachine. It will be
updated as new samples are added. Each sample has its own guide and unit tests;
the samples solution is `src/Samples/Avak.StateMachine.Samples.slnx`.

## Console Recorder

**Location:** `src/Samples/200500_ConsoleRecorder/`

A small recorder lifecycle example with `Stopped`, `Recording`, and `Paused`
states. It demonstrates XML configuration, transitions, lifecycle callbacks,
and preserving state when a trigger is invalid.

- [Sample guide and state graph](../src/Samples/200500_ConsoleRecorder/README.md)
- [Sample project](../src/Samples/200500_ConsoleRecorder/Avak.StateMachine.Samples.ConsoleRecorder/)
- [Unit tests](../src/Samples/200500_ConsoleRecorder/Avak.StateMachine.Samples.ConsoleRecorder.Tests/)

## Basic WPF State Machine Without State Classes

**Location:** `src/Samples/200800_BasicWpfStateMachineWithoutStateClasses/`

A Windows WPF UI showing `Idle` and `Running` states without defining a C# class
for each state. It demonstrates XML configuration, UI-sourced triggers, and
state-dependent button bindings.

- [Sample guide](../src/Samples/200800_BasicWpfStateMachineWithoutStateClasses/README.md)
- [WPF application](../src/Samples/200800_BasicWpfStateMachineWithoutStateClasses/Avak.StateMachine.Samples.BasicWpfStateMachineWithoutStateClasses/)
- [Unit tests](../src/Samples/200800_BasicWpfStateMachineWithoutStateClasses/Avak.StateMachine.Samples.BasicWpfStateMachineWithoutStateClasses.Tests/)

## Adding a sample

When adding a sample, include its project and unit tests in the samples
solution, provide a README alongside the sample, and add a short summary and
links here.
