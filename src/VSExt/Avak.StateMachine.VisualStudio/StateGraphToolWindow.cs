namespace Avak.StateMachine.VisualStudio;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Shell.FileDialog;
using Microsoft.VisualStudio.Extensibility.ToolWindows;
using Microsoft.VisualStudio.RpcContracts.RemoteUI;

[VisualStudioContribution]
internal sealed class StateGraphToolWindow : ToolWindow
{
    public StateGraphToolWindow(VisualStudioExtensibility extensibility)
        : base(extensibility)
    {
        this.Title = "Avak State Graph";
    }

    public override ToolWindowConfiguration ToolWindowConfiguration => new()
    {
        Placement = ToolWindowPlacement.Floating,
    };

    public override Task<IRemoteUserControl> GetContentAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IRemoteUserControl>(new StateGraphControl(new StateGraphViewModel(this.SelectXmlFileAsync)));

    private Task<string?> SelectXmlFileAsync(CancellationToken cancellationToken)
    {
        var options = new FileDialogOptions
        {
            Filters = new DialogFilters([new("XML files", "*.xml")]),
        };

        return this.Extensibility.Shell().ShowOpenFileDialogAsync(options, cancellationToken);
    }
}
