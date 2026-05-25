using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.Tests
{
	[TestClass]
	public class ArbitraryNamespaceTests
	{
		private Stream fileStream = null!;
		[TestInitialize]
		public void Setup()
		{
			var assembly = Assembly.GetExecutingAssembly();
			string appStateFile = "Avak.StateMachine.Core.Tests.StateManager.ArbitraryNamespace.xml";
			fileStream = assembly.GetManifestResourceStream(appStateFile)!;
		}

		[TestCleanup]
		public void Cleanup()
		{
			// Runs after each test (clean up files, database connections, etc.)

			// Close the stream.
			fileStream.Close();
			fileStream.Dispose();
		}

		[TestMethod]
		public void GetArbitraryStates_Has3Count_InitialSet()
		{
			// Arrange
			IXmlKeys constants = new XmlKeys();

			// IStateFileReader reader = new XmlStateFileReader(constants);

			StateMachineManager stateMachineManager = new(constants, StateDependencyImplimentation.StateDependencyObjectFinderDefaultImplimentation);

			stateMachineManager.SetStateFile(fileStream);

			bool loadResult = stateMachineManager.LoadStateFile();

			// Act
			List<StateBase> states = stateMachineManager.GetStateGraph().StateList;

			// Assert
			Assert.HasCount(3, states);
			Assert.IsTrue(states[0].IsInitial);
			Assert.IsFalse(states[1].IsInitial);
			Assert.IsFalse(states[2].IsInitial);
		}
	}
}
