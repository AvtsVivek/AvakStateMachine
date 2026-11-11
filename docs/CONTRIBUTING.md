# Contributing

Thank you for considering a contribution to AvakStateMachine. This guide
describes the project structure and the checks to run before submitting changes.

## Before you start

- For a small, focused fix, open an issue or pull request with a clear
  description of the problem and proposed change.
- For a significant API or behavior change, discuss the design first so its
  scope and compatibility implications can be considered before implementation.
- Keep changes focused. Avoid bundling unrelated cleanup with a feature or fix.

## Project layout

- `src/Framework/Avak.StateMachine.Core/` contains the reusable framework.
- `src/Framework/Avak.StateMachine.Core.Tests/` contains framework guarantee
  tests. These belong to `src/Framework/Avak.StateMachine.slnx`.
- `src/Samples/` contains usage examples and their unit tests. Sample projects
  and tests belong to `src/Samples/Avak.StateMachine.Samples.slnx`.
- `docs/` contains framework and contributor documentation. Each sample also
  has its own README beside the sample.

When adding a sample, add its tests to the samples solution, document how it
works in the sample folder, and add an entry with links to [Samples.md](Samples.md).

## Development requirements

- Preserve the framework's current scope: synchronous, flat state machines and
  single-threaded use. Do not introduce hierarchy, async execution, parallel
  states, hot reload, schema versioning, or concurrent event processing without
  prior agreement.
- Treat XML configuration as untrusted input. Keep DTD and external entity
  resolution disabled, and validate configuration before it is used.
- Keep failures explicit. Do not silently ignore invalid configuration,
  unresolved required behavior, or callback failures.
- A nested request must not allow the outer transition to succeed, even if a
  callback catches the nested-request error. Rollback restores the framework's
  internal state only; it cannot undo external side effects from callbacks.
- Add or update tests for behavior changes. Framework guarantees belong in the
  framework test project; sample-specific behavior belongs in the relevant
  sample test project. Keep tests deterministic and independent of networks,
  wall-clock timing, processes, and physical devices.
- Update relevant documentation when changing public APIs, behavior, or samples.

## Style

The root [.editorconfig](../.editorconfig) applies to the framework, samples,
and tests. Use four spaces for C# and two spaces for XML and project files.
Git-managed text files use LF line endings as specified in
[.gitattributes](../.gitattributes). Do not reformat unrelated files.

## Build and test

Run both solutions from the repository root before submitting:

```shell
dotnet build src/Framework/Avak.StateMachine.slnx
dotnet test src/Framework/Avak.StateMachine.slnx
dotnet build src/Samples/Avak.StateMachine.Samples.slnx
dotnet test src/Samples/Avak.StateMachine.Samples.slnx
```

Check C# style without modifying files:

```shell
dotnet format style src/Framework/Avak.StateMachine.slnx --severity info --verify-no-changes
dotnet format style src/Samples/Avak.StateMachine.Samples.slnx --severity info --verify-no-changes
```

If a command fails, investigate and report the failure rather than claiming the
change is verified. Mention any checks you could not run in your contribution.

## Pull request checklist

- [ ] The change has a focused description and stays within the agreed scope.
- [ ] Tests cover the changed behavior and are deterministic.
- [ ] Both solutions build and their tests pass.
- [ ] Style verification passes, or any remaining diagnostics are explained.
- [ ] Relevant documentation and sample indexes are updated.
- [ ] No unrelated changes, generated build output, or secrets are included.
