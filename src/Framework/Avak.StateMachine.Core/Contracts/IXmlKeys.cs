namespace Avak.StateMachine.Core.Contracts
{
    /// <summary>
    /// This interface lets you decouple the keys in the xml file xml reader hard coding.
    /// This will enable to customize the keys.
    /// For example, the framework users dont want to use Capital letters, 
    /// they just want to use only 'states' in stead of 'States' or 'triggers' instead of 'Triggers'.
    /// </summary>
    public interface IXmlKeys
    {
        /// <summary>
        /// Root Element name, AvakState
        /// </summary>
        string StateFileRootElementName { get; }

        /// <summary>
        /// Attribute Namespace
        /// </summary>
        string StateFileRootNamespaceAttributeName { get; }

        /// <summary>
        /// Element Name, Triggers
        /// </summary>
        string StateFileTriggerCollectionElementName { get; }

        /// <summary>
        /// Element Name, Trigger
        /// </summary>
        string StateFileTriggerElementName { get; }

        /// <summary>
        /// Attribute 'Name' on Trigger element.
        /// </summary>
        string StateFileTriggerNameAttributeName { get; }

        /// <summary>
        /// Attribute 'Source' on Trigger element.
        /// </summary>
        string StateFileTriggerSourceAttributeName { get; }

        /// <summary>
        /// States Element Name
        /// </summary>
        string StateFileStateCollectionElementName { get; }

        /// <summary>
        /// Inital attribute on states element.
        /// </summary>
        string StateFileStateCollectionInitialAttributeName { get; }

        /// <summary>
        /// State Element name
        /// </summary>
        string StateFileStateElementName { get; }

        /// <summary>
        /// Name attribute on State element
        /// </summary>
        string StateFileStateNameAttributeName { get; }

        /// <summary>
        /// Namespace attribute on state element.
        /// </summary>
        string StateFileStateNamespaceAttributeName { get; }

        /// <summary>
        /// Transition element inside of a state element.
        /// </summary>
        string StateFileTransitionElementName { get; }

        /// <summary>
        /// Trigger attribute on a transition element.
        /// </summary>
        string StateFileTransitionTriggerAttributeName { get; }

        /// <summary>
        /// Target attribute on a transition element.
        /// </summary>
        string StateFileTransitionTargetAttributeName { get; }
    }
}
