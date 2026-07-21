using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using Avak.StateMachine.Core.States;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.MasterStateTests
{
    [TestClass]
    public class WithTriggersTagAndOneStateTests
    {
        private Stream FileStream = null!;
        [TestInitialize]
        public void Setup()
        {
            // Runs before each test
            var assembly = Assembly.GetExecutingAssembly();
            string appStateFile = "Avak.StateMachine.Core.Tests.StateManager.WithTriggersTagAndOneStateStateFile.xml";
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
        public void GetStates_WithCountOne_StateFound()
        {
            // Arrange
            IXmlKeys constants = new XmlKeys();
            StateMachineManager stateMachineManager = new(constants, StateDependencyImplimentation.StateDependencyObjectFinderDefaultImplimentation);
            stateMachineManager.SetMasterStateFile(FileStream);
            bool loadResult = stateMachineManager.LoadMasterStateFile();

            // Act
            List<Trigger> triggers = stateMachineManager.GetStateGraph().TriggerList;
            List<MasterStateBase> states = stateMachineManager.GetStateGraph().StateList;
            StateBase zerothState = states[0];

            // Assert
            // Assert.AreEqual("Avak.StateMachine.Core.Tests.StateManager.States", stateNamespace);
            Assert.IsTrue(loadResult);
            Assert.IsEmpty(triggers);
            Assert.HasCount(1, states);
            Assert.AreEqual("Aa", zerothState.Name);
            Assert.AreEqual("Aa", zerothState.Id);
            Assert.AreEqual("Avak.StateMachine.Core.Tests.StateManager.States.Aa", zerothState.GetType().FullName);
        }
    }
}
