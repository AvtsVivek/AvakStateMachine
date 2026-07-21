using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.States;

namespace Avak.StateMachine.Core.Implimentation
{

    public class StateMachineManager : IStateMachineManager
    {
        public IStateGraph StateGraph { get; private set; }
        private IStateFileReader stateFileReader;
        private StateDependencyObjectFinder stateDependencyObjectFinderDelegate;
        public event EventHandler<StateBase>? StateCreated;

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
            stateFileReader.StateCreated += StateFileReader_StateCreated;
        }

        private void StateFileReader_StateCreated(object? sender, StateBase stateCreated)
        {
            StateCreated?.Invoke(this, stateCreated);
        }

        public void SetMasterStateFile(Stream stream)
        {
            stateFileReader.SetMasterStateFile(stream);
        }

        public void SetMasterStateFilePath(string filePath)
        {
            stateFileReader.SetMasterStateFilePath(filePath);
        }

        public bool LoadMasterStateFile()
        {
            return stateFileReader.LoadMasterStateFile();
        }

        public List<Trigger> GetTriggers()
        {
            return stateFileReader.GetTriggers();
        }

        public bool PopulateStateXmlFileTree()
        {
            return true;
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

        public void SetInitialState()
        {
            MasterStateBase? initialState = stateFileReader.SetInitialState(stateDependencyObjectFinderDelegate);
            this._currentState = initialState!;
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

            stateFileReader.SetTransitionsAndTargetsForState(targetState);
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

            return state.Transitions;
        }
    }
}
