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

        void SetTransitionsAndTargetsForState(StateBase state, StateDependencyResolver resolver);

        MasterStateBase SetInitialState(StateDependencyTypeFinder stateDependencyTypeFinderDelegate, StateDependencyResolver resolver);

        IStateGraph GetStateGraph(StateDependencyTypeFinder stateDependencyTypeFinderDelegate, StateDependencyResolver resolver);
    }
}
