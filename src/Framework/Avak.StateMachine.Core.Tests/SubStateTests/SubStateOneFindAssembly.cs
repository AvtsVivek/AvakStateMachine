using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.SubStateTests
{
    [TestClass]
    [DoNotParallelize]
    public class SubStateOneFindAssembly
    {
        private string masterStateXmlFile = "Avak.StateMachine.Core.Tests.StateManager.XmlFilesWithSubStates.MasterStateXmlFileWithSubStateXmlFileRefs.xml";
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

        [TestMethod]
        public void LookForAssemblyAndResource()
        {
            IXmlKeys constants = new XmlKeys();

            StateMachineManager stateMachineManager = new(constants,
                StateDependencyImplimentation.StateDependencyTypeFinderDefaultImplimentation,
                StateDependencyImplimentation.StateDependencyResolverDefaultImplimentation);

            stateMachineManager.SetMasterStateFile(Assembly.GetExecutingAssembly(), masterStateXmlFile);
            stateMachineManager.PopulateStateXmlFileTree();
        }
    }
}
