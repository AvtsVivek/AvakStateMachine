namespace Avak.StateMachine.Core;

/// <summary>
/// Thrown when a state machine operation fails.
/// </summary>
public class StateMachineException : Exception
{
    public StateMachineException(string message) : base(message) { }
    public StateMachineException(string message, Exception innerException) 
        : base(message, innerException) { }
}

/// <summary>
/// Thrown when a guard or permission denies a transition.
/// </summary>
public class TransitionDeniedException : StateMachineException
{
    public string Reason { get; }

    public TransitionDeniedException(string message, string reason) 
        : base(message)
    {
        Reason = reason;
    }
}

/// <summary>
/// Thrown when attempting to trigger a state machine from within a callback,
/// indicating a nested/recursive request.
/// </summary>
public class NestedRequestException : StateMachineException
{
    public NestedRequestException(string message) : base(message) { }
}

/// <summary>
/// Thrown when a lifecycle callback fails during a transition.
/// </summary>
public class CallbackException : StateMachineException
{
    public string CallbackName { get; }

    public CallbackException(string message, string callbackName, Exception innerException) 
        : base(message, innerException)
    {
        CallbackName = callbackName;
    }
}
