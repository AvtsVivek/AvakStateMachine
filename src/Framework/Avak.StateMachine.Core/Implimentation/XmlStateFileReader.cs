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

        public bool PopulateStateXmlFileTree()
        {
            AddSubStateXmlFilesFromLevel(StateXmlFileTree.MasterXmlHierarchyLevel);
            return true;
        }

        private static void AddSubStateXmlFilesFromLevel(int level)
        {
            // Get all of the files at this level.
            List<StateXmlFile> stateXmlFilesAtLevel = StateXmlFileTree.Instance.GetStateXmlFilesAtLevel(level);

            if (stateXmlFilesAtLevel.Count == 0)
            {
                return;
            }

            // For each file at this level, add its sub-state XML files.
            foreach (StateXmlFile stateXmlFile in stateXmlFilesAtLevel)
            {
                stateXmlFile.AddSubStateXmlFiles();
            }

            // Recursively call this method for the next level.
            AddSubStateXmlFilesFromLevel(level + 1);
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
