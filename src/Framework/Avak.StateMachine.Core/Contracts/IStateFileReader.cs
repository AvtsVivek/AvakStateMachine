using Avak.StateMachine.Core.States;

namespace Avak.StateMachine.Core.Contracts
{
    internal interface IStateFileReader
    {
        event EventHandler<StateBase> StateCreated;

        IReadOnlyList<MasterStateBase> States { get; }

        void LoadMasterStateFile(StateXmlFile stateXmlFile);

        bool PopulateStateXmlFileTree();

        string GetRootNamespace();

        void SetTransitionsAndTargetsForState(StateBase state);

        MasterStateBase SetInitialState(StateDependencyTypeFinder stateDependencyTypeFinderDelegate);

        IStateGraph GetStateGraph(StateDependencyTypeFinder stateDependencyTypeFinderDelegate);
    }
}
