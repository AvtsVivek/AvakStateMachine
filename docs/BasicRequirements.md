


1. XML Based state configuration
2. Source Aware triggers.
3. The framework will provide Guards, Permissions, and Lifecycle Callbacks 
4. When guards deny, callbacks throw, or requests are nested, transition do not succeed. Roll back to the from state.
5. Parallel State transitions will be added a bit later
6. Schema versioning, Hot reloading and Concurrent event processing will be out of scope.
7. Hierarchy, Async support will be added a bit later.
8. Regarding Rollback, the following points needs to kept in mind.

•	A denied guard or permission leaves the machine in the source state and runs no lifecycle callbacks.
•	A nested request is rejected and causes the outer transition to fail and restore its source state.
•	If a lifecycle callback throws, the transition fails and the machine restores its source state.
•	Any external side effects already performed by callbacks are not automatically undone. Callbacks should avoid irreversible work, or applications must provide explicit compensation if needed.

9. Samples will be added as we go along. Each sample will have unit tests. The samples should help in understanding the framework and its use cases. The solution file is `src/Samples/Samples.slnx`.

10. Documentation will be added to the docs folder
11. Optional logging is supported by the framework through `ILogger<StateMachine>`.
    Applications select their own logging provider.
12. Dependency injection is demonstrated in samples. The core framework remains
    independent of a DI container.
13. Modules support for large application
14. Future sample ideas:
    •	Document approval workflow (`Draft` → `InReview` → `Approved`/`Rejected`)
        to demonstrate permissions, source-aware triggers, and in-memory auditing.
    •	Order fulfillment (`Created` → `Confirmed` → `Packed` → `Shipped` →
        `Delivered`) to demonstrate guards and cancellation rules.
    •	Background job lifecycle (`Queued` → `Running` → `Paused` →
        `Completed`/`Failed`) to demonstrate retry and cancellation transitions;
        model state changes only, without adding asynchronous execution.
    •	Turnstile/access gate (`Locked` ↔ `Unlocked`) as a compact beginner example
        for accepted and rejected triggers.

    The document approval workflow is the suggested next sample. Keep future
    sample tests deterministic, using in-memory fakes rather than networks,
    clocks, devices, or external services.

15. Visual Studio extension

    A separate Visual Studio extension will help application developers author,
    understand, and validate AvakStateMachine configuration. Keep its solution
    separate from the framework and samples at
    `src/VSExt/Avak.StateMachine.VisualStudio.slnx`.

    Initial capabilities:
    • Provide XML IntelliSense, snippets, and AvakStateMachine-specific
      validation feedback.
    • Display a read-only visual graph of states and transitions, including
      trigger sources, guards, permissions, callbacks, and self-transitions.
      Selecting a graph item should navigate to its XML configuration.
    • Report configuration errors and offer quick fixes when a correction is
      unambiguous. Reuse the framework parser and validation rules rather than
      maintaining a separate implementation.
    • Provide guided editing for adding states and transitions, validating the
      resulting XML before saving.
    • Where possible, navigate from configured extension names (guards,
      permissions, callbacks, and state classes) to their C# implementations
      and registrations.

    Possible later capabilities:
    • Static analysis for unreachable states, states without outgoing
      transitions, and extension registrations that appear unused.
    • Generate C# skeletons or registration snippets for guards, permissions,
      callbacks, and state classes.
    • Generate starter tests and export state graphs as SVG or Mermaid.

    Prioritize IntelliSense, validation, and a read-only graph for the first
    usable version. Defer graph-based editing until synchronization between the
    visual editor and XML can be made reliable. The extension must not execute
    application callbacks merely to render or validate a graph.

    Graph visualization options:

    | Option | Best fit | Tradeoff |
    | --- | --- | --- |
    | [MSAGL](https://github.com/microsoft/automatic-graph-layout) | .NET graph layout and WPF viewing | Its WPF viewer cannot be embedded directly in VisualStudio.Extensibility Remote UI; the current prototype uses MSAGL for layout. |
    | [GraphX](https://github.com/panthernet/GraphX) | Interactive WPF graph control; Apache 2.0 | Check maintenance and Visual Studio hosting compatibility. |
    | [Graphviz](https://graphviz.org/) | Automatic layout and SVG export | Interactive selection and XML navigation require additional UI work. |
    | [Cytoscape.js](https://js.cytoscape.org/) | Interactive, customizable graphs; MIT | Requires a web-based UI and integration with Visual Studio. |
    | [Mermaid](https://mermaid.js.org/syntax/stateDiagram.html) | Simple state diagrams and documentation | Less suited to a rich interactive IDE designer. |
    | Commercial diagramming library | Polished editing and vendor support | Licensing cost and dependency on a vendor. |

    Evaluate graph options against self-transitions, multiple edges, labels,
    and click-to-XML navigation, as well as the chosen Visual Studio hosting
    model. The current MSAGL layout prototype does not yet include a viewer
    or XML navigation.
