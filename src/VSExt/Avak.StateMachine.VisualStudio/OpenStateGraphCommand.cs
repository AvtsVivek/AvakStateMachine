namespace Avak.StateMachine.VisualStudio;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;

[VisualStudioContribution]
internal sealed class OpenStateGraphCommand : Command
{
    public OpenStateGraphCommand(VisualStudioExtensibility extensibility)
        : base(extensibility)
    {
    }

    public override CommandConfiguration CommandConfiguration => new("%AvakStateMachine.OpenStateGraph.DisplayName%")
    {
        Placements = [CommandPlacement.KnownPlacements.ToolsMenu],
        TooltipText = "%AvakStateMachine.OpenStateGraph.TooltipText%",
    };

    public override Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken) =>
        this.Extensibility.Shell().ShowToolWindowAsync<StateGraphToolWindow>(activate: true, cancellationToken);
}
