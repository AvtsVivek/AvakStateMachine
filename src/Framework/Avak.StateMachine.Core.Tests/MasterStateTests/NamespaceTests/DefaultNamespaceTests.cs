using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using System.Reflection;
using System.Xml;

namespace Avak.StateMachine.Core.Tests.MasterStateTests.NamespaceTests
{
    // There must be a Namespace attribute. If not exception is thrown
    [TestClass]
    public class DefaultAndNoNamepsaceTests
    {
        [TestInitialize]
        public void Setup() { }

        [TestCleanup]
        public void Cleanup()
        {
            StateXmlFileTree.Instance.Clear();
        }

        [TestMethod]
        public void NoNamepsaceAttribute_ThrowsException()
        {
            // Arrange
            string masterStateXmlFile = "Avak.StateMachine.Core.Tests.StateManager.NameSpaceTestsXmlFiles.NoNamespaceAttribute.xml";
            IXmlKeys constants = new XmlKeys();

            StateMachineManager stateMachineManager = new(constants,
                StateDependencyImplimentation.StateDependencyTypeFinderDefaultImplimentation,
                StateDependencyImplimentation.StateDependencyResolverDefaultImplimentation);

            stateMachineManager.SetMasterStateFile(Assembly.GetExecutingAssembly(), masterStateXmlFile);

            // Act
            // Exception ex = Assert.Throws<XmlException>(() => stateMachineManager.LoadMasterStateFile());
            Exception ex = Assert.Throws<XmlException>(stateMachineManager.GetCurrentStateGraph);

            // Assert
            string message = "Namespace is missing at the root AvakStates in the state xml file File: " +
                "Avak.StateMachine.Core.Tests.StateManager.NameSpaceTestsXmlFiles.NoNamespaceAttribute.xml, " +
                "Assembly: Avak.StateMachine.Core.Tests, Version=1.0.0.0, Culture=neutral, " +
                "PublicKeyToken=null";
            Assert.AreEqual(message, ex.Message);
        }

        [TestMethod]
        [DoNotParallelize]
        public void InCorrectTypeName_ThrowsException()
        {
            // Arrange
            string masterStateXmlFile = "Avak.StateMachine.Core.Tests.StateManager.NameSpaceTestsXmlFiles.EmptyStringDefaultNamespace.xml";
            IXmlKeys constants = new XmlKeys();

            StateMachineManager stateMachineManager = new(constants,
                StateDependencyImplimentation.StateDependencyTypeFinderDefaultImplimentation,
                StateDependencyImplimentation.StateDependencyResolverDefaultImplimentation);

            stateMachineManager.SetMasterStateFile(Assembly.GetExecutingAssembly(), masterStateXmlFile);

            // Act () => stateMachineManager.GetCurrentStateGraph().StateList
            Exception ex = Assert.Throws<XmlException>(stateMachineManager.GetCurrentStateGraph);
            // Exception ex1 = Assert.Throws<XmlException>(stateMachineManager.LoadMasterStateFile);

            // Assert
            string message = "Namespace at the root AvakStates in the state xml file File: " +
                "Avak.StateMachine.Core.Tests.StateManager.NameSpaceTestsXmlFiles.EmptyStringDefaultNamespace.xml, " +
                "Assembly: Avak.StateMachine.Core.Tests, Version=1.0.0.0, Culture=neutral, " +
                "PublicKeyToken=null\r\nis not having any value. " +
                "Ensure to have it as some non blank, non white space value, which represents a valid namespace.";
            Assert.AreEqual(message, ex.Message);
        }
    }
}
