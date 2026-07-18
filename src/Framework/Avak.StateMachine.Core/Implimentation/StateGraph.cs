using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.States;

namespace Avak.StateMachine.Core.Implimentation
{
    public class StateGraph : IStateGraph
    {
        public List<MasterStateBase> StateList { get; }

        public MasterStateBase InitialState { get; }

        public List<Trigger> TriggerList { get; }

        public StateGraph(List<MasterStateBase> stateList, List<Trigger> triggerList, MasterStateBase stateBase)
        {
            StateList = stateList;
            TriggerList = triggerList;
            InitialState = stateBase;
        }
    }
}
