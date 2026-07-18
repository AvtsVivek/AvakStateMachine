using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using Avak.StateMachine.Core.States;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.MasterStateTests
{
    [TestClass]
    public class WithTriggerTests
    {
        private Stream FileStream = null!;
        [TestInitialize]
        public void Setup()
        {
            // Runs before each test
            var assembly = Assembly.GetExecutingAssembly();
            string appStateFile = "Avak.StateMachine.Core.Tests.StateManager.WithTriggersStateFile.xml";
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
        public void GetTriggers_AndStates_WithCountZero()
        {
            // Arrange
            IXmlKeys constants = new XmlKeys();
            // IStateFileReader reader = new XmlStateFileReader(constants);
            StateMachineManager stateMachineManager = new(constants, StateDependencyImplimentation.StateDependencyObjectFinderDefaultImplimentation);
            stateMachineManager.SetStateFile(FileStream);
            bool loadResult = stateMachineManager.LoadStateFile();

            // Act
            List<Trigger> triggers = stateMachineManager.GetStateGraph().TriggerList;
            List<MasterStateBase> states = stateMachineManager.GetStateGraph().StateList;
            Trigger zerothTrigger = triggers[0];
            Trigger firstTrigger = triggers[1];

            // Assert
            Assert.IsTrue(loadResult);
            Assert.HasCount(3, triggers);
            Assert.IsEmpty(states);
            Assert.AreEqual("EnterBbFromAa", zerothTrigger.Name);
            Assert.AreEqual(TriggerSource.Event, zerothTrigger.Source);
            Assert.AreEqual("EnterCcFromAa", firstTrigger.Name);
            Assert.AreEqual(TriggerSource.Event, firstTrigger.Source);
        }
    }
}
