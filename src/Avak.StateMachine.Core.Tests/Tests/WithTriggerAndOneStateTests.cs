using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.Tests
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
            // IStateFileReader reader = new XmlStateFileReader(constants);
            StateMachineManager stateMachineManager = new(constants);
            stateMachineManager.SetStateFile(FileStream);
            bool loadResult = stateMachineManager.LoadStateFile();

            // Act
            // string stateNamespace = stateMachineManager.GetRootNamespace();
            // string stateNamespace = "Need to take a look";
            List<Trigger> triggers = stateMachineManager.GetStateGraph().TriggerList;
            List<StateBase> states = stateMachineManager.GetStateGraph().StateList;
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
