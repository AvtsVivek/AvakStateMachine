# Visual Studio Extension UI Hosting Research

This note records the investigation into hosting a state-graph UI in the
AvakStateMachine Visual Studio extension. The extension currently uses the
out-of-process VisualStudio.Extensibility model and Remote UI, targeting
Visual Studio 2026.

## Hosting options

### Out-of-process Remote UI

The extension runs outside the Visual Studio process and presents its UI
through Remote UI. This aligns with the modern VisualStudio.Extensibility
model and isolates Visual Studio from extension-process failures. The
framework and graph-loading logic can remain in the extension process.

Remote UI is not equivalent to unrestricted WPF. A regular WPF project
reference does not, by itself, make a custom `UserControl` available across
the process boundary. Remote UI has a supported set of controls and
serialization behaviors, so a complex graph surface may be awkward to build
using only its standard controls.

### Traditional in-process WPF extension

A traditional in-process Visual Studio extension can host regular WPF
controls and integrate directly with the Visual Studio shell. This can be a
natural fit for a custom interactive graph viewer. It also brings tighter
coupling to Visual Studio APIs and runtime requirements, and extension code
runs inside the IDE process.

### Hybrid in-process/out-of-process extension

A hybrid extension keeps the VisualStudio.Extensibility component
out-of-process while packaging traditional VSSDK components, including a WPF
control, in the same VSIX. Microsoft provides an
[official composite-extension sample](https://github.com/microsoft/VSExtensibility/tree/main/New_Extensibility_Model/Samples/ExtensionWithTraditionalComponents)
that demonstrates a Remote UI tool window using a WPF `UserControl` loaded
in-process. The control is referenced from Remote UI XAML by its assembly
identity.

This is more than adding a WPF project reference. The sample uses a VSIX
container project to package the extension and the traditional component.
The in-process control assembly must be strong-named and configured for
Visual Studio to load it. The sample also notes that this packaging disables
hot reload and requires restarting Visual Studio after installation.

## Preliminary assessment

The hybrid approach may suit AvakStateMachine if the graph viewer needs
interactive WPF rendering while the XML parsing, validation, and graph
construction remain out-of-process. It can preserve the current modern
extension architecture while allowing a custom WPF visual surface.

This is a preliminary direction, not a confirmed implementation decision.
The Microsoft sample uses different target frameworks for its out-of-process
extension and in-process container. Compatibility with the current
Visual Studio 2026 / .NET 10 extension target, as well as MSAGL viewer
compatibility, must be verified before adopting the pattern.

## Follow-up checks

Before changing the solution structure:

1. Build a minimal hybrid VSIX using the current Visual Studio 2026 SDK and
   verify that it installs and loads in the experimental instance.
2. Verify that a strong-named WPF control can be referenced from the
   Remote UI XAML and render in the tool window.
3. Check whether the desired graph library and its dependencies can be loaded
   by the in-process component.
4. Confirm the packaging, debugging, installation, and Visual Studio restart
   workflow is acceptable.

If these checks pass, keep parsing and graph-model generation
out-of-process, and limit the in-process component to Visual Studio-specific
WPF presentation and interaction.
