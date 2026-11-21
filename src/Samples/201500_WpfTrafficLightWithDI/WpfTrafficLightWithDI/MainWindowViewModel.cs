namespace WpfTrafficLightWithDI;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avak.StateMachine.Core;

public sealed class MainWindowViewModel(StateMachine machine, TrafficLightSession session) : INotifyPropertyChanged
{
    private string _statusMessage = "Choose Next light to advance the signal.";

    public event PropertyChangedEventHandler? PropertyChanged;

    public string CurrentState => machine.CurrentState;

    public double RedOpacity => CurrentState == "Red" ? 1 : 0.25;

    public double YellowOpacity => CurrentState == "Yellow" ? 1 : 0.25;

    public double GreenOpacity => CurrentState == "Green" ? 1 : 0.25;

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

    public bool IsOperational
    {
        get => session.IsOperational;
        set
        {
            if (session.IsOperational == value)
            {
                return;
            }

            session.IsOperational = value;
            OnPropertyChanged();
        }
    }

    public string LastTransition => session.TransitionHistory.LastOrDefault() ?? "No transitions yet.";

    public TransitionResult Next()
    {
        var result = machine.Trigger("Next", source: "UI");

        OnPropertyChanged(nameof(CurrentState));
        OnPropertyChanged(nameof(RedOpacity));
        OnPropertyChanged(nameof(YellowOpacity));
        OnPropertyChanged(nameof(GreenOpacity));
        OnPropertyChanged(nameof(LastTransition));
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
