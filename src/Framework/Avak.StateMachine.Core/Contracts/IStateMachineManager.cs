using Avak.StateMachine.Core.States;
using System.Reflection;

namespace Avak.StateMachine.Core.Contracts
{
    public interface IStateMachineManager
    {
        event EventHandler<StateBase> StateCreated;
        public StateBase CurrentState { get; }
        void SetMasterStateFile(Assembly assembly, string manifestResourceName);
        //void SetMasterStateFile(Stream stream);
        //void SetMasterStateFilePath(string filePath);
        // void LoadMasterStateFile();
        bool PopulateStateXmlFileTree();
        IStateGraph GetCurrentStateGraph();
        // void SetInitialState();
        (bool success, string message) IsTriggeredTriansitionValid(StateBase currentState, Trigger? trigger);
        bool DoTriggeredTriansition(StateBase currentState, Trigger trigger);
    }
}
