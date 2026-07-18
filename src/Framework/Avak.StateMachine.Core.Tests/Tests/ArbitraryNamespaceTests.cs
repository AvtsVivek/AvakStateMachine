using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using Avak.StateMachine.Core.States;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.Tests
{
    /// <summary>
    /// The State type Cc will be in the namespace AribitratryDefaultNamespace. No name space is specified. The default is taken.
    /// The state type Aa will be in the namespace AribitratryNamespaceForAa.
    /// We do not assert specifically on the namespace because the fact that the state is instanciated proves that the namespaces are correct.
    /// </summary>
    [TestClass]
    public class ArbitraryNamespaceTests
    {
        private Stream FileStream = null!;
        [TestInitialize]
        public void Setup()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string appStateFile = "Avak.StateMachine.Core.Tests.StateManager.ArbitraryNamespace.xml";
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
        public void GetArbitraryStates_Has3Count_InitialSet()
        {
            // Arrange
            IXmlKeys constants = new XmlKeys();

            StateMachineManager stateMachineManager = new(constants, StateDependencyImplimentation.StateDependencyObjectFinderDefaultImplimentation);

            stateMachineManager.SetStateFile(FileStream);

            bool loadResult = stateMachineManager.LoadStateFile();

            // Act
            List<MasterStateBase> states = stateMachineManager.GetStateGraph().StateList;

            // Assert
            Assert.HasCount(3, states);
            Assert.IsTrue(states[0].IsInitial);
            Assert.IsFalse(states[1].IsInitial);
            Assert.IsFalse(states[2].IsInitial);
        }
    }
}
