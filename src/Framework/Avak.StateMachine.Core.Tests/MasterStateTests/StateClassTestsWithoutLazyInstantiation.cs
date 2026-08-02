using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.MasterStateTests
{
    [TestClass]
    [DoNotParallelize]
    public class StateClassTestsWithoutLazyInstantiation
    {
        [TestInitialize]
        public void Setup() { }

        [TestCleanup]
        public void Cleanup()
        {
            StateXmlFileTree.Instance.Clear();
        }

        [TestMethod]
        public void NoBaseClass_ThrowsException()
        {
            // Arrange
            string masterStateXmlFile = "Avak.StateMachine.Core.Tests.StateManager.IncorrectStateBaseClass.xml";
            IXmlKeys constants = new XmlKeys();

            StateMachineManager stateMachineManager = new(constants,
                StateDependencyImplimentation.StateDependencyTypeFinderDefaultImplimentation,
                StateDependencyImplimentation.StateDependencyResolverDefaultImplimentation,
                enableLazyStateInstantiation: false);

            stateMachineManager.SetMasterStateFile(Assembly.GetExecutingAssembly(), masterStateXmlFile);

            // Act
            Exception ex = Assert.Throws<Exception>(() => stateMachineManager.GetCurrentStateGraph().StateList);

            // Assert
            string message = "Error creating state InCorrectBaseClass in namespace Avak.StateMachine.Core.Tests.StateManager.States\r\nTrying to create state object of type Avak.StateMachine.Core.Tests.StateManager.States.InCorrectBaseClass. Avak.StateMachine.Core.Tests.StateManager.States.InCorrectBaseClass must inherit MasterStateBase";
            Assert.AreEqual(message, ex.Message);
        }

        [TestMethod]
        public void InCorrectTypeName_ThrowsException()
        {
            // Arrange
            string masterStateXmlFile = "Avak.StateMachine.Core.Tests.StateManager.IncorrectTypeName.xml";
            IXmlKeys constants = new XmlKeys();

            StateMachineManager stateMachineManager = new(constants,
                StateDependencyImplimentation.StateDependencyTypeFinderDefaultImplimentation,
                StateDependencyImplimentation.StateDependencyResolverDefaultImplimentation);

            stateMachineManager.SetMasterStateFile(Assembly.GetExecutingAssembly(), masterStateXmlFile);


            // Act
            Exception ex = Assert.Throws<Exception>(() => stateMachineManager.GetCurrentStateGraph().StateList);

            // Assert
            string message = "The type DifferentNamespaceTestB with namespace Avak.StateMachine.Core.Tests.StateManager.States.NamespaceBb is not found\r\nCheck the name of the type DifferentNamespaceTestB\r\nAlso Check the namespace Avak.StateMachine.Core.Tests.StateManager.States.NamespaceBb";
            Assert.AreEqual(message, ex.Message);
        }
    }
}
