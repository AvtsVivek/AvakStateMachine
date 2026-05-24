using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.Tests.TriggeredTransitions
{
	[TestClass]
	public class TriggeredTransitions
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
		public void TriggeredTransitionFromAa_FinalStateIsCc()
		{
			// Arrange
			var assembly = Assembly.GetExecutingAssembly();
			string appStateFile = "Avak.StateMachine.Core.Tests.Tests.TriggeredTransitions.BasicTransitions.xml";
			Stream resourceStream = assembly.GetManifestResourceStream(appStateFile)!;
			IXmlKeys constants = new XmlKeys();
			StateMachineManager stateMachineManager = new(constants);
			stateMachineManager.SetStateFile(resourceStream);
			bool loadResult = stateMachineManager.LoadStateFile();


			// Act 
			StateGraph stateGraph = stateMachineManager.GetStateGraph();
			StateBase stateAa = stateMachineManager.CurrentState;
			Trigger enterCcFromAa = stateGraph.TriggerList.First(t => t.Name == "EnterCcFromAa");
			(bool, string) result = stateMachineManager.DoTriggeredTriansition(stateAa, enterCcFromAa);

			// Assert
			Assert.IsTrue(loadResult);
			Assert.IsTrue(result.Item1);
			Assert.IsTrue(stateMachineManager.CurrentState.Name == "Cc");
			Assert.HasCount(1, stateMachineManager.CurrentState.Transitions);
		}

		[TestMethod]
		public void TriggeredTransitios_TwoTransitions_Aa_To_Cc_To_Bb()
		{
			// Arrange
			var assembly = Assembly.GetExecutingAssembly();
			string appStateFile = "Avak.StateMachine.Core.Tests.Tests.TriggeredTransitions.BasicTransitions.xml";
			Stream resourceStream = assembly.GetManifestResourceStream(appStateFile)!;
			IXmlKeys constants = new XmlKeys();
			StateMachineManager stateMachineManager = new(constants);
			stateMachineManager.SetStateFile(resourceStream);
			bool loadResult = stateMachineManager.LoadStateFile();


			// Act 
			StateGraph stateGraph = stateMachineManager.GetStateGraph();
			StateBase stateAa = stateMachineManager.CurrentState;
			StateBase stateCc = stateMachineManager.StateGraph.StateList.First(s => s.Name == "Cc");

			Trigger enterCcFromAa = stateGraph.TriggerList.First(t => t.Name == "EnterCcFromAa");
			(bool, string) result1 = stateMachineManager.DoTriggeredTriansition(stateAa, enterCcFromAa);

			Trigger enterBbFromCc = stateGraph.TriggerList.First(t => t.Name == "EnterBbFromCc");
			(bool, string) result2 = stateMachineManager.DoTriggeredTriansition(stateCc, enterBbFromCc);

			// Assert
			Assert.IsTrue(stateMachineManager.CurrentState.Name == "Bb");
			Assert.IsEmpty(stateMachineManager.CurrentState.Transitions);
		}
	}
}
