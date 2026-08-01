using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using Avak.StateMachine.Core.States;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.SubStateTests
{
    [TestClass]
    [DoNotParallelize]
    public class SubStateOneFindAssembly
    {
        private string masterStateXmlFile = "Avak.StateMachine.Core.Tests.StateManager" +
            ".XmlFilesWithSubStates.MasterStateXmlFileWithSubStateXmlFileRefs.xml";
        [TestInitialize]
        public void Setup()
        {

        }

        [TestCleanup]
        public void Cleanup()
        {
            // Runs after each test (clean up files, database connections, etc.)
            // Close the stream.
            StateXmlFileTree.Instance.Clear();
        }


        // This test is currently work in progress.
        [TestMethod]
        public void LookForAssemblyAndResource()
        {
            IXmlKeys constants = new XmlKeys();

            StateMachineManager stateMachineManager = new(constants,
                StateDependencyImplimentation.StateDependencyTypeFinderDefaultImplimentation,
                StateDependencyImplimentation.StateDependencyResolverDefaultImplimentation);

            stateMachineManager.SetMasterStateFile(Assembly.GetExecutingAssembly(), masterStateXmlFile);
            stateMachineManager.PopulateStateXmlFileTree();

            IStateGraph stateGraph = stateMachineManager.GetCurrentStateGraph();
            // Act
            List<MasterStateBase> states = stateGraph.StateList;

            // Assert
            Assert.HasCount(3, states);
        }
    }
}
