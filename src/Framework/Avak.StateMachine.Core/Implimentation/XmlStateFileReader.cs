using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.States;
using System.Xml.Linq;

namespace Avak.StateMachine.Core.Implimentation
{
    internal class XmlStateFileReader : IStateFileReader
    {
        private XDocument xCurrentStateDoc;

        private StateXmlFile currentStateXmlFile;

        private StateGraph stateGraph;
        public event EventHandler<StateBase>? StateCreated;

        internal XmlStateFileReader()
        {
            xCurrentStateDoc = null!;
            currentStateXmlFile = null!;
            stateGraph = null!;
        }

        public void LoadStateFile(StateXmlFile stateXmlFile)
        {
            currentStateXmlFile = stateXmlFile;
            xCurrentStateDoc = stateXmlFile.GetXmlDocument();

            if (xCurrentStateDoc == null)
            {
                throw new Exception($"The state doc object is null, for the file {stateXmlFile}");
            }
        }

        public bool PopulateStateXmlFileTree()
        {
            currentStateXmlFile.ReadRootStateNamespace();
            return true;
        }

        public MasterStateBase SetInitialState()
        {
            // First ensure root name space is read.
            // ReadRootStateNamespace();

            // Next triggers
            currentStateXmlFile.ReadTriggers();

            currentStateXmlFile.PopulateStateTypeCtorInfoObject();

            MasterStateBase initialState = currentStateXmlFile.CreateAndSetInitialState();

            return initialState;
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

            stateGraph = new StateGraph(currentStateXmlFile.States.ToList(), currentStateXmlFile.Triggers!, initialState!);

            return stateGraph;
        }
    }
}
