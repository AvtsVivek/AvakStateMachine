using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.Tests
{
    [TestClass]
    public class WithTriggerAndTwoStatesTests
    {
        private Stream FileStream = null!;
        [TestInitialize]
        public void Setup()
        {
            // Runs before each test
            var assembly = Assembly.GetExecutingAssembly();
            string appStateFile = "Avak.StateMachine.Core.Tests.StateManager.WithTriggersTagAndTwoStatesStateFile.xml";
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
        public void GetStates_WithCountTwo_StatesFound()
        {
            // Arrange
            IXmlKeys constants = new XmlKeys();
            // IStateFileReader reader = new XmlStateFileReader(constants);
            StateMachineManager stateMachineManager = new(constants);
            stateMachineManager.SetStateFile(FileStream);
            bool loadResult = stateMachineManager.LoadStateFile();

            // Act
            List<Trigger> triggers = stateMachineManager.GetStateGraph().TriggerList;
            StateGraph stateGraph = stateMachineManager.GetStateGraph();
            List<StateBase> states = stateGraph.StateList;
            StateBase zerothState = states[0];

            // Assert
            Assert.HasCount(2, states);
            Assert.AreEqual(zerothState, stateGraph.InitialState);
            Assert.IsTrue(stateGraph.InitialState.IsInitial);
        }
    }
}
