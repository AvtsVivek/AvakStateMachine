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

        private List<Trigger> triggers;

        private List<MasterStateBase> states;

        private StateGraph stateGraph;

        private IXmlKeys constants;

        private ITypeFinder typeFinder;

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
            triggers = null!;
            states = null!;
            stateGraph = null!;
            isStateFileValidAndLoaded = false;
        }

        public void SetStateFile(Stream stream)
        {
            if (IsStreamValid(stream))
            {
                XmlFileStream = stream;
            }
        }

        public void SetStateFilePath(string filePath)
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

            SetStateFile(fileStream);
        }

        public bool LoadStateFile()
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

        public string GetRootNamespace()
        {
            ReadRootStateNamespace();
            return rootNamespace;
        }

        public List<Trigger> GetTriggers()
        {
            ReadTriggers();
            return triggers;
        }

        public IStateGraph GetStateGraph(StateDependencyObjectFinder stateDependencyObjectFinderDelegate)
        {
            if (stateGraph != null)
            {
                return stateGraph;
            }

            // First ensure root namespace is read.
            ReadRootStateNamespace();

            if (triggers == null)
                ReadTriggers();

            XElement? stateCollectionElement = XResourceDoc
                .Descendants(constants.StateFileStateCollectionElementName)
                .FirstOrDefault();

            if (stateCollectionElement == null)
            {
                throw new XmlException("States element must be present in the state xml file.");
            }

            ReadStates(stateCollectionElement, stateDependencyObjectFinderDelegate);

            MasterStateBase? initialState = SetInitialState(stateCollectionElement);

            stateGraph = new StateGraph(states, triggers!, initialState!);

            return stateGraph;
        }


        private MasterStateBase? SetInitialState(XElement stateCollectionElement)
        {
            if (states.Count == 0)
            {
                // No states. So just return null.
                return null;
            }

            XAttribute? initialAttribute = stateCollectionElement
                .Attribute(constants.StateFileStateCollectionInitialAttributeName);

            if (initialAttribute == null)
            {
                // Pick the very first State Element and set that as the Initial Element.
                states[0].IsInitial = true;
                return states[0];
            }


            if (string.IsNullOrWhiteSpace(initialAttribute!.Value))
            {
                throw new XmlException($"{constants.StateFileStateCollectionInitialAttributeName} on {constants.StateFileStateCollectionElementName} must be set to a valid state");
            }

            string initialStateName = initialAttribute.Value;

            MasterStateBase? initialState = states.FirstOrDefault(state => state.Name == initialStateName);

            if (initialState == null)
            {
                throw new XmlException($"{constants.StateFileStateCollectionInitialAttributeName} " +
                    $"on {constants.StateFileStateCollectionElementName} must be set to a valid state. " +
                    $"The {initialStateName} does not represent any state.");
            }
            initialState.IsInitial = true;
            return initialState;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stateCollectionElement"></param>
        /// <exception cref="Exception"></exception>
        private void ReadStates(XElement stateCollectionElement, StateDependencyObjectFinder stateDependencyObjectFinderDelegate)
        {
            if (stateCollectionElement == null)
            {
                throw new Exception($"States not present in the file {XResourceDoc.BaseUri}");
            }

            List<XElement> stateElements = stateCollectionElement
                .Descendants(constants.StateFileStateElementName)
                .ToList();

            PopulateStates(stateElements, stateDependencyObjectFinderDelegate);

            Console.WriteLine($"States count {states.Count}");
        }

        private void PopulateStates(List<XElement> stateElements, StateDependencyObjectFinder stateDependencyObjectFinderDelegate)
        {
            states = GetStates(stateElements, stateDependencyObjectFinderDelegate);

            AddTargetStateToStateTransitions(stateElements, states);
        }

        private void AddTargetStateToStateTransitions(List<XElement> stateElements, List<MasterStateBase> states)
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

        private List<MasterStateBase> GetStates(List<XElement> stateElements, StateDependencyObjectFinder stateDependencyObjectFinderDelegate)
        {
            List<MasterStateBase> states = [];

            foreach (XElement stateElement in stateElements)
            {
                XAttribute? stateNameAttribute = stateElement.Attribute(constants.StateFileStateNameAttributeName)
                    ?? throw new Exception($"State Element name missing in state file {XResourceDoc.BaseUri}");

                string stateName = stateNameAttribute.Value;

                XAttribute? stateNamespaceAttribute = stateElement.Attribute(constants.StateFileStateNamespaceAttributeName);

                string stateNamespace = string.Empty;

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

                MasterStateBase state = CreateState(stateName, stateNamespace, stateDependencyObjectFinderDelegate);

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

        private List<Transition> GetTransitionsForState(XElement stateElement)
        {
            List<XElement> transitionElements = stateElement
                .Descendants(constants.StateFileTransitionElementName)
                .ToList();

            List<Transition> transitionsForState = new();

            foreach (XElement element in transitionElements)
            {
                Transition transition = CreateTriansition(element, stateElement);
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
                XAttribute stateAttribute = stateElement.Attribute(constants.StateFileStateNameAttributeName)!;

                throw new Exception($"Trigger Attribute missing in the file {XResourceDoc.BaseUri} for state {stateAttribute.Value}");
            }

            Trigger? triggerForTransition = triggers
                .FirstOrDefault(trigger => trigger.Name == triggerAttribute.Value);

            if (triggerForTransition == null)
            {
                string errorMessage = $"Trigger with name {triggerAttribute.Value} not found for the transition of the state {stateElement.Attribute(constants.StateFileStateNameAttributeName)!}";

                throw new Exception(errorMessage);
            }

            transition.Trigger = triggerForTransition;

            return transition;
        }

        private MasterStateBase CreateState(string stateName, string statesNamespace, StateDependencyObjectFinder stateDependencyObjectFinderDelegate)
        {
            MasterStateBase stateBase = null!;

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

                        return null!;
                    }

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
                        throw new Exception($"Trying to create state object. " +
                            $"Instanciation of the type {ctype} failed. ");
                    }

                    stateBase = (stateObject as MasterStateBase)!;

                    if (stateBase is null)
                    {
                        throw new Exception($"Trying to create state object of type {ctype}. " +
                            $"{ctype} must inherit {nameof(MasterStateBase)}");
                    }
                }
                catch (Exception ex)
                {
                    // to do need logging.
                    // logger.Error(ex, $"Error creating state {stateName} in namespace {statesNamespace}.");
                    throw;
                }

                stateBase.Name = stateName;
            }

            return stateBase;
        }

        private void ReadTriggers()
        {
            string triggersString = constants.StateFileTriggerCollectionElementName;

            XElement? triggerCollectionElement = XResourceDoc.Descendants(triggersString).FirstOrDefault();

            if (triggerCollectionElement == null)
            {
                string message = $"{triggersString} not present in the state file. " +
                    $"Add <{triggersString}></{triggersString}> if you intend to define just states without triggers.";
                throw new Exception(message);
            }

            List<XElement> triggerElements = triggerCollectionElement
                .Descendants(constants.StateFileTriggerElementName)
                .ToList();

            Console.WriteLine($"{triggersString} count {triggerElements.Count}");

            triggers = PopulateTriggers(triggerElements);

            // Ensure all of the triggers in the file are unique.
            // Get unique trigger count
            int distinctTriggerCount = triggers.DistinctBy(x => x.Name).Count();

            if (triggers.Count() != distinctTriggerCount)
            {
                throw new Exception($"{triggersString} present in the file {XResourceDoc.BaseUri} are not unique." +
                    Environment.NewLine + $"Please ensure trigger names are unique.");
            }
        }

        private List<Trigger> PopulateTriggers(List<XElement> triggerElements)
        {
            List<Trigger> triggers = [];

            foreach (XElement triggerElement in triggerElements)
            {
                XAttribute? triggerNameAttribute = triggerElement.Attribute(constants.StateFileTriggerNameAttributeName);

                if (triggerNameAttribute == null)
                {
                    throw new Exception($"Trigger Element name missing in state file {XResourceDoc.BaseUri}");
                }

                string triggerName = triggerNameAttribute.Value;

                TriggerSource triggerSource = GetTriggerSource(triggerElement);

                Trigger trigger = new(triggerName, triggerSource);

                triggers.Add(trigger);
            }

            return triggers;
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

            if (!Enum.TryParse<TriggerSource>(sourceName, out TriggerSource triggerSource))
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

        private void ReadRootStateNamespace()
        {
            XElement? rootElement = XResourceDoc.Descendants(constants.StateFileRootElementName).First();

            if (rootElement != null)
            {
                XAttribute attribute = rootElement.Attributes(constants.StateFileRootNamespaceAttributeName).First();

                if (attribute != null)
                {
                    rootNamespace = attribute.Value;

                    Console.WriteLine($"Root Namespace is {rootNamespace}");
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
