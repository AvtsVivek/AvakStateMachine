namespace WinFormsSelfTransitionNetworkConnection;

using Avak.StateMachine.Core;

public sealed class MainForm : Form
{
    private readonly ConnectionWorkflow _workflow = new();
    private readonly Label _stateLabel = new() { AutoSize = true };
    private readonly Label _statusLabel = new() { AutoSize = true, MaximumSize = new Size(550, 0) };
    private readonly Button _connectButton = new() { Text = "Connect", AutoSize = true };
    private readonly Button _refreshButton = new() { Text = "Refresh", AutoSize = true };
    private readonly Button _disconnectButton = new() { Text = "Disconnect", AutoSize = true };
    private readonly Button _clearEventsButton = new() { Text = "Clear events", AutoSize = true };
    private readonly CheckBox _availableCheckBox = new() { Text = "Connection available", Checked = true, AutoSize = true };
    private readonly ListBox _eventsList = new() { Dock = DockStyle.Fill };

    public MainForm()
    {
        Text = "Self-Transition: Fake Network Connection";
        ClientSize = new Size(600, 440);
        MinimumSize = new Size(500, 400);
        StartPosition = FormStartPosition.CenterScreen;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 1,
            RowCount = 5
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
        buttons.Controls.AddRange([_connectButton, _refreshButton, _disconnectButton, _clearEventsButton]);
        layout.Controls.Add(_stateLabel, 0, 0);
        layout.Controls.Add(_availableCheckBox, 0, 1);
        layout.Controls.Add(buttons, 0, 2);
        layout.Controls.Add(_statusLabel, 0, 3);
        layout.Controls.Add(_eventsList, 0, 4);
        Controls.Add(layout);

        _availableCheckBox.CheckedChanged += (_, _) => _workflow.Connection.IsAvailable = _availableCheckBox.Checked;
        _connectButton.Click += (_, _) => Apply(_workflow.Connect());
        _refreshButton.Click += (_, _) => Apply(_workflow.Refresh());
        _disconnectButton.Click += (_, _) => Apply(_workflow.Disconnect());
        _clearEventsButton.Click += (_, _) =>
        {
            _workflow.Connection.ClearEvents();
            _eventsList.Items.Clear();
        };
        UpdateDisplay("Choose Connect to open the fake connection.");
    }

    private void Apply(TransitionResult result)
    {
        UpdateDisplay(result.Success
            ? $"Transition: {result.FromState} -> {result.ToState}"
            : result.ErrorMessage ?? "The transition was rejected.");
    }

    private void UpdateDisplay(string status)
    {
        _stateLabel.Text = $"State: {_workflow.CurrentState} | Fake connection: {(_workflow.Connection.IsOpen ? "open" : "closed")}";
        _statusLabel.Text = status;
        bool connected = _workflow.CurrentState == "Connected";
        _connectButton.Enabled = !connected;
        _refreshButton.Enabled = connected;
        _disconnectButton.Enabled = connected;

        _eventsList.Items.Clear();
        foreach (string entry in _workflow.Connection.Events)
        {
            _eventsList.Items.Add(entry);
        }
        if (_eventsList.Items.Count > 0)
        {
            _eventsList.TopIndex = _eventsList.Items.Count - 1;
        }
    }
}
