using Avak.StateMachine.Core.Contracts;

namespace Avak.StateMachine.Core.Implimentation
{
    public class XmlKeys : IXmlKeys
    {
        public string StateFileRootElementName => DefaultXmlConstants.StateFileRootElementName; // AvakStates

        public string StateFileRootNamespaceAttributeName => DefaultXmlConstants.StateFileRootNamespaceAttributeName; // Namespace

        public string StateFileTriggerCollectionElementName => DefaultXmlConstants.StateFileTriggerCollectionElementName; // Triggers

        public string StateFileTriggerElementName => DefaultXmlConstants.StateFileTriggerElementName; // Trigger

        public string StateFileTriggerNameAttributeName => DefaultXmlConstants.StateFileTriggerNameAttributeName; // Name

        public string StateFileTriggerSourceAttributeName => DefaultXmlConstants.StateFileTriggerSourceAttributeName; // Source

        public string StateFileStateCollectionElementName => DefaultXmlConstants.StateFileStateCollectionElementName; // States

        public string StateFileStateCollectionInitialAttributeName => DefaultXmlConstants.StateFileStateCollectionInitialAttributeName; // Initial

        public string StateFileStateElementName => DefaultXmlConstants.StateFileStateElementName; // State

        public string StateFileStateNameAttributeName => DefaultXmlConstants.StateFileStateNameAttributeName; // Name

        public string StateFileStateNamespaceAttributeName => DefaultXmlConstants.StateFileStateNamespaceAttributeName; // Namespace

        public string StateFileStateSubStateAssemblyAttributeName => DefaultXmlConstants.StateFileStateSubStateAssemblyAttributeName; // "SubStateAssembly";

        public string StateFileStateSubStateXmlFileAttributeName => DefaultXmlConstants.StateFileStateSubStateXmlFileAttributeName; //  "SubStateXmlFile";

        public string StateFileTransitionElementName => DefaultXmlConstants.StateFileTransitionElementName; // Transaction

        public string StateFileTransitionTriggerAttributeName => DefaultXmlConstants.StateFileTransitionTriggerAttributeName; // Trigger

        public string StateFileTransitionTargetAttributeName => DefaultXmlConstants.StateFileTransitionTargetAttributeName; // Target
    }
}
