using Avak.StateMachine.Core.Contracts;
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

        private List<StateBase> states;

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

        public StateGraph GetStateGraph()
        {
            // First ensure root namespace is read.
            ReadRootStateNamespace();

            if (triggers == null)
                ReadTriggers();

            XElement stateCollectionElement = XResourceDoc
                .Descendants(constants.StateFileStateCollectionElementName)
                .First();

            ReadStates(stateCollectionElement);

            StateBase? initialState = SetInitialState(stateCollectionElement);

            stateGraph = new StateGraph(states, triggers!, initialState!);

            return stateGraph;
        }


        private StateBase? SetInitialState(XElement stateCollectionElement)
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

            StateBase? initialState = states.Where(state => state.Name == initialStateName).First();

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
        private void ReadStates(XElement stateCollectionElement)
        {
            if (stateCollectionElement == null)
            {
                throw new Exception($"States not present in the file {XResourceDoc.BaseUri}");
            }

            List<XElement> stateElements = stateCollectionElement
                .Descendants(constants.StateFileStateElementName)
                .ToList();

            PopulateStates(stateElements);

            Console.WriteLine($"States count {states.Count}");
        }

        private void PopulateStates(List<XElement> stateElements)
        {
            states = GetStates(stateElements);

            AddTargetStateToStateTransitions(stateElements, states);
        }

        private void AddTargetStateToStateTransitions(List<XElement> stateElements, List<StateBase> states)
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

                    StateBase targetState = states.Where(state => state.Name == targetAttribute!.Value).First();

                    if (targetState == null)
                    {
                        string errorMessage = $"Target attribute {targetAttribute!.Value} on the transition " +
                            $"with Trigger name {triggerAttribute!.Name} in the state " +
                            $"{state.Name} is invalid. No state with name {targetAttribute!.Value} is found";

                        throw new Exception(errorMessage);
                    }

                    Transition transition = state
                        .Transitions
                        .Where(transition => transition.Trigger.Name == triggerAttribute!.Value)
                        .First();

                    transition.Target = targetState;
                }
            }
        }

        private List<StateBase> GetStates(List<XElement> stateElements)
        {
            List<StateBase> states = [];

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

                StateBase state = CreateState(stateName, stateNamespace);

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

            Trigger triggerForTransition = triggers
                .Where(trigger => trigger.Name == triggerAttribute.Value)
                .First();

            if (triggerForTransition == null)
            {
                string errorMessage = $"Trigger with name {triggerAttribute.Value} not found in the transition for the state {stateElement.Attribute(constants.StateFileStateNameAttributeName)!}";

                throw new Exception(errorMessage);
            }

            transition.Trigger = triggerForTransition;

            return transition;
        }

        private StateBase CreateState(string stateName, string statesNamespace)
        {
            StateBase stateBase = null!;

            bool successfullyFound = typeFinder.TryFindType(statesNamespace, stateName, out Type ctype);

            if (!successfullyFound)
            {
                string message = $"The type {stateName} with namespace {statesNamespace} is not found" + Environment.NewLine;

                message = message + $"Check the name of the type {stateName}" + Environment.NewLine;

                message = message + $"Also Check the namespace {statesNamespace}";

                throw new Exception(message);
            }

            if (successfullyFound)
            {
                try
                {
                    ConstructorInfo ctor = ctype.GetConstructor(Type.EmptyTypes)!;

                    object stateObject = ctor.Invoke(null);

                    if (stateObject == null)
                    {
                        throw new Exception($"Trying to create state object. " +
                            $"Instanciation of the type {ctype} failed. ");
                    }

                    stateBase = (stateObject as StateBase)!;

                    if (stateBase is null)
                    {
                        throw new Exception($"Trying to create state object of type {ctype}. " +
                            $"{ctype} must inherit {nameof(StateBase)}");
                    }
                }
                catch (Exception ex)
                {

                    // logger.Error(ex, $"Error creating state {stateName} in namespace {statesNamespace}.");
                    throw;
                }

                stateBase.Name = stateName;
            }

            return stateBase;
        }

        //private bool TryFindType(string nameSpace, string typeName, out Type type)
        //{
        //    string typeFullName = nameSpace + "." + typeName;
        //    string errorMessage = string.Empty;
        //    lock (typeCache)
        //    {
        //        if (!typeCache.TryGetValue(typeFullName, out type!))
        //        {
        //            type = FindTypeInAssembliesInCurrentAppDomain(typeFullName);
        //            if (type != null)
        //            {
        //                typeCache[typeFullName] = type;
        //            }
        //        }
        //    }
        //    return type != null;
        //}

        //private Type FindTypeInAssembliesInCurrentAppDomain(string typeName)
        //{
        //    Type t = null!;

        //    foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
        //    {
        //        t = a.GetType(typeName)!;
        //        if (t != null)
        //            break;
        //    }

        //    if (t == null && numberOfTries < 2)
        //    {
        //        numberOfTries++;
        //        Thread.Sleep(100);
        //        t = FindTypeInAssembliesInCurrentAppDomain(typeName);
        //    }
        //    else
        //    {
        //        numberOfTries = 0;
        //    }

        //    return t!;
        //}

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
