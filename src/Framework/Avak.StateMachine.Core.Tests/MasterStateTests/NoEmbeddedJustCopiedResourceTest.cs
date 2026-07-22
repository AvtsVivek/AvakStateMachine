using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using Avak.StateMachine.Core.States;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.MasterStateTests
{

    // The objective here is to ensure the xml file can be passed in as a path.
    // That is, an xml file needed not be an embeded resource. It can just be a simpple file located any where.

    [TestClass]
    public class NoEmbeddedJustCopiedResourceTest
    {
        private string XmlFilePath = string.Empty;

        [TestInitialize]
        public void Setup()
        {
            // Runs before each test
            Assembly assembly = Assembly.GetExecutingAssembly();
            string assemblyDirectory = Path.GetDirectoryName(assembly.Location)!;
            XmlFilePath = $"{assemblyDirectory}\\StateManager\\NoEmbeddedJustCopiedResource.xml";
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Runs after each test (clean up files, database connections, etc.)
        }

        // Read triggers from xml file
        [TestMethod]
        public void GetTriggers_AndStates_WithCountZero_InitialStateSet()
        {
            // Arrange
            IXmlKeys constants = new XmlKeys();
            StateMachineManager stateMachineManager = new(constants, StateDependencyImplimentation.StateDependencyObjectFinderDefaultImplimentation);
            stateMachineManager.SetMasterStateFilePath(XmlFilePath);
            bool loadResult = stateMachineManager.LoadMasterStateFile();

            // Act
            List<Trigger> triggers = stateMachineManager.GetCurrentStateGraph().TriggerList;
            List<MasterStateBase> states = stateMachineManager.GetCurrentStateGraph().StateList;
            StateBase initialState = stateMachineManager.CurrentState;

            // Assert
            Assert.IsTrue(loadResult);
            Assert.IsEmpty(triggers);
            Assert.HasCount(1, states);
            Assert.IsTrue(states[0].IsInitial);
            Assert.AreEqual(initialState, states[0]);
        }
    }
}
