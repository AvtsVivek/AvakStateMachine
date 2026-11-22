# Troubleshooting

## Troubleshooting

### Configuration Won't Load
- Check XML syntax (must be valid XML)
- Verify `initialState` references an existing state
- All transition `to` attributes must reference existing states
- Remove unknown attributes/elements and duplicate states or transitions with
  the same state, trigger, and source
- Verify every configured guard, permission, and callback name is registered
  before constructing the `StateMachine`. An unresolved name throws
  `UnresolvedExtensionException` during construction.

### Guard Not Working
- Verify guard name in XML matches registration name
- Register the guard before constructing the machine
- Check that `IGuard.Evaluate()` returns `false` (not throws exception)
- Guard returns `false` = transition denied

### Permission Not Working
- Verify permission name in XML matches registration name
- Register the permission before constructing the machine
- A missing permission is an error, not an implicit allow

### Callback Not Executing
- Verify callback name in XML matches registration name
- Check callback is registered before creating the machine
- Remember: if any callback throws, entire transition fails
- A missing callback is an error, not a no-op

### State Didn't Change
- Check `TransitionResult.Success` - transition may have failed
- Verify trigger name and current state are correct
- Look at `TransitionResult.ErrorMessage` for details
## Documentation Links

- **[Quick Start](QuickStart.md)**: Installation and first state machine
- **[Extensions and triggers](ExtensionsAndTriggers.md)**: Guards, callbacks, sources, and transition data
- **[Examples and patterns](ExamplesAndPatterns.md)**: Traffic light and common XML patterns
- **[Framework Reference](../Framework/Framework.md)**: Complete API documentation
- **[Samples](../Samples.md)**: Overview of available samples
- **[Recorder Sample](../../src/Samples/200500_ConsoleRecorder/README.md)**: Full example with tests
- **[Basic Requirements](../BasicRequirements.md)**: Original specification
## Next Steps

1. **Try the Quick Start** above
2. **Read the Recorder Sample** for a complete example
3. **Review Framework Reference** for detailed API documentation
4. **Build Your Own State Machine**: Apply to your domain

Happy state machine building! 🎯
