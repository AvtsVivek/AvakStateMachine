namespace Avak.StateMachine.Core.Configuration;

/// <summary>
/// Loads and parses state machine configuration from XML.
/// </summary>
public interface IConfigurationLoader
{
    /// <summary>
    /// Loads configuration from an XML string.
    /// </summary>
    StateMachineConfiguration LoadFromXml(string xml);
    
    /// <summary>
    /// Loads configuration from an XML file.
    /// </summary>
    StateMachineConfiguration LoadFromFile(string filePath);
}
