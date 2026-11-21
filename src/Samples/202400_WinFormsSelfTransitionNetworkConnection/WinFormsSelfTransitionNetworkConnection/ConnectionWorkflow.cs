namespace WinFormsSelfTransitionNetworkConnection;

using Avak.StateMachine.Core;
using Avak.StateMachine.Core.Configuration;
using Avak.StateMachine.Core.Extensions;

public sealed class FakeConnection
{
    private readonly List<string> _events = [];

    public FakeConnection()
    {
        Events = _events.AsReadOnly();
    }

    public bool IsAvailable { get; set; } = true;
    public bool IsOpen { get; private set; }
    public IReadOnlyList<string> Events { get; }

    public void ClearEvents() => _events.Clear();

    internal void Record(string message) => _events.Add(message);

    internal void Open()
    {
        if (IsOpen)
        {
            throw new InvalidOperationException("The fake connection is already open.");
        }

        IsOpen = true;
        Record("Open connection");
    }

    internal void Close()
    {
        if (!IsOpen)
        {
            throw new InvalidOperationException("The fake connection is already closed.");
        }

        IsOpen = false;
        Record("Close connection");
    }
}

public sealed class ConnectedState(FakeConnection connection) : State
{
    public override bool CanEnter(StateContext context)
    {
        connection.Record("CanEnter Connected");
        return connection.IsAvailable;
    }

    public override bool CanExit(StateContext context)
    {
        connection.Record("CanExit Connected");
        return connection.IsOpen;
    }

    public override void Enter(StateContext context)
    {
        connection.Record("Enter Connected");
        connection.Open();
    }

    public override void Exit(StateContext context)
    {
        connection.Record("Exit Connected");
        connection.Close();
    }
}

public sealed class ConnectionWorkflow
{
    private readonly StateMachine _machine;

    public ConnectionWorkflow()
    {
        Connection = new FakeConnection();
        string configurationPath = Path.Combine(AppContext.BaseDirectory, "ConnectionWorkflow.xml");
        var configuration = new XmlConfigurationLoader().LoadFromFile(configurationPath);
        var states = new StateRegistry();
        states.Register("ConnectedState", new ConnectedState(Connection));
        _machine = new StateMachine(configuration, null, null, null, null, states);
    }

    public FakeConnection Connection { get; }
    public string CurrentState => _machine.CurrentState;

    public TransitionResult Connect() => _machine.Trigger("Connect", source: "UI");

    public TransitionResult Refresh() => _machine.Trigger("Refresh", source: "UI");

    public TransitionResult Disconnect() => _machine.Trigger("Disconnect", source: "UI");
}
