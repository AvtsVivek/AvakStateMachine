using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.States;

namespace Avak.StateMachine.Core.Implimentation
{
    internal class XmlStateFileReader : IStateFileReader
    {
        private StateXmlFile currentStateXmlFile;
        private StateGraph stateGraph;
        internal XmlStateFileReader()
        {
            currentStateXmlFile = null!;
            stateGraph = null!;
        }

        public void SetStateFile(StateXmlFile stateXmlFile)
        {
            currentStateXmlFile = stateXmlFile;
        }

        public bool AddSubStateXmlFiles()
        {
            return true;
        }

        public IStateGraph GetStateGraph()
        {
            if (stateGraph != null)
            {
                return stateGraph;
            }

            currentStateXmlFile.ReadRootStateNamespace();

            currentStateXmlFile.ReadTriggers();

            MasterStateBase? initialState = currentStateXmlFile.SetInitialState();

            stateGraph = new StateGraph([.. currentStateXmlFile.States], currentStateXmlFile.Triggers!, initialState!);

            return stateGraph;
        }
    }
}
