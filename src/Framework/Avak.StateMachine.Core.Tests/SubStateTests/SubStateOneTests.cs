using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using Avak.StateMachine.Core.States;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.SubStateTests
{
    [TestClass]
    public class SubStateOneTests
    {
        private string masterStateXmlFile = string.Empty;
        [TestInitialize]
        public void Setup()
        {
            masterStateXmlFile = "Avak.StateMachine.Core.Tests.StateManager.MasterStateXmlFileWithSubStateXmlFileRefs.xml";

        }

        [TestCleanup]
        public void Cleanup()
        {
            // Runs after each test (clean up files, database connections, etc.)
            // Close the stream.
        }

        [TestMethod]
        public void GetStates_LookForTransitions_AndTriggers()
        {
            // Arrange
            IXmlKeys constants = new XmlKeys();

            StateMachineManager stateMachineManager = new(constants,
                StateDependencyImplimentation.StateDependencyTypeFinderDefaultImplimentation,
                StateDependencyImplimentation.StateDependencyResolverDefaultImplimentation);

            stateMachineManager.SetMasterStateFile(Assembly.GetExecutingAssembly(), masterStateXmlFile);

            stateMachineManager.LoadMasterStateFile();

            // Act
            List<MasterStateBase> states = stateMachineManager.GetCurrentStateGraph().StateList;
        }
    }
}
