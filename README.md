# AvakStateMachine

A reusable .NET state-machine framework with XML-based configuration, source-aware
triggers, guards, permissions, and lifecycle callbacks.

## Give a Star! :star:
If you like or find this project useful for learning, please consider giving it a star. Thanks!

## Documentation

Start with the [Quick Start](docs/QuickStart.md), or see the [framework
reference](docs/Framework.md) for API and configuration details. The original
[basic requirements](docs/BasicRequirements.md) and
[implementation summary](docs/IMPLEMENTATION_SUMMARY.md) are also available.

Browse the [samples overview](docs/Samples.md) for a summary of available examples and links to their guides and tests.

Considering a contribution? See the [contribution guide](docs/CONTRIBUTING.md).

## Solutions

- Framework and framework tests: `src/Framework/Avak.StateMachine.slnx`
- Samples and sample tests: `src/Samples/Avak.StateMachine.Samples.slnx`

## Code style

The root [.editorconfig](.editorconfig) shares formatting and C# style preferences
across the framework, samples, and tests. It uses four-space C# indentation,
two-space XML/project-file indentation, UTF-8 encoding, and LF line endings.
C# style diagnostics are suggestions, not build errors. Markdown trailing spaces
are preserved for intentional line breaks.

The root [.gitattributes](.gitattributes) also keeps Git-managed text files on LF
line endings. If Visual Studio prompts to normalize line endings, select Unix
(LF). Reload externally changed files rather than keeping stale editor contents;
save any unsaved edits before reloading.
