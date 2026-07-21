using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using Avak.StateMachine.Core.States;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.SubStateTests
{
    [TestClass]
    public class SubStateOneTests
    {
        private Stream FileStream = null!;

        [TestInitialize]
        public void Setup()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string appStateFile = "Avak.StateMachine.Core.Tests.StateManager.MasterStateXmlFileWithSubStateXmlFileRefs.xml";
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
        public void GetStates_LookForTransitions_AndTriggers()
        {
            // Arrange
            IXmlKeys constants = new XmlKeys();
            StateMachineManager stateMachineManager = new(constants, StateDependencyImplimentation.StateDependencyObjectFinderDefaultImplimentation);

            stateMachineManager.SetMasterStateFile(FileStream);
            bool loadResult = stateMachineManager.LoadMasterStateFile();

            // Act
            List<MasterStateBase> states = stateMachineManager.GetStateGraph().StateList;

            // Assert
            // Assert.HasCount(4, states);
            //List<Transition> zerothStateTransitions = states[0].Transitions;
            //Assert.HasCount(2, zerothStateTransitions);

            //List<Transition> firstStateTransitions = states[1].Transitions;
            //Assert.IsEmpty(firstStateTransitions);

            //List<Transition> secondStateTransitions = states[2].Transitions;
            //Assert.HasCount(1, secondStateTransitions);

            //Assert.AreEqual(states[1].Name, secondStateTransitions[0].Target.Name);
        }
    }
}
