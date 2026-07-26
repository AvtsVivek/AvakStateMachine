namespace Avak.StateMachine.Core
{
    internal class StateXmlFileTree
    {
        internal const int MasterXmlHierarchyLevel = 1;

        private List<StateXmlFile> StateXmlFiles = [];

        // 1. Lazy<StateXmlFileTree> ensures thread-safe, lazy initialization automatically
        private static readonly Lazy<StateXmlFileTree> _lazyInstance = new(() => new StateXmlFileTree());

        // 2. Public static property provides global access to the single instance
        internal static StateXmlFileTree Instance => _lazyInstance.Value;

        // 3. Finally, ensure an explicit private parameterless constructor to prevent external instantiation
        private StateXmlFileTree() { }

        internal void AddStateXmlFileToTree(StateXmlFile xmlFile)
        {
            ArgumentNullException.ThrowIfNull(xmlFile);
            StateXmlFiles.Add(xmlFile);
        }
        internal StateXmlFile GetMasterXmlFile()
        {
            StateXmlFile? masterXmlFile = StateXmlFiles
                .FirstOrDefault(xmlFile => xmlFile.IsMasterXmlFile && xmlFile.Level == MasterXmlHierarchyLevel);

            if (masterXmlFile == null)
            {
                // Log 
                throw new Exception("Master xml file is null. Cannot continue");
            }
            return masterXmlFile;
        }

        internal List<StateXmlFile> GetStateXmlFiles(int level)
        {
            List<StateXmlFile> stateXmlFilesAtGivenLevel = [.. StateXmlFiles
                .Where(xmlFile => xmlFile.Level == level)];

            return stateXmlFilesAtGivenLevel;
        }
    }
}