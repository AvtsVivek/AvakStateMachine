# Scope and Development

## Scope and Limitations

### Current Version
- **Single-threaded only**: No concurrent event processing
- **Flat states only**: No hierarchy or sub-states
- **Synchronous**: No async/await support
- **No hot reload**: Configuration changes require machine restart
- **No schema versioning**: XML format is fixed

### Future Considerations (Out of Scope for Now)
- Parallel states (orthogonal regions)
- Hierarchical states
- Async transitions and callbacks
- Configuration versioning
- Hot reloading
- Concurrent event processing with proper synchronization
## Testing Strategy

### Framework Tests (`Avak.StateMachine.Core.Tests`)
Located in `src/Framework/Avak.StateMachine.Core.Tests/`:
- XML parsing and validation
- Transition resolution (source-aware)
- Guard/permission evaluation
- Callback execution and error handling
- Nested request detection
- Rollback behavior
- Exception types and messages

**All tests are deterministic** and avoid external dependencies.

Sample-specific tests are listed with each entry in the
[samples overview](../Samples.md). The samples solution runs the complete set.
## Building and Testing

From the repository root, run solution's tests with:

```powershell
dotnet test src\Samples\Samples.slnx
dotnet test src\Framework\Avak.StateMachine.slnx
dotnet test src\VSExt\Avak.StateMachine.VisualStudio.slnx
```

### Framework Solution
```bash
cd src/Framework
dotnet build Avak.StateMachine.slnx -v minimal
dotnet test Avak.StateMachine.slnx --logger:console --verbosity normal
```

### Samples Solution
```bash
cd src/Samples
dotnet build Samples.slnx -v minimal
dotnet test Samples.slnx --logger:console --verbosity normal
```
## API Stability

The following are considered stable and ready for external use:
- All public types in `Avak.StateMachine.Core` namespace
- XML configuration format
- Exception hierarchy
- Extension point interfaces (`IGuard`, `IPermission`, `ITransitionCallback`)

Internal implementation details may change without notice.
## Security Considerations

1. **XML Parsing**: DTDs and external entity resolution are prohibited by default
2. **Validation**: All configuration is validated at load time
3. **No Code Execution**: State names, trigger names, and callback names are strings with no code injection; actual logic is supplied through typed interfaces
4. **Callback Isolation**: Callbacks execute in the caller's context; isolation requires application-level design
## Next Steps

Future enhancements may include:
1. Additional sample applications demonstrating different use cases
2. Performance optimizations for high-frequency triggers
3. Hierarchical states for complex workflows
4. Async/await support for long-running callbacks
5. Integration with popular DI containers
6. Enhanced diagnostics and tracing

## Related guides

- [Framework overview](Framework.md)
- [Samples](../Samples.md)
- [Quick Start troubleshooting](../QuickStart/Troubleshooting.md)
