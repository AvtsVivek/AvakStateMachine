using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using Avak.StateMachine.Core.States;
using System.Reflection;
using System.Runtime.Loader;
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
        private string rootNamespace = string.Empty;
        internal string RootNamespace => rootNamespace;
        private readonly List<Trigger> _triggers;
        internal List<Trigger> Triggers => _triggers;
        private readonly Lazy<List<XElement>> _triggerElements;
        internal List<XElement> TriggerElements => _triggerElements.Value;
        private readonly Lazy<XElement?> _stateCollectionElement;
        internal XElement? StateCollectionElement => _stateCollectionElement.Value;
        private readonly Lazy<List<XElement>> _stateElements;
        internal List<XElement> StateElements => _stateElements.Value;
        private readonly IXmlKeys constants; // Can we make this static?
        private readonly StateDependencyTypeFinder stateDependencyTypeFinderDelegate;
        private readonly StateDependencyResolver resolver;
        private readonly static CurrentAppDomainTypeFinder typeFinder = new();
        private readonly List<(ConstructorInfo CtorInfo, List<Type?>? DependencieTypes)> stateCtorInfoWithDependencyList = [];
        internal List<StateXmlFile> SubStateXmlFiles => StateXmlFileTree.Instance.GetStateXmlFilesAtLevel(Level + 1);
        public event EventHandler<StateBase>? StateCreated;
        private readonly List<MasterStateBase> _states = [];
        internal IReadOnlyList<MasterStateBase> States => _states;
        public override string ToString() => $"File: {fileName}, Assembly: {assembly.FullName}";
        internal bool IsMasterXmlFile => Parent == null;
        private readonly Lazy<XDocument> _xDoc;
        private XDocument XDoc => _xDoc.Value;

        internal static bool enableLazyStateInstantiation;

        internal StateXmlFile(StateXmlFile? parent, IXmlKeys constants,
            StateDependencyTypeFinder stateDependencyTypeFinderDelegate,
            StateDependencyResolver resolver,
            Assembly assembly,
            string xmlFileName)
        {
            ArgumentNullException.ThrowIfNull(constants);

            this.constants = constants;

            if (stateDependencyTypeFinderDelegate == null)
            {
                string message = $"The argument/parameter to the constructor of the type {typeof(StateMachineManager).FullName}, {nameof(stateDependencyTypeFinderDelegate)} of type {typeof(StateDependencyTypeFinder).FullName} cannot be null." +
                        $"If your states do not have any dependencies, then pass the default {StateDependencyImplimentation.StateDependencyTypeFinderDefaultImplimentation}";
                throw new ArgumentNullException(message);
            }

            this.stateDependencyTypeFinderDelegate = stateDependencyTypeFinderDelegate;
            ArgumentNullException.ThrowIfNull(resolver);

            this.resolver = resolver;
            ArgumentNullException.ThrowIfNull(assembly);

            this.assembly = assembly;

            if (string.IsNullOrWhiteSpace(xmlFileName))
            {
                // Log
                ArgumentNullException argumentNullException = new(paramName: "State xml file name cannot be null. Cannot continue");
                throw argumentNullException;
            }

            ManifestResourceInfo? manifestResource = assembly.GetManifestResourceInfo(xmlFileName) ??
                throw new Exception($"Manifest resource {xmlFileName} not found in the assembly {assembly.FullName}");

            this.fileName = xmlFileName;
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
            _triggers = [];

            StateXmlFileTree.Instance.AddStateXmlFileToTree(this);

            _triggerElements = new Lazy<List<XElement>>(() =>
            {
                string triggersString = constants.StateFileTriggerCollectionElementName;
                XElement? triggerCollectionElement = XDoc.Descendants(triggersString).FirstOrDefault();

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

            _stateCollectionElement = new Lazy<XElement?>(() =>
            {
                XElement? element = XDoc.Descendants(constants.StateFileStateCollectionElementName).FirstOrDefault() ??
                throw new XmlException($"{constants.StateFileStateCollectionElementName} element must be present in the state xml file {this}.");
                return element;
            });

            _stateElements = new Lazy<List<XElement>>(() =>
            {
                List<XElement> elementList = [.. StateCollectionElement!.Descendants(constants.StateFileStateElementName)];
                if (elementList.Count == 0)
                {
                    throw new XmlException($"{constants.StateFileStateCollectionElementName} element is empty. It must contain some state elements. Verify the state xml file.");
                }
                return elementList;
            });
        }

        internal void ReadRootStateNamespace()
        {
            if (!string.IsNullOrWhiteSpace(rootNamespace))
                return;

            XElement? rootElement = XDoc.Descendants(constants.StateFileRootElementName).First() ??
                throw new XmlException($"The root element is missing in the file {this}");

            XAttribute? rootNamespaceAttribute = rootElement.Attributes(constants.StateFileRootNamespaceAttributeName).FirstOrDefault();

            if (rootNamespaceAttribute == null)
            {
                string errorMessage =
                    $"{constants.StateFileRootNamespaceAttributeName} " +
                    $"is missing at the root {constants.StateFileRootElementName} in the state xml file {this}";

                throw new XmlException(errorMessage);
            }

            if (string.IsNullOrWhiteSpace(rootNamespaceAttribute.Value))
            {
                string errorMessage =
                    $"{constants.StateFileRootNamespaceAttributeName} " +
                    $"at the root {constants.StateFileRootElementName} in the state xml file {this}" + Environment.NewLine +
                    $"is not having any value. Ensure to have it as some non blank, non white space value, which represents a valid namespace.";


                throw new XmlException(errorMessage);
            }

            rootNamespace = rootNamespaceAttribute.Value;
        }

        /// <summary>
        /// Gets the state element from the stateElements given the name of the state.
        /// </summary>
        /// <param name="stateName"></param>
        /// <returns></returns>
        internal XElement? GetStateElement(string stateName)
        {
            XElement stateElementFound = null!;
            // Get the state element from the stateElements given the name of the state.
            foreach (XElement stateElement in StateElements)
            {
                XAttribute? stateNameAttribute = stateElement.Attribute(constants.StateFileStateNameAttributeName);
                // Here we do not have to check for presence of name attribute, and its validity. 
                // Its already done before.

                if (stateNameAttribute!.Value == stateName)
                {
                    stateElementFound = stateElement;
                    break;
                }
            }

            return stateElementFound;
        }

        internal void AddSubStateXmlFiles()
        {
            foreach (XElement stateElement in StateElements)
            {
                // SubStateAssembly Attribute
                XAttribute? subStateAssemblyNameAttribute = stateElement.Attribute(constants.StateFileStateSubStateAssemblyAttributeName);

                // SubStateXmlFile Attribute
                XAttribute? subStateXmlFileNameAttribute = stateElement.Attribute(constants.StateFileStateSubStateXmlFileAttributeName);

                if (subStateAssemblyNameAttribute == null && subStateXmlFileNameAttribute == null)
                {
                    continue;
                }

                if (subStateAssemblyNameAttribute != null && subStateXmlFileNameAttribute == null)
                {
                    string errorMessage = $"{constants.StateFileStateSubStateAssemblyAttributeName} attribute is present, but {constants.StateFileStateSubStateXmlFileAttributeName} attribute is not present on the state element " + Environment.NewLine +
                        $"{stateElement}" + Environment.NewLine +
                        $"in the file {this}" + Environment.NewLine +
                        $"If you want to specify SubStateXml file, then ensure both the attributes are specified.";
                    throw new XmlException(errorMessage);
                }

                if (subStateAssemblyNameAttribute == null && subStateXmlFileNameAttribute != null)
                {
                    string errorMessage = $"{constants.StateFileStateSubStateXmlFileAttributeName} attribute is present, but {constants.StateFileStateSubStateAssemblyAttributeName} attribute is not present on the state element " + Environment.NewLine +
                        $"{stateElement}" + Environment.NewLine +
                        $"in the file {this}" + Environment.NewLine +
                        $"If you want to specify SubStateXml file, then ensure both the attributes are specified.";
                    throw new XmlException(errorMessage);
                }

                if (string.IsNullOrWhiteSpace(subStateAssemblyNameAttribute!.Value))
                {
                    string errorMessage = $"{constants.StateFileStateSubStateAssemblyAttributeName} attribute is present, but there is no value associated with it for the state " + Environment.NewLine +
                        $"{stateElement}" + Environment.NewLine +
                        $"in the file {this}" + Environment.NewLine +
                        $"Ensure correct assembly name attribute value.";
                    throw new XmlException(errorMessage);
                }

                if (string.IsNullOrWhiteSpace(subStateXmlFileNameAttribute!.Value))
                {
                    string errorMessage = $"{constants.StateFileStateSubStateXmlFileAttributeName} attribute is present, but there is no value associated with it for the state " + Environment.NewLine +
                        $"{stateElement}" + Environment.NewLine +
                        $"in the file {this}" + Environment.NewLine +
                        $"Ensure correct assembly name attribute value.";
                    throw new XmlException(errorMessage);
                }

                if (subStateAssemblyNameAttribute != null && subStateXmlFileNameAttribute != null)
                {
                    FindXmlFileInAssembly(subStateAssemblyNameAttribute.Value,
                        subStateXmlFileNameAttribute.Value, stateElement);
                }
            }
        }

        private void FindXmlFileInAssembly(string assemblyName, string xmlFileName, XElement stateElement)
        {
            Assembly? subStateAssembly = null;
            try
            {
                subStateAssembly = Assembly.Load(assemblyName);
            }
            catch
            {
                // If loading by name fails, try to construct the path from the current assembly's location
                // This supports side-by-side assemblies in the same output directory
                string? currentAssemblyDir = Path.GetDirectoryName(assembly.Location);
                if (!string.IsNullOrEmpty(currentAssemblyDir))
                {
                    string assemblyPath = Path.Combine(currentAssemblyDir, $"{assemblyName}.dll");
                    if (File.Exists(assemblyPath))
                    {
                        subStateAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
                    }
                }
            }

            if (subStateAssembly == null)
            {
                string errorMessage = $"Trying to look for sub state xml files in the following xml file" + Environment.NewLine +
                    $"{this}" + Environment.NewLine +
                    $"And the xml element is " + Environment.NewLine +
                    $"{stateElement}" + Environment.NewLine +
                    $"Looking for xml file {xmlFileName}, in the assembly {assemblyName}." + Environment.NewLine +
                    $"Unable to load assembly '{assemblyName}'. " + Environment.NewLine +
                    $"Ensure the assembly is either already loaded, or present in the same directory as {assembly.Location}" + Environment.NewLine +
                    $"Try the following." + Environment.NewLine +
                    $"Add reference from " + Assembly.GetEntryAssembly()?.Location + Environment.NewLine +
                    $"to the assembly {assemblyName}" + Environment.NewLine;
                throw new Exception(errorMessage);
            }

            string? subStateXmlFile = GetAssemblyResourceName(subStateAssembly, xmlFileName);

            if (subStateXmlFile == null)
            {
                string errorMessage = $"The file {xmlFileName} is not found in the assembly {subStateAssembly.FullName}" + Environment.NewLine +
                    $"Ensure the file is present in the assembly and its Build Action is set to Embedded Resource" + Environment.NewLine +
                    $"Also ensure the file name is correct, including the extension. It is case sensitive." + Environment.NewLine +
                    $"The following are the manifest resource names in the assembly {subStateAssembly.FullName}" + Environment.NewLine;

                foreach (string resourceName in subStateAssembly.GetManifestResourceNames())
                {
                    errorMessage += resourceName + Environment.NewLine;
                }

                throw new Exception(errorMessage);
            }

            if (SameAssembly(subStateAssembly))
            {
                string errorMessage = $"The sub state xml file {subStateXmlFile} is in the same assembly as the parent state xml file {this}. " + Environment.NewLine +
                    $"This is not allowed. The sub state xml file must be in a different assembly than the parent state xml file." + Environment.NewLine +
                    $"Ensure the sub state xml file is in a different assembly than the parent state xml file.";
                throw new Exception(errorMessage);
            }

            StateXmlFile subStateXmlFileObject = new(this, constants, stateDependencyTypeFinderDelegate, resolver, subStateAssembly, subStateXmlFile);
        }

        private string? GetAssemblyResourceName(Assembly assembly, string resourceNameSuffix)
        {
            // debug: inspect names if you are not sure
            var names = assembly.GetManifestResourceNames();
            // find by suffix (safe if you don't want full name)
            var fullName = names.FirstOrDefault(n => n.EndsWith(resourceNameSuffix, StringComparison.OrdinalIgnoreCase));
            return fullName;

            //if (fullName == null) return null;

            //using var stream = asm.GetManifestResourceStream(fullName);
            //if (stream == null) return null;
            //using var reader = new StreamReader(stream);
            //return reader.ReadToEnd();
        }

        internal void ReadTriggers()
        {
            if (_triggers.Count != 0)
            {
                return;
            }

            foreach (XElement triggerElement in TriggerElements)
            {
                XAttribute? triggerNameAttribute = triggerElement.Attribute(constants.StateFileTriggerNameAttributeName) ??
                    throw new Exception($"Trigger Element {constants.StateFileTriggerNameAttributeName} missing in state file {this}");

                string triggerName = triggerNameAttribute.Value;

                TriggerSource triggerSource = GetTriggerSource(triggerElement);

                Trigger trigger = new(triggerName, triggerSource);

                _triggers.Add(trigger);
            }

            // Ensure all of the triggers in the file are unique.
            // Get unique trigger count
            int distinctTriggerCount = _triggers.DistinctBy(x => x.Name).Count();

            if (_triggers.Count != distinctTriggerCount)
            {
                throw new XmlException($"{constants.StateFileTriggerCollectionElementName} present in the xml file {this} are not unique." +
                    Environment.NewLine + $"Please ensure trigger names are unique.");
            }
        }

        private TriggerSource GetTriggerSource(XElement triggerElement)
        {
            XAttribute? triggerSourceAttribute = triggerElement.Attribute(constants.StateFileTriggerSourceAttributeName) ??
                throw new Exception($"Trigger Attribute Source missing in state file {this}");

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
                exceptionString += Environment.NewLine;
                exceptionString += "It must be one of the following." + Environment.NewLine;
                exceptionString += triggerSourceEnumValuesString;

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

        private XDocument GetXmlDocument()
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
                message += ex.Message;
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

            return
                Parent == other.Parent && Level == other.Level &&
                fileName == other.fileName && assembly == other.assembly;
        }

        internal bool SameAssembly(StateXmlFile other)
        {
            return assembly == other.assembly;
        }

        internal bool SameAssembly(Assembly other)
        {
            return assembly == other;
        }

        internal bool SameAssemblySameFile(StateXmlFile other)
        {
            return assembly == other.assembly && fileName == other.fileName;
        }

        internal bool SameAssemblySameFile(Assembly assembly, string fileName)
        {
            return this.assembly == assembly && this.fileName == fileName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Parent, Level, assembly, fileName);
        }

        private static void CheckStreamValidity(Stream stream)
        {
            var result = DoStreamCheck(stream);
            bool isValid = result.Item1;
            if (!isValid)
            {
                // log the message, result.Item2
                throw new ArgumentException(result.Item2);
            }
        }

        private Transition CreateTriansition(XElement transitionElement, XElement stateElement)
        {
            Transition transition = new();

            XAttribute? triggerAttribute = transitionElement
                .Attribute(constants.StateFileTransitionTriggerAttributeName);

            if (triggerAttribute == null)
            {
                XAttribute stateNameAttribute = stateElement.Attribute(constants.StateFileStateNameAttributeName)!;

                throw new XmlException($"{constants.StateFileTransitionTriggerAttributeName} Attribute missing in the file {this} for one of the transitions in state {stateNameAttribute.Value}");
            }

            Trigger? triggerForTransition = _triggers
                .FirstOrDefault(trigger => trigger.Name == triggerAttribute.Value);

            if (triggerForTransition == null)
            {
                string errorMessage = $"Trigger with name {triggerAttribute.Value} not found for one of the transition of the state {stateElement.Attribute(constants.StateFileStateNameAttributeName)!}";

                throw new XmlException(errorMessage);
            }

            transition.Trigger = triggerForTransition;

            return transition;
        }

        internal MasterStateBase SetInitialState()
        {
            // First ensure root name space is read.
            // ReadRootStateNamespace();

            // Next triggers
            ReadTriggers();

            PopulateStateTypeCtorInfoObject();

            MasterStateBase initialState = CreateAndSetInitialState();

            return initialState;
        }

        internal void PopulateStateTypeCtorInfoObject()
        {
            List<string> uniqueStateNameList = []; // Used to check state names are unique in the state file.

            foreach (XElement stateElement in StateElements)
            {
                XAttribute? stateNameAttribute = stateElement.Attribute(constants.StateFileStateNameAttributeName)
                    ?? throw new XmlException($"{constants.StateFileStateElementName} Element {constants.StateFileStateNameAttributeName} missing in state file {this}");

                string stateName = stateNameAttribute.Value;

                if (string.IsNullOrWhiteSpace(stateName))
                {
                    throw new XmlException($"In the state xml file, one of the {constants.StateFileStateElementName} has missing {constants.StateFileStateNameAttributeName} attribute. Ensure every state element has a valid and unique name");
                }

                if (uniqueStateNameList.Contains(stateName))
                {
                    throw new XmlException($"{constants.StateFileStateElementName} {constants.StateFileStateNameAttributeName} must be unique. ");
                }
                else
                {
                    uniqueStateNameList.Add(stateName);
                }

                string stateNamespace = GetStateNamespaceForElement(stateElement);

                CreateStateTypeConstructorInfoObject(stateName, stateNamespace, stateDependencyTypeFinderDelegate);
            }
        }

        private void CreateStateTypeConstructorInfoObject(string stateName, string statesNamespace, StateDependencyTypeFinder stateDependencyTypeFinderDelegate)
        {
            string typeFullName = statesNamespace + "." + stateName;

            bool successfullyFound = typeFinder.TryFindType(typeFullName, out Type ctype);

            string message = string.Empty;

            if (!successfullyFound)
            {
                message = $"The type {stateName} with namespace {statesNamespace} is not found" + Environment.NewLine;
                message += $"Check the name of the type {stateName}" + Environment.NewLine;
                message += $"Also Check the namespace {statesNamespace}";
                throw new Exception(message);
            }

            if (successfullyFound)
            {
                try
                {
                    List<Type?>? stateDependencyTypes = stateDependencyTypeFinderDelegate.Invoke(ctype);
                    ConstructorInfo ctorInfo = null!;

                    if (stateDependencyTypes == null || stateDependencyTypes.Count == 0)
                    {
                        ctorInfo = ctype.GetConstructor(Type.EmptyTypes)!;
                        if (ctorInfo == null)
                        {
                            string exceptionMessage = $"A parameterless Constructor could not be found for the type {ctype.FullName}" + Environment.NewLine;
                            exceptionMessage = exceptionMessage + $"If this type has any dependencies, then ensure you provide them in your provider" + Environment.NewLine;
                            exceptionMessage = exceptionMessage + $"Take a close look at the followng, you defined." + Environment.NewLine;
                            exceptionMessage = exceptionMessage + $"Method name: {stateDependencyTypeFinderDelegate.Method.Name}" + Environment.NewLine;
                            exceptionMessage = exceptionMessage + $"Declaring Type: {stateDependencyTypeFinderDelegate.Method.DeclaringType}" + Environment.NewLine;

                            throw new Exception(exceptionMessage);
                        }
                    }
                    else
                    {

                        List<Type?>? nullStateDependencyTypes = [.. stateDependencyTypes.Where(obj => obj == null)];
                        // first check if all of the Types are null.
                        if (nullStateDependencyTypes.Count > 0 && (nullStateDependencyTypes.Count == stateDependencyTypes.Count))
                        {
                            // if yes, then simply assume that default parameter less ctor is available on the state class
                            ctorInfo = ctype.GetConstructor(Type.EmptyTypes)!;
                        }
                        else
                        {
                            // Remove nulls
                            stateDependencyTypes = [.. stateDependencyTypes.Where(obj => obj != null)];
                        }
                    }

                    Type[] stateDependencyTypeArray = stateDependencyTypes!.ToArray()!;

                    ctorInfo = ctype.GetConstructor(stateDependencyTypeArray)!;

                    if (ctorInfo == null)
                    {
                        foreach (Type? dependencyType in stateDependencyTypes)
                        {
                            message += dependencyType + " ,";
                        }
                        message = message.TrimEnd(',', ' ');
                        // Log the message
                        message = $"Cannot create the object of type {ctype.FullName} " + Environment.NewLine +
                            $"A constructor with given types namely {message} " + Environment.NewLine +
                            $"is not found for the type {ctype.FullName}.";
                    }
                    else
                    {
                        (ConstructorInfo CtorInfo, List<Type?>? DependencieTypes)
                            ctorInfoWithDependencyTypes = (ctorInfo, stateDependencyTypes);

                        if (enableLazyStateInstantiation)
                        {
                            stateCtorInfoWithDependencyList.Add(ctorInfoWithDependencyTypes);
                        }
                        else
                        {
                            CreateStateFromCtorInfoWithDependencies(ctorInfoWithDependencyTypes);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // to do need logging.
                    // logger.Error(ex, $"Error creating state {stateName} in namespace {statesNamespace}.");
                    string errorMessage = $"Error creating state {stateName} in namespace {statesNamespace}" + Environment.NewLine;
                    errorMessage += ex.Message;
                    throw new Exception(errorMessage, ex);
                }
            }
        }

        private string GetStateNamespaceForElement(XElement stateElement)
        {
            string stateNamespace;

            XAttribute? stateNamespaceAttribute = stateElement.Attribute(constants.StateFileStateNamespaceAttributeName);

            if (stateNamespaceAttribute == null)
            {
                stateNamespace = RootNamespace;
            }
            else if (string.IsNullOrWhiteSpace(stateNamespaceAttribute.Value))
            {
                stateNamespace = RootNamespace;
            }
            else
            {
                stateNamespace = stateNamespaceAttribute.Value;
            }

            return stateNamespace;
        }

        internal MasterStateBase CreateAndSetInitialState()
        {
            XAttribute? initialAttribute = StateCollectionElement!
                .Attribute(constants.StateFileStateCollectionInitialAttributeName);

            if (initialAttribute != null && string.IsNullOrWhiteSpace(initialAttribute?.Value))
            {
                throw new XmlException($"The {constants.StateFileStateCollectionInitialAttributeName} " +
                    $"attribute on {constants.StateFileStateCollectionElementName} element must be set to a valid state. " +
                    $"Its currently an invalid empty string");
            }

            XElement initialStateElement;
            XAttribute? initialStateNameAttribute;
            if (initialAttribute == null || string.IsNullOrWhiteSpace(initialAttribute?.Value))
            {
                // Pick the very first state element
                initialStateElement = StateElements[0];
                // Here we do not have to check for presence of name attribute, and its validity. 
                // Its already done before.
            }
            else
            {
                // Find the state xml element, whose name is the above initial attribute
                initialStateElement = GetStateElement(initialAttribute.Value)!;

                if (initialStateElement == null)
                {
                    string errorMessage = $"A {constants.StateFileStateElementName} element within {constants.StateFileStateCollectionElementName}" +
                        $" element with whose name attribute is {initialAttribute.Value} is not found." +
                        $"{initialAttribute.Value} is found on {constants.StateFileStateCollectionElementName} element as {constants.StateFileStateCollectionInitialAttributeName} attribute. " +
                        $" This should match one of the attribute {constants.StateFileStateNameAttributeName} on the {constants.StateFileStateElementName} ";

                    throw new Exception(errorMessage);
                }

            }

            initialStateNameAttribute = initialStateElement
                .Attribute(constants.StateFileStateNameAttributeName);

            string initialStateName = initialStateNameAttribute!.Value;

            string stateNamespace = GetStateNamespaceForElement(initialStateElement);

            MasterStateBase initialState = CreateState(initialStateName, stateNamespace);

            // Now set the transitions and targets for this state.
            SetTransitionsAndTargetsForState(initialState);

            initialState.IsInitial = true;

            return initialState;
        }

        internal void SetTransitionsAndTargetsForState(StateBase state)
        {
            XElement stateElement = GetStateElement(state.Name)!;

            List<XElement> transitionElements = [.. stateElement.Descendants(constants.StateFileTransitionElementName)];

            foreach (XElement transitionElement in transitionElements)
            {
                Transition transition = CreateTriansition(transitionElement, stateElement);
                state.Transitions.Add(transition);

                XAttribute? triggerAttribute = transitionElement
                    .Attribute(constants.StateFileTransitionTriggerAttributeName);

                if (triggerAttribute == null && string.IsNullOrWhiteSpace(triggerAttribute!.Value))
                {
                    string errorMessage = $"{constants.StateFileTransitionTriggerAttributeName} attribute is missing on one of the transition " +
                        $"in the state {state.Name}";
                    throw new XmlException(errorMessage);
                }

                XAttribute? targetAttribute = transitionElement
                    .Attribute(constants.StateFileTransitionTargetAttributeName);

                if (targetAttribute == null && string.IsNullOrWhiteSpace(targetAttribute!.Value))
                {
                    string errorMessage = $"Target attribute is missing on the transition with Trigger name " +
                        $"{triggerAttribute!.Name} in the state {state.Name}";
                    throw new Exception(errorMessage);
                }

                XElement? targetStateElement = GetStateElement(targetAttribute!.Value)!;

                if (targetStateElement == null)
                {
                    string errorMessage = $"Target attribute {targetAttribute!.Value} on the transition " +
                        $"with Trigger name {triggerAttribute!.Name} in the state " +
                        $"{state.Name} is invalid. No state with name {targetAttribute!.Value} is found";

                    throw new Exception(errorMessage);
                }

                XAttribute? targetStateNameAttribute = targetStateElement
                    .Attribute(constants.StateFileStateNameAttributeName);

                string targetStateName = targetStateNameAttribute!.Value;

                string targetStateNamespace = GetStateNamespaceForElement(targetStateElement);

                MasterStateBase targetState = CreateState(targetStateName, targetStateNamespace);

                transition.Target = targetState;
            }
        }

        /// <summary>
        /// Creates the state object along with its dependencies, if the state has any.
        /// </summary>
        /// <param name="stateName"></param>
        /// <param name="statesNamespace"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="XmlException"></exception>
        /// <exception cref="Exception"></exception>
        private MasterStateBase CreateState(string stateName, string statesNamespace)
        {
            string typeFullName = statesNamespace + "." + stateName;

            // First check if the state already exists in the state collection.

            var stateToBeCreated = States.FirstOrDefault(state => state.GetType().FullName == typeFullName);

            if (stateToBeCreated != null)
            {
                return stateToBeCreated; // already exits.
            }

            (ConstructorInfo CtorInfo, List<Type?>? DependencieTypes)? ctorInfoWithDependencyTypes =
                stateCtorInfoWithDependencyList.FirstOrDefault(ctorTuple => ctorTuple.CtorInfo.DeclaringType!.FullName == typeFullName);

            if (ctorInfoWithDependencyTypes == null || !ctorInfoWithDependencyTypes.HasValue)
            {
                // Log the error
                throw new InvalidOperationException($"Class not found for the given type {typeFullName}. Cannot continue.");
            }

            return CreateStateFromCtorInfoWithDependencies(ctorInfoWithDependencyTypes.Value);
        }

        private MasterStateBase CreateStateFromCtorInfoWithDependencies((ConstructorInfo CtorInfo, List<Type?>? DependencieTypes) ctorInfoWithDependencieTypes)
        {
            ConstructorInfo ctorInfo = ctorInfoWithDependencieTypes.CtorInfo;

            string typeFullName = ctorInfo.DeclaringType!.FullName!;

            List<Type?>? stateDependencyTypes = ctorInfoWithDependencieTypes.DependencieTypes;

            // Need to get the objects from the types.

            List<object?>? stateDependencyObjects = [];

            foreach (Type? type in ctorInfoWithDependencieTypes.DependencieTypes!)
            {
                if (type != null)
                {
                    try
                    {
                        object? dependencyObject = resolver.Invoke(type);
                        stateDependencyObjects.Add(dependencyObject);
                    }
                    catch (Exception exception)
                    {
                        string message = $"The state {ctorInfo.DeclaringType!.FullName} could not be created. " + Environment.NewLine +
                            $"It has a dependency of type {type.FullName} that could not be resovled." + Environment.NewLine +
                            $"If you are using any dependency injection container, " + Environment.NewLine +
                            $"ensure the state dependency, along with ITS dependencies are registed with the DI Container" + Environment.NewLine +
                            $"This is the exception {exception.Message}" + Environment.NewLine +
                            $"The stake trace is {exception.StackTrace}";
                        throw new Exception(message);
                    }
                }
            }

            object stateObject = null!;

            try
            {
                if (stateDependencyTypes!.Count == 0)
                {
                    stateObject = ctorInfo!.Invoke(null);
                }
                else
                {
                    stateObject = ctorInfo!.Invoke([.. stateDependencyObjects!]);
                }
            }
            catch (Exception exception)
            {
                string message = $"The state {typeFullName} could not be created. " + Environment.NewLine +
                    "The following execution failed.";
                if (stateDependencyTypes!.Count == 0)
                {
                    message += "ctorInfo!.Invoke(null);";
                }
                else
                {
                    message += "ctorInfo!.Invoke(stateDependencyObjects!.ToArray());";
                }

                message = message + $"This is the exception {exception.Message}" + Environment.NewLine +
                    $"The stake trace is {exception.StackTrace}";

                throw new Exception(message);
            }

            if (stateObject == null)
            {
                throw new XmlException($"Trying to create state object. " +
                    $"{constants.StateFileStateCollectionInitialAttributeName} " +
                    $"on {constants.StateFileStateCollectionElementName} must be set to a valid state. " +
                    $"The {typeFullName} does not represent any state." +
                    $"Instanciation of the type {typeFullName} failed. ");
            }

            MasterStateBase stateBase = (stateObject as MasterStateBase)! ??
                throw new Exception($"Trying to create state object of type {typeFullName}. " +
                    $"{typeFullName} must inherit {nameof(MasterStateBase)}");

            if (!States.Contains(stateBase))
            {
                AddState(stateBase);
            }

            return stateBase;
        }

        // Method to allow the class to safely add items
        public void AddState(MasterStateBase state)
        {
            if (state == null)
                return;

            if (!States.Contains(state))
            {
                _states.Add(state);
                StateCreated?.Invoke(this, state);
            }
        }

        private static (bool, string) DoStreamCheck(Stream stream)
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