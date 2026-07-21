using Avak.StateMachine.Core.States;

namespace Avak.StateMachine.Core.Contracts
{
    internal interface IStateFileReader
    {
        void SetMasterStateFile(Stream stream);

        void SetMasterStateFilePath(string filePath);

        IReadOnlyList<MasterStateBase> States { get; }

        bool LoadMasterStateFile();

        bool PopulateStateXmlFileTree();

        string GetRootNamespace();

        List<Trigger> GetTriggers();

        void SetTransactionsAndTargetsForState(StateBase state);

        MasterStateBase SetInitialState(StateDependencyObjectFinder stateDependencyObjectFinderDelegate);

        IStateGraph GetStateGraph(StateDependencyObjectFinder stateDependencyObjectFinderDelegate);
    }
}
