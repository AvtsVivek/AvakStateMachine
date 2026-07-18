using Avak.StateMachine.Core.States;

namespace Avak.StateMachine.Core.Contracts
{
    public interface IStateGraph
    {
        List<MasterStateBase> StateList { get; }

        MasterStateBase InitialState { get; }

        List<Trigger> TriggerList { get; }
    }
}
