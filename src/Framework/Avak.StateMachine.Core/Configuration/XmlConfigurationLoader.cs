namespace Avak.StateMachine.Core.Configuration;

using System.Xml;
using System.Xml.Linq;

/// <summary>
/// Default implementation of IConfigurationLoader that parses XML state machine configurations.
/// Prohibits DTDs and external entity resolution for security.
/// </summary>
public class XmlConfigurationLoader : IConfigurationLoader
{
    public StateMachineConfiguration LoadFromXml(string xml)
    {
        var xdoc = ParseXmlSafe(xml);
        return ParseConfiguration(xdoc);
    }

    public StateMachineConfiguration LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Configuration file not found: {filePath}");
        }

        string xml = File.ReadAllText(filePath);
        return LoadFromXml(xml);
    }

    private XDocument ParseXmlSafe(string xml)
    {
        try
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                IgnoreWhitespace = true,
                IgnoreComments = true
            };

            using var reader = XmlReader.Create(new StringReader(xml), settings);
            return XDocument.Load(reader);
        }
        catch (XmlException ex)
        {
            throw new StateMachineException($"Failed to parse configuration XML: {ex.Message}", ex);
        }
    }

    private StateMachineConfiguration ParseConfiguration(XDocument xdoc)
    {
        var root = xdoc.Root ?? throw new StateMachineException("Configuration XML has no root element");

        if (root.Name != "StateMachine")
        {
            throw new StateMachineException("Root element must be 'StateMachine'");
        }

        ValidateElement(root, ["initialState"], "States", allowMultipleChildren: false);
        var config = new StateMachineConfiguration();

        var initialStateAttr = root.Attribute("initialState");
        config.InitialState = initialStateAttr?.Value
            ?? throw new StateMachineException("Missing required 'initialState' attribute on StateMachine element");

        var statesElement = root.Element("States")
            ?? throw new StateMachineException("Missing required 'States' element");
        ValidateElement(statesElement, [], "State", allowMultipleChildren: true);

        foreach (var stateElement in statesElement.Elements("State"))
        {
            ValidateElement(stateElement, ["name"], "Transitions", allowMultipleChildren: false);
            string stateName = stateElement.Attribute("name")?.Value
                ?? throw new StateMachineException("State element missing 'name' attribute");
            if (string.IsNullOrWhiteSpace(stateName))
            {
                throw new StateMachineException("State name cannot be empty");
            }
            if (config.States.ContainsKey(stateName))
            {
                throw new StateMachineException($"Duplicate state '{stateName}'");
            }

            var stateConfig = new StateConfiguration { Name = stateName };

            var transitionsElement = stateElement.Element("Transitions");
            if (transitionsElement != null)
            {
                ValidateElement(transitionsElement, [], "Transition", allowMultipleChildren: true);
                foreach (var transitionElement in transitionsElement.Elements("Transition"))
                {
                    ValidateElement(
                        transitionElement,
                        ["trigger", "to", "source", "guard", "permission", "onLeaving", "onEntering", "onEntered"],
                        null,
                        allowMultipleChildren: false);
                    var transition = ParseTransition(transitionElement);
                    string triggerName = transitionElement.Attribute("trigger")?.Value
                        ?? throw new StateMachineException($"Transition in state '{stateName}' missing 'trigger' attribute");
                    if (string.IsNullOrWhiteSpace(triggerName))
                    {
                        throw new StateMachineException($"Transition trigger cannot be empty in state '{stateName}'");
                    }

                    if (!stateConfig.Transitions.ContainsKey(triggerName))
                    {
                        stateConfig.Transitions[triggerName] = [];
                    }

                    if (stateConfig.Transitions[triggerName].Any(existing =>
                        (existing.Source ?? string.Empty) == (transition.Source ?? string.Empty)))
                    {
                        throw new StateMachineException(
                            $"Duplicate transition for trigger '{triggerName}' with source '{transition.Source ?? string.Empty}' in state '{stateName}'");
                    }

                    stateConfig.Transitions[triggerName].Add(transition);
                }
            }

            config.States[stateName] = stateConfig;
        }

        ValidateConfiguration(config);
        return config;
    }

    private TransitionConfiguration ParseTransition(XElement element)
    {
        foreach (string attributeName in new[] { "guard", "permission", "onLeaving", "onEntering", "onEntered" })
        {
            var attribute = element.Attribute(attributeName);
            if (attribute is not null && string.IsNullOrWhiteSpace(attribute.Value))
            {
                throw new StateMachineException($"Transition attribute '{attributeName}' cannot be empty");
            }
        }

        var source = element.Attribute("source");
        if (source is not null && source.Value.Length > 0 && string.IsNullOrWhiteSpace(source.Value))
        {
            throw new StateMachineException("Transition attribute 'source' cannot contain only whitespace");
        }

        var transition = new TransitionConfiguration
        {
            ToState = element.Attribute("to")?.Value
                ?? throw new StateMachineException("Transition element missing 'to' attribute"),
            Source = element.Attribute("source")?.Value,
            GuardName = element.Attribute("guard")?.Value,
            PermissionName = element.Attribute("permission")?.Value,
            OnLeavingCallbackName = element.Attribute("onLeaving")?.Value,
            OnEnteringCallbackName = element.Attribute("onEntering")?.Value,
            OnEnteredCallbackName = element.Attribute("onEntered")?.Value
        };

        return transition;
    }

    private static void ValidateElement(
        XElement element,
        string[] allowedAttributes,
        string? allowedChild,
        bool allowMultipleChildren)
    {
        foreach (var attribute in element.Attributes())
        {
            if (attribute.Name.Namespace != XNamespace.None || !allowedAttributes.Contains(attribute.Name.LocalName))
            {
                throw new StateMachineException($"Unexpected attribute '{attribute.Name}' on element '{element.Name}'");
            }
        }

        int childCount = 0;
        foreach (var node in element.Nodes())
        {
            if (node is XText text && string.IsNullOrWhiteSpace(text.Value))
            {
                continue;
            }

            if (node is not XElement child)
            {
                throw new StateMachineException($"Unexpected content in element '{element.Name}'");
            }

            if (allowedChild is null || child.Name != allowedChild)
            {
                throw new StateMachineException($"Unexpected content: element '{child.Name}' in '{element.Name}'");
            }

            childCount++;
            if (!allowMultipleChildren && childCount > 1)
            {
                throw new StateMachineException($"Multiple '{allowedChild}' elements in '{element.Name}'");
            }
        }
    }

    private void ValidateConfiguration(StateMachineConfiguration config)
    {
        if (string.IsNullOrWhiteSpace(config.InitialState))
        {
            throw new StateMachineException("InitialState cannot be empty");
        }

        if (!config.States.ContainsKey(config.InitialState))
        {
            throw new StateMachineException($"InitialState '{config.InitialState}' not found in States");
        }

        foreach (var state in config.States.Values)
        {
            foreach (var transitions in state.Transitions.Values)
            {
                foreach (var transition in transitions)
                {
                    if (string.IsNullOrWhiteSpace(transition.ToState))
                    {
                        throw new StateMachineException($"Transition 'to' state cannot be empty in state '{state.Name}'");
                    }

                    if (!config.States.ContainsKey(transition.ToState))
                    {
                        throw new StateMachineException($"Transition target state '{transition.ToState}' not found");
                    }
                }
            }
        }
    }
}
