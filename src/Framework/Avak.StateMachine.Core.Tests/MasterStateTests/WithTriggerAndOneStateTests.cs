using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using Avak.StateMachine.Core.States;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.MasterStateTests
{
    [TestClass]
    public class WithTriggersTagAndOneStateTests
    {
        private string masterStateXmlFile = string.Empty; [TestInitialize]
        public void Setup()
        {
            // Runs before each test
            masterStateXmlFile = "Avak.StateMachine.Core.Tests.StateManager.WithTriggersTagAndOneStateStateFile.xml";
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Runs after each test (clean up files, database connections, etc.)
            StateXmlFileTree.Instance.Clear();
        }

        [TestMethod]
        public void GetStates_WithCountOne_StateFound()
        {
            // Arrange
            IXmlKeys constants = new XmlKeys();

            StateMachineManager stateMachineManager = new(constants,
                StateDependencyImplimentation.StateDependencyTypeFinderDefaultImplimentation,
                StateDependencyImplimentation.StateDependencyResolverDefaultImplimentation);

            stateMachineManager.SetMasterStateFile(Assembly.GetExecutingAssembly(), masterStateXmlFile);

            // Act
            List<Trigger> triggers = stateMachineManager.GetCurrentStateGraph().TriggerList;
            List<MasterStateBase> states = stateMachineManager.GetCurrentStateGraph().StateList;
            StateBase zerothState = states[0];

            // Assert
            // Assert.AreEqual("Avak.StateMachine.Core.Tests.StateManager.States", stateNamespace);
            Assert.IsEmpty(triggers);
            Assert.HasCount(1, states);
            Assert.AreEqual("Aa", zerothState.Name);
            Assert.AreEqual("Avak.StateMachine.Core.Tests.StateManager.States.Aa", zerothState.Id);
            Assert.AreEqual("Avak.StateMachine.Core.Tests.StateManager.States.Aa", zerothState.GetType().FullName);
        }
    }
}
