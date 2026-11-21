


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

