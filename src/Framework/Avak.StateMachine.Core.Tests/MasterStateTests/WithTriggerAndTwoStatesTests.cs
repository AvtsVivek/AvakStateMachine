using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using Avak.StateMachine.Core.States;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.MasterStateTests
{
    [TestClass]
    public class WithTriggerAndTwoStatesTests
    {
        private string masterStateXmlFile = string.Empty;

        [TestInitialize]
        public void Setup()
        {
            // Runs before each test
            masterStateXmlFile = "Avak.StateMachine.Core.Tests.StateManager.WithTriggersTagAndTwoStatesStateFile.xml";
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Runs after each test (clean up files, database connections, etc.)
            StateXmlFileTree.Instance.Clear();
            // Close the stream.
        }

        [TestMethod]
        public void GetStates_WithCountTwo_StatesFound()
        {
            // Arrange
            IXmlKeys constants = new XmlKeys();

            StateMachineManager stateMachineManager = new(constants,
                StateDependencyImplimentation.StateDependencyTypeFinderDefaultImplimentation,
                StateDependencyImplimentation.StateDependencyResolverDefaultImplimentation);

            stateMachineManager.SetMasterStateFile(Assembly.GetExecutingAssembly(), masterStateXmlFile);


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
