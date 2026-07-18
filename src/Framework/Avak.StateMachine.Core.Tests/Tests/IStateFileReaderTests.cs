using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.Tests
{
    [TestClass]
    public class IStateFileReaderTests
    {
        private Stream FileStream = null!;
        [TestInitialize]
        public void Setup()
        {
            // Runs before each test
            var assembly = Assembly.GetExecutingAssembly();
            string appStateFile = "Avak.StateMachine.Core.Tests.StateManager.TestStateFile.xml";
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
        public void Should_Load_Valid_XmlStateFile_Successfully()
        {
            // Arrange
            IXmlKeys constants = new XmlKeys();
            StateMachineManager stateMachineManager = new(constants, StateDependencyImplimentation.StateDependencyObjectFinderDefaultImplimentation);
            stateMachineManager.SetStateFile(FileStream);

            // Act
            bool loadResult = stateMachineManager.LoadStateFile();

            // Assert
            Assert.IsTrue(loadResult);
        }

        // Read triggers from xml file
        [TestMethod]
        public void GetTriggers_WithCountZero_ThrowsException()
        {
            // Arrange
            IXmlKeys constants = new XmlKeys();
            StateMachineManager stateMachineManager = new(constants, StateDependencyImplimentation.StateDependencyObjectFinderDefaultImplimentation);
            stateMachineManager.SetStateFile(FileStream);
            bool loadResult = stateMachineManager.LoadStateFile();

            // Act
            Exception ex = Assert.Throws<Exception>(() => stateMachineManager.GetStateGraph());

            // Assert
            Assert.AreEqual("Triggers not present in the state file. Add <Triggers></Triggers> if you intend to define just states without triggers.", ex.Message);
        }
    }
}
