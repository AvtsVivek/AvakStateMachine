using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.States;

namespace Avak.StateMachine.Core.Implimentation
{

    public class StateMachineManager : IStateMachineManager
    {
        public IStateGraph StateGraph { get; private set; }
        private IStateFileReader stateFileReader;
        private StateDependencyObjectFinder stateDependencyObjectFinderDelegate;

        public StateBase CurrentState
        {
            get
            {
                return _currentState;
            }
        }
        private StateBase _currentState;
        private Stack<StateBase> StateStack;

        public StateMachineManager(IXmlKeys constants, StateDependencyObjectFinder stateDependencyObjectFinderDelegate)
        {
            if (constants == null)
            {
                throw new ArgumentNullException(nameof(constants));
            }

            if (stateDependencyObjectFinderDelegate == null)
            {
                string message = $"The argument/parameter to the constructor of the type {typeof(StateMachineManager).FullName}, {nameof(StateMachineManager.stateDependencyObjectFinderDelegate)} of type {typeof(StateDependencyObjectFinder).FullName} cannot be null." +
                        $"If your states do not have any dependencies, then pass the default {StateDependencyImplimentation.StateDependencyObjectFinderDefaultImplimentation}";
                throw new ArgumentNullException(message);
            }

            this.stateDependencyObjectFinderDelegate = stateDependencyObjectFinderDelegate;

            StateGraph = null!;
            _currentState = null!;
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

        public IStateGraph GetStateGraph()
        {
            StateGraph = stateFileReader.GetStateGraph(stateDependencyObjectFinderDelegate);
            if (CurrentState == null)
            {
                _currentState = StateGraph.InitialState;
            }
            return StateGraph;
        }

        public (bool success, string message) IsTriggeredTriansitionValid(StateBase currentState, Trigger trigger)
        {
            List<Transition> stateTransitions = GetTransitionsForState(currentState);

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

            bool exists = currentState
                .Transitions
                .Any(transition => transition.Trigger == trigger);

            if (!exists)
            {
                return (false, $"The given trigger is not valid for the given state {currentState.Name}");
            }

            Transition stateTransition = currentState
                .Transitions
                .First(transition => transition.Trigger == trigger);

            if (stateTransition == null)
            {
                return (false, $"No transition found on given state {currentState.Id} {currentState.Name}");
            }

            StateBase targetState = stateTransition.Target;

            if (targetState == null)
            {
                return (false, $"Attempting transition from currentState: {currentState.Id} {currentState.Name}, " +
                    $"with transition trigger {trigger.Name}" + Environment.NewLine + $"Target state is null {stateTransition.Target}");
            }

            return (true, "Success");
        }

        public void DoTriggeredTriansition(StateBase currentState, Trigger trigger)
        {
            var result = IsTriggeredTriansitionValid(currentState, trigger);

            if (!result.success)
            {
                return;
            }

            StateBase targetState = currentState
                .Transitions
                .First(transition => transition.Trigger == trigger)
                .Target;

            SetCurrentState(targetState);
        }

        private void SetCurrentState(StateBase state)
        {
            if (_currentState == state)
            {
                return;
            }

            _currentState.InternalExit();
            _currentState = state;
            _currentState.InternalInit();
            _currentState.InternalEnter();
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

            List<MasterStateBase> states = StateGraph.StateList;
            if (!states.Contains(state))
            {
                throw new ArgumentException($"The state passed to the method {nameof(GetTransitionsForState)} is invalid. ");
            }

            return state.Transitions;
        }
    }
}
