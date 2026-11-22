# Visual Studio extension prototype

The solution targets Visual Studio 2026 18.11 or later with an out-of-process
extension. Visual Studio 2022 support is deferred.

In Visual Studio, choose **Tools > Open Avak State Graph**. In the tool window,
use **Browse...** to select a state-machine XML file, or enter its path and
choose **Load graph**. Valid configurations are displayed as a directed graph
laid out by MSAGL; the initial state is marked with a play symbol and edge
labels include trigger, source, guard, permission, and lifecycle callback
metadata. Invalid XML/configuration and file access errors appear in the tool
window instead of leaving an old graph displayed.

The graph is read-only. XML navigation, editing, and IDE integration testing
are not implemented yet. Graph loading validates XML using the framework
loader and does not execute application callbacks.

To build and test from the repository root:

```powershell
dotnet build src\VSExt\Avak.StateMachine.VisualStudio.slnx
dotnet test src\VSExt\Avak.StateMachine.VisualStudio.slnx
```

Tests cover graph layout with the traffic-light and network-connection sample
configurations, transition labels, self-transitions, and invalid configuration.
