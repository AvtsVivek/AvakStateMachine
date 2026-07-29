using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.States;
using System.Reflection;

namespace Avak.StateMachine.Core.Implimentation
{
    public class StateMachineManager : IStateMachineManager
    {
        public IStateGraph StateGraph { get; private set; }
        private readonly XmlStateFileReader stateFileReader;
        private readonly StateDependencyTypeFinder stateDependencyTypeFinderDelegate;
        private readonly StateDependencyResolver resolver;
        public event EventHandler<StateBase>? StateCreated;
        private StateXmlFile masterStateXmlFile;

        private readonly IXmlKeys constants;

        public StateBase CurrentState
        {
            get
            {
                return _currentState;
            }
        }
        private StateBase _currentState;
        private readonly Stack<StateBase> StateStack;

        public StateMachineManager(IXmlKeys constants,
            StateDependencyTypeFinder stateDependencyTypeFinderDelegate,
            StateDependencyResolver resolver)
        {

            ArgumentNullException.ThrowIfNull(resolver);

            this.resolver = resolver;

            ArgumentNullException.ThrowIfNull(constants);

            this.constants = constants;

            if (stateDependencyTypeFinderDelegate == null)
            {
                string message = $"The argument/parameter to the constructor of the type {typeof(StateMachineManager).FullName}, {nameof(StateMachineManager.stateDependencyTypeFinderDelegate)} of type {typeof(StateDependencyTypeFinder).FullName} cannot be null." +
                        $"If your states do not have any dependencies, then pass the default {StateDependencyImplimentation.StateDependencyTypeFinderDefaultImplimentation}";
                throw new ArgumentNullException(message);
            }

            this.stateDependencyTypeFinderDelegate = stateDependencyTypeFinderDelegate;

            StateGraph = null!;
            _currentState = null!;
            StateStack = new();
            stateFileReader = new XmlStateFileReader();
            masterStateXmlFile = null!;
        }

        public void SetMasterStateFile(Assembly assembly, string manifestResourceName)
        {
            masterStateXmlFile = new StateXmlFile(parent: null,
                this.constants,
                this.stateDependencyTypeFinderDelegate,
                this.resolver,
                assembly,
                manifestResourceName);
            masterStateXmlFile.StateCreated += StateFileReader_StateCreated;
            stateFileReader.SetStateFile(masterStateXmlFile);
        }

        public bool PopulateStateXmlFileTree()
        {
            stateFileReader.AddSubStateXmlFiles();
            return true;
        }

        // Gets the current state graph, not the full state graph. 
        public IStateGraph GetCurrentStateGraph()
        {
            StateGraph = stateFileReader.GetStateGraph();
            if (CurrentState == null)
            {
                _currentState = StateGraph.InitialState;
            }
            return StateGraph;
        }

        public (bool success, string message) IsTriggeredTriansitionValid(StateBase currentState, Trigger? trigger)
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

        public bool DoTriggeredTriansition(StateBase currentState, Trigger trigger)
        {
            var (success, message) = IsTriggeredTriansitionValid(currentState, trigger);

            if (!success)
            {
                return false;
            }

            StateBase targetState = currentState
                .Transitions
                .First(transition => transition.Trigger == trigger)
                .Target;

            SetCurrentState(targetState);

            masterStateXmlFile.SetTransitionsAndTargetsForState(targetState);

            return true;
        }

        private void StateFileReader_StateCreated(object? sender, StateBase stateCreated)
        {
            StateCreated?.Invoke(this, stateCreated);
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

        private static List<Transition> GetTransitionsForState(StateBase state)
        {
            ArgumentNullException.ThrowIfNull(state);

            return state.Transitions;
        }
    }
}
