namespace Avak.StateMachine.VisualStudio;

using Microsoft.VisualStudio.Extensibility;

[VisualStudioContribution]
internal class ExtensionEntrypoint : Extension
{
    public override ExtensionConfiguration ExtensionConfiguration => new()
    {
        Metadata = new(
            id: "Avak.StateMachine.VisualStudio",
            version: this.ExtensionAssemblyVersion,
            publisherName: "AvakStateMachine",
            displayName: "Avak State Machine",
            description: "Visual Studio tooling for Avak State Machine configurations."),
    };
}
