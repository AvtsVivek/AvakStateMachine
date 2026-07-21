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
        private Stream fileStream = null!;
        [TestInitialize]
        public void Setup()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string appStateFile = "Avak.StateMachine.Core.Tests.StateManager.BasicTransitions.xml";
            fileStream = assembly.GetManifestResourceStream(appStateFile)!;
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Runs after each test (clean up files, database connections, etc.)

            // Close the stream.
            fileStream.Close();
            fileStream.Dispose();
        }

        [TestMethod]
        public void GetStates_LookForTransitions_AndTriggers()
        {
            // Arrange
            IXmlKeys constants = new XmlKeys();
            StateMachineManager stateMachineManager = new(constants, StateDependencyImplimentation.StateDependencyObjectFinderDefaultImplimentation);
            stateMachineManager.SetMasterStateFile(fileStream);
            bool loadResult = stateMachineManager.LoadMasterStateFile();
            IStateGraph stateGraph = stateMachineManager.GetStateGraph();
            // Act
            List<MasterStateBase> states = stateGraph.StateList;

            // Assert
            Assert.HasCount(3, states);
            List<Transition> zerothStateTransitions = states[0].Transitions;
            Assert.HasCount(2, zerothStateTransitions);

            List<Transition> firstStateTransitions = states[1].Transitions;
            Assert.IsEmpty(firstStateTransitions);


            Trigger nextStateTrigger = stateGraph.TriggerList.First(t => t.Name == "EnterCcFromAa");

            var result = stateMachineManager.IsTriggeredTriansitionValid(stateMachineManager.CurrentState, nextStateTrigger);


            stateMachineManager.DoTriggeredTriansition(stateMachineManager.CurrentState, nextStateTrigger);


            List<Transition> secondStateTransitions = states[2].Transitions;
            Assert.HasCount(1, secondStateTransitions);

            Assert.AreEqual(states[1].Name, secondStateTransitions[0].Target.Name);
        }
    }
}
