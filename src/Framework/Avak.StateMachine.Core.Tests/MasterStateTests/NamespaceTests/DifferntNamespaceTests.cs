using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using Avak.StateMachine.Core.States;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.MasterStateTests.NamespaceTests
{
    // This test checks for three different state classes defined in three different namespaces.
    // Looking at the following app state file, the state classes and their namespaces are as follows.
    // 1. The state DifferentNamespaceTestAa is defined in the namespace Avak.StateMachine.Core.Tests.StateManager.States
    // This is the root name space, defined at the root of the xml file.
    // Avak.StateMachine.Core.Tests.StateManager.States
    // Note there is no Namespace attribute in the state element DifferentNamespaceTestAa.
    // So for this type DifferentNamespaceTestAa, the namespaces is picked up from the root namespace shown above.
    // Then we have the following two. The namespaces are declared as attributes in the respective elements.
    // 2. DifferentNamespaceTestBb defined in the namespace Avak.StateMachine.Core.Tests.StateManager.States.NamespaceBb
    // 3. DifferentNamespaceTestCc defined in the namespace Avak.StateMachine.Core.Tests.StateManager.States.NamespaceCc
    // Note all the three classes are defined in three different namespaces.
    // This test ensures these types are correctly located correctely. All of the three may not be isntanciated, because of lazy instanciation.
    // The fact that the test passes(with only one state being instanciated) proves that all of the three states are located correctly.
    // .
    // Try the following. In the xml file, try altering any of the namespace to something invalid.
    // For example, try changing the following 
    // Avak.StateMachine.Core.Tests.StateManager.States.NamespaceBb
    // to 
    // Avak.StateMachine.Core.Tests.StateManager.States.NamespaceBbbbbbb
    // Now run the test. This will result in an exception, indicating the type is not found.
    // So without altering, if the test runs to pass, then all of the three states are located correctly. 

    [TestClass]
    public class DifferntNamespaceTests
    {
        private string masterStateXmlFile = string.Empty;
        [TestInitialize]
        public void Setup()
        {
            masterStateXmlFile = "Avak.StateMachine.Core.Tests.StateManager.NameSpaceTestsXmlFiles.DifferentNamespaces.xml";
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Runs after each test (clean up files, database connections, etc.)
            // Close the stream.
        }

        [TestMethod]
        public void GetStates_WithCountOne_StatesFound()
        {
            // Arrange
            IXmlKeys constants = new XmlKeys();

            StateMachineManager stateMachineManager = new(constants,
                StateDependencyImplimentation.StateDependencyTypeFinderDefaultImplimentation,
                StateDependencyImplimentation.StateDependencyResolverDefaultImplimentation);

            stateMachineManager.SetMasterStateFile(Assembly.GetExecutingAssembly(), masterStateXmlFile);


            // Act
            List<MasterStateBase> states = stateMachineManager.GetCurrentStateGraph().StateList;

            // Assert
            Assert.HasCount(1, states);
            Assert.IsTrue(states[0].IsInitial);

            // Assert.IsFalse(states[1].IsInitial);
            // Assert.IsFalse(states[2].IsInitial);
        }
    }
}
