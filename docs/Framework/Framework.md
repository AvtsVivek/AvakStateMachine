# Framework Overview

## Overview

**AvakStateMachine** is a reusable, generic .NET state machine framework that provides a clean, declarative way to manage state transitions in applications. State machines are configured via XML, validated at startup, and executed with full support for guards, permissions, and lifecycle callbacks.

The framework emphasizes **safety and explicitness**: transitions fail cleanly when guards/permissions deny them or when callbacks throw exceptions, with automatic rollback to preserve machine integrity.

## Reference

- [Architecture and states](ArchitectureAndStates.md)
- [API reference](ApiReference.md)
- [XML configuration](XmlConfiguration.md)
- [Transition processing and lifecycle](TransitionProcessing.md)
- [Failure handling and rollback](FailureHandling.md)
- [Logging](Logging.md)
- [Scope, testing, and development](ScopeAndDevelopment.md)

For a guided introduction, see the [Quick Start](../QuickStart/QuickStart.md).
