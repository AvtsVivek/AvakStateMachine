using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.MasterStateTests
{
    // Checks for correct namespace and also checks the states are derived from correct state base 
    [TestClass]
    public class StateClassTests
    {
        [TestInitialize]
        public void Setup() { }

        [TestCleanup]
        public void Cleanup() { }

        [TestMethod]
        public void NoBaseClass_ThrowsException()
        {
            // Arrange
            Assembly assembly = Assembly.GetExecutingAssembly();
            string appStateFile = "Avak.StateMachine.Core.Tests.StateManager.IncorrectStateBaseClass.xml";
            Stream stream = assembly.GetManifestResourceStream(appStateFile)!;
            IXmlKeys constants = new XmlKeys();
            StateMachineManager stateMachineManager = new(constants, StateDependencyImplimentation.StateDependencyObjectFinderDefaultImplimentation);
            stateMachineManager.SetStateFile(stream);
            bool loadResult = stateMachineManager.LoadStateFile();

            // Act
            Exception ex = Assert.Throws<Exception>(() => stateMachineManager.GetStateGraph().StateList);

            // Assert
            string message = "Trying to create state object of type Avak.StateMachine.Core.Tests.StateManager.States.InCorrectBaseClass. Avak.StateMachine.Core.Tests.StateManager.States.InCorrectBaseClass must inherit MasterStateBase";
            Assert.AreEqual(message, ex.Message);
        }

        [TestMethod]
        public void InCorrectTypeName_ThrowsException()
        {
            // Arrange
            Assembly assembly = Assembly.GetExecutingAssembly();
            string appStateFile = "Avak.StateMachine.Core.Tests.StateManager.IncorrectTypeName.xml";
            Stream stream = assembly.GetManifestResourceStream(appStateFile)!;
            IXmlKeys constants = new XmlKeys();
            //IStateFileReader reader = new XmlStateFileReader(constants);
            StateMachineManager stateMachineManager = new(constants, StateDependencyImplimentation.StateDependencyObjectFinderDefaultImplimentation);
            stateMachineManager.SetStateFile(stream);
            bool loadResult = stateMachineManager.LoadStateFile();

            // Act
            Exception ex = Assert.Throws<Exception>(() => stateMachineManager.GetStateGraph().StateList);

            // Assert
            string message = "The type DifferentNamespaceTestB with namespace Avak.StateMachine.Core.Tests.StateManager.States.NamespaceBb is not found\r\nCheck the name of the type DifferentNamespaceTestB\r\nAlso Check the namespace Avak.StateMachine.Core.Tests.StateManager.States.NamespaceBb";
            Assert.AreEqual(message, ex.Message);
        }
    }
}
