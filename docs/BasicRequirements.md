


1. XML Based state configuration
2. Source Aware triggers.
3. The framework will provide Guards, Permissions, and Lifecycle Callbacks 
4. When guards deny, callbacks throw, or requests are nested, transition do not succeed. Roll back to the from state.
5. Parallel State transitions will be added a bit later
6. Schema versioning, Hot reloading and Concurrent event processing will be out of scope.
7. Hierarchy, Async support will be added a bit later.
8. Regarding Rollback, the following points needs to kept in mind.

•	A denied guard or permission leaves the machine in the source state and runs no lifecycle callbacks.
•	A nested request is rejected and does not change the outer transition’s state.
•	If a lifecycle callback throws, the transition fails and the machine restores its source state.
•	Any external side effects already performed by callbacks are not automatically undone. Callbacks should avoid irreversible work, or applications must provide explicit compensation if needed.

9. Samples will be added as we go along. Each sample will have unit tests. The samples should help in understanding the framework and its use cases. The solution file will be C:\Trials\AvakStateMachine\src\Samples\Avak.StateMachine.Samples.slnx

10. Documentation will be added to the docs folder
11. We need to add logging to the framework.
12. Dependency Injection support should be added.

