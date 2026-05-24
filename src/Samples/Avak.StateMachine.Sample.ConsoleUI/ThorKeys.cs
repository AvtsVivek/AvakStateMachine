using Avak.StateMachine.Core.Contracts;

namespace Avak.StateMachine.Sample.ConsoleUI
{
    internal class ThorKeys : IXmlKeys
    {
        public string StateFileRootElementName => ThorConstants.StateFileRootElementName;

        public string StateFileRootNamespaceAttributeName => ThorConstants.StateFileRootNamespaceAttributeName;

        public string StateFileTriggerCollectionElementName => ThorConstants.StateFileTriggerCollectionElementName;

        public string StateFileTriggerElementName => ThorConstants.StateFileTriggerElementName;

        public string StateFileTriggerNameAttributeName => ThorConstants.StateFileTriggerNameAttributeName;

        public string StateFileTriggerSourceAttributeName => ThorConstants.StateFileTriggerSourceAttributeName;

        public string StateFileStateCollectionElementName => ThorConstants.StateFileStateCollectionElementName;

        public string StateFileStateCollectionInitialAttributeName => ThorConstants.StateFileStateCollectionInitialAttributeName;

        public string StateFileStateElementName => ThorConstants.StateFileStateElementName;

        public string StateFileStateNameAttributeName => ThorConstants.StateFileStateNameAttributeName;

        public string StateFileStateNamespaceAttributeName => ThorConstants.StateFileStateNamespaceAttributeName;

        public string StateFileTransitionElementName => ThorConstants.StateFileTransitionElementName;

        public string StateFileTransitionTriggerAttributeName => ThorConstants.StateFileTransitionTriggerAttributeName;

        public string StateFileTransitionTargetAttributeName => ThorConstants.StateFileTransitionTargetAttributeName;


    }
}
