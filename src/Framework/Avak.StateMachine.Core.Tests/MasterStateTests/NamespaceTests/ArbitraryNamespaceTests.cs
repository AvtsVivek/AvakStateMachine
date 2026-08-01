using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using Avak.StateMachine.Core.States;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.MasterStateTests.NamespaceTests
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
        private string masterStateXmlFile = string.Empty;
        [TestInitialize]
        public void Setup()
        {
            masterStateXmlFile = "Avak.StateMachine.Core.Tests.StateManager" +
                ".NameSpaceTestsXmlFiles.ArbitraryNamespace.xml";
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Runs after each test (clean up files, database connections, etc.)
            StateXmlFileTree.Instance.Clear();
        }

        [TestMethod]
        public void GetArbitraryStates_Has1Count_InitialSet()
        {
            // Arrange
            IXmlKeys constants = new XmlKeys();

            StateMachineManager stateMachineManager = new(constants,
                StateDependencyImplimentation.StateDependencyTypeFinderDefaultImplimentation,
                StateDependencyImplimentation.StateDependencyResolverDefaultImplimentation);

            stateMachineManager.SetMasterStateFile(Assembly.GetExecutingAssembly(), masterStateXmlFile);

            // Act
            List<MasterStateBase> states = stateMachineManager.GetCurrentStateGraph().StateList;

            // Assert
            Assert.HasCount(1, states);
            Assert.IsTrue(states[0].IsInitial);
            // Since the state file does not have any triggers, the only state that the state graph
            // will have is the initial, or Aa.
            // Since the framework now instanciates the states lazyly, the first state is instanciated. 
            // And there are no triggers and transitions. So it will not go any further.

            // Assert.IsFalse(states[1].IsInitial);
            // Assert.IsFalse(states[2].IsInitial);
        }
    }
}
