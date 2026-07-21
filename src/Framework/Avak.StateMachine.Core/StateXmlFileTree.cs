using System.Reflection;

namespace Avak.StateMachine.Core
{
    public class StateXmlFileTree
    {
        private List<StateXmlFile> StateXmlFiles = [];

        public const int MasterXmlHierarchyLevel = 1;

        // 1. Lazy<T> ensures thread-safe, lazy initialization automatically
        private static readonly Lazy<StateXmlFileTree> _lazyInstance = new(() => new StateXmlFileTree());

        // 2. Public static property provides global access to the single instance
        public static StateXmlFileTree Instance => _lazyInstance.Value;

        // 3. Finally, ensure a private parameterless constructor to prevent external instantiation
        private StateXmlFileTree() { }

        internal void AddStateXmlFileToTree(StateXmlFile xmlFile)
        {
            if (xmlFile == null)
            {
                // Log 
                throw new ArgumentNullException("State Xml file cannot be null. Cannot continue!");
            }
            StateXmlFiles.Add(xmlFile);
        }
        public StateXmlFile GetMasterXmlFile()
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

        public List<StateXmlFile> GetStateXmlFiles(int level)
        {
            List<StateXmlFile> stateXmlFilesAtGivenLevel = [.. StateXmlFiles
                .Where(xmlFile => xmlFile.Level == level)];

            return stateXmlFilesAtGivenLevel;
        }
    }

    public class StateXmlFile
    {
        public StateXmlFileTree XmlFileTree = StateXmlFileTree.Instance;
        public readonly StateXmlFile? Parent;
        public List<StateXmlFile> SubStateXmlFiles
        {
            get
            {
                return XmlFileTree.GetStateXmlFiles(Level + 1);
            }
        }
        public readonly int Level;
        public bool IsMasterXmlFile => Parent == null;
        private Assembly assembly;
        private string fileName;
        public StateXmlFile(StateXmlFile parent, Assembly assembly, string fileName)
        {
            if (assembly == null)
            {
                // Log
                throw new ArgumentNullException("State xml assembly cannot be null. Cannot continue");
            }

            this.assembly = assembly;

            if (String.IsNullOrWhiteSpace(fileName))
            {
                // Log
                throw new ArgumentNullException("State xml file name cannot be null. Cannot continue");
            }

            this.fileName = fileName;
            Parent = parent;
            if (Parent == null)
            {
                // This is the master xml file.
                Level = StateXmlFileTree.MasterXmlHierarchyLevel;
            }
            else
            {
                Level = (ushort)(parent.Level + 1);
            }
            XmlFileTree.AddStateXmlFileToTree(this);
        }

        public override bool Equals(object? obj)
        {
            if (obj is not StateXmlFile other)
                return false;

            return Parent == other.Parent && Level == other.Level &&
                fileName == other.fileName && assembly == other.assembly;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Parent, Level, assembly, fileName);
        }
    }
}