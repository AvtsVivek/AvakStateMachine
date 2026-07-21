using Avak.StateMachine.Core.States;

namespace Avak.StateMachine.Core.Contracts
{
    internal interface IStateFileReader
    {
        event EventHandler<StateBase> StateCreated;
        void SetMasterStateFile(Stream stream);

        void SetMasterStateFilePath(string filePath);

        IReadOnlyList<MasterStateBase> States { get; }

        bool LoadMasterStateFile();

        bool PopulateStateXmlFileTree();

        string GetRootNamespace();

        List<Trigger> GetTriggers();

        void SetTransitionsAndTargetsForState(StateBase state);

        MasterStateBase SetInitialState(StateDependencyObjectFinder stateDependencyObjectFinderDelegate);

        IStateGraph GetStateGraph(StateDependencyObjectFinder stateDependencyObjectFinderDelegate);
    }
}
