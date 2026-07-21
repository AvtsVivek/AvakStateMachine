using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.States;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace Avak.StateMachine.Core.Implimentation
{
    internal class XmlStateFileReader : IStateFileReader
    {
        private Stream XmlFileStream;

        private string XmlFilePath = string.Empty;

        private bool isStateFileValidAndLoaded;

        private XDocument XResourceDoc { get; set; }

        private string rootNamespace = string.Empty;

        internal readonly List<Trigger> triggers;

        private List<MasterStateBase> _states = [];

        internal IReadOnlyList<MasterStateBase> States => _states;

        private StateGraph stateGraph;

        private IXmlKeys constants;

        private ITypeFinder typeFinder;

        private List<(ConstructorInfo CtorInfo, List<object?>? Dependencies)> stateCtorInfoWithDependenciesList = [];

        // 1. Declare the field without initializing it here
        private readonly Lazy<XElement?> _stateCollectionElement;

        private XElement? stateCollectionElement => _stateCollectionElement.Value;

        private readonly Lazy<List<XElement>> _stateElements;

        private List<XElement> stateElements => _stateElements.Value;

        private readonly Lazy<List<XElement>> _triggerElements;

        private List<XElement> triggerElements => _triggerElements.Value;

        IReadOnlyList<MasterStateBase> IStateFileReader.States => States;

        public XmlStateFileReader(IXmlKeys constants)
        {
            if (constants == null)
            {
                throw new ArgumentNullException(nameof(constants));
            }

            this.constants = constants;

            this.typeFinder = new CurrentAppDomainTypeFinder();
            XmlFileStream = null!;
            XResourceDoc = null!;
            triggers = [];
            stateGraph = null!;
            isStateFileValidAndLoaded = false;
            _stateCollectionElement = new Lazy<XElement?>(() =>
            {
                XElement? element = XResourceDoc.Descendants(constants.StateFileStateCollectionElementName).FirstOrDefault();
                if (element == null)
                {
                    throw new XmlException($"{constants.StateFileStateCollectionElementName} element must be present in the state xml file {XResourceDoc.BaseUri}.");
                }
                return element;
            });

            _stateElements = new Lazy<List<XElement>>(() =>
            {
                List<XElement> elementList = [.. stateCollectionElement!.Descendants(constants.StateFileStateElementName)];
                if (elementList.Count == 0)
                {
                    throw new XmlException($"{constants.StateFileStateCollectionElementName} element is empty. It must contain some state elements. Verify the state xml file.");
                }
                return elementList;
            });

            _triggerElements = new Lazy<List<XElement>>(() =>
            {
                string triggersString = constants.StateFileTriggerCollectionElementName;
                XElement? triggerCollectionElement = XResourceDoc.Descendants(triggersString).FirstOrDefault();

                if (triggerCollectionElement == null)
                {
                    string message = $"{triggersString} not present in the state file. " +
                        $"Add <{triggersString}></{triggersString}> element.";
                    throw new Exception(message);
                }
                List<XElement> elementList = [.. triggerCollectionElement!.Descendants(constants.StateFileTriggerElementName)];


                return elementList;
            });
        }

        public void SetMasterStateFile(Stream stream)
        {
            if (IsStreamValid(stream))
            {
                XmlFileStream = stream;
            }
        }

        public void SetMasterStateFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException("Invalid Xml file Path. Its null or empty.");
            }

            if (!File.Exists(filePath))
            {
                throw new ArgumentNullException($"Invalid Xml file Path. The File {filePath} does not exist.");
            }

            XmlFilePath = filePath;

            FileStream fileStream = new(XmlFilePath, FileMode.Open, FileAccess.Read);

            SetMasterStateFile(fileStream);
        }

        public bool LoadMasterStateFile()
        {
            try
            {
                if (!isStateFileValidAndLoaded)
                {
                    if (!IsStreamValid(XmlFileStream))
                        return false;

                    XResourceDoc = XDocument.Load(XmlFileStream, LoadOptions.SetBaseUri);
                    isStateFileValidAndLoaded = true;
                }
            }
            catch (XmlException ex)
            {
                // Log the exception message ex.Message
                // todo need logging.
                isStateFileValidAndLoaded = false;
            }

            return isStateFileValidAndLoaded;
        }

        public bool PopulateStateXmlFileTree()
        {
            ReadRootStateNamespace();
            return true;
        }

        public string GetRootNamespace()
        {
            ReadRootStateNamespace();
            return rootNamespace;
        }

        public List<Trigger> GetTriggers()
        {
            if (triggers.Count != 0)
            {
                return triggers;
            }

            ReadTriggers();

            return triggers;
        }

        public MasterStateBase SetInitialState(StateDependencyObjectFinder stateDependencyObjectFinderDelegate)
        {
            // First ensure root name space is read.
            ReadRootStateNamespace();

            // Next triggers
            ReadTriggers();

            PopulateStateTypeCtorInfoObject(stateDependencyObjectFinderDelegate);

            MasterStateBase initialState = SetInitialState();

            return initialState;
        }

        public IStateGraph GetStateGraph(StateDependencyObjectFinder stateDependencyObjectFinderDelegate)
        {
            if (stateGraph != null)
            {
                return stateGraph;
            }

            // First ensure root namespace is read.
            ReadRootStateNamespace();

            ReadTriggers();

            MasterStateBase? initialState = SetInitialState(stateDependencyObjectFinderDelegate);

            stateGraph = new StateGraph(States.ToList(), triggers!, initialState!);

            return stateGraph;
        }

        /// <summary>
        /// Gets the state element from the stateElements given the name of the state.
        /// </summary>
        /// <param name="stateName"></param>
        /// <returns></returns>
        private XElement? GetStateElement(string stateName)
        {
            XElement stateElementFound = null!;
            // Get the state element from the stateElements given the name of the state.
            foreach (XElement stateElement in stateElements)
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

        private MasterStateBase SetInitialState()
        {
            XAttribute? initialAttribute = stateCollectionElement!
                .Attribute(constants.StateFileStateCollectionInitialAttributeName);

            if (initialAttribute != null && string.IsNullOrWhiteSpace(initialAttribute?.Value))
            {
                throw new XmlException($"The {constants.StateFileStateCollectionInitialAttributeName} " +
                    $"attribute on {constants.StateFileStateCollectionElementName} element must be set to a valid state. " +
                    $"Its currently an invalid empty string");
            }

            XElement initialStateElement = null!;
            XAttribute? initialStateNameAttribute = null!;
            if (initialAttribute == null || string.IsNullOrWhiteSpace(initialAttribute?.Value))
            {
                // Pick the very first state element
                initialStateElement = stateElements[0];
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

        public void SetTransitionsAndTargetsForState(StateBase state)
        {
            XElement stateElement = GetStateElement(state.Name)!;

            List<XElement> transitionElements = stateElement
                .Descendants(constants.StateFileTransitionElementName)
                .ToList();

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

        private MasterStateBase? SetInitialStateOld(XElement stateCollectionElement)
        {
            if (States.Count == 0)
            {
                // No states. So just return null.
                return null;
            }

            XAttribute? initialAttribute = stateCollectionElement
                .Attribute(constants.StateFileStateCollectionInitialAttributeName);

            if (initialAttribute == null)
            {
                // Pick the very first State Element and set that as the Initial Element.
                States[0].IsInitial = true;
                return States[0];
            }


            if (string.IsNullOrWhiteSpace(initialAttribute!.Value))
            {
                throw new XmlException($"{constants.StateFileStateCollectionInitialAttributeName} on {constants.StateFileStateCollectionElementName} must be set to a valid state");
            }

            string initialStateName = initialAttribute.Value;

            MasterStateBase? initialState = States.FirstOrDefault(state => state.Name == initialStateName);

            if (initialState == null)
            {
                throw new XmlException($"{constants.StateFileStateCollectionInitialAttributeName} " +
                    $"on {constants.StateFileStateCollectionElementName} must be set to a valid state. " +
                    $"The {initialStateName} does not represent any state.");
            }
            initialState.IsInitial = true;
            return initialState;
        }

        private void ReadStates(XElement stateCollectionElement, StateDependencyObjectFinder stateDependencyObjectFinderDelegate)
        {
            if (stateCollectionElement == null)
            {
                throw new Exception($"{constants.StateFileStateCollectionElementName} not present in the file {XResourceDoc.BaseUri}");
            }

            // PopulateStates(stateElements, stateDependencyObjectFinderDelegate);
            PopulateStateTypeCtorInfoObject(stateDependencyObjectFinderDelegate);

            // states = GetStates(stateElements, stateDependencyObjectFinderDelegate);

            // AddTargetStateToStateTransitions(stateElements, states);
        }

        private void AddTargetStateToStateTransitions(List<MasterStateBase> states)
        {
            foreach (XElement stateElement in stateElements)
            {
                XAttribute stateNameAttribute = stateElement.Attribute(constants.StateFileStateNameAttributeName)!;

                string stateName = stateNameAttribute.Value;

                StateBase state = states.Where(state => state.Name == stateName).First();

                List<XElement> transitionElements = stateElement
                    .Descendants(constants.StateFileTransitionElementName)
                    .ToList();

                foreach (XElement transitionElement in transitionElements)
                {
                    XAttribute? triggerAttribute = transitionElement
                        .Attribute(constants.StateFileTransitionTriggerAttributeName);

                    XAttribute? targetAttribute = transitionElement
                        .Attribute(constants.StateFileTransitionTargetAttributeName);

                    if (targetAttribute == null && string.IsNullOrWhiteSpace(targetAttribute!.Value))
                    {
                        string errorMessage = $"Target attribute is missing on the transition with Trigger name {triggerAttribute!.Name} in the state {state.Name}";
                        throw new Exception(errorMessage);
                    }

                    // The target attribute name should match a state

                    MasterStateBase targetState = states.First(state => state.Name == targetAttribute!.Value);

                    if (targetState == null)
                    {
                        string errorMessage = $"Target attribute {targetAttribute!.Value} on the transition " +
                            $"with Trigger name {triggerAttribute!.Name} in the state " +
                            $"{state.Name} is invalid. No state with name {targetAttribute!.Value} is found";

                        throw new Exception(errorMessage);
                    }

                    Transition transition = state
                        .Transitions
                        .First(transition => transition.Trigger.Name == triggerAttribute!.Value);

                    transition.Target = targetState;
                }
            }
        }

        private void PopulateStateTypeCtorInfoObject(StateDependencyObjectFinder stateDependencyObjectFinderDelegate)
        {
            List<string> uniqueStateNameList = []; // Used to check state names are unique in the state file.

            foreach (XElement stateElement in stateElements)
            {
                XAttribute? stateNameAttribute = stateElement.Attribute(constants.StateFileStateNameAttributeName)
                    ?? throw new XmlException($"{constants.StateFileStateElementName} Element {constants.StateFileStateNameAttributeName} missing in state file {XResourceDoc.BaseUri}");

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

                CreateStateTypeConstructorInfoObject(stateName, stateNamespace, stateDependencyObjectFinderDelegate);
            }
        }

        private List<MasterStateBase> GetStates(StateDependencyObjectFinder stateDependencyObjectFinderDelegate)
        {
            List<MasterStateBase> states = [];

            foreach (XElement stateElement in stateElements)
            {
                XAttribute? stateNameAttribute = stateElement.Attribute(constants.StateFileStateNameAttributeName)
                    ?? throw new Exception($"State Element name missing in state file {XResourceDoc.BaseUri}");

                string stateName = stateNameAttribute.Value;

                string stateNamespace = GetStateNamespaceForElement(stateElement);

                CreateStateTypeConstructorInfoObject(stateName, stateNamespace, stateDependencyObjectFinderDelegate);

                MasterStateBase state = CreateState(stateName, stateNamespace);

                List<Transition> transitionsForState = GetTransitionsForState(stateElement);

                state.Transitions = transitionsForState;

                if (state != null)
                {
                    state.Name = stateName;
                    states.Add(state);
                }
            }

            return states;
        }

        private string GetStateNamespaceForElement(XElement stateElement)
        {
            string stateNamespace = string.Empty;

            XAttribute? stateNamespaceAttribute = stateElement.Attribute(constants.StateFileStateNamespaceAttributeName);

            if (stateNamespaceAttribute == null)
            {
                stateNamespace = rootNamespace;
            }
            else if (string.IsNullOrWhiteSpace(stateNamespaceAttribute.Value))
            {
                stateNamespace = rootNamespace;
            }
            else
            {
                stateNamespace = stateNamespaceAttribute.Value;
            }

            return stateNamespace;
        }

        private List<Transition> GetTransitionsForState(XElement stateElement)
        {
            List<XElement> transitionElements = stateElement
                .Descendants(constants.StateFileTransitionElementName)
                .ToList();

            List<Transition> transitionsForState = new();

            foreach (XElement initialStateElement in transitionElements)
            {
                Transition transition = CreateTriansition(initialStateElement, stateElement);
                transitionsForState.Add(transition);
            }

            return transitionsForState;
        }

        private Transition CreateTriansition(XElement transitionElement, XElement stateElement)
        {
            Transition transition = new();

            XAttribute? triggerAttribute = transitionElement
                .Attribute(constants.StateFileTransitionTriggerAttributeName);

            if (triggerAttribute == null)
            {
                XAttribute stateNameAttribute = stateElement.Attribute(constants.StateFileStateNameAttributeName)!;

                throw new XmlException($"{constants.StateFileTransitionTriggerAttributeName} Attribute missing in the file {XResourceDoc.BaseUri} for one of the transitions in state {stateNameAttribute.Value}");
            }

            Trigger? triggerForTransition = triggers
                .FirstOrDefault(trigger => trigger.Name == triggerAttribute.Value);

            if (triggerForTransition == null)
            {
                string errorMessage = $"Trigger with name {triggerAttribute.Value} not found for one of the transition of the state {stateElement.Attribute(constants.StateFileStateNameAttributeName)!}";

                throw new XmlException(errorMessage);
            }

            transition.Trigger = triggerForTransition;

            return transition;
        }

        private void CreateStateTypeConstructorInfoObject(string stateName, string statesNamespace, StateDependencyObjectFinder stateDependencyObjectFinderDelegate)
        {
            string typeFullName = statesNamespace + "." + stateName;

            bool successfullyFound = typeFinder.TryFindType(typeFullName, out Type ctype);

            string message = string.Empty;

            if (!successfullyFound)
            {
                message = $"The type {stateName} with namespace {statesNamespace} is not found" + Environment.NewLine;

                message = message + $"Check the name of the type {stateName}" + Environment.NewLine;

                message = message + $"Also Check the namespace {statesNamespace}";

                throw new Exception(message);
            }

            if (successfullyFound)
            {
                try
                {
                    List<object?>? stateDependencyObjects = stateDependencyObjectFinderDelegate.Invoke(ctype);

                    ConstructorInfo ctorInfo = null!;

                    if (stateDependencyObjects == null || stateDependencyObjects.Count == 0)
                    {
                        ctorInfo = ctype.GetConstructor(Type.EmptyTypes)!;
                        if (ctorInfo == null)
                        {
                            string exceptionMessage = $"A parameterless Constructor could not be found for the type {ctype.FullName}" + Environment.NewLine;
                            exceptionMessage = exceptionMessage + $"If this type has any dependencies, then ensure you provide them in your provider" + Environment.NewLine;
                            exceptionMessage = exceptionMessage + $"Take a close look at the followng, you defined." + Environment.NewLine;
                            exceptionMessage = exceptionMessage + $"Method name: {stateDependencyObjectFinderDelegate.Method.Name}" + Environment.NewLine;
                            exceptionMessage = exceptionMessage + $"Declaring Type: {stateDependencyObjectFinderDelegate.Method.DeclaringType}" + Environment.NewLine;

                            throw new Exception(exceptionMessage);
                        }
                    }
                    else
                    {

                        List<object?>? nullStateDependencyObjects = stateDependencyObjects.Where(obj => obj == null).ToList();
                        // first check if all of the objects are null.
                        if (nullStateDependencyObjects.Count > 0 && (nullStateDependencyObjects.Count == stateDependencyObjects.Count))
                        {
                            // if yes, then simply assume that default parameter less ctor is available on the state class
                            ctorInfo = ctype.GetConstructor(Type.EmptyTypes)!;
                        }
                        else
                        {
                            // Remove nulls
                            stateDependencyObjects = stateDependencyObjects.Where(obj => obj != null).ToList();
                        }
                    }

                    Type[] stateDependencyTypes = [.. stateDependencyObjects!.Select(obj => obj!.GetType())];

                    ctorInfo = ctype.GetConstructor(stateDependencyTypes)!;

                    if (ctorInfo == null)
                    {
                        foreach (Type dependencyType in stateDependencyTypes)
                        {
                            message = message + dependencyType + " ,";
                        }
                        message.TrimEnd(',', ' ');
                        // Log the message
                        message = $"Cannot create the object of type {ctype.FullName} " + Environment.NewLine +
                            $"A constructor with given types namely {message} " + Environment.NewLine +
                            $"is not found for the type {ctype.FullName}.";
                    }
                    else
                    {
                        (ConstructorInfo CtorInfo, List<object?>? Dependencies) ctorInfoWithDependencies = (ctorInfo, stateDependencyObjects);
                        stateCtorInfoWithDependenciesList.Add(ctorInfoWithDependencies);
                    }
                }
                catch (Exception ex)
                {
                    // to do need logging.
                    // logger.Error(ex, $"Error creating state {stateName} in namespace {statesNamespace}.");
                    throw;
                }
            }
        }

        private MasterStateBase CreateState(string stateName, string statesNamespace)
        {
            MasterStateBase stateBase = null!;

            string typeFullName = statesNamespace + "." + stateName;

            // First check if the state already exists in the state collection.

            var stateToBeCreated = States.FirstOrDefault(state => state.GetType().FullName == typeFullName);

            if (stateToBeCreated != null)
            {
                return stateToBeCreated; // already exits.
            }

            // Find the ctorInfo object
            (ConstructorInfo CtorInfo, List<object?>? Dependencies)? ctorInfoTuple = stateCtorInfoWithDependenciesList
                .FirstOrDefault(ctorTuple => ctorTuple.CtorInfo.DeclaringType!.FullName == typeFullName);

            if (ctorInfoTuple == null || !ctorInfoTuple.HasValue)
            {
                // Log the error
                throw new InvalidOperationException($"Class not found for the given type {typeFullName}. Cannot continue.");
            }

            ConstructorInfo ctorInfo = ctorInfoTuple.Value.CtorInfo;

            List<object?>? stateDependencyObjects = ctorInfoTuple.Value.Dependencies;

            object stateObject = null!;

            if (stateDependencyObjects!.Count == 0)
            {
                stateObject = ctorInfo!.Invoke(null);
            }
            else
            {
                stateObject = ctorInfo!.Invoke(stateDependencyObjects!.ToArray());
            }

            if (stateObject == null)
            {
                throw new XmlException($"Trying to create state object. " +
                    $"{constants.StateFileStateCollectionInitialAttributeName} " +
                    $"on {constants.StateFileStateCollectionElementName} must be set to a valid state. " +
                    $"The {stateName} does not represent any state." +
                    $"Instanciation of the type {typeFullName} failed. ");
            }

            stateBase = (stateObject as MasterStateBase)! ??
                throw new Exception($"Trying to create state object of type {typeFullName}. " +
                    $"{typeFullName} must inherit {nameof(MasterStateBase)}");

            stateBase.Name = stateName;

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
                _states.Add(state);
        }

        private TriggerSource GetTriggerSource(XElement triggerElement)
        {
            XAttribute? triggerSourceAttribute = triggerElement.Attribute(constants.StateFileTriggerSourceAttributeName);

            if (triggerSourceAttribute == null)
            {
                throw new Exception($"Trigger Attribute Source missing in state file {XResourceDoc.BaseUri}");
            }

            string sourceName = triggerSourceAttribute.Value;

            if (string.IsNullOrWhiteSpace(triggerSourceAttribute.Value))
            {
                throw new Exception($"Trigger Attribute Source missing in state file {XResourceDoc.BaseUri}");
            }

            if (!Enum.TryParse(sourceName, out TriggerSource triggerSource))
            {
                string triggerSourceEnumValuesString = string.Empty;
                foreach (TriggerSource sourceString in Enum.GetValues<TriggerSource>())
                {
                    triggerSourceEnumValuesString = triggerSourceEnumValuesString + sourceString + ", ";
                }

                triggerSourceEnumValuesString = triggerSourceEnumValuesString.TrimEnd(',', ' ');

                string exceptionString = $"Incorrect trigger source value in the file {XResourceDoc.BaseUri}";
                exceptionString = exceptionString + Environment.NewLine;
                exceptionString = exceptionString + "It must be one of the following." + Environment.NewLine;
                exceptionString = exceptionString + triggerSourceEnumValuesString;

                throw new Exception(exceptionString);
            }

            return triggerSource;
        }

        private void ReadTriggers()
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
                    throw new Exception($"Trigger Element {constants.StateFileTriggerNameAttributeName} missing in state file {XResourceDoc.BaseUri}");
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
                throw new XmlException($"{constants.StateFileTriggerCollectionElementName} present in the xml file {XResourceDoc.BaseUri} are not unique." +
                    Environment.NewLine + $"Please ensure trigger names are unique.");
            }
        }

        private void ReadRootStateNamespace()
        {
            if (!string.IsNullOrWhiteSpace(rootNamespace))
                return;

            XElement? rootElement = XResourceDoc.Descendants(constants.StateFileRootElementName).First();

            if (rootElement != null)
            {
                XAttribute attribute = rootElement.Attributes(constants.StateFileRootNamespaceAttributeName).First();

                if (attribute != null)
                {
                    rootNamespace = attribute.Value;
                }
            }
        }

        private bool IsStreamValid(Stream stream)
        {
            var result = DoStreamCheck(stream);
            bool isValid = result.Item1;
            if (!isValid)
            {
                // log the message, result.Item2
                throw new ArgumentException(result.Item2);
            }
            return isValid;
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
