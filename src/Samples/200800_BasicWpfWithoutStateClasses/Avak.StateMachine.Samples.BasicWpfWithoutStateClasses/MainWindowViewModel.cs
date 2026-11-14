namespace Avak.StateMachine.Samples.BasicWpfWithoutStateClasses;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avak.StateMachine.Core;

public sealed class MainWindowViewModel(StateMachine machine) : INotifyPropertyChanged
{
    private string _statusMessage = "Choose Start or Stop to trigger a transition.";

    public event PropertyChangedEventHandler? PropertyChanged;

    public string CurrentState => machine.CurrentState;

    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (_statusMessage == value)
            {
                return;
            }

            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public bool CanStart => CurrentState == "Idle";

    public bool CanStop => CurrentState == "Running";

    public TransitionResult Start()
    {
        return Trigger("Start");
    }

    public TransitionResult Stop()
    {
        return Trigger("Stop");
    }

    private TransitionResult Trigger(string triggerName)
    {
        var result = machine.Trigger(triggerName, source: "UI");

        OnPropertyChanged(nameof(CurrentState));
        OnPropertyChanged(nameof(CanStart));
        OnPropertyChanged(nameof(CanStop));
        StatusMessage = result.Success
            ? $"Transitioned from {result.FromState} to {result.ToState}."
            : result.ErrorMessage ?? "The transition was rejected.";

        return result;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
