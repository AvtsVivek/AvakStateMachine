# Quick Start

## 30-Second Overview

AvakStateMachine is a .NET framework for building state machines from XML configuration. Define states and transitions in XML, implement guards/permissions/callbacks in C#, inject them into the machine, and trigger transitions with automatic validation and error handling.
## Installation

The framework is published on NuGet.org as [`Avak.StateMachine.Core`](https://www.nuget.org/packages/Avak.StateMachine.Core).
Install the current preview in your application project with:

```shell
dotnet add package Avak.StateMachine.Core --version 0.1.0-preview.2
```

Preview 2 includes registered state classes and optional logging. The earlier
`0.1.0-preview.1` package does not include those features.

The package targets .NET 10. When working on the framework in this repository,
you can reference the project at
`src/Framework/Avak.StateMachine.Core/Avak.StateMachine.Core.csproj` instead.
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

For a complete console example with XML configuration, guards, permissions,
callbacks, and tests, see the [Recorder sample](../../src/Samples/200500_ConsoleRecorder/README.md).

## Continue

- [Add guards, permissions, and callbacks; use source-aware triggers](ExtensionsAndTriggers.md)
- [Explore a complete example and common patterns](ExamplesAndPatterns.md)
- [Troubleshoot configuration and transition problems](Troubleshooting.md)
