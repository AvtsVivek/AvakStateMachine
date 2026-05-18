namespace Avak.StateMachine.Core
{
    public class StateGraph
    {
        public List<StateBase> StateList { get; }

        public StateBase InitialState { get; }

        public List<Trigger> TriggerList { get; }

        public StateGraph(List<StateBase> stateList, List<Trigger> triggerList, StateBase stateBase)
        {
            StateList = stateList;
            TriggerList = triggerList;
            InitialState = stateBase;
        }
    }
}
