using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.MasterStateTests
{
    [TestClass]
    [DoNotParallelize]
    public class IStateFileReaderTests
    {
        string masterStateXmlFile = string.Empty;
        [TestInitialize]
        public void Setup()
        {
            // Runs before each test
            masterStateXmlFile = "Avak.StateMachine.Core.Tests.StateManager.TestStateFile.xml";
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Runs after each test (clean up files, database connections, etc.)
            StateXmlFileTree.Instance.Clear();
        }

        // Read triggers from xml file
        [TestMethod]
        public void GetTriggers_WithCountZero_ThrowsException()
        {
            // Arrange
            IXmlKeys constants = new XmlKeys();

            StateMachineManager stateMachineManager = new(constants,
                StateDependencyImplimentation.StateDependencyTypeFinderDefaultImplimentation,
                StateDependencyImplimentation.StateDependencyResolverDefaultImplimentation);

            stateMachineManager.SetMasterStateFile(Assembly.GetExecutingAssembly(), masterStateXmlFile);


            // Act
            Exception ex = Assert.Throws<Exception>(() => stateMachineManager.GetCurrentStateGraph());

            // Assert
            Assert.AreEqual("Triggers not present in the state file. Add <Triggers></Triggers> element.", ex.Message);
        }
    }
}
