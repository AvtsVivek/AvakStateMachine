using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using Avak.StateMachine.Core.States;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.MasterStateTests
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
            StateMachineManager stateMachineManager = new(constants, StateDependencyImplimentation.StateDependencyObjectFinderDefaultImplimentation);
            stateMachineManager.SetMasterStateFile(FileStream);
            bool loadResult = stateMachineManager.LoadMasterStateFile();

            // Act
            List<Trigger> triggers = stateMachineManager.GetCurrentStateGraph().TriggerList;
            IStateGraph stateGraph = stateMachineManager.GetCurrentStateGraph();
            List<MasterStateBase> states = stateGraph.StateList;
            StateBase zerothState = states[0];

            // Assert
            Assert.HasCount(1, states);
            Assert.AreEqual(zerothState, stateGraph.InitialState);
            Assert.IsTrue(stateGraph.InitialState.IsInitial);
        }
    }
}
