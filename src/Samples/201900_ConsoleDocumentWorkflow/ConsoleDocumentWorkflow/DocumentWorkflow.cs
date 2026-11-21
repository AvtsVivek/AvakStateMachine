namespace ConsoleDocumentWorkflow;

using Avak.StateMachine.Core;
using Avak.StateMachine.Core.Configuration;
using Avak.StateMachine.Core.Extensions;

public enum DocumentRole
{
    Author,
    Reviewer
}

public sealed class Document
{
    private readonly List<string> _audit = [];

    public Document(string content)
    {
        Content = content;
        Audit = _audit.AsReadOnly();
    }

    public string Content { get; set; }
    public IReadOnlyList<string> Audit { get; }

    internal void Record(string message) => _audit.Add(message);
}

public sealed class DocumentWorkflow
{
    private readonly StateMachine _machine;

    public DocumentWorkflow(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);
        Document = document;
        string configurationPath = Path.Combine(AppContext.BaseDirectory, "DocumentWorkflow.xml");
        var configuration = new XmlConfigurationLoader().LoadFromFile(configurationPath);

        var states = new StateRegistry();
        states.Register("DraftState", new DraftState(document));
        states.Register("InReviewState", new InReviewState(document));
        states.Register("ApprovedState", new ApprovedState(document));
        states.Register("RejectedState", new RejectedState(document));

        var permissions = new PermissionRegistry();
        permissions.Register("Reviewer", new ReviewerPermission());
        _machine = new StateMachine(configuration, null, permissions, null, null, states);
    }

    public Document Document { get; }
    public string CurrentState => _machine.CurrentState;

    public TransitionResult Trigger(string trigger, string source, DocumentRole role)
    {
        return _machine.Trigger(trigger, source, new Dictionary<string, object?> { ["Role"] = role });
    }
}

public sealed class DraftState(Document document) : State
{
    public override bool CanExit(StateContext context) => !string.IsNullOrWhiteSpace(document.Content);

    public override void Enter(StateContext context)
    {
        document.Record("Draft created");
    }
}

public sealed class InReviewState(Document document) : State
{
    public override void Enter(StateContext context)
    {
        document.Record("Submitted for review");
    }
}

public sealed class ApprovedState(Document document) : State
{
    public override void Enter(StateContext context)
    {
        document.Record("Approved");
    }
}

public sealed class RejectedState(Document document) : State
{
    public override void Enter(StateContext context)
    {
        document.Record("Rejected");
    }
}

public sealed class ReviewerPermission : IPermission
{
    public bool IsAllowed(TransitionContext context)
    {
        return context.Data.TryGetValue("Role", out object? role) && role is DocumentRole.Reviewer;
    }
}
