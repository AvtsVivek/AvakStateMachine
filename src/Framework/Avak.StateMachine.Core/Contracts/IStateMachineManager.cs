using Avak.StateMachine.Core.States;

namespace Avak.StateMachine.Core.Contracts
{
    public interface IStateMachineManager
    {
        event EventHandler<StateBase> StateCreated;
        public StateBase CurrentState { get; }
        void SetMasterStateFile(Stream stream);
        void SetMasterStateFilePath(string filePath);
        bool LoadMasterStateFile();
        bool PopulateStateXmlFileTree();
        IStateGraph GetStateGraph();
        List<Trigger> GetTriggers();
        void SetInitialState();
        (bool success, string message) IsTriggeredTriansitionValid(StateBase currentState, Trigger trigger);
        void DoTriggeredTriansition(StateBase currentState, Trigger trigger);
    }
}
