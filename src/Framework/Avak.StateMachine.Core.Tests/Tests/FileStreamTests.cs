using System.Reflection;

namespace Avak.StateMachine.Core.Tests.Tests
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

    [TestClass]
    public sealed class FileStreamTests
    {
        [TestMethod]
        public void StateFileStreamNotNullAssert()
        {
            // Arrange
            var assembly = Assembly.GetExecutingAssembly();
            string appStateFile = "Avak.StateMachine.Core.Tests.StateManager.TestStateFile.xml";

            // Act
            using Stream stream = assembly.GetManifestResourceStream(appStateFile)!;

            // Assert
            Assert.IsNotNull(stream);
        }
    }
}
