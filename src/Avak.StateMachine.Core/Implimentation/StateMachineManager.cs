using Avak.StateMachine.Core.Contracts;

namespace Avak.StateMachine.Core.Implimentation
{

	public class StateMachineManager : IStateMachineManager
	{
		public StateGraph StateGraph { get; private set; }
		private IStateFileReader stateFileReader;
		public StateBase CurrentState
		{
			get
			{
				return _currentState;
			}
		}
		private StateBase _currentState;
		private Stack<StateBase> StateStack;

		public StateMachineManager(IXmlKeys constants)
		{
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

		public StateGraph GetStateGraph()
		{
			StateGraph = stateFileReader.GetStateGraph();
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
				.Where(transition => transition.Trigger == trigger)
				.Any();

			if (!exists)
			{
				return (false, $"The given trigger is not valid for the given state {currentState.Name}");
			}

			Transition stateTransition = currentState
				.Transitions
				.Where(transition => transition.Trigger == trigger)
				.First();

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
				.Where(transition => transition.Trigger == trigger)
				.First().Target;

			SetCurrentState(targetState);
		}

		private void SetCurrentState(StateBase state)
		{
			if (CurrentState == state)
			{
				return;
			}

			CurrentState.InternalExit();
			_currentState = state;
			CurrentState.InternalInit();
			CurrentState.InternalEnter();
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
