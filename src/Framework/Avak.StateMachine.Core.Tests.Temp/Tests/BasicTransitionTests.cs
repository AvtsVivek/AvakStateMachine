using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using Avak.StateMachine.Core.States;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.Tests
{
    [TestClass]
    public class BasicTransitionTests
    {
        private Stream fileStream = null!;
        [TestInitialize]
        public void Setup()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string appStateFile = "Avak.StateMachine.Core.Tests.Temp.StateManager.BasicTransitions.xml";
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
            // IStateFileReader reader = new XmlStateFileReader(constants);
            StateMachineManager stateMachineManager = new(constants, StateDependencyImplimentation.StateDependencyObjectFinderDefaultImplimentation);

            stateMachineManager.SetStateFile(fileStream);
            bool loadResult = stateMachineManager.LoadStateFile();

            // Act
            List<MasterStateBase> states = stateMachineManager.GetStateGraph().StateList;

            // Assert
            Assert.HasCount(3, states);
            List<Transition> zerothStateTransitions = states[0].Transitions;
            Assert.HasCount(2, zerothStateTransitions);

            List<Transition> firstStateTransitions = states[1].Transitions;
            Assert.IsEmpty(firstStateTransitions);

            List<Transition> secondStateTransitions = states[2].Transitions;
            Assert.HasCount(1, secondStateTransitions);

            Assert.AreEqual(states[1].Name, secondStateTransitions[0].Target.Name);
        }
    }
}
