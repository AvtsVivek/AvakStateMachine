namespace Avak.StateMachine.Core.Contracts
{
    internal interface IStateFileReader
    {
        void SetStateFile(StateXmlFile stateXmlFile);

        bool PopulateStateXmlFileTree();

        IStateGraph GetStateGraph();
    }
}
