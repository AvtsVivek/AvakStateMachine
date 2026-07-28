using Avak.StateMachine.Core.States;

namespace Avak.StateMachine.Core.Contracts
{
    internal interface IStateFileReader
    {
        event EventHandler<StateBase> StateCreated;

        void SetStateFile(StateXmlFile stateXmlFile);

        bool PopulateStateXmlFileTree();

        IStateGraph GetStateGraph();
    }
}
