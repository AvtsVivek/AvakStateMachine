using Avak.StateMachine.Core.Contracts;
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
        public readonly List<Trigger> triggers;
        private readonly Lazy<List<XElement>> _triggerElements;
        private List<XElement> triggerElements => _triggerElements.Value;

        private IXmlKeys constants;

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
        internal StateXmlFile(IXmlKeys constants, StateXmlFile? parent, Assembly assembly, string fileName)
        {
            if (constants == null)
            {
                throw new ArgumentNullException(nameof(constants));
            }

            this.constants = constants;

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
            triggers = [];
            StateXmlFileTree.Instance.AddStateXmlFileToTree(this);

            _triggerElements = new Lazy<List<XElement>>(() =>
            {
                string triggersString = constants.StateFileTriggerCollectionElementName;
                XElement? triggerCollectionElement = xDoc.Descendants(triggersString).FirstOrDefault();

                if (triggerCollectionElement == null)
                {
                    string message = $"{triggersString} not present in the state file. " +
                        $"Add <{triggersString}></{triggersString}> element.";
                    throw new Exception(message);
                }
                List<XElement> elementList = [.. triggerCollectionElement!.Descendants(constants.StateFileTriggerElementName)];

                return elementList;
            });

            _xDoc = new(GetXmlDocument); // Same as new Lazy<XDocument>(() => GetXmlDocument());
        }

        private readonly Lazy<XDocument> _xDoc;

        private XDocument xDoc => _xDoc.Value;

        internal void ReadTriggers()
        {
            if (triggers.Count != 0)
            {
                return;
            }

            foreach (XElement triggerElement in triggerElements)
            {
                XAttribute? triggerNameAttribute = triggerElement.Attribute(constants.StateFileTriggerNameAttributeName);

                if (triggerNameAttribute == null)
                {
                    throw new Exception($"Trigger Element {constants.StateFileTriggerNameAttributeName} missing in state file {this}");
                }

                string triggerName = triggerNameAttribute.Value;

                TriggerSource triggerSource = GetTriggerSource(triggerElement);

                Trigger trigger = new(triggerName, triggerSource);

                triggers.Add(trigger);
            }

            // Ensure all of the triggers in the file are unique.
            // Get unique trigger count
            int distinctTriggerCount = triggers.DistinctBy(x => x.Name).Count();

            if (triggers.Count != distinctTriggerCount)
            {
                throw new XmlException($"{constants.StateFileTriggerCollectionElementName} present in the xml file {this} are not unique." +
                    Environment.NewLine + $"Please ensure trigger names are unique.");
            }
        }

        private TriggerSource GetTriggerSource(XElement triggerElement)
        {
            XAttribute? triggerSourceAttribute = triggerElement.Attribute(constants.StateFileTriggerSourceAttributeName);

            if (triggerSourceAttribute == null)
            {
                throw new Exception($"Trigger Attribute Source missing in state file {this}");
            }

            string sourceName = triggerSourceAttribute.Value;

            if (string.IsNullOrWhiteSpace(triggerSourceAttribute.Value))
            {
                throw new Exception($"Trigger Attribute Source missing in state file {this}");
            }

            if (!Enum.TryParse(sourceName, out TriggerSource triggerSource))
            {
                string triggerSourceEnumValuesString = string.Empty;
                foreach (TriggerSource sourceString in Enum.GetValues<TriggerSource>())
                {
                    triggerSourceEnumValuesString = triggerSourceEnumValuesString + sourceString + ", ";
                }

                triggerSourceEnumValuesString = triggerSourceEnumValuesString.TrimEnd(',', ' ');

                string exceptionString = $"Incorrect trigger source value in the file {this}";
                exceptionString = exceptionString + Environment.NewLine;
                exceptionString = exceptionString + "It must be one of the following." + Environment.NewLine;
                exceptionString = exceptionString + triggerSourceEnumValuesString;

                throw new Exception(exceptionString);
            }
            return triggerSource;
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