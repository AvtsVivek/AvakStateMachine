namespace Avak.StateMachine.ConsoleUI
{
    public class ThorConstants
    {
        /// <summary>
        /// Represents the Root XML element name of the state file.
        /// </summary>
        public const string StateFileRootElementName = "thorstates";

        public const string StateFileRootNamespaceAttributeName = "namespace";

        public const string StateFileTriggerCollectionElementName = "triggers";

        public const string StateFileTriggerElementName = "trigger";

        public const string StateFileTriggerNameAttributeName = "name";

        public const string StateFileTriggerSourceAttributeName = "source";

        public const string StateFileStateCollectionElementName = "states";

        public const string StateFileStateCollectionInitialAttributeName = "initial";

        public const string StateFileStateElementName = "state";

        public const string StateFileStateNameAttributeName = "name";

        public const string StateFileStateNamespaceAttributeName = "namespace";

        public const string StateFileTransitionElementName = "transition";

        public const string StateFileTransitionTriggerAttributeName = "trigger";

        public const string StateFileTransitionTargetAttributeName = "target";
    }
}
