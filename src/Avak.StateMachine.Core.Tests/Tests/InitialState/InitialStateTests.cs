using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.Tests.InitialState
{
	[TestClass]
	public class InitialStateTests
	{
		[TestInitialize]
		public void Setup()
		{

		}

		[TestCleanup]
		public void Cleanup()
		{

		}

		[TestMethod]
		public void NoInitialSpecified_TopStateIsDefaultInitial()
		{
			// Arrange
			var assembly = Assembly.GetExecutingAssembly();
			string appStateFile = "Avak.StateMachine.Core.Tests.Tests.InitialState.NoInitialStateSpecified.xml";
			Stream resourceStream = assembly.GetManifestResourceStream(appStateFile)!;
			IXmlKeys constants = new XmlKeys();
			StateMachineManager stateMachineManager = new(constants, StateDependencyImplimentation.StateDependencyObjectFinderDefaultImplimentation);
			stateMachineManager.SetStateFile(resourceStream);
			bool loadResult = stateMachineManager.LoadStateFile();

			// Act 
			StateGraph stateGraph = stateMachineManager.GetStateGraph();
			StateBase stateBb = stateGraph.StateList.Where(state => state.Name == "Bb").FirstOrDefault()!;

			// Assert
			Assert.IsNotNull(stateBb);
			Assert.IsTrue(loadResult);
			Assert.IsTrue(stateBb.IsInitial);
			Assert.AreEqual(stateBb, stateMachineManager.CurrentState);
		}

		[TestMethod]
		public void InitialSpecified_TopStateIsInitial()
		{
			// Arrange
			var assembly = Assembly.GetExecutingAssembly();
			string appStateFile = "Avak.StateMachine.Core.Tests.Tests.InitialState.InitialStateSpecified.xml";
			Stream resourceStream = assembly.GetManifestResourceStream(appStateFile)!;
			IXmlKeys constants = new XmlKeys();
			StateMachineManager stateMachineManager = new(constants, StateDependencyImplimentation.StateDependencyObjectFinderDefaultImplimentation);
			stateMachineManager.SetStateFile(resourceStream);
			bool loadResult = stateMachineManager.LoadStateFile();

			// Act 
			StateGraph stateGraph = stateMachineManager.GetStateGraph();
			// stateMachineManager.Initialize();
			StateBase stateBb = stateGraph.StateList.Where(state => state.Name == "Bb").FirstOrDefault()!;

			// Assert
			Assert.IsNotNull(stateBb);
			Assert.IsTrue(loadResult);
			Assert.IsTrue(stateBb.IsInitial);
			Assert.AreEqual(stateBb, stateMachineManager.CurrentState);
		}
	}
}
