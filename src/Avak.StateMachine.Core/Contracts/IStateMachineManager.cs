using Avak.StateMachine.Core.States;

namespace Avak.StateMachine.Core.Contracts
{
	public interface IStateMachineManager
	{
		public StateBase CurrentState { get; }
		void SetStateFile(Stream stream);
		void SetStateFilePath(string filePath);
		bool LoadStateFile();
		StateGraph GetStateGraph();
		(bool success, string message) IsTriggeredTriansitionValid(StateBase currentState, Trigger trigger);
		void DoTriggeredTriansition(StateBase currentState, Trigger trigger);
	}
}
