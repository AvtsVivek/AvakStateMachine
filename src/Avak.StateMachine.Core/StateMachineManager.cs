using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;

namespace Avak.StateMachine.Core
{
    public class StateMachineManager
    {
        public StateGraph StateGraph { get; private set; }
        private IStateFileReader stateFileReader;
        public StateBase CurrentState { get; private set; }
        private Stack<StateBase> StateStack;

        public StateMachineManager(IXmlKeys constants)
        {
            StateGraph = null!;
            CurrentState = null!;
            StateStack = new();
            stateFileReader = new XmlStateFileReader(constants);
        }

        public void SetStateFile(Stream stream)
        {
            stateFileReader.SetStateFile(stream);
        }

        public void SetStateFilePath(string filePath)
        {
            stateFileReader.SetStateFilePath(filePath);
        }

        public bool LoadStateFile()
        {
            return stateFileReader.LoadStateFile();
        }

        public StateGraph GetStateGraph()
        {
            StateGraph = stateFileReader.GetStateGraph();
            return StateGraph;
        }

        public void Initialize()
        {
            SetCurrentState(StateGraph.InitialState);
        }

        public (bool success, string message) DoTriggeredTriansition(StateBase state, Trigger trigger)
        {
            List<Transition> stateTransitions = GetTransitionsForState(state);

            if (stateTransitions.Count == 0)
            {
                return (false, "Transitions are not available for the given state");
            }

            if (trigger == null)
            {
                return (false, "Trigger argument is null.");
            }

            if (StateGraph.TriggerList is null)
            {
                return (false, "No triggers are available on the state graph");
            }

            if (StateGraph.TriggerList.Count == 0)
            {
                return (false, "No triggers are not available on the state graph");
            }

            if (!StateGraph.TriggerList.Contains(trigger))
            {
                return (false, "The given trigger is not available on the state graph");
            }

            Transition stateTransition = state
                .Transitions
                .Where(transition => transition.Trigger == trigger)
                .First();

            if (stateTransition == null)
            {
                return (false, $"No transition found on given state {state.Id} {state.Name}");
            }

            StateBase targetState = stateTransition.Target;

            SetCurrentState(targetState);

            return (true, "Transition successifull");
        }

        private void SetCurrentState(StateBase state)
        {
            CurrentState = state;
            StateStack.Push(state);
        }

        private List<Transition> GetTransitionsForState(StateBase state)
        {
            if (state == null)
            {
                throw new ArgumentNullException($"{nameof(state)} in {nameof(GetTransitionsForState)}");
            }

            if (StateGraph == null)
            {
                GetStateGraph();
            }

            if (StateGraph == null)
            {
                throw new InvalidOperationException("State Graph is null. Check the state file is valid");
            }

            List<StateBase> states = StateGraph.StateList;
            if (!states.Contains(state))
            {
                throw new ArgumentException($"The state passed to the method {nameof(GetTransitionsForState)} is invalid. ");
            }

            return state.Transitions;
        }
    }
}
