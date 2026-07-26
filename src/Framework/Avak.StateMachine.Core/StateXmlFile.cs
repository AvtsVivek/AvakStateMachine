using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace Avak.StateMachine.Core
{
    internal class StateXmlFile
    {
        internal readonly StateXmlFile? Parent;
        private readonly Assembly assembly;
        private readonly string fileName;
        internal readonly int Level;

        internal List<StateXmlFile> SubStateXmlFiles
        {
            get
            {
                return StateXmlFileTree.Instance.GetStateXmlFiles(Level + 1);
            }
        }

        public override string ToString()
        {
            return $"File: {fileName}, Assembly: {assembly.FullName}";
        }

        internal bool IsMasterXmlFile => Parent == null;
        internal StateXmlFile(StateXmlFile? parent, Assembly assembly, string fileName)
        {
            if (assembly == null)
            {
                // Log
                throw new ArgumentNullException("State xml assembly cannot be null. Cannot continue");
            }

            this.assembly = assembly;

            if (string.IsNullOrWhiteSpace(fileName))
            {
                // Log
                ArgumentNullException argumentNullException = new("State xml file name cannot be null. Cannot continue");
                throw argumentNullException;
            }

            ManifestResourceInfo? manifestResource = assembly.GetManifestResourceInfo(fileName);

            if (manifestResource == null)
            {
                throw new Exception($"Manifest resource {fileName} not found in the assembly {assembly.FullName}");
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
                Level = parent!.Level + 1;
            }
            StateXmlFileTree.Instance.AddStateXmlFileToTree(this);
        }

        private Stream GetFileStream()
        {
            Stream? stream = assembly.GetManifestResourceStream(fileName) ??
                throw new Exception($"Manifest resource stream could not be fetched " +
                $"for the file {fileName} in the assembly {assembly.FullName}");

            CheckStreamValidity(stream);

            return stream;
        }

        internal XDocument GetXmlDocument()
        {
            Stream fileStream = GetFileStream();
            try
            {
                XDocument XResourceDoc = XDocument.Load(fileStream, LoadOptions.SetBaseUri);
                return XResourceDoc;
            }
            catch (XmlException ex)
            {
                string message = $"The state file {fileName} in the assembly {assembly.FullName} could not be loaded.";
                message = message + ex.Message;
                throw new Exception(message, ex);
            }
            finally
            {
                fileStream.Close();
                fileStream.Dispose();
            }
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

        private void CheckStreamValidity(Stream stream)
        {
            var result = DoStreamCheck(stream);
            bool isValid = result.Item1;
            if (!isValid)
            {
                // log the message, result.Item2
                throw new ArgumentException(result.Item2);
            }
        }

        private (bool, string) DoStreamCheck(Stream stream)
        {
            string message;
            if (stream == null)
            {
                message = "Ensure correct stream object. Stream object is null";
                return (false, message);
            }

            if (stream != null && !stream!.CanRead)
            {
                message = "Ensure correct stream object. The stream object cannot read. Stream.CanRead is false";
                return (false, message);
            }

            if (!stream!.CanSeek)
            {
                message = "Ensure correct stream object. The stream object cannot seek. Stream.CanSeek is false";
                return (false, message);
            }

            return (true, string.Empty);
        }
    }
}