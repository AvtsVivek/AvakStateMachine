using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using Avak.StateMachine.Core.States;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.MasterStateTests
{
    // Verifies the Transitions are read from the state file and set to the state objs.
    [TestClass]
    public class BasicTransitionTests
    {
        private string masterStateXmlFile = string.Empty;
        [TestInitialize]
        public void Setup()
        {
            masterStateXmlFile = "Avak.StateMachine.Core.Tests.StateManager.BasicTransitions.xml";
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Runs after each test (clean up files, database connections, etc.)
        }

        [TestMethod]
        public void GetStates_LookForTransitions_AndTriggers()
        {
            // Arrange
            int numberOfStateObjectsCreated = 0;
            string nameOfStateJustCreated = string.Empty;
            IXmlKeys constants = new XmlKeys();

            StateMachineManager stateMachineManager = new(constants,
                StateDependencyImplimentation.StateDependencyTypeFinderDefaultImplimentation,
                StateDependencyImplimentation.StateDependencyResolverDefaultImplimentation);

            stateMachineManager.SetMasterStateFile(Assembly.GetExecutingAssembly(), masterStateXmlFile);

            // Tests the state created event on state machine manager.

            stateMachineManager.StateCreated += (sender, state) =>
            {
                numberOfStateObjectsCreated++;
                nameOfStateJustCreated = state.Name;
            };

            Assert.AreEqual(0, numberOfStateObjectsCreated);
            Assert.AreEqual("", nameOfStateJustCreated);
            IStateGraph stateGraph = stateMachineManager.GetCurrentStateGraph();
            Assert.AreEqual("Cc", nameOfStateJustCreated);
            Assert.AreEqual(3, numberOfStateObjectsCreated);
            // Act
            List<MasterStateBase> states = stateGraph.StateList;

            // Assert
            Assert.HasCount(3, states);
            List<Transition> zerothStateTransitions = states[0].Transitions;
            Assert.HasCount(2, zerothStateTransitions);

            List<Transition> firstStateTransitions = states[1].Transitions;
            Assert.IsEmpty(firstStateTransitions);

            Trigger enterCcFromAa = stateMachineManager.CurrentState.Transitions[1].Trigger;
            Assert.AreEqual("Aa", stateMachineManager.CurrentState.Name);
            (bool, string) result = stateMachineManager.IsTriggeredTriansitionValid(stateMachineManager.CurrentState, enterCcFromAa);
            bool success = stateMachineManager.DoTriggeredTriansition(stateMachineManager.CurrentState, enterCcFromAa);

            Assert.IsTrue(success);

            Assert.HasCount(1, stateMachineManager.CurrentState.Transitions);

            Assert.AreEqual("Cc", stateMachineManager.CurrentState.Name);
            Assert.AreEqual("Cc", nameOfStateJustCreated);
            Assert.AreEqual(3, numberOfStateObjectsCreated);
            List<Transition> secondStateTransitions = states[2].Transitions;
            Assert.HasCount(1, secondStateTransitions);
            Assert.AreEqual(states[1].Name, secondStateTransitions[0].Target.Name);
            success = stateMachineManager.DoTriggeredTriansition(stateMachineManager.CurrentState, stateMachineManager.CurrentState.Transitions[0].Trigger);
            Assert.IsTrue(success);
            Assert.AreEqual("Bb", stateMachineManager.CurrentState.Name);
            Assert.AreEqual("Dd", nameOfStateJustCreated);
            Assert.AreEqual(4, numberOfStateObjectsCreated);
        }
    }
}
