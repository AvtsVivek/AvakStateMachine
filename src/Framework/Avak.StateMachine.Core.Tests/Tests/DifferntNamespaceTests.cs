using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using Avak.StateMachine.Core.States;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.Tests
{
    [TestClass]
    public class DifferntNamespaceTests
    {
        private Stream FileStream = null!;
        [TestInitialize]
        public void Setup()
        {
            var assembly = Assembly.GetExecutingAssembly();

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
            // This test ensures these types are correctly located and instanciated.
            // 

            string appStateFile = "Avak.StateMachine.Core.Tests.StateManager.DifferentNamespaces.xml";
            FileStream = assembly.GetManifestResourceStream(appStateFile)!;
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Runs after each test (clean up files, database connections, etc.)

            // Close the stream.
            FileStream.Close();
            FileStream.Dispose();
        }

        [TestMethod]
        public void GetStates_WithCountThree_StatesFound()
        {
            // Arrange
            IXmlKeys constants = new XmlKeys();

            StateMachineManager stateMachineManager = new(constants, StateDependencyImplimentation.StateDependencyObjectFinderDefaultImplimentation);

            stateMachineManager.SetStateFile(FileStream);

            bool loadResult = stateMachineManager.LoadStateFile();

            // Act
            List<MasterStateBase> states = stateMachineManager.GetStateGraph().StateList;

            // Assert
            Assert.HasCount(3, states);
            Assert.IsTrue(states[0].IsInitial);
            Assert.IsFalse(states[1].IsInitial);
            Assert.IsFalse(states[2].IsInitial);
        }
    }
}
