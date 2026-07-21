using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using Avak.StateMachine.Core.States;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.MasterStateTests
{
    // Some references
    // https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-writing-tests-lifecycle

    /*
[MethodName]_[Scenario]_[ExpectedBehavior]: This is the most common standard recommended by Microsoft Learn.
Example: Add_TwoPositiveNumbers_ReturnsSum
Should_[ExpectedBehavior]_When_[StateUnderTest]: This pattern creates a readable sentence that focuses on behavior.
Example: Should_ThrowException_When_AgeLessThan18
Given_[Precondition]_When_[Action]_Then_[ExpectedResult]: A Behavior-Driven Development (BDD) style that is highly descriptive but can result in very long names.
    
            [TestInitialize]
        public void Setup()
        {
            // Runs before each test
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Runs after each test (clean up files, database connections, etc.)
        }
     
     */

    /// <summary>
    /// The State type Cc will be in the namespace AribitratryDefaultNamespace. No name space is specified for Cc. The default is taken.
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

            stateMachineManager.SetMasterStateFile(FileStream);

            bool loadResult = stateMachineManager.LoadMasterStateFile();

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
