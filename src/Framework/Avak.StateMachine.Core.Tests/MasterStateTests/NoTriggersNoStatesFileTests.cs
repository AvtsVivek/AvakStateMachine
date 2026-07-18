using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using Avak.StateMachine.Core.States;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.MasterStateTests
{
    [TestClass]
    public class NoTriggersNoStatesFileTests
    {
        private Stream FileStream = null!;
        [TestInitialize]
        public void Setup()
        {
            // Runs before each test
            var assembly = Assembly.GetExecutingAssembly();
            string appStateFile = "Avak.StateMachine.Core.Tests.StateManager.NoStatesNoTriggers.xml";
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

        // Read triggers from xml file
        [TestMethod]
        public void GetTriggers_AndStates_WithCountZero()
        {
            // Arrange
            IXmlKeys constants = new XmlKeys();
            StateMachineManager stateMachineManager = new(constants, StateDependencyImplimentation.StateDependencyObjectFinderDefaultImplimentation);
            stateMachineManager.SetStateFile(FileStream);
            bool loadResult = stateMachineManager.LoadStateFile();

            // Act
            List<Trigger> triggers = stateMachineManager.GetStateGraph().TriggerList;
            List<MasterStateBase> states = stateMachineManager.GetStateGraph().StateList;

            // Assert
            Assert.IsTrue(loadResult);
            Assert.IsEmpty(triggers);
            Assert.IsEmpty(states);
        }
    }
}
