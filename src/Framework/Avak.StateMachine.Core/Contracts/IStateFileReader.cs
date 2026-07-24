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

        void SetTransitionsAndTargetsForState(StateBase state);

        MasterStateBase SetInitialState(StateDependencyTypeFinder stateDependencyTypeFinderDelegate);

        IStateGraph GetStateGraph(StateDependencyTypeFinder stateDependencyTypeFinderDelegate);
    }
}
