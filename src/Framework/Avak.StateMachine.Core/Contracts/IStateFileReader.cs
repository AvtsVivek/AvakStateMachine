using Avak.StateMachine.Core.States;

namespace Avak.StateMachine.Core.Contracts
{
    internal interface IStateFileReader
    {
        event EventHandler<StateBase> StateCreated;

        IReadOnlyList<MasterStateBase> States { get; }

        void LoadStateFile(StateXmlFile stateXmlFile);

        bool PopulateStateXmlFileTree();

        void SetTransitionsAndTargetsForState(StateBase state);

        MasterStateBase SetInitialState(StateDependencyTypeFinder stateDependencyTypeFinderDelegate);

        IStateGraph GetStateGraph(StateDependencyTypeFinder stateDependencyTypeFinderDelegate);
    }
}
