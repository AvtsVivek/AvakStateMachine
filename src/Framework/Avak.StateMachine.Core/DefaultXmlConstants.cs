namespace Avak.StateMachine.Core
{
    internal class DefaultXmlConstants
    {
        /// <summary>
        /// Represents the Root XML element name of the state file.
        /// </summary>
        public const string StateFileRootElementName = "AvakStates";

        public const string StateFileRootNamespaceAttributeName = "Namespace";

        public const string StateFileTriggerCollectionElementName = "Triggers";

        public const string StateFileTriggerElementName = "Trigger";

        public const string StateFileTriggerNameAttributeName = "Name";

        public const string StateFileTriggerSourceAttributeName = "Source";

        public const string StateFileStateCollectionElementName = "States";

        public const string StateFileStateCollectionInitialAttributeName = "Initial";

        public const string StateFileStateElementName = "State";

        public const string StateFileStateNameAttributeName = "Name";

        public const string StateFileStateNamespaceAttributeName = "Namespace";

        public const string StateFileStateSubStateAssemblyAttributeName = "SubStateAssembly";

        public const string StateFileStateSubStateXmlFileAttributeName = "SubStateXmlFile";

        public const string StateFileTransitionElementName = "Transition";

        public const string StateFileTransitionTriggerAttributeName = "Trigger";

        public const string StateFileTransitionTargetAttributeName = "Target";
    }
}