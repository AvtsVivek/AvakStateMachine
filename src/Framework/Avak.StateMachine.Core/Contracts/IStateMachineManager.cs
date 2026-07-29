using Avak.StateMachine.Core.States;
using System.Reflection;

namespace Avak.StateMachine.Core.Contracts
{
    public interface IStateMachineManager
    {
        event EventHandler<StateBase> StateCreated;
        public StateBase CurrentState { get; }
        void SetMasterStateFile(Assembly assembly, string manifestResourceName);
        bool PopulateStateXmlFileTree();
        IStateGraph GetCurrentStateGraph();
        (bool success, string message) IsTriggeredTriansitionValid(StateBase currentState, Trigger? trigger);
        bool DoTriggeredTriansition(StateBase currentState, Trigger trigger);
    }
}
