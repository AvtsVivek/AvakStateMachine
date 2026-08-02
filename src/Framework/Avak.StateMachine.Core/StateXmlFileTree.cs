namespace Avak.StateMachine.Core
{
    internal class StateXmlFileTree
    {
        internal const int MasterXmlHierarchyLevel = 1;

        private readonly List<StateXmlFile> StateXmlFiles = [];

        // 1. Lazy<StateXmlFileTree> ensures thread-safe, lazy initialization automatically
        private static readonly Lazy<StateXmlFileTree> _lazyInstance = new(() => new StateXmlFileTree());

        // 2. Public static property provides global access to the single instance
        internal static StateXmlFileTree Instance => _lazyInstance.Value;

        // 3. Finally, ensure an explicit private parameterless constructor to prevent external instantiation
        private StateXmlFileTree() { }

        internal void AddStateXmlFileToTree(StateXmlFile xmlFile)
        {
            ArgumentNullException.ThrowIfNull(xmlFile);

            StateXmlFiles.ForEach(existingXmlFile =>
            {
                if (existingXmlFile.SameAssemblySameFile(xmlFile))
                {
                    string errorMessage = $"StateXmlFileTree already contains the specified xmlFile: " + Environment.NewLine +
                    $"{xmlFile}" + Environment.NewLine +
                    $"Existing xmlFile: " + Environment.NewLine +
                    $"{existingXmlFile}" + Environment.NewLine +
                    $"Existing xmlFile Level: {existingXmlFile.Level}, New xmlFile Level: {xmlFile.Level}" + Environment.NewLine +
                    $"You are trying to add and the same xml file at Level: {xmlFile.Level} which is already added at level: {existingXmlFile.Level}";
                    throw new Exception(errorMessage);
                }

                if (existingXmlFile.SameAssembly(xmlFile))
                {
                    string errorMessage = $"StateXmlFileTree already contains an xmlFile from the same assembly: " + Environment.NewLine +
                    $"{xmlFile}" + Environment.NewLine +
                    $"Existing xmlFile: " + Environment.NewLine +
                    $"{existingXmlFile}" + Environment.NewLine +
                    $"Existing xmlFile Level: {existingXmlFile.Level}, New xmlFile Level: {xmlFile.Level}" + Environment.NewLine +
                    $"You are trying to add xml file at Level: {xmlFile.Level} whose assembly is already added at level: {existingXmlFile.Level}";
                    throw new Exception(errorMessage);
                }
            });

            StateXmlFiles.Add(xmlFile);
        }

        /// <summary>
        /// Clears the StateXmlFiles list, effectively resetting the tree. 
        /// This is used in the cleanup of the unit tests to ensure that each test starts with a clean state.
        /// The method and the class is internal, meaning it can be accessed within the same assembly but not from outside.
        /// But we have the following in the csproj file to allow the test project to access internal members of this assembly.
        ///  <ItemGroup>
        ///    <InternalsVisibleTo Include = "Avak.StateMachine.Core.Tests" />
        ///  </ItemGroup>
        /// Currently the only purpose of this method is to be used in the unit tests. It is not intended for use in production code.
        /// Specifically, SubStateOneFindAssembly and SubStateOneMissingAttributeTests
        /// Note, both the above test classes are marked with [DoNotParallelize] attribute, so that the tests in these classes are not run in parallel. 
        /// This is to ensure that the state of the StateXmlFileTree is not shared between tests, which could lead to unpredictable results.
        /// </summary>
        internal void Clear()
        {
            StateXmlFiles.Clear();
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

        internal List<StateXmlFile> GetStateXmlFilesAtLevel(int level)
        {
            List<StateXmlFile> stateXmlFilesAtGivenLevel = [.. StateXmlFiles
                .Where(xmlFile => xmlFile.Level == level)];

            return stateXmlFilesAtGivenLevel;
        }
    }
}