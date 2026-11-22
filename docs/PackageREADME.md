# Avak.StateMachine.Core

A synchronous, flat, single-threaded state-machine framework for .NET 10.
Configure transitions in XML and supply named guards, permissions, and lifecycle
callbacks in C#.

States can optionally use registered C# state objects for state-specific
checks and entry/exit behavior.

Optional logging uses `ILogger<StateMachine>` from
`Microsoft.Extensions.Logging.Abstractions`. Applications select the logging
provider; existing constructor calls work without a logger.

This is a preview package; its API and behavior may change.

See the [Quick Start](https://github.com/AvtsVivek/AvakStateMachine/blob/main/docs/QuickStart/QuickStart.md)
and [framework reference](https://github.com/AvtsVivek/AvakStateMachine/blob/main/docs/Framework/Framework.md)
for configuration, usage, and rollback behavior.
