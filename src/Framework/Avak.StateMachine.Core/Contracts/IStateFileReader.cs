using Avak.StateMachine.Core.States;

namespace Avak.StateMachine.Core.Contracts
{
    internal interface IStateFileReader
    {
        event EventHandler<StateBase> StateCreated;


        void LoadStateFile(StateXmlFile stateXmlFile);

        bool PopulateStateXmlFileTree();

        // void SetTransitionsAndTargetsForState(StateBase state);

        MasterStateBase SetInitialState();

        IStateGraph GetStateGraph();
    }
}
