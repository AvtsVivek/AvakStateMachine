using Avak.StateMachine.Core;
using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using System.Reflection;

namespace Avak.StateMachine.Sample.ConsoleUI
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello, World!");

			PopulateStateListUsingFileStream();

		}

		private static void PopulateStateListUsingFileStream()
		{
			var assembly = Assembly.GetExecutingAssembly();

			string appStateTable = "Avak.StateMachine.Sample.ConsoleUI.StateManager.State.xml";

			using Stream stream = assembly.GetManifestResourceStream(appStateTable)!;

			if (stream == null)
			{
				return;
			}

			IXmlKeys constants = new XmlKeys();

			StateMachineManager stateMachineManager = new StateMachineManager(constants);

			stateMachineManager.SetStateFile(stream);

			bool isLoaded = stateMachineManager.LoadStateFile();

			StateGraph stateGraph = stateMachineManager.GetStateGraph();

			List<Trigger> triggers = stateMachineManager.StateGraph.TriggerList;

			Console.WriteLine($"The number of Triggers in the state file are {triggers.Count}");

			Console.WriteLine($"The number of states in the state file are {stateGraph.StateList.Count}");
		}
	}
}
