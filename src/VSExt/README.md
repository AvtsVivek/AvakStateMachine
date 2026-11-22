# Visual Studio extension prototype

The solution targets Visual Studio 2026 18.11 or later with an out-of-process
extension. Visual Studio 2022 support is deferred.

The current prototype contains a graph-layout foundation, not a usable
Visual Studio graph viewer: it has no command, tool window, or XML navigation
yet. The graph builder validates XML using the framework loader and uses MSAGL
to lay out states and transitions without running application callbacks.

To build and test from the repository root:

```powershell
dotnet build src\VSExt\Avak.StateMachine.VisualStudio.slnx
dotnet test src\VSExt\Avak.StateMachine.VisualStudio.slnx
```

Tests cover graph layout with the traffic-light and network-connection sample
configurations, transition labels, self-transitions, and invalid configuration.
